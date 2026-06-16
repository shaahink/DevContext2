namespace DevContext.Core.Pipeline;

/// <summary>How much source detail a Trace carries at each step.</summary>
public enum TraceDetail { Signature, Salient, Full }

/// <summary>What the user wants to see right now. Cheap to construct, cheap to apply.</summary>
public sealed record RenderRequest
{
    public required string Format { get; init; }
    public required int MaxTokens { get; init; }
    public ImmutableArray<string> Sections { get; init; } = [];
    public bool IncludeProvenance { get; init; }
    public bool IncludeDiagnostics { get; init; }
    public bool TokenView { get; init; }
    /// <summary>Entry point to trace from (e.g. "POST /api/orders"). When null and Graph is available, renders the Map.</summary>
    public string? Entry { get; init; }
    /// <summary>Maximum trace depth. Defaults to 6 in TraceBuilder.</summary>
    public int? Depth { get; init; }
    /// <summary>Trace detail level — controls body inclusion per step.</summary>
    public TraceDetail Detail { get; init; } = TraceDetail.Salient;
}
