namespace DevContext.Core.Graph;

/// <summary>The kind of application entry point — the universal abstraction the Map and Trace start from.</summary>
public enum EntryPointKind
{
    HttpEndpoint, MessageConsumer, HostedService, ScheduledJob, DomainEventHandler, PublicApi,
    UiEntry, BlazorPage, GrpcService, SignalRHub, FunctionEntry, GrainMethod,
}

/// <summary>
/// A place execution can enter the system. <see cref="Node"/> is where a <see cref="Trace"/> begins.
/// Built by GraphBuilder from EndpointDetection / MessageConsumerDetection / hosted services / (for
/// libraries) public API. There is no scoring here — the inventory is the inventory.
/// </summary>
public sealed record EntryPoint(
    EntryPointKind Kind,
    string Title,
    NodeId Node)
{
    /// <summary>HTTP verb, for <see cref="EntryPointKind.HttpEndpoint"/>.</summary>
    public string? HttpMethod { get; init; }
    /// <summary>Route template, for <see cref="EntryPointKind.HttpEndpoint"/>.</summary>
    public string? Route { get; init; }
    /// <summary>Declaring "file:line".</summary>
    public string? Provenance { get; init; }
    /// <summary>Owning project/service.</summary>
    public string? Project { get; init; }
    /// <summary>What this entry dispatches to — the command/request it sends, or the handler class it
    /// invokes. Resolved from the graph after assembly. Null when ambiguous (e.g. a minimal-API
    /// registration class that dispatches several commands). Surfaced as "route → Target" in the Map
    /// and the desktop entry picker so both convey the wiring at a glance.</summary>
    public string? Target { get; init; }
    /// <summary>The graph node (Type or Member) that this entry's Calls edge points to. Set during
    /// graph construction so EnrichEntryTargets can resolve targets without scanning by name.</summary>
    public NodeId? HandlerNode { get; init; }
}
