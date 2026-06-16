using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(45)]
public sealed class DiRegistrationExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableHashSet<string> LifetimeMethods =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "AddSingleton", "AddScoped", "AddTransient");

    public string Name => "DiRegistrationExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;

    public ExtractorCapabilities Capabilities => new(
        [], ["di-registrations"],
        ["model.Detections"],
        "Cheap syntax matching for services.AddSingleton/AddScoped/AddTransient and AddX extension methods");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            // Use shared syntax node cache — populated first by SyntaxStructureExtractor
            FileSyntaxNodes nodes;
            try
            {
                var tree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
                nodes = await context.Analysis.GetOrParseSyntaxNodesAsync(filePath, async () =>
                {
                    var root = await tree.GetRootAsync(ct).ConfigureAwait(false);
                    return new FileSyntaxNodes(
                        [.. root.DescendantNodes().OfType<TypeDeclarationSyntax>()],
                        [.. root.DescendantNodes().OfType<InvocationExpressionSyntax>()]
                    );
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning(ex, "Failed to parse syntax for DI registrations: {Path}", filePath);
                continue;
            }

            foreach (var invocation in nodes.Invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                var methodName = memberAccess.Name.Identifier.ValueText;
                if (!IsServicesChain(memberAccess)) continue;

                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                ProcessInvocation(invocation, memberAccess, methodName, filePath, lineNumber, model, Name);
            }
        }
    }

    private static void ProcessInvocation(
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax memberAccess,
        string methodName,
        string filePath,
        int lineNumber,
        DiscoveryModel model,
        string extractorName)
    {
        if (LifetimeMethods.Contains(methodName))
        {
            ProcessLifetimeRegistration(invocation, memberAccess, methodName, filePath, lineNumber, model, extractorName);
        }
        else if (methodName.StartsWith("Add", StringComparison.Ordinal) && methodName.Length > 3)
        {
            ProcessExtensionRegistration(invocation, methodName, filePath, lineNumber, model, extractorName);
        }
        else if (methodName.StartsWith("Auto", StringComparison.Ordinal) || methodName.StartsWith("Scan", StringComparison.Ordinal))
        {
            ProcessBulkRegistration(methodName, filePath, lineNumber, model, extractorName);
        }
    }

    private static void ProcessLifetimeRegistration(
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax memberAccess,
        string methodName,
        string filePath,
        int lineNumber,
        DiscoveryModel model,
        string extractorName)
    {
        var lifetime = methodName.ToLowerInvariant() switch
        {
            "addsingleton" => "Singleton",
            "addscoped" => "Scoped",
            "addtransient" => "Transient",
            _ => "Unknown",
        };

        var args = invocation.ArgumentList.Arguments;
        string serviceType;
        string implementationType;
        DiRegistrationShape shape = DiRegistrationShape.DirectBinding;
        string? factorySummary = null;

        if (args.Count >= 2)
        {
            serviceType = args[0].Expression?.ToString() ?? "?";
            implementationType = args[1].Expression?.ToString() ?? "?";
            (shape, factorySummary) = ClassifyShape(args[1].Expression);
        }
        else if (args.Count == 1)
        {
            serviceType = args[0].Expression?.ToString() ?? "?";
            implementationType = serviceType;
            (shape, factorySummary) = ClassifyShape(args[0].Expression);
        }
        else if (memberAccess.Name is GenericNameSyntax genericName)
        {
            var typeArgs = genericName.TypeArgumentList.Arguments;
            serviceType = typeArgs.Count >= 1 ? typeArgs[0].ToString() : "?";
            implementationType = typeArgs.Count >= 2 ? typeArgs[1].ToString() : serviceType;
            shape = typeArgs.Count >= 2 ? DiRegistrationShape.DirectBinding : DiRegistrationShape.SelfRegistration;
        }
        else
        {
            serviceType = "?";
            implementationType = "?";
        }

        var extensions = ExtractExtensionMethods(invocation);

        model.Detections.Add(new DiRegistrationDetection(
            ServiceType: serviceType,
            ImplementationType: implementationType,
            Lifetime: lifetime,
            ExtensionsUsed: extensions,
            Shape: shape,
            FactorySummary: factorySummary)
        {
            ExtractorName = extractorName,
            SourceFile = filePath,
            LineNumber = lineNumber,
        });
    }

    private static void ProcessExtensionRegistration(
        InvocationExpressionSyntax invocation,
        string methodName,
        string filePath,
        int lineNumber,
        DiscoveryModel model,
        string extractorName)
    {
        var args = invocation.ArgumentList.Arguments;
        var argTypes = args
            .Select(a => a.Expression?.ToString() ?? "?")
            .ToImmutableArray();

        var implType = argTypes.Length > 0 ? argTypes[0] : "?";
        if (string.Equals(implType, "?", StringComparison.Ordinal) && invocation.Expression is MemberAccessExpressionSyntax ma
            && ma.Name is GenericNameSyntax genericName)
        {
            var typeArgs = genericName.TypeArgumentList.Arguments;
            implType = typeArgs.Count >= 1 ? typeArgs[0].ToString() : "?";
        }

        model.Detections.Add(new DiRegistrationDetection(
            ServiceType: methodName,
            ImplementationType: implType,
            Lifetime: "Extension",
            ExtensionsUsed: [methodName])
        {
            ExtractorName = extractorName,
            SourceFile = filePath,
            LineNumber = lineNumber,
            Confidence = 0.7f,
        });
    }

    private static void ProcessBulkRegistration(
        string methodName,
        string filePath,
        int lineNumber,
        DiscoveryModel model,
        string extractorName)
    {
        model.Detections.Add(new DiRegistrationDetection(
            ServiceType: methodName,
            ImplementationType: "*",
            Lifetime: "Bulk",
            ExtensionsUsed: [methodName],
            Shape: DiRegistrationShape.InlineFactory,
            FactorySummary: "[bulk auto-registration]")
        {
            ExtractorName = extractorName,
            SourceFile = filePath,
            LineNumber = lineNumber,
            Confidence = 0.6f,
        });
    }

    private static bool IsServicesChain(MemberAccessExpressionSyntax memberAccess)
    {
        var expr = memberAccess.Expression;

        while (expr is InvocationExpressionSyntax innerInvocation)
        {
            if (innerInvocation.Expression is MemberAccessExpressionSyntax innerAccess)
            {
                expr = innerAccess.Expression;
            }
            else
            {
                return false;
            }
        }

        var target = expr.ToString();
        return string.Equals(target, "Services", StringComparison.OrdinalIgnoreCase)
            || target.EndsWith(".Services", StringComparison.OrdinalIgnoreCase)
            || target.EndsWith(".services", StringComparison.Ordinal);
    }

    private static ImmutableArray<string> ExtractExtensionMethods(InvocationExpressionSyntax invocation)
    {
        var extensions = new List<string>();
        var current = invocation;

        while (current.Expression is MemberAccessExpressionSyntax ma)
        {
            if (!string.Equals(ma.Name.Identifier.ValueText, "AddSingleton", StringComparison.Ordinal) &&
!string.Equals(ma.Name.Identifier.ValueText, "AddScoped", StringComparison.Ordinal) &&
!string.Equals(ma.Name.Identifier.ValueText, "AddTransient", StringComparison.Ordinal) &&
                ma.Name.Identifier.ValueText.StartsWith("Add", StringComparison.Ordinal))
            {
                extensions.Add(ma.Name.Identifier.ValueText);
            }

            if (ma.Expression is InvocationExpressionSyntax inner)
            {
                current = inner;
            }
            else
            {
                break;
            }
        }

        return extensions.ToImmutableArray();
    }

    private static (DiRegistrationShape Shape, string? Summary) ClassifyShape(ExpressionSyntax? expr)
    {
        if (expr is not LambdaExpressionSyntax lambda)
            return (DiRegistrationShape.DirectBinding, null);

        var body = lambda.Body;

        // Expression body: sp => sp.GetRequiredService<T>()
        if (body is ExpressionSyntax exprBody)
        {
            if (exprBody is InvocationExpressionSyntax inv
                && inv.Expression is MemberAccessExpressionSyntax ma
                && (string.Equals(ma.Name.Identifier.ValueText, "GetRequiredService"
, StringComparison.Ordinal) || string.Equals(ma.Name.Identifier.ValueText, "GetService", StringComparison.Ordinal)))
            {
                return (DiRegistrationShape.ForwardingAlias,
                    $"alias → {ma.Name.Identifier.ValueText}");
            }

            var summary = BuildFactorySummary(exprBody);
            return (DiRegistrationShape.InlineFactory, summary);
        }

        // Block body: sp => { ... }
        if (body is BlockSyntax block)
        {
            var summary = BuildFactorySummary(block);
            return (DiRegistrationShape.InlineFactory, summary);
        }

        return (DiRegistrationShape.InlineFactory, "[factory]");
    }

    private static string? BuildFactorySummary(SyntaxNode body)
    {
        // Check if body itself is an object creation
        if (body is ObjectCreationExpressionSyntax bodyCreation)
        {
            var typeName = bodyCreation.Type.ToString();
            return $"[factory: new {typeName}]";
        }

        // Walk descendants for object creations
        foreach (var creation in body.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            var typeName = creation.Type.ToString();
            var deps = creation.ArgumentList?.Arguments
                .Select(a => a.Expression.ToString())
                .Where(s => !s.StartsWith("sp.", StringComparison.Ordinal) && !s.StartsWith('"'))
                .ToList() ?? [];

            if (deps.Count > 0)
                return $"[factory: new {typeName}({string.Join(", ", deps.Take(3))})]";

            return $"[factory: new {typeName}]";
        }

        // Detect File.ReadAllText, File.Exists patterns
        if (body.ToString().Contains("File.ReadAllText", StringComparison.Ordinal) || body.ToString().Contains("File.Exists", StringComparison.Ordinal))
            return "[factory: reads from disk]";

        // Detect foreach bulk registration
        if (body.ToString().Contains("foreach", StringComparison.Ordinal))
            return "[factory: bulk registration]";

        return "[factory]";
    }
}
