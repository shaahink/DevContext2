using System.Collections.Concurrent;

namespace DevContext.Core.Utilities;

public sealed class RecordingDiscoveryObserver : IDiscoveryObserver
{
    public List<string> Events { get; } = [];
    public ConcurrentBag<DiagnosticEntry> Diagnostics { get; } = [];

    public void OnPipelineStarted(DiscoveryContext context)
        => Events.Add($"PipelineStarted:{context.RootPath}");

    public void OnStageStarted(PipelineStage stage)
        => Events.Add($"StageStarted:{stage}");

    public void OnExtractorStarted(string name, ExtractorTier tier)
        => Events.Add($"ExtractorStarted:{name}:{tier}");

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason)
        => Events.Add($"ExtractorCompleted:{name}:{elapsed.TotalMilliseconds:F0}ms:skipped={skipped}");

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals)
        => Events.Add($"SignalsSealed:{signals.Count} signals");

    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter)
        => Events.Add($"PrunerCompleted:{name}:{itemsBefore}->{itemsAfter}");

    public void OnCompressionApplied(CompressionResult result)
        => Events.Add($"CompressionApplied:{result.StrategyName}:{result.TokensBefore}->{result.TokensAfter}");

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
        => Events.Add($"StageCompleted:{stage}:{elapsed.TotalMilliseconds:F0}ms");

    public void OnRenderCompleted(RenderedContext result)
        => Events.Add($"RenderCompleted:{result.EstimatedTokens}tokens");

    public void OnPipelineCompleted(DiscoveryModel model)
        => Events.Add($"PipelineCompleted:{model.Types.Count}types");

    public void OnDiagnostic(DiagnosticEntry entry)
        => Diagnostics.Add(entry);
}
