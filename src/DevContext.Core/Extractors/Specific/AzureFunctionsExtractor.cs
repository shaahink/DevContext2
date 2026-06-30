namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects Azure Functions entry points — methods decorated with <c>[Function]</c>
/// and trigger attributes (<c>[HttpTrigger]</c>, <c>[TimerTrigger]</c>, <c>[QueueTrigger]</c>,
/// <c>[BlobTrigger]</c>, etc.). Produces <see cref="FunctionEntryDetection"/> entries.</summary>
[ExtractorOrder(59)]
public sealed class AzureFunctionsExtractor : IDiscoveryExtractor
{
    public string Name => "AzureFunctionsExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.Functions],
        ["function-entry-detections"],
        ["model.Detections"],
        "Scans syntax trees for [Function] and trigger-attribute-decorated methods");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.Functions);

    private static readonly HashSet<string> TriggerAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Function", "HttpTrigger", "TimerTrigger", "QueueTrigger",
        "BlobTrigger", "EventHubTrigger", "ServiceBusTrigger",
        "CosmosDBTrigger", "EventGridTrigger", "SignalRTrigger",
        "OrchestrationTrigger", "ActivityTrigger", "EntityTrigger",
    };

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

            foreach (var method in root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();

                var triggers = new List<string>();
                foreach (var attrList in method.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        var attrName = GetSimpleName(attr.Name.ToString());
                        if (TriggerAttributes.Contains(attrName))
                            triggers.Add(attrName);
                    }
                }

                if (triggers.Count == 0) continue;

                var className = (method.Parent as Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)
                    ?.Identifier.ValueText ?? "?";
                var methodName = method.Identifier.ValueText;

                model.Detections.Add(new FunctionEntryDetection(
                    className, methodName, triggers.ToImmutableArray())
                {
                    ExtractorName = Name,
                    SourceFile = filePath,
                    LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Confidence = 0.95f,
                });
            }
        }
    }

    private static string GetSimpleName(string fullAttrName)
    {
        var name = fullAttrName;
        if (name.EndsWith("Attribute", StringComparison.Ordinal))
            name = name[..^9];
        var dot = name.LastIndexOf('.');
        return dot >= 0 ? name[(dot + 1)..] : name;
    }
}
