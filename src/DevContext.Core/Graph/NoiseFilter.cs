namespace DevContext.Core.Graph;

/// <summary>
/// Classifies projects as test vs production by STRUCTURE (project name + test packages), never by
/// type-name suffix. This is the fix for the live bug where the old name-suffix heuristic excluded DDD
/// Specification types (e.g. ContributorByIdSpec, IncompleteItemsSpec) as "tests".
/// </summary>
public sealed class ProjectClassifier
{
    private static readonly string[] TestPackageMarkers =
        ["xunit", "nunit", "MSTest", "Microsoft.NET.Test.Sdk", "FluentAssertions", "Moq", "NSubstitute", "Shouldly"];

    private readonly HashSet<string> _testProjectDirs; // normalized directory prefixes of test projects
    private readonly HashSet<string> _buildToolingProjects; // project names, transitive over project refs

    /// <summary>Classifies every project up front; production code under a test project's directory is excluded.
    /// <paramref name="analysisRoot"/> (when known) makes the <see cref="SamplesAreTheProduct"/> computation
    /// repo-relative, so a repo that itself lives under a <c>…/samples/…</c> path isn't misread as samples-only.</summary>
    public ProjectClassifier(ImmutableArray<ProjectInfo> projects, string? analysisRoot = null)
    {
        _testProjectDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in projects)
        {
            if (!IsTestProject(p)) continue;
            var dir = Path.GetDirectoryName(p.FilePath);
            if (!string.IsNullOrEmpty(dir))
                _testProjectDirs.Add(Normalize(dir));
        }

        // A3 (Prism D1.1b): build-tooling closes over project references — GitVersion's
        // artifacts/publish/release exes reference only build/common, which holds the Cake packages.
        // Fixed point: seed = direct package markers; expand = any project referencing a seeded one.
        _buildToolingProjects = new HashSet<string>(
            projects.Where(IsBuildToolingProject).Select(p => p.Name),
            StringComparer.OrdinalIgnoreCase);
        var changed = _buildToolingProjects.Count > 0;
        while (changed)
        {
            changed = false;
            foreach (var p in projects)
            {
                if (_buildToolingProjects.Contains(p.Name)) continue;
                foreach (var r in p.ProjectReferences)
                {
                    if (_buildToolingProjects.Contains(Path.GetFileNameWithoutExtension(r)))
                    {
                        _buildToolingProjects.Add(p.Name);
                        changed = true;
                        break;
                    }
                }
            }
        }

