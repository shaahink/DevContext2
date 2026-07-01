namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects Orleans grain implementations — classes implementing <c>IGrainWithXKey</c>
/// or inheriting <c>Grain</c>. Collects public grain-interface methods. Produces
/// <see cref="GrainDetection"/> entries.</summary>
[ExtractorOrder(60)]
public sealed class OrleansGrainExtractor : IDiscoveryExtractor
{
    public string Name => "OrleansGrainExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Orleans],
        ["orleans-grain-detections"],
        ["model.Detections"],
        "Scans syntax trees for Grain / IGrainWithKey implementations");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Orleans);

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

                string? grainInterface = null;
                var isGrain = false;

                foreach (var bt in baseList.Types)
                {
                    var name = bt.Type.ToString();
                    if (name == "Grain" || name.StartsWith("Grain<", StringComparison.Ordinal))
                    {
                        isGrain = true;
                    }
                    else if (name.StartsWith("IGrainWith", StringComparison.Ordinal))
                    {
                        grainInterface = name;
                        isGrain = true;
                    }
                }

                if (!isGrain) continue;

                var className = classDecl.Identifier.ValueText;

                var methods = new List<string>();
                foreach (var method in classDecl.Members
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
                {
                    if (method.Modifiers.Any(m => m.ValueText == "public"))
                        methods.Add(method.Identifier.ValueText);
                }

                model.Detections.Add(new GrainDetection(
                    className, grainInterface ?? "Grain", methods.ToImmutableArray())
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.85f,
                });
            }
        }
    }
}
