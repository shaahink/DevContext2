namespace DevContext.Core.Extractors.Generic;

public sealed class ArchitectureStyleDetector
{
    public static (ArchitectureStyle Style, float Confidence, string? Via) Detect(DiscoveryModel model)
    {
        var signals = model.Architecture.All;
        var scores = new Dictionary<ArchitectureStyle, (float Score, string Evidence)>();

        var hasMediatR = signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected;
        var hasEfCore = signals.ContainsKey(ArchitectureSignals.Keys.EfCore);
        var hasAspire = signals.TryGetValue(ArchitectureSignals.Keys.Aspire, out var aspire) && aspire.Detected;
        var hasMinimalApis = signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) && ma.Detected;
        var hasControllers = signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var ctrl) && ctrl.Detected;
        var hasFastEndpoints = signals.TryGetValue(ArchitectureSignals.Keys.FastEndpoints, out var fe) && fe.Detected;

        var mediatRHandlerCount = model.Detections.OfType<MediatRHandlerDetection>().Count(h => h.Kind is MediatRKind.Command or MediatRKind.Query);
        var notificationHandlerCount = model.Detections.OfType<MediatRHandlerDetection>().Count(h => h.Kind == MediatRKind.Notification);
        var aggregateCount = model.Detections.OfType<EfEntityDetection>().Count(d => d.IsAggregate);
        var refCounts = ComputeReferenceCounts(model.Projects);
        var folderRoles = DetectFolderRoles(model);
        var projectCount = model.Projects.Length;

        ScoreMicroservices(scores, model, hasAspire, projectCount);
        ScoreCleanArchitecture(scores, model, hasMediatR, mediatRHandlerCount, notificationHandlerCount, aggregateCount, refCounts, folderRoles);
        ScoreVerticalSlices(scores, hasFastEndpoints, hasMediatR, mediatRHandlerCount);
        ScoreNLayer(scores, hasEfCore, projectCount, folderRoles);
        ScoreMinimalApi(scores, hasMinimalApis, hasMediatR, projectCount);
        ScoreModularMonolith(scores, model);
        ScoreControllerBased(scores, signals, hasControllers, hasMediatR, hasMinimalApis);

        if (scores.Count == 0)
            return (ArchitectureStyle.Unknown, 0, null);

        var best = scores.MaxBy(kv => kv.Value.Score);
        return (best.Key, Math.Min(best.Value.Score, 1.0f), best.Value.Evidence);
    }

    private static void ScoreMicroservices(
        Dictionary<ArchitectureStyle, (float Score, string Evidence)> scores,
        DiscoveryModel model,
        bool hasAspire,
        int projectCount)
    {
        if (!hasAspire || projectCount < 3) return;

        var hasAppHost = model.Projects.Any(p =>
            p.Name.EndsWith(".AppHost", StringComparison.OrdinalIgnoreCase)
            || p.PackageReferences.Any(pr => pr.Name.StartsWith("Aspire.Hosting", StringComparison.OrdinalIgnoreCase)));
        if (!hasAppHost) return;

        var svcCount = model.Projects.Count(p => !IsInfrastructureProject(p.Name));
        var evidence = new List<string> { $"Aspire orchestration with {svcCount} service projects" };
        var score = Math.Min(0.65f + svcCount * 0.05f, 0.82f);
        scores[ArchitectureStyle.Microservices] = (score, string.Join("; ", evidence));
    }

    private static void ScoreCleanArchitecture(
        Dictionary<ArchitectureStyle, (float Score, string Evidence)> scores,
        DiscoveryModel model,
        bool hasMediatR,
        int mediatRHandlerCount,
        int notificationHandlerCount,
        int aggregateCount,
        List<ProjectRefStats> refCounts,
        HashSet<string> folderRoles)
    {
        if (!hasMediatR) return;

        var dddLayers = (folderRoles.Contains("Domain") ? 1 : 0)
                      + (folderRoles.Contains("Application") ? 1 : 0)
                      + (folderRoles.Contains("Infrastructure") ? 1 : 0)
                      + (folderRoles.Contains("Api") ? 1 : 0);
        var hasDomainCore = refCounts.Exists(r => r.HighFanIn && r.LowFanOut);

        if (dddLayers < 2 && aggregateCount < 1 && notificationHandlerCount < 1) return;

        var dddEvidence = new List<string>();
        if (dddLayers >= 2) dddEvidence.Add($"DDD folder layers: {string.Join(", ", folderRoles)}");
        if (aggregateCount >= 1) dddEvidence.Add($"{aggregateCount} aggregates");
        if (notificationHandlerCount >= 1) dddEvidence.Add($"{notificationHandlerCount} domain-event handlers");
        if (hasDomainCore) dddEvidence.Add("domain-core ref pattern (high fan-in, low fan-out)");
        dddEvidence.Add($"MediatR with {mediatRHandlerCount} handlers");

        scores[ArchitectureStyle.CleanArchitecture] = (Math.Min(0.5f + dddLayers * 0.1f + aggregateCount * 0.05f, 0.95f),
            string.Join("; ", dddEvidence));
    }

    private static void ScoreVerticalSlices(
        Dictionary<ArchitectureStyle, (float Score, string Evidence)> scores,
        bool hasFastEndpoints,
        bool hasMediatR,
        int mediatRHandlerCount)
    {
        if (!hasFastEndpoints) return;

        var vEvidence = new List<string> { "FastEndpoints detected" };
        if (hasMediatR) vEvidence.Add($"MediatR with {mediatRHandlerCount} handlers");
        scores[ArchitectureStyle.VerticalSlices] = (hasMediatR ? 0.85f : 0.7f,
            string.Join("; ", vEvidence));
    }

    private static void ScoreNLayer(
        Dictionary<ArchitectureStyle, (float Score, string Evidence)> scores,
        bool hasEfCore,
        int projectCount,
        HashSet<string> folderRoles)
    {
        if (!hasEfCore || projectCount <= 2) return;
        if (scores.ContainsKey(ArchitectureStyle.CleanArchitecture)) return;

        scores[ArchitectureStyle.NLayer] = (0.6f,
            $"EF Core + {projectCount} projects; folder roles: {string.Join(", ", folderRoles)}");
    }

    private static void ScoreMinimalApi(
        Dictionary<ArchitectureStyle, (float Score, string Evidence)> scores,
        bool hasMinimalApis,
        bool hasMediatR,
        int projectCount)
    {
        if (!hasMinimalApis || hasMediatR) return;

        scores[ArchitectureStyle.MinimalApi] = (projectCount == 1 ? 0.9f : 0.65f,
            $"Minimal APIs + {projectCount} project(s); no MediatR");
    }

    private static void ScoreModularMonolith(
        Dictionary<ArchitectureStyle, (float Score, string Evidence)> scores,
        DiscoveryModel model)
    {
        var moduleNames = model.Projects.Select(p => p.Name.ToLowerInvariant())
            .Where(n => n.Contains("module", StringComparison.Ordinal) || n.Contains("bounded", StringComparison.Ordinal) || n.Contains("context", StringComparison.Ordinal))
            .ToList();
        if (moduleNames.Count < 2) return;
        if (scores.ContainsKey(ArchitectureStyle.Microservices)) return;

        scores[ArchitectureStyle.ModularMonolith] = (0.55f + moduleNames.Count * 0.05f,
            $"{moduleNames.Count} module-like sub-projects: {string.Join(", ", moduleNames)}");
    }

    private static void ScoreControllerBased(
        Dictionary<ArchitectureStyle, (float Score, string Evidence)> scores,
        IReadOnlyDictionary<string, FeatureSignal> signals,
        bool hasControllers,
        bool hasMediatR,
        bool hasMinimalApis)
    {
        if (!hasControllers || hasMediatR) return;

        var ctrlConf = signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var cs) ? cs.Confidence : 0;
        var maConf = signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var mas) ? mas.Confidence : 0;
        if (hasMinimalApis && ctrlConf < maConf) return;

        float score = !hasMinimalApis ? 0.7f : 0.55f;
        scores[ArchitectureStyle.ControllerBased] = (score,
            $"Controllers detected (conf={ctrlConf:F1}); MediatR=no, MinimalApi={(hasMinimalApis ? $"yes(conf={maConf:F1})" : "no")}");

        if (hasMinimalApis && ctrlConf >= maConf)
            scores.Remove(ArchitectureStyle.MinimalApi);
    }

    private static List<ProjectRefStats> ComputeReferenceCounts(ImmutableArray<ProjectInfo> projects)
    {
        var projNames = projects.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var results = new List<ProjectRefStats>(projects.Length);
        foreach (var p in projects)
        {
            var outgoing = p.ProjectReferences.Length;
            var incoming = projects.Count(other =>
                other.ProjectReferences.Any(r => string.Equals(r, p.Name, StringComparison.OrdinalIgnoreCase)));
            results.Add(new ProjectRefStats(p.Name, incoming, outgoing));
        }
        return results;
    }

    private static HashSet<string> DetectFolderRoles(DiscoveryModel model)
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var conventions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Domain"] = ["/Domain/", "/DomainModel/", "/AggregatesModel/"],
            ["Application"] = ["/Application/", "/UseCases/", "/ApplicationCore/"],
            ["Infrastructure"] = ["/Infrastructure/", "/Infra/", "/Persistence/"],
            ["Api"] = ["/Api/", "/Controllers/", "/Endpoints/"],
            ["Core"] = ["/Core/", "/Abstractions/"],
        };

        foreach (var (role, patterns) in conventions)
        {
            foreach (var project in model.Projects)
            {
                if (project.Name.Contains(role, StringComparison.OrdinalIgnoreCase))
                {
                    roles.Add(role);
                    break;
                }
            }
            if (roles.Contains(role)) continue;

            foreach (var type in model.Types.Values.Take(200))
            {
                var norm = type.FilePath.Replace('\\', '/');
                if (patterns.Any(pt => norm.Contains(pt, StringComparison.OrdinalIgnoreCase)))
                {
                    roles.Add(role);
                    break;
                }
            }
        }

        return roles;
    }

    private static bool IsInfrastructureProject(string name)
    {
        var lowered = name.ToLowerInvariant();
        return lowered.Contains(".servicedefaults", StringComparison.Ordinal)
            || lowered.Contains(".apphost", StringComparison.Ordinal)
            || lowered.Contains("shared", StringComparison.Ordinal)
            || lowered.Contains("common", StringComparison.Ordinal)
            || lowered.Contains(".eventbus", StringComparison.Ordinal);
    }

    private readonly record struct ProjectRefStats(string Name, int Incoming, int Outgoing)
    {
        public bool HighFanIn => Incoming >= 2;
        public bool LowFanOut => Outgoing <= 2;
    }
}
