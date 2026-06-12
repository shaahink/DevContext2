using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

using DevContext.Cli.Services;
using DevContext.Core.Analysis;
using DevContext.Core.Configuration;
using DevContext.Core.Contracts;
using DevContext.Core.IO;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;
using DevContext.Core.Resolvers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevContext.Desktop.Services;

public interface IAnalysisService
{
    Task<SnapshotResult> AnalyzeAsync(AnalysisOptions opts, IProgress<AnalysisProgress>? progress = null, CancellationToken ct = default);
    Task<RenderResult> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct = default);
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

    public async Task<SnapshotResult> AnalyzeAsync(
        AnalysisOptions opts,
        IProgress<AnalysisProgress>? progress = null,
        CancellationToken ct = default)
    {
        var fs = new RealFileSystem();

        var resolver = new ProjectRootResolver();
        var rootResult = await ProjectRootResolver.ResolveAsync(opts.ProjectPath, fs, ct).ConfigureAwait(false);

        // --task overrides scenario/profile via intent inference
        var scenarioKey = opts.Scenario;
        var profileStr = opts.Profile;
        if (!string.IsNullOrWhiteSpace(opts.Task))
        {
            var (inferredScenario, inferredProfile) = IntentInferrer.Infer(opts.Task);
            scenarioKey = inferredScenario;
            profileStr = inferredProfile.ToString().ToLowerInvariant();
        }

        // audit is a deprecated alias
        if (scenarioKey == "audit") scenarioKey = "overview";
        if (scenarioKey == "trace") scenarioKey = "deep-dive";

        if (!ScenarioRegistry.BuiltIn.TryGetValue(scenarioKey, out var scenarioBase))
            return new SnapshotResult { Success = false, Error = $"Unknown scenario: {scenarioKey}" };

        var profile = profileStr.ToLowerInvariant() switch
        {
            "debug" => ExtractionProfile.Debug,
            "full" => ExtractionProfile.Full,
            _ => ExtractionProfile.Focused,
        };

        // Build effective scenario: filter RequiredSections to what user selected.
        // If ActiveSections is empty, use the scenario defaults unchanged.
        var scenario = opts.ActiveSections.Length > 0
            ? scenarioBase with
            {
                RequiredSections = scenarioBase.RequiredSections
                    .Where(s => opts.ActiveSections.Contains(s))
                    .Concat(opts.ActiveSections.Where(s => !scenarioBase.RequiredSections.Contains(s)))
                    .ToImmutableArray()
            }
            : scenarioBase;

        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = profile,
            MaxOutputTokens = opts.MaxTokens,
            AllowRoslyn = !opts.NoRoslyn,
            DryRun = opts.DryRun,
            IncludeAntiPatterns = opts.IncludeAntiPatterns,
            IncludeProvenance = opts.IncludeProvenance,
            IncludeDiagnostics = opts.IncludeDiagnostics,
            OutputFormat = opts.Format == "json" ? OutputFormat.Json
                : opts.Format == "html" ? OutputFormat.Html
                : OutputFormat.Markdown,
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
        try
        {
            var snapshot = await pipeline.AnalyzeAsync(ctx, ct);
            sw.Stop();
            return new SnapshotResult
            {
                Success = true,
                Snapshot = snapshot,
                ElapsedMs = sw.ElapsedMilliseconds,
            };
        }
        catch (OperationCanceledException)
        {
            return new SnapshotResult { Success = false, Error = "Cancelled" };
        }
    }

    public async Task<RenderResult> RenderAsync(AnalysisSnapshot snapshot, RenderRequest request, CancellationToken ct = default)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDevContextServices(".");
        var sp = services.BuildServiceProvider();
        var pipeline = sp.GetRequiredService<DiscoveryPipeline>();

        var md = await pipeline.RenderAsync(snapshot, request with { Format = "markdown" }, ct);
        var html = await pipeline.RenderAsync(snapshot, request with { Format = "html" }, ct);

        return new RenderResult
        {
            Content = md.Content,
            HtmlContent = html.Content,
            EstimatedTokens = md.EstimatedTokens,
            Sections = md.Sections,
        };
    }

    public AppSettings LoadSettings()
    {
        var path = Path.Combine(_dataDir, "settings.json");
        if (!File.Exists(path)) return new AppSettings();
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings(); }
        catch (Exception ex) { Serilog.Log.Warning(ex, "Failed to load settings, using defaults"); return new AppSettings(); }
    }

    public void SaveSettings(AppSettings s)
    {
        try
        {
            File.WriteAllText(Path.Combine(_dataDir, "settings.json"),
                JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to save settings");
        }
    }

    public string[] LoadRecent()
    {
        var p = Path.Combine(_dataDir, "recent.json");
        if (!File.Exists(p)) return [];
        try { return JsonSerializer.Deserialize<string[]>(File.ReadAllText(p)) ?? []; }
        catch (Exception ex) { Serilog.Log.Warning(ex, "Failed to load recent paths"); return []; }
    }

    public void AddRecent(string path)
    {
        try
        {
            var recent = LoadRecent()
                .Where(r => !r.Equals(path, StringComparison.OrdinalIgnoreCase))
                .Take(9)
                .Prepend(path)
                .ToArray();
            File.WriteAllText(Path.Combine(_dataDir, "recent.json"), JsonSerializer.Serialize(recent));
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to save recent paths");
        }
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
                PipelineStage.Scoring => "Scoring...",
                PipelineStage.Compression => "Compressing...",
                PipelineStage.Rendering => "Rendering output...",
                _ => $"Stage: {stage}"
            };
            _progress?.Report(new AnalysisProgress(text, null));
        }

        public void OnExtractorStarted(string name, ExtractorTier tier) { }
        public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
            int typesAdded = 0, int detectionsAdded = 0)
        { }
        public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals) { }
        public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter) { }
        public void OnCompressionApplied(CompressionResult result) { }
        public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed) { }
        public void OnRenderCompleted(RenderedContext result) { }
        public void OnPipelineCompleted(DiscoveryModel model) { }
        public void OnDiagnostic(DiagnosticEntry entry) { }
    }
}

