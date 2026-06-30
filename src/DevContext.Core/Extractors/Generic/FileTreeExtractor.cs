namespace DevContext.Core.Extractors.Generic;

/// <summary>Walks the file tree, discovers .cs, .razor, .csproj, and .sln files, and registers them in the analysis context and cache.</summary>
[ExtractorOrder(-100)]
public sealed class FileTreeExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "FileTreeExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage1Sequential;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["SharedAnalysisContext.AllSourceFiles", "IAnalysisCache path registration"],
        "Walks the file tree and registers all source file paths in the cache");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var sourceFiles = new List<string>();
        var projectFiles = new List<string>();

        // Hybrid scope (G1): when a closure scan set was resolved, walk the union of those project
        // dirs (deduped) — the closure can live outside RootPath (e.g. Domain/Infrastructure under a
        // sibling folder). Empty scan set ⇒ walk RootPath (whole-solution / folder mode), unchanged.
        var roots = context.ScopedProjectDirs.IsDefaultOrEmpty
            ? [context.RootPath]
            : context.ScopedProjectDirs.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var seenSource = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenProject = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in roots)
        {
            await foreach (var file in context.FileSystem.EnumerateFilesAsync(
                root, "*.cs", SearchOption.AllDirectories, ct))
            {
                if (IsExcluded(file, context.Options.ExcludePatterns)) continue;
                if (!seenSource.Add(file)) continue;
                sourceFiles.Add(file);
                context.Cache.RegisterPath(file);
            }

            // Include .razor files for Blazor component detection
            await foreach (var file in context.FileSystem.EnumerateFilesAsync(
                root, "*.razor", SearchOption.AllDirectories, ct))
            {
                if (IsExcluded(file, context.Options.ExcludePatterns)) continue;
                if (!seenSource.Add(file)) continue;
                sourceFiles.Add(file);
                context.Cache.RegisterPath(file);
            }

            await foreach (var file in context.FileSystem.EnumerateFilesAsync(
                root, "*.csproj", SearchOption.AllDirectories, ct))
            {
                if (IsExcluded(file, context.Options.ExcludePatterns)) continue;
                if (!seenProject.Add(file)) continue;
                projectFiles.Add(file);
                context.Cache.RegisterPath(file);
            }
        }

        // Solution files are always discovered from RootPath (the solution dir), independent of scope.
        await foreach (var file in context.FileSystem.EnumerateFilesAsync(
            context.RootPath, "*.sln*", SearchOption.AllDirectories, ct))
        {
            if (IsExcluded(file, context.Options.ExcludePatterns)) continue;
            context.Cache.RegisterPath(file);
        }

        context.Analysis.AllSourceFiles = sourceFiles;
        context.Analysis.AllProjectFiles = projectFiles;
    }

    private static bool IsExcluded(string path, ImmutableArray<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            if (path.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
