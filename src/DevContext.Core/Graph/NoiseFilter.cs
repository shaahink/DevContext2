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

    /// <summary>Creates a filter over the given project classification.</summary>
    public NoiseFilter(ProjectClassifier projects) => _projects = projects;

    /// <summary>True when the type is production code worth modelling.</summary>
    public bool IsProductionCode(TypeDiscovery type)
    {
        if (_projects.IsInTestProject(type.FilePath)) return false;
        if (IsGeneratedPath(type.FilePath)) return false;
        // NOTE: deliberately NO type-name-suffix rule. "OrderSpec" / "...Should" are production code.
        return true;
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
