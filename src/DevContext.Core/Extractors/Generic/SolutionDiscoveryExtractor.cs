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
        var slnFiles = new List<string>();
        await foreach (var file in context.FileSystem.EnumerateFilesAsync(
            context.RootPath, "*.sln", SearchOption.AllDirectories, ct).ConfigureAwait(false))
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
        var content = await context.FileSystem.ReadAllTextAsync(primary, ct).ConfigureAwait(false);
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
            if (trimmed.StartsWith("Project(", StringComparison.Ordinal))
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
