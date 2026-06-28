namespace DevContext.Core.Extractors.Generic;

/// <summary>
/// Analyzes project structure using EVIDENCE (reference direction, folder roles, signal/presence data)
/// rather than brittle project-name substrings. Called by the pipeline between Stage 2 and 3.
/// PLAN-10 B2: replaces the old name-substring heuristic that misclassified eShop and VerticalSlice
/// as MinimalApi.
/// </summary>
public sealed class ArchitectureStyleDetector
{
    public static (ArchitectureStyle Style, float Confidence, string? Via) Detect(DiscoveryModel model)
    {
        var signals = model.Architecture.All;
        var evidence = new List<string>();
        var scores = new Dictionary<ArchitectureStyle, (float Score, string Evidence)>();

        // Compute reference-direction evidence (core/domain projects have high fan-in, low fan-out)
        var refCounts = ComputeReferenceCounts(model.Projects);
        // Detect folder-role conventions from file paths
        var folderRoles = DetectFolderRoles(model);
        // Detect aggregate presence from EfEntityDetection
        var aggregateCount = model.Detections
            .OfType<EfEntityDetection>().Count(d => d.IsAggregate);
        // Count MediatR handlers from the implemented interfaces captured in Stage 2, NOT from
        // MediatRHandlerDetection: this detector runs between Stage 2 and Stage 3, and the MediatR
        // extractor that emits those detections is a Stage 3 specific extractor — so model.Detections
        // is still empty here. The interface strings ("IRequestHandler<…>") are already on the types.
        var mediatRHandlerCount = model.Types.Values.Count(t => t.ImplementedInterfaces.Any(i =>
            i.StartsWith("IRequestHandler", StringComparison.Ordinal)
            || i.StartsWith("IStreamRequestHandler", StringComparison.Ordinal)));
        var notificationHandlerCount = model.Types.Values.Count(t => t.ImplementedInterfaces.Any(i =>
            i.StartsWith("INotificationHandler", StringComparison.Ordinal)));
        var totalHandlerCount = mediatRHandlerCount + notificationHandlerCount;
        // The MediatR architecture *signal* keys off the package reference, which is missed when only a
        // sub-project of the closure is scoped (e.g. eShop's handlers live in Ordering.API while the
        // package is referenced from Ordering.Domain). The handler *detections* come straight from the
        // code, so treat them as first-class evidence of MediatR — otherwise the style falls through to
        // MinimalApi even though Send→handler is clearly wired (assessment G7).
        var hasMediatR = HasMediatREvidence(model);
        var hasEfCore = signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var _);
        var hasAspire = signals.TryGetValue(ArchitectureSignals.Keys.Aspire, out var aspire) && aspire.Detected;
        var hasMinimalApis = signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) && ma.Detected;
        var hasControllers = signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var ctrl) && ctrl.Detected;
        var hasFastEndpoints = signals.TryGetValue(ArchitectureSignals.Keys.FastEndpoints, out var fe) && fe.Detected;
        // Count NON-test projects so the style verdict agrees with the (test-excluded) topology. Counting
        // raw model.Projects let a single test project trip the NLayer rule (EfCore + >2 projects),
        // misreading a controller app as NLayer at repo-root (assessment: DntSite audit).
        var projectClassifier = new Graph.ProjectClassifier(model.Projects);
        var projectCount = model.Projects.Count(p => !projectClassifier.IsInTestProject(p.FilePath));

        // ── Evidence-driven scoring ──────────────────────────────────────────────────

        // Microservices: Aspire + many projects (constellation)
        // Only scores when there's an explicit AppHost — not just Aspire infra packages
        var hasAppHost = model.Projects.Any(p =>
            p.Name.EndsWith(".AppHost", StringComparison.OrdinalIgnoreCase)
            || p.PackageReferences.Any(pr => pr.Name.StartsWith("Aspire.Hosting", StringComparison.OrdinalIgnoreCase)));
        if (hasAspire && hasAppHost && projectCount >= 3)
        {
            var svcCount = model.Projects.Count(p => !IsInfrastructureProject(p.Name));
            evidence.Add($"Aspire orchestration with {svcCount} service projects");
            var score = Math.Min(0.65f + svcCount * 0.05f, 0.82f); // cap below VerticalSlices (0.85)
            scores[ArchitectureStyle.Microservices] = (score, string.Join("; ", evidence));
        }

        // CleanArchitecture: MediatR + DDD layer conventions + aggregates
        if (hasMediatR)
        {
            var dddLayers = (folderRoles.Contains("Domain") ? 1 : 0)
                          + (folderRoles.Contains("Application") ? 1 : 0)
                          + (folderRoles.Contains("Infrastructure") ? 1 : 0)
                          + (folderRoles.Contains("Api") ? 1 : 0);
            var hasDomainCore = refCounts.Any(r => r.HighFanIn && r.LowFanOut);

            if (dddLayers >= 2 || aggregateCount >= 1 || notificationHandlerCount >= 1)
            {
                var dddEvidence = new List<string>();
                if (dddLayers >= 2) dddEvidence.Add($"DDD folder layers: {string.Join(", ", folderRoles)}");
                if (aggregateCount >= 1) dddEvidence.Add($"{aggregateCount} aggregates");
                if (notificationHandlerCount >= 1) dddEvidence.Add($"{notificationHandlerCount} domain-event handlers");
                if (hasDomainCore) dddEvidence.Add("domain-core ref pattern (high fan-in, low fan-out)");
                dddEvidence.Add($"MediatR with {totalHandlerCount} handlers");

                scores[ArchitectureStyle.CleanArchitecture] = (Math.Min(0.5f + dddLayers * 0.1f + aggregateCount * 0.05f, 0.95f),
                    string.Join("; ", dddEvidence));
            }
        }

        // VerticalSlices: FastEndpoints + MediatR + feature-folder conventions
        if (hasFastEndpoints)
        {
            var vEvidence = new List<string> { "FastEndpoints detected" };
            if (hasMediatR) vEvidence.Add($"MediatR with {totalHandlerCount} handlers");
            scores[ArchitectureStyle.VerticalSlices] = (hasMediatR ? 0.85f : 0.7f,
                string.Join("; ", vEvidence));
        }

        // NLayer: multiple projects, EF Core, no strong DDD/MediatR signals
        if (hasEfCore && projectCount > 2 && !scores.ContainsKey(ArchitectureStyle.CleanArchitecture))
        {
            scores[ArchitectureStyle.NLayer] = (0.6f,
                $"EF Core + {projectCount} projects; folder roles: {string.Join(", ", folderRoles)}");
        }

        // MinimalApi: minimal APIs are the entry style, no MediatR. Project count is NOT a disqualifier —
        // a minimal-API backend is routinely accompanied by a Blazor/SPA frontend split and Aspire infra
        // projects (e.g. TodoApi's 7 projects). When minimal APIs are present, they outrank a bare NLayer
        // multi-project+EF reading (0.65 > 0.6); a single API project is a near-certain MinimalApi (0.9).
        if (hasMinimalApis && !hasMediatR)
        {
            scores[ArchitectureStyle.MinimalApi] = (projectCount == 1 ? 0.9f : 0.65f,
                $"Minimal APIs + {projectCount} project(s); no MediatR");
        }

        // ModularMonolith: bounded-context / module naming in projects
        var moduleNames = model.Projects.Select(p => p.Name.ToLowerInvariant())
            .Where(n => n.Contains("module") || n.Contains("bounded") || n.Contains("context"))
            .ToList();
        if (moduleNames.Count >= 2 && !scores.ContainsKey(ArchitectureStyle.Microservices))
        {
            scores[ArchitectureStyle.ModularMonolith] = (0.55f + moduleNames.Count * 0.05f,
                $"{moduleNames.Count} module-like sub-projects: {string.Join(", ", moduleNames)}");
        }

        // ControllerBased: controllers present, controllers dominant over minimal APIs
        if (hasControllers && !hasMediatR)
        {
            var ctrlConf = signals.TryGetValue(ArchitectureSignals.Keys.Controllers, out var cs) ? cs.Confidence : 0;
            var maConf = signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var mas) ? mas.Confidence : 0;
            if (!hasMinimalApis || ctrlConf >= maConf)
            {
                float score = !hasMinimalApis ? 0.7f : 0.55f;
                scores[ArchitectureStyle.ControllerBased] = (score,
                    $"Controllers detected (conf={ctrlConf:F1}); MediatR=no, MinimalApi={(hasMinimalApis ? $"yes(conf={maConf:F1})" : "no")}");

                // Remove MinimalApi if controllers are dominant
                if (hasMinimalApis && ctrlConf >= maConf)
                    scores.Remove(ArchitectureStyle.MinimalApi);
            }
        }

        if (scores.Count == 0)
            return (ArchitectureStyle.Unknown, 0, null);

        // Topology-over-structure rule: when Aspire AppHost orchestration is present,
        // the Microservices topology signal outranks any intra-service style (e.g.
        // CleanArchitecture within individual services). A monorepo of CleanArchitecture
        // services behind an AppHost IS Microservices — the structural style is a
        // secondary trait of each service, not the primary system architecture.
        if (scores.TryGetValue(ArchitectureStyle.Microservices, out var msEntry)
            && scores.TryGetValue(ArchitectureStyle.CleanArchitecture, out var caEntry)
            && hasAppHost)
        {
            // Boost Microservices just above the strongest CleanArchitecture score so
            // it wins the MaxBy. Keep the original evidence so the user sees what was
            // detected.
            scores[ArchitectureStyle.Microservices] = (Math.Max(msEntry.Score, caEntry.Score + 0.01f), msEntry.Evidence);
        }

        var best = scores.MaxBy(kv => kv.Value.Score);
        return (best.Key, Math.Min(best.Value.Score, 1.0f), best.Value.Evidence);
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

    /// <summary>
    /// True when MediatR is present as EVIDENCE — either the package signal fired, or the code itself
    /// implements MediatR handler interfaces. The package signal is missed when only a sub-project of the
    /// closure is scoped (handlers live in one project, the package reference in another), so the handler
    /// interfaces are first-class evidence. Single source of truth so the STACK line (MapRenderer) and the
    /// style verdict can't drift (assessment G7 + residual).
    /// </summary>
    public static bool HasMediatREvidence(DiscoveryModel model)
    {
        if (model.Architecture.All.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected)
            return true;
        return model.Types.Values.Any(t => t.ImplementedInterfaces.Any(i =>
            i.StartsWith("IRequestHandler", StringComparison.Ordinal)
            || i.StartsWith("IStreamRequestHandler", StringComparison.Ordinal)
            || i.StartsWith("INotificationHandler", StringComparison.Ordinal)));
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

        // Check both file paths and project names for conventions
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
        return lowered.Contains(".servicedefaults")
            || lowered.Contains(".apphost")
            || lowered.Contains("shared")
            || lowered.Contains("common")
            || lowered.Contains(".eventbus");
    }

    private readonly record struct ProjectRefStats(string Name, int Incoming, int Outgoing)
    {
        public bool HighFanIn => Incoming >= 2;
        public bool LowFanOut => Outgoing <= 2;
    }
}
