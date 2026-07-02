using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Insights;

namespace DevContext.Core.Pipeline;

/// <summary>Immutable result of the analyze phase. The model must not be mutated after this is created.</summary>
public sealed record AnalysisSnapshot
{
    public required DiscoveryModel Model { get; init; }
    public required SharedAnalysisContext Analysis { get; init; }
    public required Scenario Scenario { get; init; }
    public required ExtractionOptions Options { get; init; }
    public required RunReport Report { get; init; }
    /// <summary>The root path the analysis was run against. Used by front-ends to re-acquire a pipeline for rendering.</summary>
    public string RootPath { get; init; } = "";
    public bool IsDryRun { get; init; }
    public string? DryRunContent { get; init; }
    public string Explanation { get; init; } = "";
    public ImmutableArray<string> Warnings { get; init; } = [];

    /// <summary>Connected code graph assembled at analyze-time (PLAN-10). Null on dry-run.</summary>
    public CodeGraph? Graph { get; init; }
    /// <summary>Orientation map derived from the graph (PLAN-10). Null on dry-run.</summary>
    public MapModel? Map { get; init; }
    /// <summary>Entry-point inventory — the roots a Trace can start from (PLAN-10).</summary>
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    /// <summary>Ranked, capped insights computed after GraphAssembly (I3).</summary>
    public ImmutableArray<Insight> Insights { get; init; } = [];
}
