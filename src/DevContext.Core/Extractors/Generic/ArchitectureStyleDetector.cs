namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(50)]
public sealed class ArchitectureStyleDetector : IDiscoveryExtractor
{
    public string Name => "ArchitectureStyleDetector";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;

    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MediatR, ArchitectureSignals.Keys.MinimalApis, ArchitectureSignals.Keys.EfCore],
        [],
        ["model.DetectedStyle", "model.StyleConfidence", "model.StyleDetectedVia"],
        "Analyzes sealed signals and project structure to determine architecture style");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var signals = model.Architecture.All;
        float maxConfidence = 0;
        var style = ArchitectureStyle.Unknown;
        string? via = null;
        var projectCount = model.Projects.Length;

        if (signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) && ma.Detected && ma.Confidence > maxConfidence)
        {
            maxConfidence = ma.Confidence;
            style = ArchitectureStyle.MinimalApi;
            via = $"Signal:{ArchitectureSignals.Keys.MinimalApis}";
        }

        if (projectCount > 3
            && signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected)
        {
            var layerNamesExist = HasLayerNamedFolders(context.Analysis.AllProjectFiles);
            var combined = mr.Confidence * 1.2f;
            if (combined > maxConfidence && layerNamesExist)
            {
                maxConfidence = combined;
                style = ArchitectureStyle.CleanArchitecture;
                via = $"Signal:{ArchitectureSignals.Keys.MediatR}+LayerFolders";
            }
        }

        if (projectCount > 2
            && signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var ef) && ef.Detected)
        {
            if (style == ArchitectureStyle.Unknown && ef.Confidence > 0.5f)
            {
                style = ArchitectureStyle.NLayer;
                maxConfidence = ef.Confidence;
                via = $"Signal:{ArchitectureSignals.Keys.EfCore}";
            }
        }

        model.DetectedStyle = style;
        model.StyleConfidence = Math.Min(maxConfidence, 1.0f);
        model.StyleDetectedVia = via;
        return default;
    }

    private static bool HasLayerNamedFolders(IReadOnlyList<string> projectFiles)
    {
        var layerKeywords = new[] { "domain", "application", "infrastructure", "presentation", "api" };
        var anyMatch = false;

        foreach (var proj in projectFiles)
        {
            var dirName = Path.GetFileName(Path.GetDirectoryName(proj)) ?? "";
            var lower = dirName.ToLowerInvariant();
            if (layerKeywords.Any(k => lower.Contains(k)))
            {
                anyMatch = true;
            }
        }

        return anyMatch;
    }
}
