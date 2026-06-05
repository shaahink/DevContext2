namespace DevContext.Core.Observers;

public sealed class CompositeDiscoveryObserver : IDiscoveryObserver
{
    private readonly IDiscoveryObserver[] _inner;

    public CompositeDiscoveryObserver(params IDiscoveryObserver[] observers)
    {
        _inner = observers;
    }

    public void OnPipelineStarted(DiscoveryContext context)
    { foreach (var o in _inner) o.OnPipelineStarted(context); }

    public void OnStageStarted(PipelineStage stage)
    { foreach (var o in _inner) o.OnStageStarted(stage); }

    public void OnExtractorStarted(string name, ExtractorTier tier)
    { foreach (var o in _inner) o.OnExtractorStarted(name, tier); }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason)
    { foreach (var o in _inner) o.OnExtractorCompleted(name, elapsed, skipped, skipReason); }

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals)
    { foreach (var o in _inner) o.OnSignalsSealed(signals); }

    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter)
    { foreach (var o in _inner) o.OnPrunerCompleted(name, itemsBefore, itemsAfter); }

    public void OnCompressionApplied(CompressionResult result)
    { foreach (var o in _inner) o.OnCompressionApplied(result); }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    { foreach (var o in _inner) o.OnStageCompleted(stage, elapsed); }

    public void OnRenderCompleted(RenderedContext result)
    { foreach (var o in _inner) o.OnRenderCompleted(result); }

    public void OnPipelineCompleted(DiscoveryModel model)
    { foreach (var o in _inner) o.OnPipelineCompleted(model); }

    public void OnDiagnostic(DiagnosticEntry entry)
    { foreach (var o in _inner) o.OnDiagnostic(entry); }
}
