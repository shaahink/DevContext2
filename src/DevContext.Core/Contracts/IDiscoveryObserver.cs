namespace DevContext.Core.Contracts;

/// <summary>Observer interface for receiving lifecycle events from the discovery pipeline.</summary>
public interface IDiscoveryObserver
{
    /// <summary>Called when the pipeline starts execution.</summary>
    void OnPipelineStarted(DiscoveryContext context);
    /// <summary>Called when a pipeline stage begins.</summary>
    void OnStageStarted(PipelineStage stage);
    /// <summary>Called when an individual extractor starts.</summary>
    void OnExtractorStarted(string name, ExtractorTier tier);
    /// <summary>Called when an extractor completes (or is skipped).</summary>
    void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0);
    /// <summary>Called when architecture signals are sealed (no more writes allowed).</summary>
    void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals);
    /// <summary>Called when a pruner finishes processing.</summary>
    void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter);
    /// <summary>Called when a compression strategy is applied.</summary>
    void OnCompressionApplied(CompressionResult result);
    /// <summary>Called when a pipeline stage completes.</summary>
    void OnStageCompleted(PipelineStage stage, TimeSpan elapsed);
    /// <summary>Called when rendering is complete.</summary>
    void OnRenderCompleted(RenderedContext result);
    /// <summary>Called when the entire pipeline completes.</summary>
    void OnPipelineCompleted(DiscoveryModel model);
    /// <summary>Called when a diagnostic entry is recorded.</summary>
    void OnDiagnostic(DiagnosticEntry entry);
}
