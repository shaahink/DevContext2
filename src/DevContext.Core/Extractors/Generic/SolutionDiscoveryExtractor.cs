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
                if (file.Contains("\\.git\\", StringComparison.OrdinalIgnoreCase)) continue;
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
        // At equal depth, deprioritise *.Samples/*.Tests/*.Benchmarks (those are scaffolding, not the
        // product) and prefer the solution whose name matches the repo directory (W6).
        var byDepth = slnFiles
            .OrderBy(f => f.Count(c => c is '/' or '\\'))
            .GroupBy(f => f.Count(c => c is '/' or '\\'))
            .First();
        var repoName = Path.GetFileName(context.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var candidates = byDepth.ToList();
        var primary = PickPrimarySolution(candidates, repoName);
        if (candidates.Count > 1)
            model.AddDiagnostic(DiagnosticLevel.Info, Name,
                $"Selected {Path.GetFileName(primary)} over {string.Join(", ", candidates.Where(f => f != primary).Select(Path.GetFileName))}");

        var content = await context.FileSystem.ReadAllTextAsync(primary, ct);
        var projects = SolutionFileParser.ParseProjectPaths(content, primary);

        model.Solution = new SolutionInfo(primary, Path.GetFileNameWithoutExtension(primary), projects);
    }

    private static string PickPrimarySolution(List<string> candidates, string repoName)
    {
        if (candidates.Count == 1) return candidates[0];

        var scored = candidates.Select(f =>
        {
            var name = Path.GetFileNameWithoutExtension(f);
            var score = 0;
            if (name.Contains(".Samples", StringComparison.OrdinalIgnoreCase)
                || name.Contains(".Tests", StringComparison.OrdinalIgnoreCase)
                || name.Contains(".Benchmarks", StringComparison.OrdinalIgnoreCase))
                score -= 100;
            if (string.Equals(name, repoName, StringComparison.OrdinalIgnoreCase))
                score += 1;
            return (file: f, score);
        }).ToList();

        return scored.OrderByDescending(x => x.score).First().file;
    }
}
