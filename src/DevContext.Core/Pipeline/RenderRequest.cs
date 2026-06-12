namespace DevContext.Core.Pipeline;

/// <summary>What the user wants to see right now. Cheap to construct, cheap to apply.</summary>
public sealed record RenderRequest
{
    public required string Format { get; init; }
    public required int MaxTokens { get; init; }
    public ImmutableArray<string> Sections { get; init; } = [];
    public bool IncludeProvenance { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool TokenView { get; init; }
}
