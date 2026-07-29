namespace DevContext.Server.Sessions;

/// <summary>The one component that knows how to drive <c>DevContext.Core</c>: resolve the root,
/// resolve intent, build options, stand up the engine's per-repo DI container, and analyze. Keeping
/// this knowledge in a single place is what stops engine wiring from leaking into the transport layer
/// (and from drifting away from the CLI's canonical recipe).</summary>
public interface IEngineRunner
{
    Task<EngineResult> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct);
}

/// <summary>The product of one analysis: the immutable snapshot plus the live pipeline that produced
/// it (kept so cheap re-renders use the same path-bound resolvers), plus display metadata. The DI
/// container that owns the pipeline lives in <see cref="EngineHostCache"/> — the session must NOT
/// dispose it; <see cref="EngineHostCache"/> tears everything down on app shutdown.</summary>
public sealed record EngineResult(
    AnalysisSnapshot Snapshot,
    DiscoveryPipeline Pipeline,
    string Label,
    int ProjectCount,
    long ElapsedMs,
    string Explanation,
    ImmutableArray<string> Warnings,
    string? GitClonePath,
    string? Cleanup = null,
    bool Stale = false,
    string? StaleMessage = null,
    /// <summary>R4 item 7 — this result was rehydrated from the snapshot cache; no analysis ran.
    /// The runner has always had two such return paths and neither said so, which is why
    /// <c>analyze</c> could sit for 2ms or 8 minutes and report the same thing.</summary>
    bool FromSnapshotCache = false,
    /// <summary>R4 item 10 — when the analysis these numbers describe actually finished. For a
    /// rehydrate that is the instant persisted in the snapshot file, NOT the instant of the call
    /// that rehydrated it: <see cref="ElapsedMs"/> on that path measures the 200ms load, so without
    /// this field every cache-served answer reads as freshly computed. Default = unknown.</summary>
    DateTime AnalyzedAtUtc = default,
    /// <summary>R4 item 10 — the commit these numbers describe (<c>git rev-parse HEAD</c> of the
    /// analysed root). The snapshot cache already KEYS on this, so on a hit it is a guarantee and
    /// not a guess; it was simply computed and thrown away. Null when the tree is not a git repo.</summary>
    string? GitHead = null);
