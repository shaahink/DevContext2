namespace DevContext.Server.Sessions;

/// <summary>Bridges the engine's <see cref="IDiscoveryObserver"/> callbacks to a coarse
/// <see cref="IProgress{T}"/> stream. The stage→percent mapping mirrors the desktop's observer so the
/// UX reads identically across faces. Every other callback is intentionally a no-op — the rich
/// telemetry (RunReport) is queried after analysis, not streamed.</summary>
internal sealed class StreamingProgressObserver(IProgress<AnalysisProgress>? progress) : IDiscoveryObserver
{
    public void OnStageStarted(PipelineStage stage)
    {
        var (text, pct) = stage switch
        {
            PipelineStage.DiscoveryAndCacheWarmup => ("Discovering files", 10.0),
            PipelineStage.GenericExtraction => ("Extracting structure", 25.0),
            PipelineStage.SignalSealing => ("Sealing signals", 35.0),
            PipelineStage.SpecificExtraction => ("Deep analysis", 50.0),
            PipelineStage.Scoring => ("Scoring", 70.0),
            PipelineStage.Compression => ("Compressing", 80.0),
            PipelineStage.Rendering => ("Rendering output", 90.0),
            _ => (stage.ToString(), 0.0),
        };
        progress?.Report(new AnalysisProgress(text, pct, $"{text}…"));
    }

    public void OnPipelineCompleted(DiscoveryModel model)
        => progress?.Report(new AnalysisProgress("Done", 100, "Analysis complete"));

    public void OnPipelineStarted(DiscoveryContext context) { }
    public void OnExtractorStarted(string name, ExtractorTier tier) { }
    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
    { }
    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals) { }
    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter) { }
    public void OnCompressionApplied(CompressionResult result) { }
    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed) { }
    public void OnRenderCompleted(RenderedContext result) { }
    public void OnDiagnostic(DiagnosticEntry entry) { }
}
