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

    /// <summary>Classifies every project up front; production code under a test project's directory is excluded.</summary>
    public ProjectClassifier(ImmutableArray<ProjectInfo> projects)
    {
        _testProjectDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in projects)
        {
            if (!IsTestProject(p)) continue;
            var dir = Path.GetDirectoryName(p.FilePath);
            if (!string.IsNullOrEmpty(dir))
                _testProjectDirs.Add(Normalize(dir));
        }
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
            || p.Contains("/demo/", StringComparison.OrdinalIgnoreCase);
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

    private static bool IsTestProject(ProjectInfo p)
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

    private static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');
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
        var below = RelativeToRoot(filePath);
        return !ProjectClassifier.IsSamplePath(below)
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
