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
using DevContext.Core.Observers;
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
    private ServiceProvider? _serviceProvider;
    private DiscoveryPipeline? _cachedPipeline;

    public AnalysisService()
    {
        _dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext");
        Directory.CreateDirectory(_dataDir);
    }

    private DiscoveryPipeline GetPipeline(string rootPath)
    {
        if (_cachedPipeline is not null) return _cachedPipeline;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDevContextServices();
        _serviceProvider = services.BuildServiceProvider();
        _cachedPipeline = _serviceProvider.GetRequiredService<DiscoveryPipeline>();
        return _cachedPipeline;
    }

    public async Task<SnapshotResult> AnalyzeAsync(
        AnalysisOptions opts,
        IProgress<AnalysisProgress>? progress = null,
        CancellationToken ct = default)
    {
        var fs = new RealFileSystem();

        var rootResult = await ProjectRootResolver.ResolveAsync(opts.ProjectPath, fs, ct).ConfigureAwait(false);

        // Build IntentInput and resolve via shared resolver
        var intentInput = new IntentInput
        {
            Focus = opts.Around,
            Depth = null, // desktop doesn't expose --depth yet
            ExplicitScenario = opts.Scenario,
            ExplicitProfile = opts.Profile,
        };

        ResolvedIntent resolvedIntent;
        try
        {
            resolvedIntent = AnalysisIntentResolver.Resolve(intentInput);
        }
        catch (ArgumentException ex)
        {
            return new SnapshotResult { Success = false, Error = ex.Message };
        }

        var explanation = resolvedIntent.Explanation;
        var focusWarnings = resolvedIntent.Warnings;

        // Build effective scenario: filter RequiredSections to what user selected.
        var scenario = opts.ActiveSections.Length > 0
            ? resolvedIntent.Scenario with
            {
                RequiredSections = resolvedIntent.Scenario.RequiredSections
                    .Where(s => opts.ActiveSections.Contains(s))
                    .Concat(opts.ActiveSections.Where(s => !resolvedIntent.Scenario.RequiredSections.Contains(s)))
                    .ToImmutableArray()
            }
            : resolvedIntent.Scenario;

        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = resolvedIntent.Profile,
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

        var analysis = new SharedAnalysisContext
        {
            UnresolvedFocusPoints = resolvedIntent.FocusPoints,
            FocusPoints = resolvedIntent.FocusPoints,
        };

        var pipeline = GetPipeline(rootResult.RootPath);

        var loggerFactory = _serviceProvider!.GetRequiredService<ILoggerFactory>();

        var roslyn = opts.NoRoslyn || rootResult.SolutionFilePath is null
            ? (IRoslynWorkspaceProvider)new NullRoslynProvider()
            : new DevContext.Roslyn.Services.RoslynWorkspaceProvider(
                rootResult.SolutionFilePath, fs,
                loggerFactory.CreateLogger<DevContext.Roslyn.Services.RoslynWorkspaceProvider>());

        var collector = new RunReportCollector();
        collector.SetBudget(opts.MaxTokens);
        var observer = new CompositeDiscoveryObserver(new DesktopProgressObserver(progress), collector);

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
            snapshot = snapshot with { Explanation = explanation, Warnings = focusWarnings };
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
        var pipeline = GetPipeline(".");

        var format = request.Format ?? "markdown";
        var rendered = await pipeline.RenderAsync(snapshot, request with { Format = format }, ct);

        // Render HTML only for markdown format (human view); JSON needs no HTML companion
        var htmlContent = format == "markdown"
            ? (await pipeline.RenderAsync(snapshot, request with { Format = "html" }, ct)).Content
            : null;

        return new RenderResult
        {
            Content = rendered.Content,
            HtmlContent = htmlContent,
            EstimatedTokens = rendered.EstimatedTokens,
            Sections = rendered.Sections,
            RenderFunnel = rendered.RenderFunnel,
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
    public string Explanation { get; init; } = "";
    public ImmutableArray<string> Warnings { get; init; } = [];
}

public record RenderResult
{
    public string? Content { get; init; }
    public string? HtmlContent { get; init; }
    public int EstimatedTokens { get; init; }
    public ImmutableArray<SectionStat> Sections { get; init; } = [];
    public TokenFunnel? RenderFunnel { get; init; }
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
    public ImmutableArray<string> ActiveSections { get; init; } = [];
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
    public List<string>? LastActiveSections { get; set; }
}
