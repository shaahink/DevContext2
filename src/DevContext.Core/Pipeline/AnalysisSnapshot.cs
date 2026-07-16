using System.Collections.Immutable;

using DevContext.Core.Analysis;
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

    /// <summary>T4.1 — when the analysis ran (UTC). Stamped by the pipeline; null on hand-built snapshots.</summary>
    public DateTimeOffset? AnalyzedAtUtc { get; init; }
    /// <summary>T4.1 — git HEAD sha of the analyzed repo at analyze time. Null when not a git checkout.</summary>
    public string? GitHead { get; init; }
    /// <summary>T4.5 — analyze-time fingerprints of every node-bearing source file (absolute path →
    /// sha256 + line count). verify_context compares these against disk to flag stale pack sections.</summary>
    public ImmutableDictionary<string, FileFingerprint> FileFingerprints { get; init; }
        = ImmutableDictionary<string, FileFingerprint>.Empty;

    /// <summary>Connected code graph assembled at analyze-time (PLAN-10). Null on dry-run.</summary>
    public CodeGraph? Graph { get; init; }
    /// <summary>Orientation map derived from the graph (PLAN-10). Null on dry-run.</summary>
    public MapModel? Map { get; init; }
    /// <summary>Entry-point inventory — the roots a Trace can start from (PLAN-10).</summary>
    public ImmutableArray<EntryPoint> Entries { get; init; } = [];
    /// <summary>Ranked, capped insights computed after GraphAssembly (I3).</summary>
    public ImmutableArray<Insight> Insights { get; init; } = [];
}