        var root = string.IsNullOrEmpty(analysisRoot) ? null : Normalize(analysisRoot);
        var candidates = projects.Where(p => !IsTestProject(p) && !IsBenchmarkProject(p) && !IsBuildTooling(p)).ToList();
        SamplesAreTheProduct = candidates.Count > 0
            && candidates.All(p => IsSamplePath(BelowRoot(p.FilePath, root)));
    }

    /// <summary>A3 (Prism D1.1b) — true when the project is build tooling, directly (see
    /// <see cref="IsBuildToolingProject"/>) or by transitively referencing a build-tooling project.</summary>
    public bool IsBuildTooling(ProjectInfo p) => _buildToolingProjects.Contains(p.Name);

    /// <summary>D1.1b — the full production predicate: <see cref="IsProductionProject(ProjectInfo,bool)"/>
    /// plus the transitive build-tooling closure. Prefer this wherever a classifier instance exists.</summary>
    public bool IsProduction(ProjectInfo p, bool samplesAreTheProduct)
        => IsProductionProject(p, samplesAreTheProduct) && !IsBuildTooling(p);

    /// <summary>D1.1b — <see cref="IsProduction(ProjectInfo,bool)"/> using this classifier's own
    /// <see cref="SamplesAreTheProduct"/> verdict.</summary>
    public bool IsProduction(ProjectInfo p) => IsProduction(p, SamplesAreTheProduct);

    /// <summary>T8 — true when every non-test, non-benchmark project lives under a sample path: the repo
    /// is a sample COLLECTION (dotnet/aspire-samples) whose samples ARE the product. Sample-path
    /// suppression (entry inventory, archetype ladder, service topology) must not apply to such a repo —
    /// otherwise it renders as an empty Library ("0 public types", no STYLE line). A repo with any real
    /// production project (MediatR: src/MediatR + samples/) keeps full suppression (T1.9 unchanged).</summary>
    public bool SamplesAreTheProduct { get; }

    /// <summary>Strips <paramref name="root"/> from <paramref name="filePath"/> so sample-path matching is
    /// repo-relative when the analysis root is known; absolute matching otherwise.</summary>
    private static string BelowRoot(string filePath, string? root)
    {
        if (root is null) return filePath;
        var norm = Normalize(filePath);
        if (norm.Length > root.Length
            && norm.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            && norm[root.Length] == '/')
            return norm[root.Length..];
        return norm;
    }

    /// <summary>True when the file lives under a test project's directory.</summary>
    public bool IsInTestProject(string filePath)
    {
        var norm = Normalize(filePath);
        foreach (var prefix in _testProjectDirs)
            if (norm.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    /// <summary>True when the file lives under a samples / snippets / examples / demos path. A library's
    /// sample apps are not the library — they must not flip its archetype to App or pollute its surface.</summary>
    public static bool IsSamplePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return false;
        var p = Normalize(filePath);
        return p.Contains("/samples/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/sample/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/snippets/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/snippet/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/examples/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/example/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/demos/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/demo/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/benchmarks/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/benchmark/", StringComparison.OrdinalIgnoreCase)
            // A2 (Prism D1.1b): StackExchange.Redis keeps its aux hosts under toys/ — same intent
            // as samples/, and they flipped the library's archetype to App + style to MinimalApi.
            || p.Contains("/toys/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/toy/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>True when the file lives under a <c>test</c>/<c>tests</c> path segment. Catches shared test
    /// source (e.g. <c>test/Shared/*.cs</c> linked into several test projects) that the project-directory
    /// classifier misses. Used only by the library surface — never by the app graph filter.</summary>
    public static bool IsTestPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return false;
        var p = Normalize(filePath);
        return p.Contains("/test/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/tests/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>True when the project is a test project (name suffix or a test-framework package ref).</summary>
    public static bool IsTestProject(ProjectInfo p)
    {
        var name = p.Name;
        if (name.EndsWith("Tests", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("Test", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("Specs", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("IntegrationTests", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("FunctionalTests", StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var pkg in p.PackageReferences)
            foreach (var marker in TestPackageMarkers)
                if (pkg.Name.Contains(marker, StringComparison.OrdinalIgnoreCase))
                    return true;

        return false;
    }

    /// <summary>True when the project is a benchmark harness (name suffix or a BenchmarkDotNet package ref).</summary>
    public static bool IsBenchmarkProject(ProjectInfo p)
    {
        var name = p.Name;
        if (name.EndsWith("Benchmarks", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("Benchmark", StringComparison.OrdinalIgnoreCase))
            return true;
        foreach (var pkg in p.PackageReferences)
            if (pkg.Name.Contains("BenchmarkDotNet", StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    /// <summary>E2 (Prism D1.1b) — true for a HOLDER csproj: a project whose SDK builds no code
    /// (<c>Microsoft.Build.NoTargets</c> — StackExchange.Redis's .github/docs/docker/RedisConfigs hubs —
    /// or <c>Microsoft.Build.Traversal</c>, its root Build.csproj). These exist for solution-explorer
    /// convenience and must never render as topology nodes, services, or archetype evidence.</summary>
    public static bool IsHolderProject(ProjectInfo p)
        => p.Sdk is { } sdk
            && (sdk.Contains("Microsoft.Build.NoTargets", StringComparison.OrdinalIgnoreCase)
                || sdk.Contains("Microsoft.Build.Traversal", StringComparison.OrdinalIgnoreCase));

    // A3 (Prism D1.1b): build-orchestration frameworks. A project referencing one is the repo's build
    // SCRIPT (GitVersion's Cake-Frosting build/** tree, wolverine's Nuke build/build.csproj), not a
    // service or app host — evidence-based, like TestPackageMarkers.
    private static readonly string[] BuildToolingPackageMarkers =
        ["Cake.", "Nuke.Common", "Bullseye", "SimpleExec", "FlubuCore"];

    /// <summary>A3 (Prism D1.1b) — true when the project is build tooling (references a build-orchestration
    /// framework: Cake.*, Nuke, Bullseye, SimpleExec). GitVersion's Cake Frosting exes rendered as seven
    /// "services"; wolverine ships a Nuke build exe.</summary>
    public static bool IsBuildToolingProject(ProjectInfo p)
    {
        foreach (var pkg in p.PackageReferences)
            foreach (var marker in BuildToolingPackageMarkers)
                if (pkg.Name.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                    return true;
        return false;
    }

    /// <summary>T1.9 — project-level classification (not path regex): true when the project is real
    /// production code, i.e. NOT a test project, benchmark harness, or a sample/example/demo project
    /// (by its directory). The service topology (diagram, services count, most-depended-upon, dead-code)
    /// consumes only production projects so tests/samples/benchmarks stop rendering as service cards or
    /// out-ranking the real library (e.g. MediatR.Examples over MediatR).</summary>
    public static bool IsProductionProject(ProjectInfo p)
        => IsProductionProject(p, samplesAreTheProduct: false);

    /// <summary>T8 overload — when <paramref name="samplesAreTheProduct"/> (see
    /// <see cref="SamplesAreTheProduct"/>), sample-path projects count as production: in a samples-only
    /// repo they are the only product there is. D1.1b: holder and build-tooling projects are never
    /// production, in any repo shape.</summary>
    public static bool IsProductionProject(ProjectInfo p, bool samplesAreTheProduct)
        => !IsTestProject(p) && !IsBenchmarkProject(p)
            && !IsHolderProject(p) && !IsBuildToolingProject(p)
            && (samplesAreTheProduct || !IsSamplePath(p.FilePath));

    private static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');
}

/// <summary>B2 (Prism D1.2b) — shared MAUI evidence probes. <c>UseMaui</c> is csproj-level (probed by
/// DependencyExtractor where the XDocument is loaded); the mobile TFM triple is visible on
/// <see cref="ProjectInfo.TargetFrameworks"/> and shared by the per-service style rung.</summary>
public static class MauiEvidence
{
    /// <summary>True when any TFM targets android/ios/maccatalyst — the MAUI mobile triple.</summary>
    public static bool HasMauiTfm(ProjectInfo p) => p.TargetFrameworks.Any(t =>
        t.Contains("-android", StringComparison.OrdinalIgnoreCase)
        || t.Contains("-ios", StringComparison.OrdinalIgnoreCase)
        || t.Contains("-maccatalyst", StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Deterministic, weight-free filter deciding whether a type is a first-class graph node. The ONLY
/// survivor of the old PathProximity/CallReachability/PatternRelevance trio — and it FILTERS (binary),
/// it does not score. All relevance is now structural (graph reachability), not a tuned weight.
/// </summary>
public sealed class NoiseFilter
{
    private readonly ProjectClassifier _projects;
    private readonly string? _root; // normalized analysis-root prefix; path-convention checks are relative to it

    /// <summary>Creates a filter over the given project classification. <paramref name="analysisRoot"/> is the
    /// resolved root of the system being analysed: the test/non-runtime <i>path-convention</i> checks
    /// (<c>/test/</c>, <c>/testassets/</c>, …) are applied to the portion of a file path <b>below</b> that
    /// root, so analysing a repo that itself lives under a <c>…/tests/…</c> path (e.g. our own
    /// <c>tests/fixtures/ControllerApp</c>) doesn't exclude its entire surface. Null = match absolute paths
    /// (the unit-test default).</summary>
    public NoiseFilter(ProjectClassifier projects, string? analysisRoot = null)
    {
        _projects = projects;
        _root = string.IsNullOrEmpty(analysisRoot) ? null : NormalizePath(analysisRoot);
    }

    /// <summary>True when the type is production code worth modelling.</summary>
    public bool IsProductionCode(TypeDiscovery type)
    {
        if (_projects.IsInTestProject(type.FilePath)) return false;
        if (IsGeneratedPath(type.FilePath)) return false;
        // NOTE: deliberately NO type-name-suffix rule. "OrderSpec" / "...Should" are production code.
        return true;
    }

    /// <summary>True when a detection's source file is a production entry source — not a test project,
    /// generated code, or a samples/snippets path. Gates the entry-point inventory so a library's (or an
    /// app's) test fixtures and sample apps don't surface as application entry points (e.g. MediatR's
    /// samples/MediatR.Examples handlers + the MediatR.Tests handlers).
    ///
    /// On framework-scale repos (aspnetcore) the entry list is otherwise flooded with non-runtime routes:
    /// the project-level test classifier can't catch <i>test assets</i> (web/console apps used BY tests —
    /// they don't reference xunit and aren't named <c>*Tests</c>), so we add the path conventions
    /// (<see cref="ProjectClassifier.IsTestPath"/> for the <c>/test/</c> tree, plus stress/perf harnesses,
    /// test-server infrastructure, and project-template scaffolding). Measured: aspnetcore HTTP entries
    /// 518 → production-only after this gate (assessment W1).</summary>
    public bool IsProductionEntrySource(string filePath)
    {
        if (_projects.IsInTestProject(filePath)) return false;
        if (IsGeneratedPath(filePath)) return false;
        // Path-convention checks run on the portion below the analysis root (see ctor): a repo's own
        // internal test/sample/template dirs are excluded, but the repo's root path itself never is.
        // T8: in a samples-only repo (ProjectClassifier.SamplesAreTheProduct) the sample rule is waived —
        // suppressing samples there empties the entry inventory of a repo whose samples ARE the product.
        var below = RelativeToRoot(filePath);
        return (_projects.SamplesAreTheProduct || !ProjectClassifier.IsSamplePath(below))
            && !ProjectClassifier.IsTestPath(below)
            && !IsNonRuntimeEntrySource(below);
    }

    /// <summary>Strips the analysis-root prefix so path-convention matching is repo-relative. No root → the
    /// path is returned unchanged (absolute matching, the unit-test default).</summary>
    private string RelativeToRoot(string filePath)
    {
        if (_root is null) return filePath;
        var norm = NormalizePath(filePath);
        if (norm.Length > _root.Length
            && norm.StartsWith(_root, StringComparison.OrdinalIgnoreCase)
            && norm[_root.Length] == '/')
            return norm[_root.Length..];
        return norm;
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').TrimEnd('/');

    /// <summary>True for source that registers entry points but is NOT application runtime: stress/perf
    /// harnesses, test-server <c>testassets</c>/<c>Testing</c> infrastructure (a support library, often
    /// outside a <c>/test/</c> tree), and project-template scaffolding (<c>.cs</c> under
    /// <c>ProjectTemplates/.../content/</c> that is stamped into NEW projects, never executed here). These
    /// are <i>path</i> conventions, not test <i>projects</i>, so the project classifier misses them — but
    /// they make a framework repo's Map read as if the framework itself were a pile of test apps.</summary>
    private static bool IsNonRuntimeEntrySource(string filePath)
    {
        var norm = filePath.Replace('\\', '/');
        return norm.Contains("/testassets/", StringComparison.OrdinalIgnoreCase)
            || norm.Contains("/Testing/", StringComparison.OrdinalIgnoreCase)
            || norm.Contains("/stress/", StringComparison.OrdinalIgnoreCase)
            || norm.Contains("/perf/", StringComparison.OrdinalIgnoreCase)
            || norm.Contains("/FunctionalTests/", StringComparison.OrdinalIgnoreCase)
            || norm.Contains("/ProjectTemplates/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGeneratedPath(string filePath)
    {
        var norm = filePath.Replace('\\', '/');
        return norm.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || norm.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || norm.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase)
            || norm.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || norm.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase);
    }
}
