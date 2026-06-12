using System.Collections.Concurrent;
using System.Diagnostics;

using DevContext.Core.Contracts;

namespace DevContext.Core.Models;

/// <summary>Collects structured run statistics from the observer callback stream. Thread-safe for parallel stages.</summary>
public sealed class RunReportCollector : IDiscoveryObserver
{
    private readonly Stopwatch _totalSw = Stopwatch.StartNew();
    private readonly ConcurrentBag<ExtractorStat> _extractorRows = [];
    private readonly ConcurrentBag<StageStat> _stageRows = [];
    private readonly ConcurrentBag<ScorerStat> _scorerRows = [];
    private readonly ConcurrentBag<CompressionStat> _compressionRows = [];
    private string _currentStage = "";

    private TimeSpan _stage2Wall;
    private TimeSpan _stage2CpuSum;
    private TimeSpan _stage3Wall;
    private TimeSpan _stage3CpuSum;

    private CacheStats _cacheStats = new(0, 0, 0, 0);
    private CorpusStats _corpusStats = new(0, 0, 0);
    private TokenFunnel _funnel = new(0, 0, 0, 0, 0, 0);
    private int _budget;

    public void OnPipelineStarted(DiscoveryContext context) { }

    public void OnStageStarted(PipelineStage stage)
    {
        _currentStage = stage.ToString();
    }

    public void OnExtractorStarted(string name, ExtractorTier tier) { }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
    {
        _extractorRows.Add(new ExtractorStat(name,
            skipped ? "" : "extractor",
            skipped ? "" : "",
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
            result.TokensBefore - result.TokensAfter));
    }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        _stageRows.Add(new StageStat(stage.ToString(), elapsed));

        if (stage == PipelineStage.GenericExtraction) _stage2Wall = elapsed;
        if (stage == PipelineStage.SpecificExtraction) _stage3Wall = elapsed;
    }

    public void OnRenderCompleted(RenderedContext result) { }

    public void OnPipelineCompleted(DiscoveryModel model)
    {
        _totalSw.Stop();

        _corpusStats = new CorpusStats(0, 0, model.Projects.Length);

        var total = model.Types.Count;
        var hardExcluded = model.Types.Values.Count(t => t.IsHardExcluded);
        _funnel = new TokenFunnel(total, hardExcluded, total - hardExcluded,
            0, 0, _budget);
    }

    public void OnDiagnostic(DiagnosticEntry entry) { }

    public void RecordExtractorMetrics(string name, ExtractorTier tier, ExtractorCategory category,
        TimeSpan elapsed, bool skipped, int typesAdded, int detectionsAdded)
    {
    }

    /// <summary>Sets cache stats after the pipeline reads them from AnalysisCache.</summary>
    public void SetCacheStats(CacheStats stats) => _cacheStats = stats;

    /// <summary>Sets corpus file counts from SharedAnalysisContext.</summary>
    public void SetCorpusFileCounts(int totalFiles, int csharpFiles)
        => _corpusStats = _corpusStats with { TotalFiles = totalFiles, CSharpFiles = csharpFiles };

    /// <summary>Sets the budget value for funnel reporting.</summary>
    public void SetBudget(int budget) => _budget = budget;

    /// <summary>Adds extractor CPU time to the parallel speedup accumulators.</summary>
    public void AccumulateCpuTime(string stage, TimeSpan elapsed)
    {
        if (stage == "GenericExtraction") _stage2CpuSum += elapsed;
        if (stage == "SpecificExtraction") _stage3CpuSum += elapsed;
    }

    /// <summary>Builds the final immutable RunReport from accumulated data.</summary>
    public RunReport Build()
    {
        return new RunReport
        {
            Stages = _stageRows.OrderBy(s => s.Stage).ToImmutableArray(),
            Extractors = _extractorRows.OrderByDescending(e => e.Elapsed).ThenBy(e => e.Name).ToImmutableArray(),
            Scorers = _scorerRows.ToImmutableArray(),
            Compressions = _compressionRows.ToImmutableArray(),
            Cache = _cacheStats,
            Corpus = _corpusStats,
            Funnel = _funnel,
            Parallelism = new ParallelismStats(_stage2Wall, _stage2CpuSum, _stage3Wall, _stage3CpuSum),
            TotalWall = _totalSw.Elapsed,
        };
    }
}
