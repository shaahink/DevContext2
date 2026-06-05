namespace DevContext.Core.Extractors.Generic;

/// <summary>
/// Analyzes sealed architecture signals and project structure to determine
/// the overall architecture style. Called by the pipeline between Stage 2 and 3
/// (after signals are sealed, before Specific extractors run).
/// This is NOT an IDiscoveryExtractor — it is invoked directly by the orchestrator.
/// </summary>
public sealed class ArchitectureStyleDetector
{
    public (ArchitectureStyle Style, float Confidence, string? Via) Detect(DiscoveryModel model)
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

        if (projectCount > 4 && signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected)
        {
            var combined = mr.Confidence * 1.2f;
            if (combined > maxConfidence)
            {
                maxConfidence = combined;
                style = ArchitectureStyle.CleanArchitecture;
                via = $"Signal:{ArchitectureSignals.Keys.MediatR}";
            }
        }

        if (projectCount > 2 && signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var ef) && ef.Detected)
        {
            if (style == ArchitectureStyle.Unknown && ef.Confidence > 0.5f)
            {
                style = ArchitectureStyle.NLayer;
                maxConfidence = ef.Confidence;
                via = $"Signal:{ArchitectureSignals.Keys.EfCore}";
            }
        }

        return (style, Math.Min(maxConfidence, 1.0f), via);
    }
}
