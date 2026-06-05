using System.Collections.Concurrent;
using System.Diagnostics;

namespace DevContext.Core.Observers;

public sealed class MetricsDiscoveryObserver : IDiscoveryObserver
{
    private readonly Stopwatch _pipelineSw = new();
    private readonly ConcurrentDictionary<string, ExtractorMetrics> _extractorMetrics = new();
    private readonly ConcurrentBag<string> _eventLog = new();
    private PipelineStage _currentStage;

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
        _eventLog.Add($"PipelineStarted:{context.RootPath}");
    }

    public void OnStageStarted(PipelineStage stage)
    {
        _currentStage = stage;
        _eventLog.Add($"StageStarted:{stage}");
    }

    public void OnExtractorStarted(string name, ExtractorTier tier) { }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason)
    {
        _eventLog.Add($"ExtractorCompleted:{name}:{elapsed.TotalMilliseconds:F0}ms");
    }

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals)
    {
        var detected = signals.Values.Where(s => s.Detected).Select(s => $"{s.Key}({s.Confidence:P0})");
        _eventLog.Add($"SignalsSealed:{string.Join(",", detected)}");
    }

    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter)
    {
        _eventLog.Add($"PrunerCompleted:{name}:{itemsBefore}->{itemsAfter}");
    }

    public void OnCompressionApplied(CompressionResult result)
    {
        _eventLog.Add($"CompressionApplied:{result.StrategyName}:{result.TokensBefore}->{result.TokensAfter}");
    }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        _eventLog.Add($"StageCompleted:{stage}:{elapsed.TotalMilliseconds:F0}ms");
    }

    public void OnRenderCompleted(RenderedContext result)
    {
        _eventLog.Add($"RenderCompleted:{result.EstimatedTokens}tokens:{result.ElapsedTotal.TotalMilliseconds:F0}ms");
    }

    public void OnPipelineCompleted(DiscoveryModel model)
    {
        _pipelineSw.Stop();
        _eventLog.Add($"PipelineCompleted:{model.Types.Count}types:{model.Detections.Count}detections");
    }

    public void OnDiagnostic(DiagnosticEntry entry)
    {
        _eventLog.Add($"Diagnostic:{entry.Level}:{entry.Source}:{entry.Message}");
    }

    public string GetMetricsSummary()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## Metrics Report");
        sb.AppendLine();
        sb.AppendLine($"**Total time**: {_pipelineSw.Elapsed.TotalMilliseconds:F0}ms");
        sb.AppendLine();
        sb.AppendLine("### Extractors");
        sb.AppendLine("| Name | Tier | Category | Time |");
        sb.AppendLine("|------|------|----------|------|");
        foreach (var line in _eventLog.Where(e => e.StartsWith("ExtractorCompleted:")))
        {
            var parts = line.Replace("ExtractorCompleted:", "").Split(':');
            if (parts.Length >= 2)
                sb.AppendLine($"| {parts[0]} | | | {parts[1]} |");
        }
        sb.AppendLine();
        sb.AppendLine("### Pipeline Events");
        foreach (var evt in _eventLog)
            sb.AppendLine($"- {evt}");

        return sb.ToString();
    }
}
