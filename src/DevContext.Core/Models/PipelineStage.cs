namespace DevContext.Core.Models;

/// <summary>Defines the stages of the discovery pipeline execution lifecycle.</summary>
public enum PipelineStage
{
    ProjectRootResolution,
    DiscoveryAndCacheWarmup,
    GenericExtraction,
    SignalSealing,
    SpecificExtraction,
    Pruning,
    Compression,
    Rendering
}
