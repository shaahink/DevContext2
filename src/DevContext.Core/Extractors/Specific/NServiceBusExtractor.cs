namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects NServiceBus message handlers — classes implementing <c>IHandleMessages&lt;T&gt;</c>.
/// Produces <see cref="MessageConsumerDetection"/> entries.</summary>
[ExtractorOrder(61)]
public sealed class NServiceBusExtractor : IDiscoveryExtractor
{
    public string Name => "NServiceBusExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.NServiceBus],
        ["nsb-message-handler-detections"],
        ["model.Detections"],
        "Scans for IHandleMessages<T> implementations");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.NServiceBus);

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
                var baseList = classDecl.BaseList;
                if (baseList is null) continue;

                foreach (var bt in baseList.Types)
                {
                    var name = bt.Type.ToString();
                    if (!name.StartsWith("IHandleMessages<", StringComparison.Ordinal)) continue;

                    var className = classDecl.Identifier.ValueText;
                    model.Detections.Add(new MessageConsumerDetection(
                        name[17..^1], className, "NServiceBus")
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        Confidence = 0.9f,
                    });
                    break;
                }
            }
        }
    }
}
