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
    /// <summary>Maximum trace depth. Defaults to <see cref="Graph.TracePolicy.DefaultDepth"/>.</summary>
    public int? Depth { get; init; }
    /// <summary>Batch E (R2 §2.E item 1) — a trace the CALLER already built, to be rendered as-is.
    /// <para>The gRPC <c>GetTrace</c> returned a structured tree AND a markdown document, and built the
    /// trace twice to do it: once through <c>GraphQuery.Trace</c> for the tree, then again inside this
    /// render. On a hub focus that is two full walks per request — and worse, the two could DISAGREE,
    /// because the tree was shaped by the request's budgetTokens while the markdown was shaped by
    /// MaxTokens. One build, one shaping, one answer.</para></summary>
    public Graph.Trace? PrebuiltTrace { get; init; }
    /// <summary>Trace detail level — controls body inclusion per step.</summary>
    public TraceDetail Detail { get; init; } = TraceDetail.Salient;
    /// <summary>When tracing (Entry set), also render the Map/architecture sections alongside the trace,
    /// so the orientation view stays visible while drilling into a call stack. Off by default (the CLI
    /// keeps a focused trace-only output); the desktop turns it on.</summary>
    public bool IncludeMapWithTrace { get; init; }
}
