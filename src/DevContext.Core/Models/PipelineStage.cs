namespace DevContext.Core.Models;

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
