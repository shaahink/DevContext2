using DevContext.Core.Models;

namespace DevContext.Core.Graph;

/// <summary>One step on a flow's spine — the ordered primary path from an entry through dispatch
/// handlers to leaf operations. The spine is flat (no branching); <see cref="Flow.Touches"/> and
/// <see cref="Flow.Emits"/> collect the data/event interactions of each spine member.</summary>
public sealed record FlowStep(
    NodeId Node,
    EdgeKind? Via,           // null for the first (entry) step; the edge kind that led here for others
    Resolution Tier,
    string? Provenance)
{
    /// <summary>Short title of the node at this step, for rendering convenience.</summary>
    public string? Title { get; init; }
}

/// <summary>A cross-service transition on a flow's spine — recorded when a <see cref="EdgeKind.ServiceLink"/>
/// edge is crossed during spine construction.</summary>
public sealed record ServiceHop(
    string? FromService,
    string? ToService,
    string? Transport,
    string? Evidence);

/// <summary>An entry-rooted flow: the spine a reader walks to understand what happens when this
/// entry fires. Computed once at graph assembly, stored on <see cref="CodeGraph"/>, consumed by
/// projections, MCP tools, and UI surfaces (design §1.4). Fix baked in: <see cref="Touches"/> comes
/// only from spine-member <see cref="EdgeKind.ReadsWrites"/> edges — no EntityRelation reachability
/// (audit E5).</summary>
public sealed record Flow(
    string Id,
    EntryPoint Entry,
    ImmutableArray<FlowStep> Steps)
{
    /// <summary>Entity/store IDs touched by spine members' ReadsWrites edges (spine-only — audit E5 fix).</summary>
    public ImmutableArray<NodeId> Touches { get; init; } = [];

    /// <summary>Event IDs emitted by spine members' Raises edges.</summary>
    public ImmutableArray<NodeId> Emits { get; init; } = [];

    /// <summary>Cross-service transitions encountered on the spine's ServiceLink edges.</summary>
    public ImmutableArray<ServiceHop> Hops { get; init; } = [];

    /// <summary>True when the spine depth budget was exhausted (maxSpineDepth=24 by default).
    /// Signals to consumers that the flow may be incomplete — the real dispatch path could be longer
    /// than what was captured.</summary>
    public bool IsTruncated { get; init; }
}
