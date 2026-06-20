namespace DevContext.Core.Resolvers;

/// <summary>
/// Computes the <b>scan set</b> for the Hybrid scope policy (G1): the project directories that
/// discovery should walk. Project/subfolder input (<see cref="ResolutionMethod.ExplicitCsproj"/> /
/// <see cref="ResolutionMethod.WalkedUp"/>) resolves to the anchor project plus its transitive
/// <c>ProjectReference</c> closure; <c>.sln</c>/repo-root input resolves to the whole solution
/// (returned as an empty scan set — discovery walks the root and <see cref="Graph.SolutionScope"/>
/// filters to the solution). An empty result means "no narrowing".
/// </summary>
public static class ScopeResolver
{
    /// <summary>Resolves (closure project dirs, anchor csproj) for the given base resolution.</summary>
    public static async Task<(ImmutableArray<string> Dirs, string? Anchor)> ResolveAsync(
        ProjectRootResult baseResult, IFileSystem fs, CancellationToken ct = default)
    {
        var closureMode = baseResult.Method is ResolutionMethod.ExplicitCsproj or ResolutionMethod.WalkedUp;
        if (!closureMode)
            return (default, null);

        // Bound the closure to the resolved solution's projects when one is known.
        List<string>? slnProjects = null;
        HashSet<string>? slnSet = null;
        if (baseResult.SolutionFilePath is { } slnPath)
        {
            try
            {
                var content = await fs.ReadAllTextAsync(slnPath, ct);
                var slnDir = fs.GetDirectoryName(slnPath)!;
                slnProjects = SolutionFileParser.ParseProjectPaths(content, slnPath)
                    .Select(rel => ToAbsolute(slnDir, rel))
                    .ToList();
                slnSet = slnProjects.Select(Normalize).ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                // Unreadable solution — fall back to unbounded closure from the anchor csproj.
            }
        }

        var anchor = baseResult.Method == ResolutionMethod.ExplicitCsproj
            ? baseResult.EntryCandidates.FirstOrDefault()
            : FindAnchor(baseResult.RootPath, slnProjects, fs);

        // No single owning project (e.g. input points at a folder of several projects) → whole-solution.
        if (anchor is null)
            return (default, null);

        var closure = await ComputeClosureAsync(anchor, slnSet, fs, ct);
        var dirs = closure
            .Select(fs.GetDirectoryName)
            .Where(d => !string.IsNullOrEmpty(d))
            .Select(d => d!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToImmutableArray();
        return (dirs, anchor);
    }

    /// <summary>The solution project whose directory most specifically contains the input path.</summary>
    private static string? FindAnchor(string inputPath, List<string>? slnProjects, IFileSystem fs)
    {
        if (slnProjects is null) return null;
        var input = Normalize(inputPath);
        string? best = null;
        var bestLen = -1;
        foreach (var proj in slnProjects)
        {
            var dir = Normalize(fs.GetDirectoryName(proj) ?? "");
            if (dir.Length == 0) continue;
            if (input.Equals(dir, StringComparison.OrdinalIgnoreCase)
                || input.StartsWith(dir + "/", StringComparison.OrdinalIgnoreCase))
            {
                if (dir.Length > bestLen) { best = proj; bestLen = dir.Length; }
            }
        }
        return best;
    }

    private static async Task<List<string>> ComputeClosureAsync(
        string anchor, HashSet<string>? slnSet, IFileSystem fs, CancellationToken ct)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string>();
        var queue = new Queue<string>();
        queue.Enqueue(anchor);

        while (queue.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var proj = queue.Dequeue();
            if (!visited.Add(Normalize(proj))) continue;
            ordered.Add(proj);

            var projDir = fs.GetDirectoryName(proj);
            if (string.IsNullOrEmpty(projDir)) continue;

            ImmutableArray<string> refs;
            try
            {
                var content = await fs.ReadAllTextAsync(proj, ct);
                refs = CsprojReader.ParseProjectReferences(System.Xml.Linq.XDocument.Parse(content));
            }
            catch
            {
                continue; // missing/malformed csproj — keep walking the rest of the closure
            }

            foreach (var rel in refs)
            {
                var abs = ToAbsolute(projDir, rel.Replace('\\', '/'));
                if (slnSet is not null && !slnSet.Contains(Normalize(abs))) continue;
                if (!visited.Contains(Normalize(abs))) queue.Enqueue(abs);
            }
        }
        return ordered;
    }

    private static string ToAbsolute(string baseDir, string path)
        => Path.IsPathRooted(path) || baseDir.Length == 0
            ? path
            : Path.GetFullPath(Path.Combine(baseDir, path));

    private static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');
}
