namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(-100)]
public sealed class FileTreeExtractor : IDiscoveryExtractor
{
    public string Name => "FileTreeExtractor";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;
    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["SharedAnalysisContext.AllSourceFiles", "IAnalysisCache path registration"],
        "Walks the file tree and registers all source file paths in the cache");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var sourceFiles = new List<string>();
        var projectFiles = new List<string>();

        await foreach (var file in context.FileSystem.EnumerateFilesAsync(
            context.RootPath, "*.cs", SearchOption.AllDirectories, ct))
        {
            if (IsExcluded(file, context.Options.ExcludePatterns)) continue;
            sourceFiles.Add(file);
            context.Cache.RegisterPath(file);
        }

        await foreach (var file in context.FileSystem.EnumerateFilesAsync(
            context.RootPath, "*.csproj", SearchOption.AllDirectories, ct))
        {
            if (IsExcluded(file, context.Options.ExcludePatterns)) continue;
            projectFiles.Add(file);
            context.Cache.RegisterPath(file);
        }

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
