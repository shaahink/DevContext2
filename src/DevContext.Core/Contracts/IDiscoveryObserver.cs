namespace DevContext.Core.Contracts;

public interface IDiscoveryObserver
{
    void OnPipelineStarted(DiscoveryContext context);
    void OnStageStarted(PipelineStage stage);
    void OnExtractorStarted(string name, ExtractorTier tier);
    void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason);
    void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals);
    void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter);
    void OnCompressionApplied(CompressionResult result);
    void OnStageCompleted(PipelineStage stage, TimeSpan elapsed);
    void OnRenderCompleted(RenderedContext result);
    void OnPipelineCompleted(DiscoveryModel model);
    void OnDiagnostic(DiagnosticEntry entry);
}
