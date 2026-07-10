using System.Collections.Concurrent;

using DevContext.Core.Graph2;

using Microsoft.CodeAnalysis;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Builds structured <see cref="BodyFacts"/> for every source file in a single pass over the
/// already-parsed syntax trees (Loom L2.1). This is the "existing parse" reuse the design mandates: it
/// pulls each tree from the memoised <see cref="IAnalysisCache"/> (never re-parses a body) and caches the
/// facts per file (<c>facts-v1</c>). It writes ONLY to <see cref="SharedAnalysisContext.AllBodyFacts"/> —
/// no model mutation — so it is output-neutral until the assembler consumes it (L2.3). Seam detectors
/// (<c>DevContext.Core.Graph2.Seams</c>) run over these facts.</summary>
[ExtractorOrder(35)]
public sealed class BodyFactsExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "BodyFactsExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Deep;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["context.Analysis.AllBodyFacts"],
        "Builds structured BodyFacts (facts-v1) from the shared parse for seam detection (Loom L2)");

    /// <summary>Runs whenever the full graph is built (report/analyze) — the same gate as the call graph,
    /// since body facts feed the same wiring the full graph needs. Output-neutral, so it can only help.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.Options.BuildFullGraph
            || context.Options.Profile is ExtractionProfile.Debug or ExtractionProfile.Full;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var files = context.Analysis.AllSourceFiles;
        var perFile = new ImmutableArray<BodyFacts>[files.Count];
        var fileToProject = BuildFileToProject(context);

        var opts = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount };
        await Parallel.ForEachAsync(Enumerable.Range(0, files.Count), opts, async (i, innerCt) =>
        {
            var filePath = files[i];
            try
            {
                var project = fileToProject(filePath) ?? "";
                var facts = await context.Analysis.GetOrBuildBodyFactsAsync(filePath, async () =>
                {
                    // Reuse the memoised syntax tree — no second parse of the body (design §2.1).
                    var tree = await context.Cache.GetSyntaxTreeAsync(filePath, innerCt);
                    var root = await tree.GetRootAsync(innerCt).ConfigureAwait(false);
                    return BodyFactExtractor.Extract(root, filePath, project);
                });
                perFile[i] = facts;
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning(ex, "Failed to build body facts: {Path}", filePath);
            }
        });

        var all = new List<BodyFacts>();
        foreach (var list in perFile)
            if (!list.IsDefault) all.AddRange(list);

        context.Analysis.AllBodyFacts = all;
    }

    /// <summary>Maps a file path to its owning project short-name using the discovered project files
    /// (longest matching project directory wins). Best-effort — empty when unknown.</summary>
    private static Func<string, string?> BuildFileToProject(DiscoveryContext context)
    {
        var projectDirs = new List<(string Dir, string Name)>();
        foreach (var projPath in context.Analysis.AllProjectFiles)
        {
            var dir = System.IO.Path.GetDirectoryName(projPath);
            if (string.IsNullOrEmpty(dir)) continue;
            var name = System.IO.Path.GetFileNameWithoutExtension(projPath);
            projectDirs.Add((dir.Replace('\\', '/').TrimEnd('/') + "/", name));
        }
        projectDirs.Sort((a, b) => b.Dir.Length.CompareTo(a.Dir.Length));

        return filePath =>
        {
            var norm = filePath.Replace('\\', '/');
            foreach (var (dir, name) in projectDirs)
                if (norm.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
                    return name;
            return null;
        };
    }
}
