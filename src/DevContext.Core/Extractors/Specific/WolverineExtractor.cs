namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects Wolverine message handlers — classes with <c>Handle</c> methods
/// in Wolverine-convention projects. Produces <see cref="MessageConsumerDetection"/> entries.</summary>
[ExtractorOrder(62)]
public sealed class WolverineExtractor : IDiscoveryExtractor
{
    public string Name => "WolverineExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Wolverine],
        ["wolverine-handler-detections"],
        ["model.Detections"],
        "Scans for Wolverine handler convention classes");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Wolverine);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

            Microsoft.CodeAnalysis.SyntaxTree syntaxTree;
            try { syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct); }
            catch { continue; }

            var root = await syntaxTree.GetRootAsync(ct);
            foreach (var classDecl in root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();
                var className = classDecl.Identifier.ValueText;
                if (!className.EndsWith("Handler", StringComparison.Ordinal)) continue;

                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    if (method.Identifier.ValueText is "Handle" or "Consume"
                        && method.ParameterList.Parameters.Count >= 1)
                    {
                        var msgType = method.ParameterList.Parameters[0].Type?.ToString() ?? "TMessage";
                        model.Detections.Add(new MessageConsumerDetection(
                            msgType, className, "Wolverine")
                        {
                            ExtractorName = Name,
                            SourceFile = filePath,
                            LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            Confidence = 0.8f,
                        });
                        break;
                    }
                }
            }
        }
    }
}
