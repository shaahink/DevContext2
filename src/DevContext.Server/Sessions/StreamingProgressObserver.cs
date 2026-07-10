using System.Diagnostics;
using DevContext.Core.Analysis;
using DevContext.Core.Contracts;

namespace DevContext.Server.Sessions;

/// <summary>Bridges the engine's <see cref="IDiscoveryObserver"/> callbacks to an
/// <see cref="IProgress{T}"/> stream. L1.3 adds event throttling (≤4/s) so the client
/// never chokes on a progress flood, and uses extractor completions to interpolate
/// progress within stages instead of freezing at a single percent.</summary>
internal sealed class StreamingProgressObserver(IProgress<AnalysisProgress>? progress) : IDiscoveryObserver
{
    private long _lastReportTicks; // Stopwatch.GetTimestamp()
    private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(250);
    private int _extractorsSeen;
    private int _extractorsSinceStage;
    private int _stageBasePercentBefore = 10;

    public void OnPipelineStarted(DiscoveryContext context)
    {
        _lastReportTicks = Stopwatch.GetTimestamp();
        _extractorsSeen = 0;
        _extractorsSinceStage = 0;
        _stageBasePercentBefore = 10;
    }

    public void OnStageStarted(PipelineStage stage)
    {
        _extractorsSinceStage = 0;
        var (text, pct) = stage switch
        {
            PipelineStage.DiscoveryAndCacheWarmup => ("Discovering files", 10.0),
            PipelineStage.GenericExtraction => ("Extracting structure", 25.0),
            PipelineStage.SignalSealing => ("Sealing signals", 35.0),
            PipelineStage.SpecificExtraction => ("Deep analysis", 50.0),
            PipelineStage.Scoring => ("Scoring", 70.0),
            PipelineStage.Compression => ("Compressing", 80.0),
            PipelineStage.Rendering => ("Rendering output", 90.0),
            _ => (stage.ToString(), 0.0),
        };
        _stageBasePercentBefore = (int)pct;
        ReportThrottled(text, pct, $"{text}…");
    }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        var (_, pct) = stage switch
        {
            PipelineStage.DiscoveryAndCacheWarmup => ("", 24.0),
            PipelineStage.GenericExtraction => ("", 34.0),
            PipelineStage.SignalSealing => ("", 49.0),
            PipelineStage.SpecificExtraction => ("", 69.0),
            PipelineStage.Scoring => ("", 79.0),
            PipelineStage.Compression => ("", 89.0),
            PipelineStage.Rendering => ("", 99.0),
            _ => ("", _stageBasePercentBefore + 1),
        };
        ReportThrottled(stage.ToString(), pct, $"Completed {stage}");
    }

    public void OnExtractorStarted(string name, ExtractorTier tier) { }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
    {
        _extractorsSeen++;
        _extractorsSinceStage++;
        // Interpolate within the current stage: each completed extractor nudges
        // the percent forward a few points within the stage's [base, nextBase) range.
        // After ~10 extractors the stage is approximately at its upper bound.
        var interpPct = Math.Min(_stageBasePercentBefore + _extractorsSinceStage * 1.5, _stageBasePercentBefore + 14);
        var label = skipped
            ? $"Skipped {name}"
            : typesAdded > 0
                ? $"{name} — {typesAdded} types"
                : name;
        ReportThrottled(name, interpPct, label);
    }

    public void OnPipelineCompleted(DiscoveryModel model)
        => ReportThrottled("Done", 100, "Analysis complete", force: true);

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals) { }
    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter) { }
    public void OnCompressionApplied(CompressionResult result) { }
    public void OnRenderCompleted(RenderedContext result) { }
    public void OnDiagnostic(DiagnosticEntry entry) { }

    private void ReportThrottled(string stage, double pct, string msg, bool force = false)
    {
        if (progress is null) return;
        var now = Stopwatch.GetTimestamp();
        if (!force && now - _lastReportTicks < MinInterval.Ticks)
            return;
        _lastReportTicks = now;
        progress.Report(new AnalysisProgress(stage, pct, msg));
    }
}
