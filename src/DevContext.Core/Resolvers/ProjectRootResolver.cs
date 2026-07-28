namespace DevContext.Core.Resolvers;

/// <summary>Resolves the project root directory from a user-provided input path using solution/project file discovery.</summary>
public sealed class ProjectRootResolver
{
    /// <summary>Resolves the project root, finding .sln or .csproj files by walking up and down the
    /// directory tree, then computes the Hybrid scan set (closure for project/subfolder input,
    /// whole-solution otherwise) via <see cref="ScopeResolver"/>.</summary>
    public static async Task<ProjectRootResult> ResolveAsync(string inputPath, IFileSystem fs, CancellationToken ct = default)
        => await ResolveAsync(inputPath, fs, null, ct);

    /// <summary>Batch C (DC6) overload: <paramref name="requestedSolution"/> is the caller's <c>--sln</c>
    /// choice among a multi-solution repo's systems. Unmatched requests fall back to the scored pick — the
    /// discovery extractor is where that mismatch is reported, so it is reported once, not twice.</summary>
    public static async Task<ProjectRootResult> ResolveAsync(
        string inputPath, IFileSystem fs, string? requestedSolution, CancellationToken ct = default)
    {
        var baseResult = await ResolveBaseAsync(inputPath, fs, requestedSolution, ct);
        var (dirs, anchor) = await ScopeResolver.ResolveAsync(baseResult, fs, ct);
        return baseResult with { ScopeProjectDirs = dirs, AnchorProjectPath = anchor };
    }

    private static async Task<ProjectRootResult> ResolveBaseAsync(
        string inputPath, IFileSystem fs, string? requestedSolution, CancellationToken ct)
    {
        var fullPath = fs.GetFullPath(inputPath);

        if (fs.FileExists(fullPath))
        {
            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            if (ext is ".sln" or ".slnx")
            {
                var dir = fs.GetDirectoryName(fullPath)!;
                return new ProjectRootResult(dir, fullPath, [fullPath], ResolutionMethod.ExplicitSln, null);
            }
            if (ext == ".csproj")
            {
                var dir = fs.GetDirectoryName(fullPath)!;
                return new ProjectRootResult(dir, null, [fullPath], ResolutionMethod.ExplicitCsproj, null);
            }
        }

        if (!fs.DirectoryExists(fullPath))
        {
            throw new DirectoryNotFoundException($"Path not found: {fullPath}");
        }

        // DC6 — an explicit --sln is resolved against EVERY solution in the tree, so a caller can name a
        // nested one from the repo root (the whole point of the flag: GitVersion's modern CLI lives in
        // new-cli/, three levels away from the solution the scorer picks).
        if (!string.IsNullOrWhiteSpace(requestedSolution))
        {
            var all = await SolutionCatalog.EnumerateAsync(fullPath, fs, ct);
            if (SolutionCatalog.ResolveRequested(all, fullPath, requestedSolution) is { } asked)
                return new ProjectRootResult(fullPath, asked, [asked],
                    ResolutionMethod.DirectoryContainsSln, $"--sln {requestedSolution}");
        }

        var slnFiles = await FindSolutionsAsync(fs, fullPath, ct);
        if (slnFiles.Count > 0)
        {
            // Batch C: one scorer for both pickers. This used to take slnFiles[0] — enumeration order —
            // while SolutionDiscoveryExtractor scored, so the two could name different solutions.
            var picked = SolutionCatalog.Pick(slnFiles, fullPath, SolutionCatalog.RepoNameOf(fullPath))!;
            return new ProjectRootResult(fullPath, picked, slnFiles.ToImmutableArray(),
                ResolutionMethod.DirectoryContainsSln, null);
        }

        var current = fullPath;
        for (int i = 0; i < 5; i++)
        {
            var parent = fs.GetDirectoryName(current);
            if (parent == null) break;
            var parentSlns = await FindSolutionsAsync(fs, parent, ct);
            if (parentSlns.Count > 0)
            {
                return new ProjectRootResult(fullPath,
                    SolutionCatalog.Pick(parentSlns, parent, SolutionCatalog.RepoNameOf(parent)),
                    parentSlns.ToImmutableArray(),
                    ResolutionMethod.WalkedUp, $"walked up {i + 1} levels");
            }
            current = parent;
        }

        current = fullPath;
        for (int i = 0; i < 3; i++)
        {
            // T7.1 — never walk down into dot-directories (.github, .vs, .config) or build output:
            // a tooling solution there (e.g. aspire-samples' .github/for.dependabot.only.sln) must
            // not define the root.
            var dirs = fs.EnumerateDirectories(current, "*", SearchOption.TopDirectoryOnly)
                .Where(d =>
                {
                    var name = Path.GetFileName(d.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    return !name.StartsWith('.')
                        && !name.Equals("bin", StringComparison.OrdinalIgnoreCase)
                        && !name.Equals("obj", StringComparison.OrdinalIgnoreCase)
                        && !name.Equals("node_modules", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
            // Batch C: score across EVERY sibling at this level before returning, not the first directory
            // that happens to hold one. GitVersion's walk-down hit build/CI.slnx (a Cake build tree) while
            // the discovery extractor scored src/GitVersion.slnx — the two pickers named different systems.
            var levelSlns = new List<string>();
            foreach (var dir in dirs)
                levelSlns.AddRange(await FindSolutionsAsync(fs, dir, ct));
            if (levelSlns.Count > 0)
            {
                return new ProjectRootResult(fullPath,
                    SolutionCatalog.Pick(levelSlns, fullPath, SolutionCatalog.RepoNameOf(fullPath)),
                    levelSlns.ToImmutableArray(),
                    ResolutionMethod.WalkedDown, $"walked down {i + 1} level(s), {levelSlns.Count} solution(s) found");
            }
            var next = dirs.FirstOrDefault();
            if (next == null) break;
            current = next;
        }

        var csprojFiles = await fs.EnumerateFilesAsync(fullPath, "*.csproj", SearchOption.AllDirectories, ct).ToListAsync2(ct);
        if (csprojFiles.Count > 0)
        {
            return new ProjectRootResult(fullPath, null, csprojFiles.ToImmutableArray(),
                ResolutionMethod.FolderMode, $"no .sln found, {csprojFiles.Count} .csproj files");
        }

        return new ProjectRootResult(fullPath, null, [], ResolutionMethod.FolderMode,
            "no .sln or .csproj found, folder mode");
    }

    /// <summary>Finds solution files in a directory — both legacy <c>.sln</c> and XML <c>.slnx</c>.
    /// <c>.sln</c> is listed first so single-<c>.sln</c> repos keep their existing primary selection;
    /// on Windows the <c>*.sln</c> glob can also match <c>.slnx</c>, so duplicates are dropped.</summary>
    private static async Task<List<string>> FindSolutionsAsync(IFileSystem fs, string dir, CancellationToken ct)
    {
        var result = new List<string>();
        foreach (var pattern in new[] { "*.sln", "*.slnx" })
        {
            foreach (var file in await fs.EnumerateFilesAsync(dir, pattern, SearchOption.TopDirectoryOnly, ct).ToListAsync2(ct))
                if (!result.Contains(file)) result.Add(file);
        }
        return result;
    }
}
