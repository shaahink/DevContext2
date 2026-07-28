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
        // Batch C (DC6) — enumeration + scoring now live in SolutionCatalog, so this extractor and
        // ProjectRootResolver make the SAME pick. They used to disagree: the resolver took the first
        // file in enumeration order, this scored candidates, and nobody told the user either way.
        var slnFiles = await SolutionCatalog.EnumerateAsync(context.RootPath, context.FileSystem, ct);

        if (slnFiles.Length == 0)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, Name, "No .sln or .slnx file found");
            return;
        }

        var repoName = SolutionCatalog.RepoNameOf(context.RootPath);
        var requested = SolutionCatalog.ResolveRequested(slnFiles, context.RootPath, context.RequestedSolution);
        if (context.RequestedSolution is { Length: > 0 } asked && requested is null)
        {
            // A typo must never silently analyse a different system than the one named.
            model.AddDiagnostic(DiagnosticLevel.Warning, Name,
                $"--sln '{asked}' matched none of the {slnFiles.Length} solutions found; using the default pick. "
                + $"Candidates: {string.Join(", ", slnFiles.Select(Path.GetFileName).Take(8))}");
        }
        var primary = requested ?? SolutionCatalog.Pick(slnFiles, context.RootPath, repoName)!;

        if (slnFiles.Length > 1)
        {
            var others = slnFiles.Where(f => f != primary).Select(Path.GetFileName).Take(6).ToList();
            var suffix = slnFiles.Length - 1 > others.Count ? $", +{slnFiles.Length - 1 - others.Count} more" : "";
            model.AddDiagnostic(DiagnosticLevel.Info, Name,
                $"Selected {Path.GetFileName(primary)} over {string.Join(", ", others)}{suffix}");
        }

        var content = await context.FileSystem.ReadAllTextAsync(primary, ct);
        var projects = SolutionFileParser.ParseProjectPaths(content, primary);

        model.Solution = new SolutionInfo(primary, Path.GetFileNameWithoutExtension(primary), projects);
        model.ScopeNote = new SolutionScopeNote(
            primary,
            Path.GetFileNameWithoutExtension(primary),
            RelativeTo(context.RootPath, primary),
            slnFiles.Length,
            [.. slnFiles.Where(f => f != primary).Select(f => RelativeTo(context.RootPath, f))],
            requested is not null);
    }

    private static string RelativeTo(string root, string file)
    {
        var norm = file.Replace('\\', '/');
        var r = root.Replace('\\', '/').TrimEnd('/');
        return norm.Length > r.Length && norm.StartsWith(r, StringComparison.OrdinalIgnoreCase)
            ? norm[(r.Length + 1)..]
            : norm;
    }
}
