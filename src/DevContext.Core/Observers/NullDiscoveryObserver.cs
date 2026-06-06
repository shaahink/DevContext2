namespace DevContext.Core.Observers;

/// <summary>No-op implementation of <see cref="IDiscoveryObserver"/> that ignores all events.</summary>
public sealed class NullDiscoveryObserver : IDiscoveryObserver
{
    public void OnPipelineStarted(DiscoveryContext context) { }
    public void OnStageStarted(PipelineStage stage) { }
    public void OnExtractorStarted(string name, ExtractorTier tier) { }
    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
                                      int typesAdded = 0, int detectionsAdded = 0)
    { }
    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals) { }
    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter) { }
    public void OnCompressionApplied(CompressionResult result) { }
    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed) { }
    public void OnRenderCompleted(RenderedContext result) { }
    public void OnPipelineCompleted(DiscoveryModel model) { }
    public void OnDiagnostic(DiagnosticEntry entry) { }
}
