namespace DevContext.Core.Graph;

/// <summary>
/// The set of projects that make up ONE system being analysed — the resolved <c>.sln</c>/<c>.slnx</c>
/// (or all discovered projects when there's no solution). <see cref="GraphBuilder"/> uses this to avoid
/// unioning independent solutions in a multi-app repo (design-doc R1). Single-scope now; multi-solution
/// later is just "run the engine once per scope" — this type is the seam that makes that a loop, not a
/// refactor. A microservice constellation (one AppHost, many service projects) is correctly ONE scope.
/// </summary>
public sealed class SolutionScope
{
    private readonly ImmutableArray<string> _projectDirs; // normalized directory prefixes

    /// <summary>Projects in this scope.</summary>
    public ImmutableArray<ProjectInfo> Projects { get; }
    /// <summary>The owning solution name, when one was resolved.</summary>
    public string? SolutionName { get; }

    /// <summary>Creates a scope over the given projects.</summary>
    public SolutionScope(ImmutableArray<ProjectInfo> projects, string? solutionName = null)
    {
        Projects = projects;
        SolutionName = solutionName;
        _projectDirs =
        [
            .. projects
                .Select(p => Path.GetDirectoryName(p.FilePath))
                .Where(d => !string.IsNullOrEmpty(d))
                .Select(d => Normalize(d!))
                .Distinct(),
        ];
    }

    /// <summary>True when the file belongs to a project in this scope. With no scope projects, nothing is excluded.</summary>
    public bool Contains(string filePath)
    {
        if (_projectDirs.IsDefaultOrEmpty) return true;
        var norm = Normalize(filePath);
        foreach (var dir in _projectDirs)
            if (norm.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    /// <summary>
    /// Builds the scope from the discovery model: the projects in the resolved solution, or all
    /// discovered projects when no <c>.sln</c> was resolved. For P1 this is the single resolved system;
    /// a repo with multiple independent solutions runs the engine once per scope (future).
    /// </summary>
    public static SolutionScope FromModel(DiscoveryModel model)
    {
        if (model.Solution is { ProjectPaths.Length: > 0 } sln)
        {
            // ProjectPaths are written relative to the solution directory (SolutionFileParser); project
            // FilePaths are absolute (Roslyn). Resolve the relative paths against the solution dir before
            // comparing — otherwise relative-vs-absolute never matches and the scope silently falls back
            // to "all discovered projects" (assessment G1, Phase 0).
            var slnDir = Path.GetDirectoryName(sln.FilePath) ?? "";
            var inSln = sln.ProjectPaths
                .Select(rel => Normalize(ToAbsolute(slnDir, rel)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var scoped = model.Projects
                .Where(p => inSln.Contains(Normalize(p.FilePath)))
                .ToImmutableArray();
            if (scoped.Length > 0)
                return new SolutionScope(scoped, sln.Name);
        }
        return new SolutionScope(model.Projects, model.Solution?.Name);
    }

    private static string ToAbsolute(string baseDir, string path)
        => Path.IsPathRooted(path) || baseDir.Length == 0
            ? path
            : Path.GetFullPath(Path.Combine(baseDir, path));

    private static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');
}
