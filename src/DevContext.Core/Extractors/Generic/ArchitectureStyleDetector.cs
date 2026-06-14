namespace DevContext.Core.Extractors.Generic;

/// <summary>
/// Analyzes sealed architecture signals and project structure to determine
/// the overall architecture style. Called by the pipeline between Stage 2 and 3.
/// Detects hybrid/multi-style architectures and provides confidence scoring
/// with evidence items for transparency.
/// </summary>
public sealed class ArchitectureStyleDetector
{
    public static (ArchitectureStyle Style, float Confidence, string? Via, ImmutableArray<string> Evidence) Detect(DiscoveryModel model)
    {
        var signals = model.Architecture.All;
        var projectCount = model.Projects.Length;
        var projectNames = model.Projects.Select(p => p.Name.ToLowerInvariant()).ToArray();
        var evidence = ImmutableArray.CreateBuilder<string>();

        var scores = new Dictionary<ArchitectureStyle, (float Score, string Via)>();

        ScoreMinimalApi(signals, evidence, scores);
        ScoreCleanArchitecture(signals, projectCount, projectNames, evidence, scores);
        ScoreVerticalSlices(signals, projectNames, model, evidence, scores);
        ScoreNLayer(signals, projectCount, projectNames, evidence, scores);
        ScoreModularMonolith(signals, projectNames, evidence, scores);
        ScoreMicroservices(signals, projectCount, signals, scores);

        // When controllers are dominant over minimal-apis, classify as ControllerBased
        if (signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var ctrlSignal) && ctrlSignal.Detected
            && signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var maSignal) && maSignal.Detected
            && ctrlSignal.Confidence >= maSignal.Confidence
            && scores.TryGetValue(ArchitectureStyle.MinimalApi, out var maScore))
        {
            scores.Remove(ArchitectureStyle.MinimalApi);
            var baseScore = scores.TryGetValue(ArchitectureStyle.NLayer, out var nlScore)
                ? nlScore.Score : maScore.Score * 0.8f;
            scores[ArchitectureStyle.ControllerBased] = (baseScore, $"Signal:{ArchitectureSignals.Keys.Controllers}+minimal-apis (controller-dominant web app)");
        }

        if (scores.Count == 0)
            return (ArchitectureStyle.Unknown, 0, null, []);

        var best = scores.MaxBy(kv => kv.Value.Score);
        return (best.Key, Math.Min(best.Value.Score, 1.0f), best.Value.Via, evidence.ToImmutable());
    }

    private static void ScoreMinimalApi(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        ImmutableArray<string>.Builder evidence,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) || !ma.Detected) return;

        var confidence = ma.Confidence;
        scores[ArchitectureStyle.MinimalApi] = (confidence, $"Signal:{ArchitectureSignals.Keys.MinimalApis}");
        evidence.Add($"minimal-apis signal detected (confidence {ma.Confidence:P0})");
    }

    private static void ScoreCleanArchitecture(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        int projectCount,
        string[] projectNames,
        ImmutableArray<string>.Builder evidence,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) || !mr.Detected) return;

        var hasDomain = projectNames.Any(n => n.Contains("domain"));
        var hasApplication = projectNames.Any(n => n.Contains("application"));
        var hasInfrastructure = projectNames.Any(n => n.Contains("infrastructure"));

        var layerCount = (hasDomain ? 1 : 0) + (hasApplication ? 1 : 0) + (hasInfrastructure ? 1 : 0);
        var confidence = mr.Confidence * (0.5f + layerCount * 0.2f);

        if (layerCount >= 2)
        {
            scores[ArchitectureStyle.CleanArchitecture] = (confidence, $"Signal:{ArchitectureSignals.Keys.MediatR}+layers:{layerCount}");
            evidence.Add($"Clean Architecture: MediatR + {layerCount} named layers ({string.Join(", ", projectNames.Where(n => n.Contains("domain") || n.Contains("application") || n.Contains("infrastructure")).Take(3))})");
        }
    }

    private static void ScoreVerticalSlices(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        string[] projectNames,
        DiscoveryModel model,
        ImmutableArray<string>.Builder evidence,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        var featureFolders = AnalyzeFeatureFolders(model);

        if (featureFolders.verticalSliceCount >= 5)
        {
            var confidence = Math.Min(0.85f, 0.5f + featureFolders.verticalSliceCount * 0.05f);
            scores[ArchitectureStyle.VerticalSlices] = (confidence, $"feature-folders:{featureFolders.verticalSliceCount}slices");
            evidence.Add($"vertical-slice: {featureFolders.verticalSliceCount} feature folders with 3+ artifact types each (e.g. {string.Join(", ", featureFolders.topFolders.Take(4))})");
        }

        if (featureFolders.verticalSliceCount >= 3 && featureFolders.totalFeatures >= 10)
        {
            evidence.Add($"feature-based: {featureFolders.totalFeatures} feature directories in single project, {featureFolders.verticalSliceCount} are self-contained slices");
        }
    }

    private static void ScoreNLayer(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        int projectCount,
        string[] projectNames,
        ImmutableArray<string>.Builder evidence,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var ef) || !ef.Detected) return;

        if (projectCount > 2)
        {
            scores[ArchitectureStyle.NLayer] = (ef.Confidence * 0.8f, $"Signal:{ArchitectureSignals.Keys.EfCore}+{projectCount}projects");
            evidence.Add($"N-layer: EF Core + {projectCount} projects ({string.Join(", ", projectNames.Take(3))})");
        }
    }

    private static void ScoreModularMonolith(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        string[] projectNames,
        ImmutableArray<string>.Builder evidence,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        var moduleCount = projectNames.Count(n =>
            n.Contains("module") || n.Contains("bounded") || n.Contains("context"));
        if (moduleCount >= 2)
        {
            scores[ArchitectureStyle.ModularMonolith] = (0.6f, $"{moduleCount}module-like projects");
            evidence.Add($"modular-monolith: {moduleCount} module-like projects");
        }
    }

    private static void ScoreMicroservices(
        IReadOnlyDictionary<string, FeatureSignal> signals,
        int projectCount,
        IReadOnlyDictionary<string, FeatureSignal> signals2,
        Dictionary<ArchitectureStyle, (float, string)> scores)
    {
        if (!signals.TryGetValue(ArchitectureSignals.Keys.Aspire, out var aspire) || !aspire.Detected) return;
        if (projectCount >= 3)
            scores[ArchitectureStyle.Microservices] = (aspire.Confidence * 0.7f, $"Signal:{ArchitectureSignals.Keys.Aspire}+{projectCount}projects");
    }

    /// <summary>Analyzes type file paths to detect vertical-slice feature-folder organization.</summary>
    private static (int verticalSliceCount, int totalFeatures, List<string> topFolders) AnalyzeFeatureFolders(DiscoveryModel model)
    {
        // Group types by their feature directory: the directory ~2 levels under src/project that contains sub-folders
        // Pattern: .../Features/{FeatureName}/{Subfolder}/*.cs
        var featureGroups = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in model.Types.Values)
        {
            if (string.IsNullOrEmpty(type.FilePath)) continue;

            var file = type.FilePath.Replace('\\', '/');
            var idx = file.LastIndexOf("/Features/", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            var afterFeatures = file[(idx + "/Features/".Length)..];
            var parts = afterFeatures.Split('/');
            if (parts.Length < 2) continue;

            var featureName = parts[0];
            // The subfolder can be something like Entities, Services, EfConfig, Components, etc.
            // But also deeper like Posts/Entities or Posts/Services/SomeDir
            // We care about the immediate artifact type category
            var subfolder = parts.Length >= 2 ? parts[1] : parts[0];
            var category = NormalizeArtifactCategory(subfolder);

            if (!featureGroups.TryGetValue(featureName, out var cats))
            {
                cats = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                featureGroups[featureName] = cats;
            }
            cats.Add(category);
        }

        // Count how many feature folders have 3+ distinct artifact categories (true vertical slices)
        var verticalSliceCount = 0;
        var totalFeatures = featureGroups.Count;
        var topFolders = new List<string>();

        foreach (var (feature, cats) in featureGroups.OrderByDescending(kv => kv.Value.Count))
        {
            if (cats.Count >= 3)
            {
                verticalSliceCount++;
                topFolders.Add($"{feature}({cats.Count} types)");
            }
        }

        return (verticalSliceCount, totalFeatures, topFolders);
    }

    private static string NormalizeArtifactCategory(string folder)
    {
        var lower = folder.ToLowerInvariant();

        if (lower.Contains("entit")) return "entities";
        if (lower.Contains("service") || lower.Contains("manager")) return "services";
        if (lower.Contains("config") || lower.Contains("efconfig") || lower.Contains("efc")) return "config";
        if (lower.Contains("component") || lower.Contains("razor")) return "components";
        if (lower.Contains("controller") || lower.Contains("endpoint")) return "endpoints";
        if (lower.Contains("model") || lower.Contains("viewmodel") || lower.Contains("dto")) return "models";
        if (lower.Contains("scheduledtask") || lower.Contains("job") || lower.Contains("scheduler")) return "jobs";
        if (lower.Contains("rout") || lower.Contains("page")) return "routing";
        if (lower.Contains("validat")) return "validation";
        if (lower.Contains("email") || lower.Contains("notification")) return "notifications";
        if (lower.Contains("tag") || lower.Contains("utils") || lower.Contains("helper")) return "utils";

        // Catch-all: directories named like "Posts", "Courses", "Users" etc. that ARE the feature dir themselves
        // (when there's no subfolder beyond Features/FeatureName/Type.cs)
        if (folder == lower) return folder;

        return "other";
    }
}
