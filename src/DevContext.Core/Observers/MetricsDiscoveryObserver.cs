using System.Collections.Concurrent;
using System.Diagnostics;

namespace DevContext.Core.Observers;

public sealed class MetricsDiscoveryObserver : IDiscoveryObserver
{
    private readonly Stopwatch _pipelineSw = new();
    private readonly ConcurrentDictionary<string, ExtractorMetrics> _extractorMetrics = new();
    private readonly ConcurrentQueue<string> _eventLog = new();
    private PipelineStage _currentStage;
    private int _lastTypeCount;
    private int _lastDetCount;

    public IReadOnlyDictionary<string, ExtractorMetrics> Metrics => _extractorMetrics;
    public IReadOnlyCollection<string> EventLog => _eventLog;
    public TimeSpan TotalElapsed => _pipelineSw.Elapsed;

    public sealed record ExtractorMetrics(
        string Name,
        ExtractorTier Tier,
        ExtractorCategory Category,
        TimeSpan Elapsed,
        bool Skipped,
        int TypesBefore,
        int TypesAfter,
        int DetectionsBefore,
        int DetectionsAfter
    );

    public void OnPipelineStarted(DiscoveryContext context)
    {
        _pipelineSw.Start();
        _lastTypeCount = 0;
        _lastDetCount = 0;
        _eventLog.Enqueue($"PipelineStarted:{context.RootPath}");
    }

    public void OnStageStarted(PipelineStage stage)
    {
        _currentStage = stage;
        _eventLog.Enqueue($"StageStarted:{stage}");
    }

    public void OnExtractorStarted(string name, ExtractorTier tier) { }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
    {
        _eventLog.Enqueue($"ExtractorCompleted:{name}:{elapsed.TotalMilliseconds:F0}ms:+{typesAdded}t+{detectionsAdded}d");
    }

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals)
    {
        var detected = signals.Values.Where(s => s.Detected).Select(s => $"{s.Key}({s.Confidence:P0})");
        _eventLog.Enqueue($"SignalsSealed:{string.Join(",", detected)}");
    }

    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter)
    {
        _eventLog.Enqueue($"PrunerCompleted:{name}:{itemsBefore}->{itemsAfter}");
    }

    public void OnCompressionApplied(CompressionResult result)
    {
        _eventLog.Enqueue($"CompressionApplied:{result.StrategyName}:{result.TokensBefore}->{result.TokensAfter}");
    }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        _eventLog.Enqueue($"StageCompleted:{stage}:{elapsed.TotalMilliseconds:F0}ms");
    }

    public void OnRenderCompleted(RenderedContext result)
    {
        _eventLog.Enqueue($"RenderCompleted:{result.EstimatedTokens}tokens:{result.ElapsedTotal.TotalMilliseconds:F0}ms");
    }

    public void OnPipelineCompleted(DiscoveryModel model)
    {
        _pipelineSw.Stop();
        _eventLog.Enqueue($"PipelineCompleted:{model.Types.Count}types:{model.Detections.Count}detections");
    }

    public void OnDiagnostic(DiagnosticEntry entry)
    {
        _eventLog.Enqueue($"Diagnostic:{entry.Level}:{entry.Source}:{entry.Message}");
    }

    /// <summary>Records extractor metrics from the pipeline after completion. Called by the pipeline directly.</summary>
    public void RecordExtractorMetrics(string name, ExtractorTier tier, ExtractorCategory category,
        TimeSpan elapsed, bool skipped, int typesAdded, int detectionsAdded)
    {
        var typesBefore = _lastTypeCount;
        var detsBefore = _lastDetCount;
        _lastTypeCount += typesAdded;
        _lastDetCount += detectionsAdded;

        _extractorMetrics[name] = new ExtractorMetrics(
            name, tier, category, elapsed, skipped,
            typesBefore, typesBefore + typesAdded,
            detsBefore, detsBefore + detectionsAdded);
    }

    public string GetMetricsSummary()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## Metrics Report");
        sb.AppendLine();
        sb.AppendLine($"**Total time**: {_pipelineSw.Elapsed.TotalMilliseconds:F0}ms");
        sb.AppendLine();

        if (_extractorMetrics.Count > 0)
        {
            sb.AppendLine("### Extractors");
            sb.AppendLine("| Name | Tier | Category | Time | Types+ | Dets+ |");
            sb.AppendLine("|------|------|----------|------|--------|-------|");
            foreach (var (name, m) in _extractorMetrics.OrderBy(kv => kv.Key))
            {
                var typeDelta = m.TypesAfter - m.TypesBefore;
                var detDelta = m.DetectionsAfter - m.DetectionsBefore;
                sb.AppendLine($"| {name} | {m.Tier} | {m.Category} | {m.Elapsed.TotalMilliseconds:F0}ms | +{typeDelta} | +{detDelta} |");
            }
        }

        sb.AppendLine();
        sb.AppendLine("### Pipeline Events");
        foreach (var evt in _eventLog)
            sb.AppendLine($"- {evt}");

        return sb.ToString();
    }
}
