using System.Collections.Concurrent;
using System.Diagnostics;

using DevContext.Core.Contracts;

namespace DevContext.Core.Observers;

/// <summary>Collects structured run statistics from the observer callback stream. Thread-safe for parallel stages.</summary>
public sealed class RunReportCollector : IDiscoveryObserver
{
    private readonly Stopwatch _totalSw = Stopwatch.StartNew();
    private readonly ConcurrentBag<ExtractorStat> _extractorRows = [];
    private readonly ConcurrentBag<StageStat> _stageRows = [];
    private readonly ConcurrentBag<ScorerStat> _scorerRows = [];
    private readonly ConcurrentBag<CompressionStat> _compressionRows = [];
    private string _currentStage = "";
    private string _currentTier = "";
    private string _currentCategory = "";
    private int _stageOrdinal;

    private CacheStats _cacheStats = new(0, 0, 0, 0);
    private CorpusStats _corpusStats = new(0, 0, 0);
    private int _budget;

    public void OnPipelineStarted(DiscoveryContext context) { }

    public void OnStageStarted(PipelineStage stage)
    {
        _currentStage = stage.ToString();
    }

    public void OnExtractorStarted(string name, ExtractorTier tier)
    {
        _currentTier = tier.ToString();
        _currentCategory = ""; // reset; set by RecordExtractorMetrics
    }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
    {
        _extractorRows.Add(new ExtractorStat(name,
            skipped ? "" : _currentTier,
            skipped ? "" : _currentCategory,
            _currentStage,
            elapsed, typesAdded, detectionsAdded, skipped, skipReason));
    }

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals) { }

    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter)
    {
        _scorerRows.Add(new ScorerStat(name, itemsBefore, itemsAfter));
    }

    public void OnCompressionApplied(CompressionResult result)
    {
        _compressionRows.Add(new CompressionStat(result.StrategyName,
            result.TokensBefore, result.TokensAfter,
            result.TokensBefore - result.TokensAfter));
    }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        var ordinal = Interlocked.Increment(ref _stageOrdinal);
        _stageRows.Add(new StageStat(stage.ToString(), elapsed, ordinal));
    }

    public void OnRenderCompleted(RenderedContext result) { }

    public void OnPipelineCompleted(DiscoveryModel model)
    {
        _totalSw.Stop();

        // Analysis-time funnel: discovered + hard-excluded counts only.
        // Render-time funnel (from plan) is set on RenderedContext.RenderFunnel.
    }

    public void OnDiagnostic(DiagnosticEntry entry) { }

    public void RecordExtractorMetrics(string name, ExtractorTier tier, ExtractorCategory category,
        TimeSpan elapsed, bool skipped, int typesAdded, int detectionsAdded)
    {
        _currentCategory = category.ToString();
    }

    public void SetCacheStats(CacheStats stats) => _cacheStats = stats;

    public void SetCorpusFileCounts(int totalFiles, int csharpFiles)
        => _corpusStats = _corpusStats with { TotalFiles = totalFiles, CSharpFiles = csharpFiles };

    public void SetBudget(int budget) => _budget = budget;

    /// <summary>Returns the budget value for funnel reporting.</summary>
    public int Budget => _budget;

    /// <summary>Builds the final immutable RunReport from accumulated data.</summary>
    public RunReport Build()
    {
        var rows = _extractorRows.ToImmutableArray();

        var stage2Cpu = TimeSpan.FromMilliseconds(
            rows.Where(r => string.Equals(r.Stage, "GenericExtraction", StringComparison.Ordinal) && !r.Skipped).Sum(r => r.Elapsed.TotalMilliseconds));
        var stage3Cpu = TimeSpan.FromMilliseconds(
            rows.Where(r => string.Equals(r.Stage, "SpecificExtraction", StringComparison.Ordinal) && !r.Skipped).Sum(r => r.Elapsed.TotalMilliseconds));

        var stage2Wall = _stageRows
            .Where(s => string.Equals(s.Stage, "GenericExtraction", StringComparison.Ordinal)).Select(s => s.Elapsed).FirstOrDefault();
        var stage3Wall = _stageRows
            .Where(s => string.Equals(s.Stage, "SpecificExtraction", StringComparison.Ordinal)).Select(s => s.Elapsed).FirstOrDefault();

        return new RunReport
        {
            Stages = _stageRows.OrderBy(s => s.Ordinal).Select(s => new StageStat(s.Stage, s.Elapsed, s.Ordinal)).ToImmutableArray(),
            Extractors = rows.OrderByDescending(e => e.Elapsed).ThenBy(e => e.Name, StringComparer.Ordinal).ToImmutableArray(),
            Scorers = _scorerRows.ToImmutableArray(),
            Compressions = _compressionRows.ToImmutableArray(),
            Cache = _cacheStats,
            Corpus = _corpusStats,
            Funnel = new TokenFunnel(0, 0, 0, 0, 0, _budget),
            Parallelism = new ParallelismStats(stage2Wall, stage2Cpu, stage3Wall, stage3Cpu),
            TotalWall = _totalSw.Elapsed,
        };
    }
}
