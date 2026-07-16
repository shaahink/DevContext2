using DevContext.Core.Resolvers;

namespace DevContext.Core.Extractors.Generic;

/// <summary>Parses .sln and .slnx files to discover solution structure and project references.</summary>
[ExtractorOrder(-50)]
public sealed class SolutionDiscoveryExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "SolutionDiscovery";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage1Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [], ["model.Solution"],
        "Parses .sln and .slnx files to discover solution structure");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        // Enumerate .sln (legacy text) and .slnx (XML) — many modern repos ship .slnx only.
        // .sln is gathered first so single-.sln repos keep their existing primary selection.
        var slnFiles = new List<string>();
        foreach (var pattern in new[] { "*.sln", "*.slnx" })
        {
            await foreach (var file in context.FileSystem.EnumerateFilesAsync(
                context.RootPath, pattern, SearchOption.AllDirectories, ct))
            {
                // T7.1 — a solution under a dot-directory (.github/for.dependabot.only.sln,
                // .config, .vs) is tooling scaffolding, never the product solution: on
                // dotnet/aspire-samples the depth-first pick made the dependabot decoy "the"
                // solution over 13 real per-sample .slnx files. Only segments BELOW the root
                // count — the root path itself may legitimately contain dots.
                var rel = file.Length > context.RootPath.Length ? file[context.RootPath.Length..] : file;
                if (rel.Split('/', '\\').Any(s => s.StartsWith('.'))) continue;
                if (file.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)) continue;
                if (file.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase)) continue;
                // On Windows the "*.sln" glob also matches ".slnx", so the same file can surface in
                // both passes — keep first occurrence (.sln) only.
                if (!slnFiles.Contains(file)) slnFiles.Add(file);
            }
        }

        if (slnFiles.Count == 0)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, Name, "No .sln or .slnx file found");
            return;
        }

        // Prefer the solution closest to the root — the canonical one. AllDirectories order can
        // otherwise surface a nested solution first (e.g. eShop's src/ClientApp client app over the
        // root eShop.slnx).
        // Deprioritise scaffolding (tests/benchmarks/build worse than samples/docs) by name AND by
        // path segment, and prefer the solution whose name matches the repo directory (W6).
        // T7.1 — depth is a score, not a hard cut: on dotnet/aspire-samples the old shallowest-group
        // pick let tests/SamplesTests.slnx (depth 1) win unchallenged over 13 real per-sample .slnx
        // files (depth 2); scaffolding must lose to deeper product solutions.
        var repoName = Path.GetFileName(context.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var primary = PickPrimarySolution(slnFiles, context.RootPath, repoName);
        if (slnFiles.Count > 1)
        {
            var others = slnFiles.Where(f => f != primary).Select(Path.GetFileName).Take(6).ToList();
            var suffix = slnFiles.Count - 1 > others.Count ? $", +{slnFiles.Count - 1 - others.Count} more" : "";
            model.AddDiagnostic(DiagnosticLevel.Info, Name,
                $"Selected {Path.GetFileName(primary)} over {string.Join(", ", others)}{suffix}");
        }

        var content = await context.FileSystem.ReadAllTextAsync(primary, ct);
        var projects = SolutionFileParser.ParseProjectPaths(content, primary);

        model.Solution = new SolutionInfo(primary, Path.GetFileNameWithoutExtension(primary), projects);
    }

    private static string PickPrimarySolution(List<string> candidates, string rootPath, string repoName)
    {
        if (candidates.Count == 1) return candidates[0];

        var scored = candidates.Select(f =>
        {
            var rel = f.Length > rootPath.Length ? f[rootPath.Length..].TrimStart('/', '\\') : f;
            var segments = rel.Split('/', '\\');
            var name = Path.GetFileNameWithoutExtension(f);

            // Depth below root: one level costs more than any name-match bonus can buy back.
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

            if (isTestish) score -= 200;        // tooling: never the product
            else if (isSampleish) score -= 100; // samples: the product only when nothing better exists

            if (string.Equals(name, repoName, StringComparison.OrdinalIgnoreCase))
                score += 5;
            return (file: f, score);
        }).ToList();

        // Deterministic tie-break: shortest path, then ordinal.
        return scored
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.file.Length)
            .ThenBy(x => x.file, StringComparer.Ordinal)
            .First().file;
    }
}
