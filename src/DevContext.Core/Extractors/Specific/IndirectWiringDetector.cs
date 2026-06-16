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
    /// <summary>Only runs for deep-dive (Trace) scenario.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.ActiveScenario.Name is "deep-dive";

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                ct.ThrowIfCancellationRequested();
                CheckIndirectWiringPatterns(invocation, root, model, filePath);
            }
        }
    }

    private void CheckIndirectWiringPatterns(InvocationExpressionSyntax invocation, SyntaxNode root, DiscoveryModel model, string filePath)
    {
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null) return;

        var methodName = memberAccess.Name.Identifier.ValueText;
        var methodDecls = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        var containingMethod = FindContainingMethod(invocation, methodDecls);
        if (containingMethod == null) return;

        var callerType = FindContainingType(invocation)?.Identifier.ValueText ?? "?";
        var callerMethod = containingMethod.Identifier.ValueText;

        CheckActivatorPattern(invocation, memberAccess, methodName, callerType, callerMethod, model, filePath);
        CheckProxyGeneratorPattern(invocation, memberAccess, methodName, callerType, callerMethod, model, filePath);
        CheckServiceLocatorPattern(invocation, memberAccess, methodName, callerType, callerMethod, model, filePath);
        CheckReflectionScanPattern(invocation, memberAccess, methodName, callerType, callerMethod, model, filePath);
    }

    private void CheckActivatorPattern(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess, string methodName, string callerType, string callerMethod, DiscoveryModel model, string filePath)
    {
        if (!string.Equals(methodName, "CreateInstance", StringComparison.Ordinal)) return;
        if (!string.Equals(memberAccess.Expression.ToString(), "Activator", StringComparison.Ordinal)) return;

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

    private void CheckProxyGeneratorPattern(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess, string methodName, string callerType, string callerMethod, DiscoveryModel model, string filePath)
    {
        if (methodName is not "CreateProxy" and not "CreateClassProxy") return;
        if (!memberAccess.Expression.ToString().Contains("ProxyGenerator", StringComparison.Ordinal)) return;

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

    private void CheckServiceLocatorPattern(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess, string methodName, string callerType, string callerMethod, DiscoveryModel model, string filePath)
    {
        if (!ServiceLocatorPatterns.Contains(methodName, StringComparer.Ordinal)) return;
        if (memberAccess.Expression is not IdentifierNameSyntax) return;
        if (memberAccess.Expression.ToString() is not "serviceProvider" and not "sp" and not "provider" and not "services") return;

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

    private void CheckReflectionScanPattern(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess, string methodName, string callerType, string callerMethod, DiscoveryModel model, string filePath)
    {
        if (!ReflectionScanPatterns.Contains(methodName, StringComparer.Ordinal)) return;
        var exprStr = memberAccess.Expression.ToString();
        if (!string.Equals(exprStr, "Assembly", StringComparison.Ordinal) && !exprStr.Contains("Assembly", StringComparison.Ordinal)) return;

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
