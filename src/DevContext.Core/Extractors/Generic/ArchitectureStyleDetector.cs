namespace DevContext.Core.Extractors.Generic;

/// <summary>
/// Analyzes sealed architecture signals and project structure to determine
/// the overall architecture style. Called by the pipeline between Stage 2 and 3.
/// Detects hybrid/multi-style architectures and provides confidence scoring.
/// </summary>
public sealed class ArchitectureStyleDetector
{
    public static (ArchitectureStyle Style, float Confidence, string? Via) Detect(DiscoveryModel model)
    {
        var signals = model.Architecture.All;
        var projectCount = model.Projects.Length;
        var projectNames = model.Projects.Select(p => p.Name.ToLowerInvariant()).ToArray();

        // Score each candidate style independently, pick the highest confidence
        var scores = new Dictionary<ArchitectureStyle, (float Score, string Via)>();

        ScoreMinimalApi(signals, projectNames, scores);
        ScoreCleanArchitecture(signals, projectCount, projectNames, scores);
        ScoreVerticalSlices(signals, projectNames, scores);
        ScoreNLayer(signals, projectCount, scores);
        ScoreModularMonolith(signals, projectNames, scores);
        ScoreMicroservices(signals, projectCount, scores);

        // Demote MinimalApi when controllers signal is stronger
        if (signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var ctrlSignal) && ctrlSignal.Detected
            && signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var maSignal) && maSignal.Detected
            && ctrlSignal.Confidence >= maSignal.Confidence
            && scores.TryGetValue(ArchitectureStyle.MinimalApi, out var maScore))
        {
            scores.Remove(ArchitectureStyle.MinimalApi);
            // Controller-based web app
            scores[ArchitectureStyle.MinimalApi] = (maScore.Score * 0.6f, $"Signal:{ArchitectureSignals.Keys.Controllers}+minimal-apis (controller-dominant web app)");
        }

        if (scores.Count == 0)
            return (ArchitectureStyle.Unknown, 0, null);

        var best = scores.MaxBy(kv => kv.Value.Score);
        return (best.Key, Math.Min(best.Value.Score, 1.0f), best.Value.Via);
    }

    private static void ScoreMinimalApi(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        string[] projectNames,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) || !ma.Detected) return;

        var confidence = ma.Confidence;
        scores[ArchitectureStyle.MinimalApi] = (confidence, $"Signal:{ArchitectureSignals.Keys.MinimalApis}");
    }

    private static void ScoreCleanArchitecture(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        int projectCount,
        string[] projectNames,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) || !mr.Detected) return;

        var hasDomain = projectNames.Any(n => n.Contains("domain"));
        var hasApplication = projectNames.Any(n => n.Contains("application"));
        var hasInfrastructure = projectNames.Any(n => n.Contains("infrastructure"));

        var layerCount = (hasDomain ? 1 : 0) + (hasApplication ? 1 : 0) + (hasInfrastructure ? 1 : 0);
        var confidence = mr.Confidence * (0.5f + layerCount * 0.2f);

        if (layerCount >= 2)
            scores[ArchitectureStyle.CleanArchitecture] = (confidence, $"Signal:{ArchitectureSignals.Keys.MediatR}+layers:{layerCount}");
    }

    private static void ScoreVerticalSlices(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        string[] projectNames,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        var hasFastEndpoints = signals.TryGetValue("fast-endpoints", out var fe) && fe.Detected;

        if (!hasFastEndpoints) return;

        var confidence = hasFastEndpoints ? 0.7f : 0.3f;
        scores[ArchitectureStyle.VerticalSlices] = (confidence, "FastEndpoints detected");
    }

    private static void ScoreNLayer(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        int projectCount,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var ef) || !ef.Detected) return;

        if (projectCount > 2)
            scores[ArchitectureStyle.NLayer] = (ef.Confidence * 0.8f, $"Signal:{ArchitectureSignals.Keys.EfCore}+{projectCount}projects");
    }

    private static void ScoreModularMonolith(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        string[] projectNames,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        var moduleCount = projectNames.Count(n =>
            n.Contains("module") || n.Contains("bounded") || n.Contains("context"));
        if (moduleCount >= 2)
            scores[ArchitectureStyle.ModularMonolith] = (0.6f, $"{moduleCount}module-like projects");
    }

    private static void ScoreMicroservices(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        int projectCount,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.Aspire, out var aspire) || !aspire.Detected) return;
        if (projectCount >= 3)
            scores[ArchitectureStyle.Microservices] = (aspire.Confidence * 0.7f, $"Signal:{ArchitectureSignals.Keys.Aspire}+{projectCount}projects");
    }
}
