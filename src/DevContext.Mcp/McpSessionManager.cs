using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevContext.Core.Analysis;
using DevContext.Core.Configuration;
using DevContext.Core.Contracts;
using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;
using DevContext.Core.Resolvers;
using Microsoft.Extensions.Logging;

namespace DevContext.Mcp;

public sealed class McpSessionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _pathToHandle = new(StringComparer.OrdinalIgnoreCase);
    private readonly SnapshotCacheService _snapCache = new();
    private readonly DiscoveryPipeline _pipeline;
    private readonly IFileSystem _fs;
    private readonly ILogger<McpSessionManager> _logger;
    private const int MaxSessions = 3;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public McpSessionManager(DiscoveryPipeline pipeline, IFileSystem fs, ILogger<McpSessionManager> logger)
    {
        _pipeline = pipeline;
        _fs = fs;
        _logger = logger;
    }

    public string StartAnalysis(string path)
    {
        // Reuse existing handle for this path if already analyzing or ready.
        var normalized = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (_pathToHandle.TryGetValue(normalized, out var existingHandle)
            && _sessions.TryGetValue(existingHandle, out var existingEntry)
            && existingEntry.Status is "analyzing" or "ready")
        {
            existingEntry.LastAccess = DateTime.UtcNow;
            return existingHandle;
        }

        EvictLru();
        var handle = Guid.NewGuid().ToString("N");
        var entry = new SessionEntry(handle) { Status = "analyzing" };
        _sessions[handle] = entry;
        _pathToHandle[normalized] = handle;
        _ = RunAnalysisAsync(handle, path);
        return handle;
    }

    public SessionStatusResult GetStatus(string handle)
    {
        if (!_sessions.TryGetValue(handle, out var entry))
            return new SessionStatusResult { Found = false, Error = $"Unknown handle: {handle}" };

        return new SessionStatusResult
        {
            Found = true,
            Status = entry.Status,
            ProgressPercent = entry.ProgressPercent,
            ProgressMessage = entry.ProgressMessage,
            Summary = entry.Summary,
            ElapsedMs = entry.ElapsedMs,
        };
    }

    public McpAnalysisSession? GetSession(string handle)
    {
        if (!_sessions.TryGetValue(handle, out var entry)) return null;
        if (entry.Status != "ready" || entry.Session is null) return null;
        entry.LastAccess = DateTime.UtcNow;
        return entry.Session;
    }

    public bool CloseSession(string handle)
    {
        if (!_sessions.TryRemove(handle, out var entry)) return false;
        // Clean up path→handle mapping.
        foreach (var kv in _pathToHandle)
            if (kv.Value == handle)
                _pathToHandle.TryRemove(kv.Key, out _);
        return true;
    }

    public SessionStatusResult[] ListSessions()
    {
        return _sessions.Values
            .OrderByDescending(e => e.LastAccess)
            .Select(e => new SessionStatusResult
            {
                Handle = e.Handle,
                Found = true,
                Status = e.Status,
                ProgressPercent = e.ProgressPercent,
                ProgressMessage = e.ProgressMessage,
                Summary = e.Summary,
                ElapsedMs = e.ElapsedMs,
            })
            .ToArray();
    }

    private async Task RunAnalysisAsync(string handle, string path)
    {
        try
        {
            if (!_sessions.TryGetValue(handle, out var entry)) return;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var rootResult = await ProjectRootResolver.ResolveAsync(path, _fs, CancellationToken.None);
            if (string.IsNullOrEmpty(rootResult.EffectiveRootPath))
            {
                _sessions[handle] = entry with { Status = "error", Error = "Could not resolve project root." };
                return;
            }

            var rootPath = rootResult.EffectiveRootPath;

            // ── I8 snapshot cache check ──
            var (repoKey, versionKey) = SnapshotCacheService.ComputeKeys(rootPath);
            var fromCache = false;
            AnalysisSnapshot? snapshot = null;
            if (_snapCache.Exists(repoKey, versionKey))
            {
                snapshot = await _snapCache.TryLoadAsync<AnalysisSnapshot>(repoKey, versionKey, CancellationToken.None);
                if (snapshot is not null)
                {
                    fromCache = true;
                    snapshot = snapshot with { RootPath = rootPath };
                }
            }

            if (!fromCache)
            {
                var options = new ExtractionOptions
                {
                    Profile = ExtractionProfile.Focused,
                    AllowRoslyn = true,
                    BuildFullGraph = true,
                    OutputFormat = OutputFormat.Json,
                };

                var scenario = ScenarioRegistry.BuiltIn["overview"];
                var cache = new AnalysisCache(_fs);
                var analysis = new SharedAnalysisContext();

                var observer = new McpPipelineObserver(
                    stage => OnProgress(handle, StageToPercent(stage), stage.ToString()));

                var ctx = new DiscoveryContext
                {
                    RootPath = rootPath,
                    Options = options,
                    ActiveScenario = scenario,
                    Observer = observer,
                    FileSystem = _fs,
                    Cache = cache,
                    Analysis = analysis,
                    Logger = _logger,
                };

                snapshot = await _pipeline.AnalyzeAsync(ctx);

                // Write through cache (fire-and-forget — failure is non-fatal).
                _ = _snapCache.SaveAsync(repoKey, versionKey, snapshot, CancellationToken.None);
            }

            sw.Stop();

            if (snapshot?.Graph is null)
            {
                _sessions[handle] = entry with { Status = "error", Error = "Analysis produced no graph." };
                return;
            }

            var graph = snapshot.Graph;
            var query = new GraphQuery(graph, snapshot.Entries, snapshot.Map);
            var (seams, entriesWithTarget) = query.Stats();

            var nodeCount = graph.NodeCount;
            var edgeCount = graph.EdgeCount;

            _sessions[handle] = entry with
            {
                Status = "ready",
                Session = new McpAnalysisSession(snapshot, query),
                ProgressPercent = 100,
                ProgressMessage = fromCache ? "Analysis complete (from cache)" : "Analysis complete",
                Summary = new AnalysisSummaryResult
                {
                    Label = Path.GetFileName(rootResult.SolutionFilePath ?? rootPath.TrimEnd('\\', '/')),
                    Projects = snapshot.Map?.Topology.Length ?? 0,
                    Nodes = nodeCount,
                    Edges = edgeCount,
                    Entries = snapshot.Entries.Length,
                    EntriesWithTarget = entriesWithTarget,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    Archetype = snapshot.Map?.Archetype.ToString().ToLowerInvariant() ?? "app",
                    Warnings = [.. snapshot.Warnings],
                },
                ElapsedMs = sw.ElapsedMilliseconds,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP analysis failed for {Path}", path);
            if (_sessions.TryGetValue(handle, out var current))
                _sessions[handle] = current with { Status = "error", Error = ex.Message };
        }
    }

    private static double StageToPercent(PipelineStage stage) => stage switch
    {
        PipelineStage.ProjectRootResolution => 5,
        PipelineStage.DiscoveryAndCacheWarmup => 15,
        PipelineStage.GenericExtraction => 40,
        PipelineStage.SignalSealing => 55,
        PipelineStage.SpecificExtraction => 70,
        PipelineStage.Scoring => 80,
        PipelineStage.Compression => 90,
        PipelineStage.Rendering => 100,
        _ => 0,
    };

    private void OnProgress(string handle, double percent, string? message)
    {
        if (_sessions.TryGetValue(handle, out var entry))
            _sessions[handle] = entry with { ProgressPercent = percent, ProgressMessage = message };
    }

    private void EvictLru()
    {
        while (_sessions.Count >= MaxSessions)
        {
            var oldest = _sessions.Values.OrderBy(e => e.LastAccess).FirstOrDefault();
            if (oldest is null) break;
            _sessions.TryRemove(oldest.Handle, out _);
            foreach (var kv in _pathToHandle)
                if (kv.Value == oldest.Handle)
                    _pathToHandle.TryRemove(kv.Key, out _);
        }
    }

    public void Dispose() => _sessions.Clear();

    public static string Serialize(object obj) => JsonSerializer.Serialize(obj, JsonOpts);
}

