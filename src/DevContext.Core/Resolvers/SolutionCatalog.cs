namespace DevContext.Core.Resolvers;

/// <summary>
/// The ONE solution-picking algebra (Batch C / DC6). A repo can declare several solutions — GitVersion
/// ships three (<c>src/</c>, <c>new-cli/</c>, <c>build/</c>), dotnet/aspire-samples fourteen — and before
/// this type two independent code paths picked one: <see cref="ProjectRootResolver"/> took the FIRST file
/// in enumeration order, while <c>SolutionDiscoveryExtractor</c> scored candidates. They could disagree,
/// and neither told the user that a choice had been made at all.
///
/// This type does three things and nothing else: enumerate the candidates, score ONE pick, and honor an
/// explicit request. Whether the analysis then narrows to that solution is <see cref="Graph.SolutionScope"/>'s
/// business; whether the user hears about it is <see cref="Models.SolutionScopeNote"/>'s.
/// </summary>
public static class SolutionCatalog
{
    /// <summary>Solution file extensions, legacy first (a single-<c>.sln</c> repo keeps its old pick).</summary>
    public static readonly string[] Patterns = ["*.sln", "*.slnx"];

    /// <summary>True when a discovered solution path is scaffolding rather than a candidate: anything under
    /// a dot-directory (<c>.github/for.dependabot.only.sln</c> — the aspire-samples decoy), build output, or
    /// a package/tool cache. Segments BELOW <paramref name="rootPath"/> are what count; the root itself may
    /// legitimately contain dots.</summary>
    public static bool IsIgnoredPath(string rootPath, string file)
    {
        var rel = file.Length > rootPath.Length ? file[rootPath.Length..] : file;
        foreach (var seg in rel.Split('/', '\\'))
        {
            if (seg.StartsWith('.')) return true;
            if (seg.Equals("bin", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("obj", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("node_modules", StringComparison.OrdinalIgnoreCase)
                || seg.Equals("artifacts", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Every solution file under <paramref name="rootPath"/>, scaffolding excluded, in a
    /// deterministic order (<c>.sln</c> before <c>.slnx</c>, then ordinal by path).</summary>
    public static async Task<ImmutableArray<string>> EnumerateAsync(
        string rootPath, IFileSystem fs, CancellationToken ct = default)
    {
        var found = new List<string>();
        foreach (var pattern in Patterns)
        {
            await foreach (var file in fs.EnumerateFilesAsync(rootPath, pattern, SearchOption.AllDirectories, ct))
            {
                if (IsIgnoredPath(rootPath, file)) continue;
                // On Windows the "*.sln" glob also matches ".slnx" — keep the first occurrence only.
                if (!found.Contains(file)) found.Add(file);
            }
        }
        return [.. found];
    }

    /// <summary>
    /// Scores the candidates and returns the primary. Depth below the root dominates (one level costs more
    /// than any name bonus can buy back); test/benchmark/build trees are never the product; samples are the
    /// product only when nothing better exists; a solution named after the repo gets a nudge. Ties break on
    /// shortest path then ordinal, so the pick is stable across runs and machines.
    /// </summary>
    public static string? Pick(IReadOnlyList<string> candidates, string rootPath, string repoName)
    {
        if (candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        var best = candidates[0];
        var bestScore = int.MinValue;
        var bestLen = int.MaxValue;
        foreach (var f in candidates)
        {
            var score = Score(f, rootPath, repoName);
            if (score > bestScore
                || (score == bestScore && f.Length < bestLen)
                || (score == bestScore && f.Length == bestLen && string.CompareOrdinal(f, best) < 0))
            {
                best = f;
                bestScore = score;
                bestLen = f.Length;
            }
        }
        return best;
    }

    /// <summary>The scoring rung, exposed so callers can explain a pick.</summary>
    public static int Score(string file, string rootPath, string repoName)
    {
        var rel = file.Length > rootPath.Length ? file[rootPath.Length..].TrimStart('/', '\\') : file;
        var segments = rel.Split('/', '\\');
        var name = Path.GetFileNameWithoutExtension(file);

        var score = -10 * (segments.Length - 1);

        var dirSegments = segments[..^1];
        var isTestish = name.Contains(".Tests", StringComparison.OrdinalIgnoreCase)
            || name.Contains(".Benchmarks", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("Tests", StringComparison.Ordinal)
            || dirSegments.Any(s => s.Equals("test", StringComparison.OrdinalIgnoreCase)
                || s.Equals("tests", StringComparison.OrdinalIgnoreCase)
                || s.Equals("benchmarks", StringComparison.OrdinalIgnoreCase)
                || s.Equals("build", StringComparison.OrdinalIgnoreCase)
                || s.Equals("eng", StringComparison.OrdinalIgnoreCase));
        var isSampleish = name.Contains(".Samples", StringComparison.OrdinalIgnoreCase)
            || dirSegments.Any(s => s.Equals("samples", StringComparison.OrdinalIgnoreCase)
                || s.Equals("sample", StringComparison.OrdinalIgnoreCase)
                || s.Equals("examples", StringComparison.OrdinalIgnoreCase)
                || s.Equals("demos", StringComparison.OrdinalIgnoreCase)
                || s.Equals("docs", StringComparison.OrdinalIgnoreCase));

        if (isTestish) score -= 200;
        else if (isSampleish) score -= 100;

        if (string.Equals(name, repoName, StringComparison.OrdinalIgnoreCase)) score += 5;
        return score;
    }

    /// <summary>
    /// Resolves what the user asked for with <c>--sln</c> against the candidates: an absolute path, a
    /// root-relative path (either slash flavor), a file name, or a bare solution name — all
    /// case-insensitive. Returns null when nothing matches, which callers report rather than silently
    /// falling back (a typo must not analyse a different system than the one asked for).
    /// </summary>
    public static string? ResolveRequested(IReadOnlyList<string> candidates, string rootPath, string? requested)
    {
        if (string.IsNullOrWhiteSpace(requested) || candidates.Count == 0) return null;
        var want = requested.Trim().Replace('\\', '/').TrimStart('.', '/');

        foreach (var mode in new[] { 0, 1, 2, 3 })
        {
            foreach (var c in candidates)
            {
                var norm = c.Replace('\\', '/');
                var rel = norm.Length > rootPath.Length
                    ? norm[rootPath.Length..].Replace('\\', '/').TrimStart('/')
                    : norm;
                var match = mode switch
                {
                    0 => norm.Equals(want, StringComparison.OrdinalIgnoreCase),
                    1 => rel.Equals(want, StringComparison.OrdinalIgnoreCase),
                    2 => Path.GetFileName(norm).Equals(want, StringComparison.OrdinalIgnoreCase),
                    _ => Path.GetFileNameWithoutExtension(norm).Equals(want, StringComparison.OrdinalIgnoreCase),
                };
                if (match) return c;
            }
        }
        return null;
    }

    /// <summary>The repo/directory name used for the name-match nudge.</summary>
    public static string RepoNameOf(string rootPath)
        => Path.GetFileName(rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
}
