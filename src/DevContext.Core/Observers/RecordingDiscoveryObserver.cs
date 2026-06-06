using System.Collections.Concurrent;

namespace DevContext.Core.Observers;

/// <summary>Records all pipeline events and diagnostics into in-memory collections for test verification.</summary>
public sealed class RecordingDiscoveryObserver : IDiscoveryObserver
{
    /// <summary>List of event strings recorded during pipeline execution.</summary>
    public List<string> Events { get; } = [];
    /// <summary>Bag of diagnostic entries recorded during pipeline execution.</summary>
    public ConcurrentBag<DiagnosticEntry> Diagnostics { get; } = [];

    public void OnPipelineStarted(DiscoveryContext context)
        => Events.Add($"PipelineStarted:{context.RootPath}");

    public void OnStageStarted(PipelineStage stage)
        => Events.Add($"StageStarted:{stage}");

    public void OnExtractorStarted(string name, ExtractorTier tier)
        => Events.Add($"ExtractorStarted:{name}:{tier}");

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
        => Events.Add($"ExtractorCompleted:{name}:{elapsed.TotalMilliseconds:F0}ms:skipped={skipped}:t+={typesAdded}:d+={detectionsAdded}");

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