public sealed class McpAnalysisSession
{
    public AnalysisSnapshot Snapshot { get; }
    public GraphQuery Query { get; }

    public McpAnalysisSession(AnalysisSnapshot snapshot, GraphQuery query)
    {
        Snapshot = snapshot;
        Query = query;
    }

    public McpEnvelope Envelope() => McpEnvelope.From(Snapshot);
}

public sealed class McpPipelineObserver : IDiscoveryObserver
{
    private readonly Action<PipelineStage> _onStage;

    public McpPipelineObserver(Action<PipelineStage> onStage) => _onStage = onStage;

    public void OnPipelineStarted(DiscoveryContext context) { }
    public void OnStageStarted(PipelineStage stage) => _onStage(stage);
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

public sealed record SessionEntry(string Handle)
{
    public string Status { get; init; } = "analyzing";
    public double ProgressPercent { get; init; }
    public string? ProgressMessage { get; init; }
    public string? Error { get; init; }
    public AnalysisSummaryResult? Summary { get; init; }
    public McpAnalysisSession? Session { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastAccess { get; set; } = DateTime.UtcNow;
    public long ElapsedMs { get; init; }
}

public sealed class SessionStatusResult
{
    public string? Handle { get; set; }
    public bool Found { get; set; }
    public string? Error { get; set; }
    public string Status { get; set; } = "unknown";
    public double ProgressPercent { get; set; }
    public string? ProgressMessage { get; set; }
    public AnalysisSummaryResult? Summary { get; set; }
    public long ElapsedMs { get; set; }
}

public sealed class AnalysisSummaryResult
{
    public string Label { get; set; } = "";
    public int Projects { get; set; }
    public int Nodes { get; set; }
    public int Edges { get; set; }
    public int Entries { get; set; }
    public int EntriesWithTarget { get; set; }
    public long ElapsedMs { get; set; }
    public string Archetype { get; set; } = "app";
    public List<string> Warnings { get; set; } = [];
}

public sealed class McpEnvelope
{
    public string Scope { get; set; } = "";
    public double Coverage { get; set; }
    public double Confidence { get; set; }

    public static McpEnvelope From(AnalysisSnapshot snapshot)
    {
        var graph = snapshot.Graph;
        var (seams, entriesWithTarget) = graph is not null
            ? GraphStats.Compute(graph, snapshot.Entries)
            : ([], 0);

        var ledger = graph is not null
            ? ConfidenceLedger.Compute(graph, snapshot.Entries)
            : null;

        return new McpEnvelope
        {
            Scope = snapshot.Map?.Archetype.ToString().ToLowerInvariant() ?? "app",
            Coverage = snapshot.Entries.Length > 0
                ? (double)entriesWithTarget / snapshot.Entries.Length
                : 0,
            Confidence = ledger?.OverallConfidence ?? 0,
        };
    }
}
