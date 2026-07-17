namespace DevContext.Core.Extractors.Generic;

/// <summary>
/// Analyzes project structure using EVIDENCE (reference direction, folder roles, signal/presence data)
/// rather than brittle project-name substrings. Called by the pipeline between Stage 2 and 3.
/// PLAN-10 B2: replaces the old name-substring heuristic that misclassified eShop and VerticalSlice
/// as MinimalApi.
///
/// L7.3 — Scope behavior: when given a subfolder within a multi-service repo, the pipeline resolves
/// the closest ancestor .sln. The partial-closure guard (< 75% of sln projects) drops system-level
/// verdicts (Microservices, ModularMonolith) so the style stays local. Multi-sample/docs repos with
/// no unifying solution are detected as <see cref="ArchitectureStyle.SampleCollection"/> (E4 fix).
/// </summary>
/// <summary>How MediatR-shaped CQRS evidence should be branded (B6): the real library, a repo's own
/// hand-rolled mediator interfaces, or absent.</summary>
public enum MediatREvidenceKind { None, Package, HandRolled }

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
        var mediatREvidence = GetMediatREvidence(model);
        var hasMediatR = mediatREvidence != MediatREvidenceKind.None;
        // B6: the evidence strings must brand a hand-rolled mediator honestly — podcasts has zero
        // MediatR references, only its own IRequestHandler<,> interface.
        var mediatorBrand = mediatREvidence == MediatREvidenceKind.HandRolled ? "hand-rolled mediator" : "MediatR";
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

        // L7.3 — Sample-collection guard (E4): when most projects live under sample/demo/docs
        // paths, the repo is a showcase, not a unified architecture. Never report Microservices
        // for a sample repo. Also triggers when there is no unifying solution + >3 projects.
        // L7.4 — third trigger: project names carry sample-like words (e.g. BlazorSample_*).
        // This catches repos like dotnet/blazor-samples where project dirs don't contain
        // "/samples/" path segments (they're version-numbered directories instead), yet the
        // project names themselves announce the sample nature.
        var samplePathProjectCount = model.Projects.Count(p =>
            !projectClassifier.IsInTestProject(p.FilePath)
            && Graph.ProjectClassifier.IsSamplePath(p.FilePath));
        var nonTestProjectCount = model.Projects.Count(p =>
            !projectClassifier.IsInTestProject(p.FilePath));
        var noUnifyingSolution = model.Solution is null;
        if (samplePathProjectCount > 0 && nonTestProjectCount > 0
            && (decimal)samplePathProjectCount / nonTestProjectCount > 0.5m)
        {
            var ratio = (float)samplePathProjectCount / nonTestProjectCount;
            scores[ArchitectureStyle.SampleCollection] = (0.6f + ratio * 0.2f,
                $"{samplePathProjectCount}/{nonTestProjectCount} projects are samples/demos/docs — no unified architecture");
        }
        else if (noUnifyingSolution && nonTestProjectCount > 3)
        {
            scores[ArchitectureStyle.SampleCollection] = (0.65f,
                $"{nonTestProjectCount} projects, no unifying solution — likely sample collection");
        }
        // L7.4 — Multi-.sln directory detection: when the resolver walked down into a
        // subdirectory to find a single .sln, but the overall analyzed project count is
        // much larger than that .sln's project set, we have a multi-sample directory
        // (e.g. dotnet/blazor-samples where each sample has its own .sln). Score as
        // SampleCollection with moderate confidence. Only fires when there is a stark
        // mismatch (≥ 5×), avoiding false positives on normal multi-.sln repos.
        if (model.Solution is { } sln
            && sln.ProjectPaths.Length > 0
            && nonTestProjectCount > sln.ProjectPaths.Length * 5)
        {
            var scConfidence = 0.62f;
            scores[ArchitectureStyle.SampleCollection] = (scConfidence,
                $"{nonTestProjectCount} projects but .sln only covers {sln.ProjectPaths.Length} — multi-sample directory");
        }

        // ── Evidence-driven scoring ──────────────────────────────────────────────────

        // Microservices: at least 2 runnable web services + (gateway OR bus) evidence.
        // Detects multi-service constellations even without Aspire orchestration (M1.9 / D5).
        var runnableWebCount = CountRunnableWebProjects(model);
        var hasGatewayEvidence = signals.TryGetValue(ArchitectureSignals.Keys.Gateway, out var gwSig) && gwSig.Detected;
        var hasBusEvidence = signals.TryGetValue(ArchitectureSignals.Keys.MassTransit, out _)
            || signals.TryGetValue(ArchitectureSignals.Keys.NServiceBus, out _);

        if (runnableWebCount >= 2 && (hasGatewayEvidence || hasBusEvidence))
        {
            evidence.Add($"{runnableWebCount} runnable web services with "
                + (hasGatewayEvidence ? "gateway" : "") + (hasGatewayEvidence && hasBusEvidence ? " + " : "") + (hasBusEvidence ? "message bus" : ""));
            scores[ArchitectureStyle.Microservices] = (Math.Min(0.6f + runnableWebCount * 0.05f, 0.85f),
                string.Join("; ", evidence));
        }

        // Microservices: Aspire + many projects (constellation) — keep existing detection for Aspire repos
        // Only scores when there's an explicit AppHost — not just Aspire infra packages
        var hasAppHost = model.Projects.Any(p =>
            p.Name.EndsWith(".AppHost", StringComparison.OrdinalIgnoreCase)
            || p.PackageReferences.Any(pr => pr.Name.StartsWith("Aspire.Hosting", StringComparison.OrdinalIgnoreCase)));
        if (hasAspire && hasAppHost && projectCount >= 3)
        {
            // The AppHost's ProjectReferences ARE the orchestrated runnables (typed AddProject<T>
            // requires one per service). Counting every solution project mislabels an
            // Aspire-orchestrated monolith-plus-worker (2 runnables) as Microservices. When the
            // AppHost carries no ProjectReferences (path-based AddProject overload), fall back to
            // the whole-solution service count.
            var orchestrated = model.Projects
                .Where(p => p.Name.EndsWith(".AppHost", StringComparison.OrdinalIgnoreCase)
                    || p.PackageReferences.Any(pr => pr.Name.StartsWith("Aspire.Hosting", StringComparison.OrdinalIgnoreCase)))
                .SelectMany(p => p.ProjectReferences)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            var svcCount = orchestrated > 0
                ? orchestrated
                : model.Projects.Count(p => !IsInfrastructureProject(p.Name));
            if (svcCount >= 3)
            {
                evidence.Add(orchestrated > 0
                    ? $"Aspire orchestration of {svcCount} runnable services"
                    : $"Aspire orchestration with {svcCount} service projects");
                var score = Math.Min(0.65f + svcCount * 0.05f, 0.82f); // cap below VerticalSlices (0.85)
                scores[ArchitectureStyle.Microservices] = (score, string.Join("; ", evidence));
            }
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
                dddEvidence.Add($"{mediatorBrand} with {totalHandlerCount} handlers");

                scores[ArchitectureStyle.CleanArchitecture] = (Math.Min(0.5f + dddLayers * 0.1f + aggregateCount * 0.05f, 0.95f),
                    string.Join("; ", dddEvidence));
            }
        }

        // VerticalSlices: FastEndpoints + MediatR + feature-folder conventions
        if (hasFastEndpoints)
        {
            var vEvidence = new List<string> { "FastEndpoints detected" };
            if (hasMediatR) vEvidence.Add($"{mediatorBrand} with {totalHandlerCount} handlers");
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

        // ModularMonolith: bounded-context / module naming in projects. E8: match a whole dot-separated
        // NAME SEGMENT, not any substring — a bare "context" substring matched DevContext's own product
        // name ("DevContext.Cli", "DevContext.Core", ...), giving every project in this repo a false
        // "module" credit ("9 module-like sub-projects"). Test/bench projects never count either — a
        // *.Tests or benchmarks project is never itself a bounded-context module.
        var moduleNames = model.Projects
            .Where(p => !projectClassifier.IsInTestProject(p.FilePath) && !Graph.ProjectClassifier.IsSamplePath(p.FilePath))
            .Select(p => p.Name)
            .Where(n => n.Split('.').Any(seg =>
                seg.Equals("Module", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("Modules", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("BoundedContext", StringComparison.OrdinalIgnoreCase)))
            .Select(n => n.ToLowerInvariant())
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

        // L7.3 — Partial-closure guard (Iteration 4 / Critical 3 / E9 partial-scope fix):
        // a single-service closure of a larger solution may state its LOCAL style (CleanArchitecture/
        // ControllerBased/MinimalApi) but cannot pronounce the SYSTEM architecture. When we clearly
        // analysed a subset (< 75% of the .sln's projects), drop the system-level verdicts. Whole-
        // solution runs (incl. eShop's Aspire AppHost constellation) analyse ~all projects, so they
        // keep Microservices.
        //
        // Root-vs-subfolder behavior: when the user passes a subfolder path (not a .sln file), the
        // pipeline resolves the closest ancestor .sln or walks up to repo root. If the subfolder
        // contains a single service's project tree within a multi-service repo, this guard ensures
        // the style remains local. If no .sln is found at all, the no-unifying-solution branch above
        // (SampleCollection) fires.
        var slnProjectCount = model.Solution?.ProjectPaths.Length ?? 0;
        if (slnProjectCount > 0 && model.Projects.Length < slnProjectCount * 3 / 4)
        {
            scores.Remove(ArchitectureStyle.Microservices);
            scores.Remove(ArchitectureStyle.ModularMonolith);
        }

        // C4 (Prism D1.3a): desktop MVVM rung — a WPF/WinForms repo organized around ViewModels
        // (ScreenToGif read Unknown). Fallback only: it never competes with a scored web/system style.
        if (scores.Count == 0)
        {
            var desktopUiProjects = model.Projects.Count(p =>
                p.FilePath is { } fp && IsDesktopUiProject(fp));
            var viewModelCount = model.Types.Values.Count(t =>
                t.Name.EndsWith("ViewModel", StringComparison.Ordinal));
            if (desktopUiProjects > 0 && viewModelCount >= 3)
            {
                scores[ArchitectureStyle.DesktopMvvm] = (0.7f,
                    $"{desktopUiProjects} WPF/WinForms project(s) + {viewModelCount} ViewModels");
            }
        }

        if (scores.Count == 0)
            return (ArchitectureStyle.Unknown, 0, null);

        // Topology-over-structure rule: when Aspire AppHost orchestration is present,
        // the Microservices topology signal outranks any intra-service style. Same rule
        // applies without Aspire: ≥2 web services + gateway + bus evidence outranks
        // single-project CleanArchitecture scores (M1.9 / D5).
        if (scores.TryGetValue(ArchitectureStyle.Microservices, out var msEntry)
            && scores.TryGetValue(ArchitectureStyle.CleanArchitecture, out var caEntry)
            && (hasAppHost || (runnableWebCount >= 2 && hasGatewayEvidence && hasBusEvidence)))
        {
            // Boost Microservices just above the strongest CleanArchitecture score so
            // it wins the MaxBy.
            scores[ArchitectureStyle.Microservices] = (Math.Max(msEntry.Score, caEntry.Score + 0.01f), msEntry.Evidence);
        }

        // L7.3 — SampleCollection guardrail (E4): a sample/docs repo is NEVER Microservices.
        // When the detector has evidence this is a sample collection, it outranks any system-level
        // style verdict. The confidence floor is 0.60 so it beats a bare MinimalApi/NLayer default.
        if (scores.TryGetValue(ArchitectureStyle.SampleCollection, out var scEntry))
        {
            scores.Remove(ArchitectureStyle.Microservices);
            scores.Remove(ArchitectureStyle.CleanArchitecture);
            scores.Remove(ArchitectureStyle.VerticalSlices);
            scores.Remove(ArchitectureStyle.ModularMonolith);
            // Let local styles (ControllerBased, MinimalApi, NLayer) stay in case the sample
            // collection contains a coherent subset, but boost SampleCollection above them.
            foreach (var other in scores.Keys.Except([ArchitectureStyle.SampleCollection]).ToList())
            {
                var cur = scores[other];
                if (cur.Score >= scEntry.Score)
                    scores[ArchitectureStyle.SampleCollection] = (cur.Score + 0.02f, scEntry.Evidence);
            }
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
        => GetMediatREvidence(model) != MediatREvidenceKind.None;

    /// <summary>
    /// B6 (Prism D1.2e): distinguishes the MediatR LIBRARY from a hand-rolled mediator so the brand is
    /// honest. A repo that DECLARES the handler interface itself (podcasts'
    /// `ListenTogether.Application.Interfaces.IRequestHandler&lt;,&gt;`) has no MediatR at all — calling
    /// its style "MediatR (CQRS)" is a name-only match. Package signal (or handler implementations
    /// without a local interface declaration — the scoped-sub-project case where the interface comes
    /// from the real package outside the closure, G7) still reads as MediatR.
    /// </summary>
    public static MediatREvidenceKind GetMediatREvidence(DiscoveryModel model)
    {
        if (model.Architecture.All.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected)
            return MediatREvidenceKind.Package;

        var hasHandlerImpls = model.Types.Values.Any(t => t.ImplementedInterfaces.Any(i =>
            i.StartsWith("IRequestHandler", StringComparison.Ordinal)
            || i.StartsWith("IStreamRequestHandler", StringComparison.Ordinal)
            || i.StartsWith("INotificationHandler", StringComparison.Ordinal)));
        if (!hasHandlerImpls) return MediatREvidenceKind.None;

        var declaresOwnHandlerInterface = model.Types.Values.Any(t =>
            t.Kind == TypeKind.Interface
            && (t.Name == "IRequestHandler" || t.Name.StartsWith("IRequestHandler<", StringComparison.Ordinal)));
        return declaresOwnHandlerInterface ? MediatREvidenceKind.HandRolled : MediatREvidenceKind.Package;
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

    /// <summary>M1.9 — counts runnable web service projects (Exe output with web-server packages
    /// or "api"/"web" project naming convention). Used for microservices detection.</summary>
    private static int CountRunnableWebProjects(DiscoveryModel model)
    {
        var count = 0;
        foreach (var proj in model.Projects)
        {
            if (IsInfrastructureProject(proj.Name)) continue;
            var isExe = proj.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
            var isWebSdk = proj.FilePath is { } cp && IsWebSdkProject(cp);
            var hasWebPkg = proj.PackageReferences.Any(pr =>
                pr.Name.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase)
                || pr.Name.Contains("Grpc.AspNetCore", StringComparison.OrdinalIgnoreCase));
            var isWebByName = proj.Name.EndsWith(".API", StringComparison.OrdinalIgnoreCase)
                || proj.Name.EndsWith(".Web", StringComparison.OrdinalIgnoreCase)
                || proj.Name.EndsWith(".Grpc", StringComparison.OrdinalIgnoreCase);
            if (isExe || isWebSdk || hasWebPkg || isWebByName)
                count++;
        }
        return count;
    }

    private static bool IsWebSdkProject(string csprojPath)
    {
        try
        {
            var text = File.ReadAllText(csprojPath);
            return text.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    /// <summary>C4 (Prism D1.3a): true when the csproj opts into WPF or WinForms — desktop UI evidence
    /// with no package to probe (both are SDK-provided).</summary>
    private static bool IsDesktopUiProject(string csprojPath)
    {
        try
        {
            var text = File.ReadAllText(csprojPath);
            return text.Contains("<UseWPF>true", StringComparison.OrdinalIgnoreCase)
                || text.Contains("<UseWindowsForms>true", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private readonly record struct ProjectRefStats(string Name, int Incoming, int Outgoing)
    {
        public bool HighFanIn => Incoming >= 2;
        public bool LowFanOut => Outgoing <= 2;
    }

    // ── M1.9 Per-service style rollup ─────────────────────────────────────────

    /// <summary>M1.9 / D5 — For each runnable web service project, detect its local architecture
    /// style and technology stack. Called by the pipeline after solution-level style detection.</summary>
    public static ImmutableArray<PerServiceStyle> DetectPerServiceStyles(DiscoveryModel model)
    {
        var results = ImmutableArray.CreateBuilder<PerServiceStyle>();
        var projectClassifier = new Graph.ProjectClassifier(model.Projects);
        var scope = Graph.SolutionScope.FromModel(model);   // T1.4 — canonical file→project mapping
        var signals = model.Architecture.All;

        foreach (var proj in model.Projects)
        {
            if (!IsRunnableService(proj)) continue;
            // D1.1b (E2/A3): holder csproj and build-tooling exes never get per-service rows —
            // GitVersion's Cake build tree rendered as seven "Unknown" services.
            if (Graph.ProjectClassifier.IsHolderProject(proj)
                || projectClassifier.IsBuildTooling(proj)) continue;
            // C4 (Prism D1.3a): a benchmark harness is not a service (bitwarden MicroBenchmarks).
            if (Graph.ProjectClassifier.IsBenchmarkProject(proj)) continue;
            // A4 (Prism D1.1c): runnable-service inference honors the sample filter — wolverine's
            // per-service table was ~80 rows of samples/ and test hosts. SamplesAreTheProduct repos
            // (aspire-samples) keep their sample hosts: they ARE the services there (T8).
            if (!model.SamplesAreTheProduct && Graph.ProjectClassifier.IsSamplePath(proj.FilePath)) continue;
            // T1.4 — the Aspire AppHost is a runnable orchestrator; surface it (before the infra skip that
            // otherwise hides ".apphost") so the constellation's conductor isn't dropped to "no services".
            // D1.3a: bitwarden names its host exactly `AppHost` — no dotted suffix.
            if (proj.Name.EndsWith(".AppHost", StringComparison.OrdinalIgnoreCase)
                || proj.Name.Equals("AppHost", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new PerServiceStyle(proj.Name, "Aspire AppHost", ["Aspire"]));
                continue;
            }
            if (IsInfrastructureProject(proj.Name)) continue;
            if (projectClassifier.IsInTestProject(proj.FilePath)) continue;

            var pkgs = proj.PackageReferences;
            var hasMediatR = pkgs.Any(p => p.Name.Contains("MediatR", StringComparison.OrdinalIgnoreCase));
            var hasEfCore = pkgs.Any(p => p.Name.Contains("EntityFramework", StringComparison.OrdinalIgnoreCase));
            var hasMassTransit = pkgs.Any(p => p.Name.Contains("MassTransit", StringComparison.OrdinalIgnoreCase));
            var hasFluentValidation = pkgs.Any(p => p.Name.Contains("FluentValidation", StringComparison.OrdinalIgnoreCase));
            var hasGrpc = pkgs.Any(p => p.Name.Contains("Grpc.AspNetCore", StringComparison.OrdinalIgnoreCase));
            var hasYarp = pkgs.Any(p => p.Name.Contains("Yarp", StringComparison.OrdinalIgnoreCase));
            var hasRefit = pkgs.Any(p => p.Name.Contains("Refit", StringComparison.OrdinalIgnoreCase));
            var hasRazorPages = pkgs.Any(p => p.Name.Contains("Microsoft.AspNetCore.Mvc.RazorPages", StringComparison.OrdinalIgnoreCase));
            // B2 (Prism D1.2b): UseMaui is SDK-provided — the mobile TFM triple identifies a MAUI app
            // even with zero Microsoft.Maui package references (podcasts' two MAUI apps read Unknown).
            var hasMaui = pkgs.Any(p => p.Name.Contains("Microsoft.Maui", StringComparison.OrdinalIgnoreCase))
                || Graph.MauiEvidence.HasMauiTfm(proj);
            var hasCliParser = pkgs.Any(p => p.Name.Contains("Spectre.Console.Cli", StringComparison.OrdinalIgnoreCase)
                || p.Name.Contains("System.CommandLine", StringComparison.OrdinalIgnoreCase));
            var isWorkerSdk = proj.FilePath is { } wp && IsWorkerSdkProject(wp);
            var isWebByName = proj.Name.EndsWith(".Web", StringComparison.OrdinalIgnoreCase);
            var isGrpcByName = proj.Name.EndsWith(".Grpc", StringComparison.OrdinalIgnoreCase)
                || proj.Name.EndsWith(".GrpcService", StringComparison.OrdinalIgnoreCase);
            var isConsoleExe = proj.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true
                && !(proj.FilePath is { } cpx && IsWebSdkProject(cpx));

            // T1.4 — per-service evidence from this project's OWN detections (SourceFile under its dir):
            // Blazor page routes, HTTP endpoints, and message/worker entries distinguish a Blazor storefront
            // from a REST API, and a background consumer from a web host, when packages alone are ambiguous.
            bool Owns(string? file) => file is not null
                && string.Equals(scope.ProjectForFile(file), proj.Name, StringComparison.OrdinalIgnoreCase);
            var ownsBlazorPages = model.Detections.OfType<EndpointDetection>()
                .Any(d => d.HandlerMethod == "<component>" && Owns(d.SourceFile));
            var ownsHttpEndpoints = model.Detections.OfType<EndpointDetection>()
                .Any(d => d.HandlerMethod != "<component>" && Owns(d.SourceFile));
            var ownsBackground = model.Detections.Any(d =>
                (d is MessageConsumerDetection || d is BackgroundWorkerDetection) && Owns(d.SourceFile));
            // C4 (Prism D1.3a): hub/razor-page/controller OWNERSHIP as first-class style evidence —
            // bitwarden read 17/17 Unknown because every rung below was package- or name-gated.
            var ownsHubs = model.Detections.OfType<SignalRHubDetection>().Any(d => Owns(d.SourceFile));
            var ownsRazorPages = model.Detections.OfType<EndpointDetection>()
                .Any(d => d.ExtractorName == "RazorPagesExtractor" && Owns(d.SourceFile));
            // IdentityServer/OpenIddict: the framework package often lives in a shared Core library
            // (bitwarden), so a host NAMED for identity that reaches the package one hop away counts.
            static bool IsIdentityPackage(PackageReferenceInfo p) =>
                p.Name.StartsWith("Duende.IdentityServer", StringComparison.OrdinalIgnoreCase)
                || p.Name.StartsWith("IdentityServer4", StringComparison.OrdinalIgnoreCase)
                || p.Name.StartsWith("OpenIddict", StringComparison.OrdinalIgnoreCase);
            var hasIdentityByName = proj.Name.Split('.').Any(seg =>
                seg.Equals("Identity", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("Sso", StringComparison.OrdinalIgnoreCase));
            var hasIdentityServer = pkgs.Any(IsIdentityPackage)
                || (hasIdentityByName && proj.ProjectReferences
                    .Select(Path.GetFileNameWithoutExtension)
                    .Select(rn => model.Projects.FirstOrDefault(mp =>
                        string.Equals(mp.Name, rn, StringComparison.OrdinalIgnoreCase)))
                    .Any(rp => rp is not null && rp.PackageReferences.Any(IsIdentityPackage)));

            var stackTags = ImmutableArray.CreateBuilder<string>();
            var style = "Unknown";

            // MAUI first — a cross-platform client is not a web service whatever else it references.
            if (hasMaui)
            {
                style = "MAUI App";
                stackTags.Add(".NET MAUI");
                if (hasEfCore) stackTags.Add("EF Core");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // C4 (Prism D1.3a): a WPF/WinForms host is a desktop app whatever else it references —
            // decisive identity like MAUI. ViewModels upgrade it to MVVM (ScreenToGif).
            if (proj.FilePath is { } dp && IsDesktopUiProject(dp))
            {
                var ownsViewModels = model.Types.Values.Any(t =>
                    t.Name.EndsWith("ViewModel", StringComparison.Ordinal) && Owns(t.FilePath));
                style = ownsViewModels ? "Desktop (MVVM)" : "Desktop";
                stackTags.Add("WPF/WinForms");
                if (hasEfCore) stackTags.Add("EF Core");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // Blazor storefront/client — BEFORE the Gateway rung: eShop's WebApp is a Blazor BFF that also
            // references YARP, and read "Gateway [YARP]" while it is really the storefront. A project that
            // owns Blazor @page routes is a Blazor app; the YARP proxy is a stack detail, not the identity.
            if (ownsBlazorPages)
            {
                style = "Blazor";
                stackTags.Add("Blazor");
                if (hasYarp) stackTags.Add("YARP");
                if (hasEfCore) stackTags.Add("EF Core");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // gRPC-dedicated service: has gRPC server packages AND name matches gRPC convention,
            // with NO competing web/app framework. A web API that uses gRPC client should NOT
            // be classified as a gRPC service.
            var hasOtherWebPkg = pkgs.Any(p =>
                (p.Name.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase)
                 && !p.Name.Contains("Grpc.AspNetCore", StringComparison.OrdinalIgnoreCase))
                || p.Name.Contains("MediatR", StringComparison.OrdinalIgnoreCase)
                || p.Name.Contains("Microsoft.AspNetCore.Mvc", StringComparison.OrdinalIgnoreCase));
            var isGrpcDedicated = hasGrpc && !hasOtherWebPkg && (isGrpcByName
                || !pkgs.Any(p => p.Name.Contains("Refit", StringComparison.OrdinalIgnoreCase)));
            if (isGrpcDedicated)
            {
                style = "gRPC Service";
                stackTags.Add("gRPC");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // Worker service — a Worker-SDK host, OR a host that consumes messages / runs background work
            // and exposes NO HTTP endpoints (eShop PaymentProcessor is a Web-SDK project with only a
            // RabbitMQ consumer; OrderProcessor is a Worker SDK). It read "Unknown" before.
            if (isWorkerSdk || (ownsBackground && !ownsHttpEndpoints))
            {
                style = "Worker Service";
                stackTags.Add("Worker");
                if (hasMassTransit) stackTags.Add("MassTransit");
                if (hasEfCore) stackTags.Add("EF Core");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // C4 (Prism D1.3a): identity provider — IdentityServer/OpenIddict directly, or an
            // identity-named host reaching the package one project-reference hop away (bitwarden
            // Identity/Sso reference it via Core).
            if (hasIdentityServer)
            {
                style = "Identity provider";
                stackTags.Add("IdentityServer");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // C4 (Prism D1.3a): a host whose only entry surface is SignalR hubs is a SignalR host —
            // and so is a hub host NAMED for real-time work (bitwarden Notifications hosts its hub
            // plus a small internal send API; the hub is its identity, the controllers are auxiliary).
            // A generically-named host with hubs AND endpoints keeps its web style; the hub presence
            // rides as a stack tag below.
            var isRealtimeByName = proj.Name.Split('.').Any(seg =>
                seg.Equals("Notifications", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("Hub", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("Hubs", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("SignalR", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("Push", StringComparison.OrdinalIgnoreCase));
            if (ownsHubs && (!ownsHttpEndpoints || isRealtimeByName))
            {
                style = "SignalR host";
                stackTags.Add("SignalR");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // Gateway (YARP/Ocelot and not a Blazor/normal web service)
            if (hasYarp)
            {
                style = "Gateway";
                stackTags.Add("YARP");
                results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
                continue;
            }

            // Web application styles
            if (hasMediatR)
            {
                stackTags.Add("MediatR");
                style = hasEfCore ? "Clean Architecture" : "CQRS";
            }
            if (hasEfCore) stackTags.Add("EF Core");
            if (hasMassTransit) stackTags.Add("MassTransit");
            if (hasFluentValidation) stackTags.Add("FluentValidation");
            if (hasRefit) stackTags.Add("Refit");
            if (ownsHubs) stackTags.Add("SignalR");

            // Infer from project naming if no strong signal
            if (style == "Unknown")
            {
                // C4 (Prism D1.3a): "Api" as a whole name segment, not just the dotted ".API" suffix —
                // bitwarden's host is named exactly `Api`.
                var isApiByName = proj.Name.EndsWith(".API", StringComparison.OrdinalIgnoreCase)
                    || proj.Name.Split('.')[^1].Equals("Api", StringComparison.OrdinalIgnoreCase);
                if (isWebByName)
                {
                    var razor = hasRazorPages || ownsRazorPages;
                    style = razor ? "Razor Pages" : "Web App";
                    if (razor) stackTags.Add("Razor");
                }
                else if (isApiByName)
                {
                    style = "Web API";
                }
                // T1.4 — a console Exe with a CLI parser (or by convention) is a CLI tool, not "Unknown"
                // (shamshir's ResearchCli). Guarded to console Exes so it never mislabels a web host.
                else if (hasCliParser || (isConsoleExe && !ownsHttpEndpoints
                    && (proj.Name.EndsWith("Cli", StringComparison.OrdinalIgnoreCase)
                        || proj.Name.EndsWith("Console", StringComparison.OrdinalIgnoreCase)
                        || proj.Name.EndsWith("Tool", StringComparison.OrdinalIgnoreCase)
                        || proj.Name.EndsWith("Utility", StringComparison.OrdinalIgnoreCase))))
                {
                    style = "CLI";
                    stackTags.Add("CLI");
                }
                // C4 (Prism D1.3a): endpoint OWNERSHIP as the last positive rung — a host that serves
                // razor pages / controller actions / minimal APIs is never "Unknown" (bitwarden 17/17).
                else if (ownsRazorPages)
                {
                    style = "Razor Pages";
                    stackTags.Add("Razor");
                }
                else if (ownsHttpEndpoints)
                {
                    var ownsControllerActions = model.Detections.OfType<EndpointDetection>()
                        .Any(d => d.ExtractorName == "ControllerActionExtractor" && Owns(d.SourceFile));
                    style = ownsControllerActions ? "MVC" : "Web API";
                }
            }

            results.Add(new PerServiceStyle(proj.Name, style, stackTags.ToImmutable()));
        }

        return results.ToImmutable();
    }

    /// <summary>True when a project is a runnable service. Delegates to the canonical runnable check
    /// (<see cref="Graph2.ServiceBoundaryInference.IsRunnableService"/>) so per-service styles and service
    /// nodes agree on Exe / Web-SDK / Worker-SDK / Aspire-AppHost signals (T1.4).</summary>
    private static bool IsRunnableService(ProjectInfo proj)
        => Graph2.ServiceBoundaryInference.IsRunnableService(proj);

    /// <summary>True when the csproj uses the Worker SDK (<c>Microsoft.NET.Sdk.Worker</c>). Cached like
    /// the web-SDK probe; a background-processing host is a Worker Service, not "Unknown".</summary>
    private static bool IsWorkerSdkProject(string csprojPath)
    {
        try
        {
            var text = File.ReadAllText(csprojPath);
            return text.Contains("Microsoft.NET.Sdk.Worker", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }
}
