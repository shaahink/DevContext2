namespace DevContext.Core.Contracts;

public sealed record RenderOptions(
    bool IncludeProvenance,
    bool IncludeDiagnostics,
    int EstimatedTokens
);

public sealed record RenderedContext(
    string Content,
    int EstimatedTokens,
    IReadOnlyList<CompressionResult> AppliedCompressions,
    TimeSpan ElapsedTotal,
    string SchemaVersion
);

public interface IContextRenderer
{
    string Format { get; }
    ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct);
}