public record SnapshotResult
{
    public bool Success { get; init; }
    public AnalysisSnapshot? Snapshot { get; init; }
    public string? Error { get; init; }
    public long ElapsedMs { get; init; }
}

public record RenderResult
{
    public string? Content { get; init; }
    public string? HtmlContent { get; init; }
    public int EstimatedTokens { get; init; }
    public ImmutableArray<SectionStat> Sections { get; init; } = [];
}

public record AnalysisOptions
{
    public string ProjectPath { get; init; } = "";
    public string Scenario { get; init; } = "overview";
    public string Profile { get; init; } = "focused";       // derived by VM; kept for plumbing
    public string Around { get; init; } = "";
    public int MaxTokens { get; init; } = 8000;
    public string Format { get; init; } = "markdown";
    public bool IncludeProvenance { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool NoRoslyn { get; init; }
    public bool DryRun { get; init; }
    public bool IncludeAntiPatterns { get; init; }
    public string Task { get; init; } = "";                 // natural language intent
    public ImmutableArray<string> ActiveSections { get; init; } = [];  // section names to include; empty = use scenario defaults
}

public record AnalysisResult
{
    public bool Success { get; init; }
    public string? Content { get; init; }
    public string? HtmlContent { get; init; }
    public string? Error { get; init; }
    public long ElapsedMs { get; init; }
}

public record AnalysisProgress(string Text, double? Value);

public class AppSettings
{
    public string? LastScenario { get; set; } = "overview";
    public string? LastProfile { get; set; } = "focused";
    public string? LastFormat { get; set; } = "markdown";
    public int LastTokens { get; set; } = 8000;
    public string? LastAround { get; set; } = "";
    public bool IncludeProvenance { get; set; }
    public bool IncludeDiagnostics { get; set; }
    public bool NoRoslyn { get; set; }
    public string? LastTask { get; set; } = "";
    public List<string>? LastActiveSections { get; set; }
}
