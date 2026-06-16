namespace DevContext.Core.Graph;

/// <summary>The kind of application entry point — the universal abstraction the Map and Trace start from.</summary>
public enum EntryPointKind
{
    HttpEndpoint, MessageConsumer, HostedService, ScheduledJob, DomainEventHandler, PublicApi,
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
}
