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
    string? Cleanup = null);
