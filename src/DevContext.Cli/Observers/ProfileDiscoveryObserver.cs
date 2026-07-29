using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace DevContext.Cli.Observers;

/// <summary>G8 — the profile instrument for an analysis that does not terminate.
///
/// Neither existing observer can profile a hang under a redirected stdout. The plain
/// <see cref="SpectreDiscoveryObserver"/> enqueues every line into its buffer and returns BEFORE the
/// console write when the console is non-interactive, so a run that never reaches the end prints
/// nothing at all; the <see cref="WaterfallDiscoveryObserver"/>'s Spectre Progress fallback emits
/// percentage rows without elapsed times and only flushes on task completion. Both report a hang as
/// silence.
///
/// This one streams one line per lifecycle event to STDERR the moment it happens, plus a HEARTBEAT
/// every <c>DEVCONTEXT_PROFILE_HEARTBEAT_SEC</c> seconds naming the stage in flight and the
/// extractors still outstanding. Killing the process therefore still leaves a per-phase timing log
/// whose LAST started-but-never-completed stage is the answer.
///
/// Off unless <c>DEVCONTEXT_PROFILE</c> is set to 1/on/true — every harness's stdout stays
/// byte-stable, and stderr carries the profile so it never contaminates a captured map.</summary>
public sealed class ProfileDiscoveryObserver : IDiscoveryObserver, IDisposable
{
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private readonly object _writeLock = new();
    private readonly ConcurrentDictionary<string, byte> _running = new();
    private readonly Timer? _heartbeat;
    private volatile string _stage = "(none)";

    /// <summary>Whether the profile stream is enabled for this process.</summary>
    public static bool Enabled => Environment.GetEnvironmentVariable("DEVCONTEXT_PROFILE") is "1" or "on" or "true";

    public ProfileDiscoveryObserver()
    {
        var seconds = int.TryParse(
            Environment.GetEnvironmentVariable("DEVCONTEXT_PROFILE_HEARTBEAT_SEC"),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed : 15;
        var period = TimeSpan.FromSeconds(seconds);
        _heartbeat = new Timer(_ => Beat(), null, period, period);
        Write("PROFILE-START", $"pid={Environment.ProcessId} heartbeat={seconds}s");
    }

    private void Beat()
    {
        var outstanding = _running.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
        var detail = outstanding.Length == 0 ? "running=none" : $"running={outstanding.Length}:{string.Join(",", outstanding)}";
        Write("HEARTBEAT", $"stage={_stage} {detail}");
    }

    private void Write(string kind, string detail)
    {
        var line = string.Create(CultureInfo.InvariantCulture, $"[profile] T+{_sw.Elapsed.TotalSeconds,8:F1}s {kind} {detail}");
        lock (_writeLock)
        {
            Console.Error.WriteLine(line);
            Console.Error.Flush();
        }
    }

    public void OnPipelineStarted(DiscoveryContext context)
        => Write("PIPELINE-START", $"root={context.RootPath}");

    public void OnStageStarted(PipelineStage stage)
    {
        _stage = stage.ToString();
        Write("STAGE-START", _stage);
    }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        Write("STAGE-DONE", string.Create(CultureInfo.InvariantCulture, $"{stage} elapsed={elapsed.TotalMilliseconds:F0}ms"));
        _stage = $"(after {stage})";
    }

    public void OnExtractorStarted(string name, ExtractorTier tier)
    {
        _running[name] = 0;
        Write("EXTRACTOR-START", $"{name} tier={tier}");
    }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
    {
        _running.TryRemove(name, out _);
        var note = skipped ? $" skipped={skipReason}" : $" +{typesAdded}t +{detectionsAdded}d";
        Write("EXTRACTOR-DONE", string.Create(CultureInfo.InvariantCulture, $"{name} elapsed={elapsed.TotalMilliseconds:F0}ms{note}"));
    }

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals)
        => Write("SIGNALS-SEALED", $"detected={signals.Values.Count(s => s.Detected)}");

    public void OnCompressionApplied(CompressionResult result)
        => Write("COMPRESSION", $"{result.StrategyName} {result.TokensBefore}->{result.TokensAfter}tok");

    public void OnRenderCompleted(RenderedContext result)
        => Write("RENDER-DONE", $"tokens={result.EstimatedTokens}");

    public void OnPipelineCompleted(DiscoveryModel model)
        => Write("PIPELINE-DONE", $"types={model.Types.Count} detections={model.Detections.Count}");

    public void OnDiagnostic(DiagnosticEntry entry)
        => Write("DIAG", $"[{entry.Level}] {entry.Source}: {entry.Message}");

    public void Dispose()
    {
        _heartbeat?.Dispose();
        Write("PROFILE-END", "");
    }
}
