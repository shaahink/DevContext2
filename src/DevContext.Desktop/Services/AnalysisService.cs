using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DevContext.Cli.Services;
using DevContext.Core.Analysis;
using DevContext.Core.Configuration;
using DevContext.Core.Contracts;
using DevContext.Core.IO;
using DevContext.Core.Models;
using DevContext.Core.Observers;
using DevContext.Core.Pipeline;
using DevContext.Core.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Desktop.Services;

public interface IAnalysisService
{
    Task<AnalysisResult> AnalyzeAsync(AnalysisOptions opts, IProgress<AnalysisProgress>? progress = null, CancellationToken ct = default);
    AppSettings LoadSettings();
    void SaveSettings(AppSettings s);
    string[] LoadRecent();
    void AddRecent(string path);
}

public class AnalysisService : IAnalysisService
{
    private readonly string _dataDir;

    public AnalysisService()
    {
        _dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext");
        Directory.CreateDirectory(_dataDir);
    }

    public async Task<AnalysisResult> AnalyzeAsync(
        AnalysisOptions opts,
        IProgress<AnalysisProgress>? progress = null,
        CancellationToken ct = default)
    {
        var fs = new RealFileSystem();

        var resolver = new ProjectRootResolver();
        var rootResult = await resolver.ResolveAsync(opts.ProjectPath, fs, ct);

        if (!ScenarioRegistry.BuiltIn.TryGetValue(opts.Scenario, out var scenario))
            return new AnalysisResult { Success = false, Error = $"Unknown scenario: {opts.Scenario}" };

        var profile = opts.Profile.ToLowerInvariant() switch
        {
            "quick" => ExtractionProfile.Quick,
            "debug" => ExtractionProfile.Debug,
            "full" => ExtractionProfile.Full,
            _ => ExtractionProfile.Focused,
        };

        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = profile,
            MaxOutputTokens = opts.MaxTokens,
            AllowRoslyn = !opts.NoRoslyn,
            DryRun = opts.DryRun,
            IncludeProvenance = opts.IncludeProvenance,
            IncludeDiagnostics = opts.IncludeDiagnostics,
            OutputFormat = opts.Format == "json" ? OutputFormat.Json : OutputFormat.Markdown,
            ExcludePatterns = [".git", "bin", "obj", ".vs", "node_modules", ".idea"],
            ExcludeExtractors = scenario.DisableExtractors,
        };

        var cache = new AnalysisCache(fs);

        var focusPoints = !string.IsNullOrWhiteSpace(opts.Around)
            ? [FocusPointParser.Parse(opts.Around, fs)!]
            : ImmutableArray<FocusPoint>.Empty;

        var analysis = new SharedAnalysisContext
        {
            UnresolvedFocusPoints = focusPoints,
            FocusPoints = focusPoints,
        };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDevContextServices(rootResult.RootPath);
        var sp = services.BuildServiceProvider();
        var pipeline = sp.GetRequiredService<DiscoveryPipeline>();

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var roslyn = opts.NoRoslyn || rootResult.SolutionFilePath is null
            ? (IRoslynWorkspaceProvider)new NullRoslynProvider()
            : new DevContext.Roslyn.Services.RoslynWorkspaceProvider(
                rootResult.SolutionFilePath, fs,
                loggerFactory.CreateLogger<DevContext.Roslyn.Services.RoslynWorkspaceProvider>());

        var observer = new DesktopProgressObserver(progress);

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.RootPath,
            Options = options,
            ActiveScenario = scenario,
            Observer = observer,
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("DevContext"),
            RoslynWorkspace = roslyn,
            CancellationToken = ct,
        };

        var sw = Stopwatch.StartNew();
        RenderedContext result;
        try
        {
            result = await pipeline.RunAsync(ctx, ct);
        }
        catch (OperationCanceledException)
        {
            return new AnalysisResult { Success = false, Error = "Cancelled" };
        }
        sw.Stop();

        return new AnalysisResult
        {
            Success = true,
            Content = result.Content,
            ElapsedMs = sw.ElapsedMilliseconds,
        };
    }

    public AppSettings LoadSettings()
    {
        var path = Path.Combine(_dataDir, "settings.json");
        if (!File.Exists(path)) return new AppSettings();
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings(); }
        catch { return new AppSettings(); }
    }

    public void SaveSettings(AppSettings s) =>
        File.WriteAllText(Path.Combine(_dataDir, "settings.json"),
            JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true }));

    public string[] LoadRecent()
    {
        var p = Path.Combine(_dataDir, "recent.json");
        if (!File.Exists(p)) return [];
        try { return JsonSerializer.Deserialize<string[]>(File.ReadAllText(p)) ?? []; }
        catch { return []; }
    }

    public void AddRecent(string path)
    {
        var recent = LoadRecent()
            .Where(r => !r.Equals(path, StringComparison.OrdinalIgnoreCase))
            .Take(9)
            .Prepend(path)
            .ToArray();
        File.WriteAllText(Path.Combine(_dataDir, "recent.json"), JsonSerializer.Serialize(recent));
    }

    private sealed class DesktopProgressObserver : IDiscoveryObserver
    {
        private readonly IProgress<AnalysisProgress>? _progress;

        public DesktopProgressObserver(IProgress<AnalysisProgress>? progress) => _progress = progress;

        public void OnPipelineStarted(DiscoveryContext context) { }

        public void OnStageStarted(PipelineStage stage)
        {
            var text = stage switch
            {
                PipelineStage.DiscoveryAndCacheWarmup => "Discovering files...",
                PipelineStage.GenericExtraction => "Extracting structure...",
                PipelineStage.SignalSealing => "Sealing signals...",
                PipelineStage.SpecificExtraction => "Deep analysis...",
                PipelineStage.Pruning => "Pruning...",
                PipelineStage.Compression => "Compressing...",
                PipelineStage.Rendering => "Rendering output...",
                _ => $"Stage: {stage}"
            };
            _progress?.Report(new AnalysisProgress(text, null));
        }

        public void OnExtractorStarted(string name, ExtractorTier tier) { }
        public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
            int typesAdded = 0, int detectionsAdded = 0) { }
        public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals) { }
        public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter) { }
        public void OnCompressionApplied(CompressionResult result) { }
        public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed) { }
        public void OnRenderCompleted(RenderedContext result) { }
        public void OnPipelineCompleted(DiscoveryModel model) { }
        public void OnDiagnostic(DiagnosticEntry entry) { }
    }
}

public record AnalysisOptions
{
    public string ProjectPath { get; init; } = "";
    public string Scenario { get; init; } = "architecture";
    public string Profile { get; init; } = "focused";
    public string Around { get; init; } = "";
    public int MaxTokens { get; init; } = 8000;
    public string Format { get; init; } = "markdown";
    public bool IncludeProvenance { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool NoRoslyn { get; init; }
    public bool DryRun { get; init; }
}

public record AnalysisResult
{
    public bool Success { get; init; }
    public string? Content { get; init; }
    public string? Error { get; init; }
    public long ElapsedMs { get; init; }
}

public record AnalysisProgress(string Text, double? Value);

public class AppSettings
{
    public string? LastScenario { get; set; } = "debug-endpoint";
    public string? LastProfile { get; set; } = "focused";
    public string? LastFormat { get; set; } = "markdown";
    public int LastTokens { get; set; } = 8000;
    public string? LastAround { get; set; } = "";
    public bool IncludeProvenance { get; set; }
    public bool IncludeDiagnostics { get; set; }
    public bool NoRoslyn { get; set; }
}
