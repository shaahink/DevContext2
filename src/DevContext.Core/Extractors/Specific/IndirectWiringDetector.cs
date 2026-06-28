using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects indirect wiring patterns (Activator.CreateInstance, DynamicProxy, service locator, reflection scanning).</summary>
[ExtractorOrder(50)]
public sealed class IndirectWiringDetector : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> ServiceLocatorPatterns =
        ["GetService", "GetRequiredService", "GetServices"];

    private static readonly ImmutableArray<string> ReflectionScanPatterns =
        ["GetTypes", "GetExportedTypes", "GetReferencedAssemblies", "LoadFrom", "LoadFile"];

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "IndirectWiringDetector";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], ["indirect-wiring-detections"],
        ["model.Detections"],
        "Detects indirect wiring patterns like Activator.CreateInstance, Castle DynamicProxy, service locator, and reflection scanning");
    /// <summary>Runs for both the Map (overview) and Trace (deep-dive) narrative scenarios, so reflection/
    /// service-locator wiring is surfaced on a plain Map too (Iteration 3 Step 5 / Low-15). Fast tier and
    /// already perf-optimized (ancestor-walk method lookup), so no Map-time regression.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.ActiveScenario.Name is "deep-dive" or "overview";

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        // Parallel across files; purely syntactic. The containing method is found by an ancestor walk
        // (O(depth)) — the old per-invocation scan of every method's line span was O(invocations×methods)
        // per file and dominated trace time on large repos (DntSite audit). model.Detections is a
        // ConcurrentBag (thread-safe Add).
        var opts = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount };
        await Parallel.ForEachAsync(context.Analysis.AllSourceFiles, opts, async (filePath, innerCt) =>
        {
            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, innerCt);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                return;
            }

            var root = await syntaxTree.GetRootAsync(innerCt).ConfigureAwait(false);

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;

                var containingMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (containingMethod is null) continue;

                var methodName = memberAccess.Name.Identifier.ValueText;
                var callerType = FindContainingType(invocation)?.Identifier.ValueText ?? "?";
                var callerMethod = containingMethod.Identifier.ValueText;

                if (methodName == "CreateInstance"
                    && memberAccess.Expression.ToString() == "Activator")
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    model.Detections.Add(new IndirectWiringDetection(
                        Kind: IndirectWiringKind.ReflectionActivation,
                        CallerType: callerType,
                        CallerMethod: callerMethod,
                        TargetType: ExtractActivatorTargetType(invocation))
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }

                if (methodName is "CreateProxy" or "CreateClassProxy"
                    && memberAccess.Expression.ToString().Contains("ProxyGenerator"))
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    model.Detections.Add(new IndirectWiringDetection(
                        Kind: IndirectWiringKind.DynamicProxy,
                        CallerType: callerType,
                        CallerMethod: callerMethod,
                        TargetType: memberAccess.Expression.ToString())
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }

                if (ServiceLocatorPatterns.Contains(methodName)
                    && memberAccess.Expression is IdentifierNameSyntax
                    && memberAccess.Expression.ToString() is "serviceProvider" or "sp" or "provider" or "services")
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    model.Detections.Add(new IndirectWiringDetection(
                        Kind: IndirectWiringKind.ManualServiceLocator,
                        CallerType: callerType,
                        CallerMethod: callerMethod,
                        TargetType: null)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    });
                }

                if (ReflectionScanPatterns.Contains(methodName)
                    && (memberAccess.Expression.ToString() == "Assembly"
                        || memberAccess.Expression.ToString().Contains("Assembly")))
                {
                    var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    model.Detections.Add(new IndirectWiringDetection(
                        Kind: IndirectWiringKind.ReflectionActivation,
                        CallerType: callerType,
                        CallerMethod: callerMethod,
                        TargetType: methodName)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                        Confidence = 0.8f,
                    });
                }
            }
        });
    }

    private static ClassDeclarationSyntax? FindContainingType(SyntaxNode? node)
    {
        while (node != null)
        {
            node = node.Parent;
            if (node is ClassDeclarationSyntax classDecl)
                return classDecl;
        }
        return null;
    }

    private static string? ExtractActivatorTargetType(InvocationExpressionSyntax invocation)
    {
        if (invocation.ArgumentList.Arguments.Count == 0) return null;

        var firstArg = invocation.ArgumentList.Arguments[0].Expression;

        if (firstArg is TypeOfExpressionSyntax typeofExpr)
            return typeofExpr.Type.ToString();

        if (firstArg is LiteralExpressionSyntax lit)
            return lit.Token.ValueText;

        return firstArg.ToString();
    }
}
