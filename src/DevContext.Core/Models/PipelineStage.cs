namespace DevContext.Core.Models;

/// <summary>Defines the sequential stages of the discovery pipeline used for observer notifications.</summary>
public enum PipelineStage
{
    /// <summary>Project root resolution phase.</summary>
    ProjectRootResolution,
    /// <summary>File tree discovery + cache warmup.</summary>
    DiscoveryAndCacheWarmup,
    /// <summary>Generic (stage 2) extraction.</summary>
    GenericExtraction,
    /// <summary>Signal sealing point.</summary>
    SignalSealing,
    /// <summary>Specific (stage 3) extraction.</summary>
    SpecificExtraction,
    /// <summary>Tier-B semantic-lite population + call-edge upgrade (T7.3 — was invisible in the waterfall).</summary>
    SemanticLite,
    /// <summary>Graph assembly: gateway routes + CodeGraph + Map build (T7.3 — was invisible in the waterfall).</summary>
    GraphAssembly,
    /// <summary>Post-graph insight computation (T7.3 — was invisible in the waterfall).</summary>
    Insights,
    /// <summary>Scoring stage.</summary>
    Scoring,
    /// <summary>Compression stage.</summary>
    Compression,
    /// <summary>Snapshot finalization: file fingerprints + snapshot assembly (T7.3 — ran after the wall clock stopped).</summary>
    Snapshot,
    /// <summary>Rendering stage.</summary>
    Rendering
}

/// <summary>One aggregated silent-failure row (J1): a component swallowed <paramref name="Count"/>
/// failures of one category during a run; <paramref name="SampleException"/> is the first seen
/// ("TypeName: message", truncated). Surfaced by stats + the analyze waterfall (J3).</summary>
public sealed record SwallowedFailure(string Source, string Category, int Count, string? SampleException);

/// <summary>
/// Defines the execution stage within the pipeline for an extractor.
/// Stage 1 runs sequentially (file tree, solution, project structure — builds foundational data).
/// Stage 2 runs in parallel (all other Generic extractors that consume Stage 1 data).
/// Stage 3 runs sequentially (Specific extractors gated by sealed signals).
/// ArchitectureStyle detection happens explicitly between Stage 2 and 3 (not an extractor).
/// Default (unspecified) is Stage2Parallel.
/// </summary>
public enum ExecutionStage
{
    /// <summary>Sequential: builds file tree, solution info, project structure. Must complete before Stage 2.</summary>
    Stage1Sequential,
    /// <summary>Parallel: all remaining Generic extractors consuming Stage 1 data.</summary>
    Stage2Parallel,
    /// <summary>Parallel: Specific extractors gated by sealed architecture signals.</summary>
    Stage3Specific
}
