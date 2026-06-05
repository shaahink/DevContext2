using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], ["indirect-wiring-detections"],
        ["model.Detections"],
        "Detects indirect wiring patterns like Activator.CreateInstance, Castle DynamicProxy, service locator, and reflection scanning");
    /// <summary>Only runs for debug-endpoint and harden-di scenarios.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.ActiveScenario.Name is "debug-endpoint" or "harden-di";

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
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();
            var methodDecls = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            var methodMap = methodDecls.ToDictionary(
                m => (SyntaxTree: m.SyntaxTree, Span: m.SpanStart),
                m => m);

            foreach (var invocation in invocations)
            {
                ct.ThrowIfCancellationRequested();

                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess == null) continue;

                var methodName = memberAccess.Name.Identifier.ValueText;
                var containingMethod = FindContainingMethod(invocation, methodDecls);

                if (containingMethod == null) continue;

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
        }
    }

    private static MethodDeclarationSyntax? FindContainingMethod(
        InvocationExpressionSyntax invocation,
        IEnumerable<MethodDeclarationSyntax> methodDecls)
    {
        var invocationLine = invocation.GetLocation().GetLineSpan().StartLinePosition.Line;

        foreach (var method in methodDecls)
        {
            var methodStart = method.GetLocation().GetLineSpan().StartLinePosition.Line;
            var methodEnd = method.Body?.GetLocation().GetLineSpan().EndLinePosition.Line
                ?? method.GetLocation().GetLineSpan().EndLinePosition.Line;

            if (invocationLine >= methodStart && invocationLine <= methodEnd)
                return method;
        }

        return null;
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
