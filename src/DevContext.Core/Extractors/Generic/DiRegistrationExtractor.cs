using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(45)]
public sealed class DiRegistrationExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> LifetimeMethods =
        ["AddSingleton", "AddScoped", "AddTransient"];

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

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch
            {
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                var methodName = memberAccess.Name.Identifier.ValueText;
                if (!IsServicesChain(memberAccess)) continue;

                var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                if (LifetimeMethods.Contains(methodName))
                {
                    var lifetime = methodName switch
                    {
                        "AddSingleton" => "Singleton",
                        "AddScoped" => "Scoped",
                        "AddTransient" => "Transient",
                        _ => "Unknown",
                    };

                    var args = invocation.ArgumentList.Arguments;
                    string serviceType;
                    string implementationType;

                    if (args.Count >= 2)
                    {
                        serviceType = args[0].Expression?.ToString() ?? "?";
                        implementationType = args[1].Expression?.ToString() ?? "?";
                    }
                    else if (args.Count == 1)
                    {
                        serviceType = args[0].Expression?.ToString() ?? "?";
                        implementationType = serviceType;
                    }
                    else if (memberAccess.Name is GenericNameSyntax genericName)
                    {
                        var typeArgs = genericName.TypeArgumentList.Arguments;
                        serviceType = typeArgs.Count >= 1 ? typeArgs[0].ToString() : "?";
                        implementationType = typeArgs.Count >= 2 ? typeArgs[1].ToString() : serviceType;
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
                        ExtensionsUsed: extensions)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }
                else if (methodName.StartsWith("Add") && methodName.Length > 3)
                {
                    var args = invocation.ArgumentList.Arguments;
                    var argTypes = args
                        .Select(a => a.Expression?.ToString() ?? "?")
                        .ToImmutableArray();

                    model.Detections.Add(new DiRegistrationDetection(
                        ServiceType: methodName,
                        ImplementationType: argTypes.Length > 0 ? argTypes[0] : "?",
                        Lifetime: "Extension",
                        ExtensionsUsed: [methodName])
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                        Confidence = 0.7f,
                    });
                }
            }
        }
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
        return target == "Services" || target.EndsWith(".Services");
    }

    private static ImmutableArray<string> ExtractExtensionMethods(InvocationExpressionSyntax invocation)
    {
        var extensions = new List<string>();
        var current = invocation;

        while (current.Expression is MemberAccessExpressionSyntax ma)
        {
            if (ma.Name.Identifier.ValueText != "AddSingleton" &&
                ma.Name.Identifier.ValueText != "AddScoped" &&
                ma.Name.Identifier.ValueText != "AddTransient" &&
                ma.Name.Identifier.ValueText.StartsWith("Add"))
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
}
