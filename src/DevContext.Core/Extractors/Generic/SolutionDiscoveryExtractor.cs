namespace DevContext.Core.Extractors.Generic;

[ExtractorOrder(-50)]
public sealed class SolutionDiscoveryExtractor : IDiscoveryExtractor
{
    public string Name => "SolutionDiscovery";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Generic;
    public ExtractorCapabilities Capabilities => new(
        [], [], ["model.Solution"],
        "Parses .sln and .slnx files to discover solution structure");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        var slnFiles = new List<string>();
        await foreach (var file in context.FileSystem.EnumerateFilesAsync(
            context.RootPath, "*.sln", SearchOption.AllDirectories, ct))
        {
            if (file.Contains("\\.git\\", StringComparison.OrdinalIgnoreCase)) continue;
            if (file.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)) continue;
            if (file.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase)) continue;
            slnFiles.Add(file);
        }

        if (slnFiles.Count == 0)
        {
            model.AddDiagnostic(DiagnosticLevel.Info, Name, "No .sln file found");
            return;
        }

        var primary = slnFiles[0];
        var content = await context.FileSystem.ReadAllTextAsync(primary, ct);
        var projects = ParseProjectPaths(content);

        model.Solution = new SolutionInfo(primary, Path.GetFileNameWithoutExtension(primary), projects);
    }

    private static ImmutableArray<string> ParseProjectPaths(string slnContent)
    {
        var projects = new List<string>();
        var lines = slnContent.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("Project("))
            {
                var parts = trimmed.Split(',');
                if (parts.Length >= 2)
                {
                    var path = parts[1].Trim().Trim('"');
                    if (!string.IsNullOrEmpty(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        projects.Add(path);
                    }
                }
            }
        }
        return projects.ToImmutableArray();
    }
}
