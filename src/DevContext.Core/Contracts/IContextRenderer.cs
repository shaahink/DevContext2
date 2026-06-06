namespace DevContext.Core.Contracts;

/// <summary>Options that control how a renderer produces output.</summary>
public sealed record RenderOptions(
    bool IncludeProvenance,
    bool IncludeDiagnostics,
    int EstimatedTokens,
    string? ScenarioDisplayName = null,
    ImmutableArray<string> RequiredSections = default
);

/// <summary>The result of rendering a discovery model into a specific output format.</summary>
public sealed record RenderedContext(
    string Content,
    int EstimatedTokens,
    IReadOnlyList<CompressionResult> AppliedCompressions,
    TimeSpan ElapsedTotal,
    string SchemaVersion
);

/// <summary>Renders a discovery model into a specific output format (e.g. markdown, JSON).</summary>
public interface IContextRenderer
{
    /// <summary>Gets the format identifier (e.g. "markdown", "json").</summary>
    string Format { get; }
    /// <summary>Renders the model and returns the formatted output.</summary>
    ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct);
}
