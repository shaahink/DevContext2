namespace DevContext.Core.Models;

/// <summary>Structured summary of a single analysis run, produced by the pipeline and available in every output.</summary>
public sealed record RunReport
{
    public required ImmutableArray<StageStat> Stages { get; init; }
    public required ImmutableArray<ExtractorStat> Extractors { get; init; }
    public required ImmutableArray<ScorerStat> Scorers { get; init; }
    public required ImmutableArray<CompressionStat> Compressions { get; init; }
    public required CacheStats Cache { get; init; }
    public required CorpusStats Corpus { get; init; }
    public required TokenFunnel Funnel { get; init; }
    public required ParallelismStats Parallelism { get; init; }
    public required TimeSpan TotalWall { get; init; }
}

public sealed record StageStat(string Stage, TimeSpan Elapsed, int Ordinal = 0);
public sealed record ExtractorStat(string Name, string Tier, string Category, string Stage,
    TimeSpan Elapsed, int TypesAdded, int DetectionsAdded, bool Skipped, string? SkipReason);
public sealed record ScorerStat(string Name, int TypesBefore, int TypesAfter);
public sealed record CompressionStat(string Name, int TokensSaved);
public sealed record CacheStats(int TextHits, int TextMisses, int SyntaxTreeHits, int SyntaxTreeMisses);
public sealed record CorpusStats(int TotalFiles, int CSharpFiles, int Projects);
public sealed record TokenFunnel(int TypesDiscovered, int TypesHardExcluded, int TypesIncluded,
    int RawEstimatedTokens, int RenderedEstimatedTokens, int Budget);
/// <summary>Graph-shaped stats for the Map/Trace narrative, where the type-funnel ("N types kept")
/// is meaningless. <see cref="TraceDepth"/> is null for a Map (whole-codebase, no walk).</summary>
public sealed record GraphSummary(int Nodes, int Edges, int Entries, int? TraceDepth);
public sealed record ParallelismStats(TimeSpan Stage2Wall, TimeSpan Stage2CpuSum,
    TimeSpan Stage3Wall, TimeSpan Stage3CpuSum);
