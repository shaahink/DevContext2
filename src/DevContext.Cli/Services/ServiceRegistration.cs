namespace DevContext.Cli.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddDevContextServices(this IServiceCollection services, string rootPath, string? configPath = null)
    {
        var config = configPath != null ? DevContextConfig.Load(configPath) : null;

        services.AddSingleton<IFileSystem>(_ => new RealFileSystem());
        services.AddSingleton<IAnalysisCache>(sp => new AnalysisCache(sp.GetRequiredService<IFileSystem>()));

        services.AddSingleton<IDiscoveryExtractor>(_ => new FileTreeExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new SolutionDiscoveryExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new ProjectStructureExtractor());
        services.AddSingleton<IDiscoveryExtractor>(_ => new SyntaxStructureExtractor());

        services.AddSingleton<ArchitectureStyleDetector>();

        services.AddSingleton<IContextRenderer>(_ => new MarkdownRenderer());
        services.AddSingleton<IContextRenderer>(_ => new JsonContextRenderer());

        services.AddSingleton<DiscoveryPipeline>(sp =>
        {
            var extractors = sp.GetServices<IDiscoveryExtractor>().ToArray();
            var pruners = sp.GetServices<IPruner>().ToArray();
            var compressors = sp.GetServices<ICompressionStrategy>().ToArray();
            var renderers = sp.GetServices<IContextRenderer>().ToDictionary(r => r.Format, r => r);
            var logger = sp.GetRequiredService<ILogger<DiscoveryPipeline>>();
            return new DiscoveryPipeline(extractors, pruners, compressors, renderers, logger);
        });

        return services;
    }
}

public sealed class ArchitectureStyleDetector
{
    public (ArchitectureStyle Style, float Confidence) Detect(DiscoveryModel model)
    {
        var signals = model.Architecture.All;
        float maxConfidence = 0;
        var style = ArchitectureStyle.Unknown;

        if (signals.TryGetValue(ArchitectureSignals.Keys.MinimalApis, out var ma) && ma.Detected && ma.Confidence > maxConfidence)
        {
            maxConfidence = ma.Confidence;
            style = ArchitectureStyle.MinimalApi;
        }

        if (model.Projects.Length > 4 && signals.TryGetValue(ArchitectureSignals.Keys.MediatR, out var mr) && mr.Detected)
        {
            var combined = mr.Confidence * 1.2f;
            if (combined > maxConfidence)
            {
                maxConfidence = combined;
                style = ArchitectureStyle.CleanArchitecture;
            }
        }

        if (model.Projects.Length > 2 && signals.TryGetValue(ArchitectureSignals.Keys.EfCore, out var ef) && ef.Detected)
        {
            if (style == ArchitectureStyle.Unknown && ef.Confidence > 0.5f)
            {
                style = ArchitectureStyle.NLayer;
                maxConfidence = ef.Confidence;
            }
        }

        return (style, Math.Min(maxConfidence, 1.0f));
    }
}

public sealed class MarkdownRenderer : IContextRenderer
{
    public string Format => "markdown";

    public async ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# DevContext Analysis");
        sb.AppendLine();

        if (model.Solution != null)
        {
            sb.AppendLine($"**Solution**: {model.Solution.Name}");
            sb.AppendLine($"**Projects**: {model.Projects.Length}");
        }

        if (model.DetectedStyle != ArchitectureStyle.Unknown)
        {
            sb.AppendLine($"**Architecture**: {model.DetectedStyle} ({model.StyleConfidence:P0} confidence)");
        }

        var signals = model.Architecture.All.Values.Where(s => s.Detected).ToList();
        if (signals.Count > 0)
        {
            sb.AppendLine($"**Signals**: {string.Join(" · ", signals.Select(s => s.Key))}");
        }

        sb.AppendLine($"**Types found**: {model.Types.Count}");
        var nonPruned = model.Types.Count(t => !t.Value.IsPruned);
        sb.AppendLine($"**Types in output**: {nonPruned}");
        sb.AppendLine($"**Detections**: {model.Detections.Count}");

        if (options.IncludeDiagnostics && model.Diagnostics.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Diagnostics");
            foreach (var diag in model.Diagnostics)
            {
                sb.AppendLine($"- [{diag.Level}] {diag.Source}: {diag.Message}");
            }
        }

        var estimatedTokens = sb.Length / 4;
        return new RenderedContext(sb.ToString(), estimatedTokens, [], TimeSpan.Zero, "2.0");
    }
}

public sealed class JsonContextRenderer : IContextRenderer
{
    public string Format => "json";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var obj = new
        {
            schemaVersion = "2.0",
            solution = model.Solution == null ? null : new
            {
                name = model.Solution.Name,
                filePath = model.Solution.FilePath,
                projects = model.Solution.ProjectPaths
            },
            architecture = model.DetectedStyle == ArchitectureStyle.Unknown ? null : new
            {
                style = model.DetectedStyle.ToString(),
                confidence = model.StyleConfidence
            },
            signals = model.Architecture.All.Values
                .Where(s => s.Detected)
                .Select(s => new { key = s.Key, confidence = s.Confidence })
                .ToList(),
            typesFound = model.Types.Count,
            typesInOutput = model.Types.Count(t => !t.Value.IsPruned),
            detections = model.Detections.Count
        };

        var json = System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        var estimatedTokens = json.Length / 4;
        return new ValueTask<RenderedContext>(new RenderedContext(json, estimatedTokens, [], TimeSpan.Zero, "2.0"));
    }
}
