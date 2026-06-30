namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects SignalR hub classes — classes extending <c>Hub</c> or <c>Hub&lt;T&gt;</c>.
/// Collects public hub methods as entry-point methods. Produces <see cref="SignalRHubDetection"/> entries.</summary>
[ExtractorOrder(57)]
public sealed class SignalRHubExtractor : IDiscoveryExtractor
{
    public string Name => "SignalRHubExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.SignalR],
        ["signalr-hub-detections"],
        ["model.Detections"],
        "Scans syntax trees for Hub/Hub<T> subclasses and their public methods");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.SignalR);

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

                // Check for Hub or Hub<T> base
                var isHub = false;
                foreach (var bt in baseList.Types)
                {
                    var name = bt.Type.ToString();
                    if (name.StartsWith("Hub", StringComparison.Ordinal)
                        && (name.Length == 3 || name[3] == '<'))
                    {
                        isHub = true;
                        break;
                    }
                }
                if (!isHub) continue;

                var className = classDecl.Identifier.ValueText;
                var methods = new List<string>();
                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    methods.Add(method.Identifier.ValueText);
                }

                model.Detections.Add(new SignalRHubDetection(className, methods.ToImmutableArray())
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.9f,
                });
            }
        }
    }
}
