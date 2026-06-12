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
    /// <summary>Scoring stage.</summary>
    Scoring,
    /// <summary>Compression stage.</summary>
    Compression,
    /// <summary>Rendering stage.</summary>
    Rendering
}

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
    /// <summary>Sequential: Specific extractors gated by sealed architecture signals.</summary>
    Stage3Sequential
}
