namespace DevContext.Core.Graph;

/// <summary>One participant on an event wire — the member (publisher) or type (consumer handler)
/// node that raises or handles the event, together with its owning service and the file:line of the
/// seam.</summary>
public sealed record EventParticipant(NodeId Node, string Title, string? Service, string? Provenance);

/// <summary>T2.6 — one publisher→event→consumer wire. The <see cref="EventWiringProjection"/> builds these
/// ONCE from the graph's <see cref="EdgeKind.Raises"/> (publisher → event) and <see cref="EdgeKind.Consumes"/>
/// (event → consumer) seams and stores them on the <see cref="CodeGraph"/>, so the Atlas event board, the
/// report Event Wiring section, and flow cross-service markers all render from the same numbers instead of
/// three independent joins. Join key is the event's SHORT type name: eShop (and most microservice repos)
/// declare each integration event twice — a publisher copy and a consumer copy in different namespaces — so
/// the two never share a node id; keying on the short name within the model's detected event set is the join,
/// not arbitrary substring text (the T2.6 anti-overfit trap).</summary>
public sealed record EventWire(
    string EventName,
    NodeId EventNode,
    bool IsIntegration,
    ImmutableArray<EventParticipant> Publishers,
    ImmutableArray<EventParticipant> Consumers)
{
    /// <summary>Distinct (publisherService → consumerService) hops where the two services differ —
    /// the cross-service reach of this event. Derived once at projection time.</summary>
    public ImmutableArray<(string From, string To)> CrossServicePairs { get; init; } = [];

    /// <summary>True when this event is a cross-SERVICE integration hop. Only integration events count:
    /// a domain event handled in a sibling layer project of the same service (eShop's
    /// <c>Ordering.Domain → Ordering.API</c>) crosses a PROJECT boundary but not a service boundary, so
    /// it is in-process wiring, not a bus link.</summary>
    public bool IsCrossService => IsIntegration && !CrossServicePairs.IsDefaultOrEmpty && CrossServicePairs.Length > 0;

    /// <summary>True when the event is published but has no in-repo consumer.</summary>
    public bool IsOrphan => Consumers.IsDefaultOrEmpty || Consumers.Length == 0;
}

/// <summary>F4 (backlog #35) — one in-repo transport port (a queue/bus/outbox interface) whose callers
/// split into write-verb sites (producers) and read-verb sites (consumers). Both sides call INTO the
/// port — a call through a DI interface lands on the interface Type because its methods have no bodies —
/// so without a join the port is a sink (in-degree N, out-degree 0) and no walk can route through it.
/// The wire is the verb-classified evidence <see cref="EmitPortBridges"/> turns into joined edges.</summary>
public sealed record TransportPortWire(
    NodeId Port,
    string PortName,
    ImmutableArray<EventParticipant> Producers,
    ImmutableArray<EventParticipant> Consumers);

/// <summary>Builds the single event-wiring projection from an assembled graph. Pure over the graph's
/// Raises/Consumes edges — no re-parsing, no second detection pass — so the board, one-pager, and flow
/// surfaces cannot disagree. Also home to the TRANSPORT-PORT half of the same join
/// (<see cref="BuildTransportPorts"/>/<see cref="EmitPortBridges"/>): event wiring has exactly one
/// join, and a queue port bridged by verb evidence is that join over Calls edges instead of
/// Raises/Consumes — never a second ad-hoc pass elsewhere.</summary>
public static class EventWiringProjection
{
    private sealed class Accumulator
    {
        public NodeId EventNode;
        public bool IsIntegration;
        public readonly List<EventParticipant> Publishers = [];
        public readonly List<EventParticipant> Consumers = [];
    }

    /// <summary>Short names that are base marker types, not concrete events — never a wire of their own.</summary>
    private static readonly HashSet<string> MarkerTypes = new(StringComparer.Ordinal)
    {
        "IntegrationEvent", "DomainEvent", "BaseEntity", "Event",
        "INotification", "INotificationHandler", "IEvent", "IDomainEvent", "IIntegrationEvent",
    };

    /// <summary>Joins publishers (Raises in-edges) and consumers (Consumes out-edges) of every event node
    /// by short type name, one <see cref="EventWire"/> per event. <paramref name="projectForFile"/> resolves
    /// a participant's owning service from the seam's provenance file (the authoritative source — a member
    /// node created by another pass may lack <see cref="GraphNode.Project"/>); <paramref name="isProduction"/>
    /// drops test-project publishers/consumers. Both are optional so a synthetic graph (with Project stamped
    /// on nodes) projects without a solution scope.</summary>
    public static ImmutableArray<EventWire> Build(
        CodeGraph graph,
        Func<string, string?>? projectForFile = null,
        Func<string, bool>? isProduction = null)
    {
        var byName = new Dictionary<string, Accumulator>(StringComparer.Ordinal);

        foreach (var edge in graph.AllEdges)
        {
            if (edge.Kind == EdgeKind.Raises)
            {
                var eventNode = graph.Node(edge.To);
                if (eventNode is null || IsMarker(eventNode)) continue;
                var pub = graph.Node(edge.From);
                if (pub is null) continue;
                if (!IsProductionSeam(edge, pub, isProduction)) continue;
                Acc(byName, eventNode).Publishers.Add(Participant(pub, edge, projectForFile));
            }
            else if (edge.Kind == EdgeKind.Consumes)
            {
                var eventNode = graph.Node(edge.From);
                if (eventNode is null || IsMarker(eventNode)) continue;
                var con = graph.Node(edge.To);
                if (con is null) continue;
                if (!IsProductionSeam(edge, con, isProduction)) continue;
                Acc(byName, eventNode).Consumers.Add(Participant(con, edge, projectForFile));
            }
        }

        var wires = ImmutableArray.CreateBuilder<EventWire>(byName.Count);
        foreach (var (_, acc) in byName.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var publishers = Dedup(acc.Publishers);
            var consumers = Dedup(acc.Consumers);
            wires.Add(new EventWire(
                ShortName(acc.EventNode), acc.EventNode, acc.IsIntegration, publishers, consumers)
            {
                CrossServicePairs = CrossServicePairs(publishers, consumers),
            });
        }
        return wires.ToImmutable();
    }

    /// <summary>Emits the cross-service bus <see cref="EdgeKind.ServiceLink"/> edges from the projection —
    /// the single successor to the old project-name join (<c>AddBusServiceLinks</c>). Provenance anchors on
    /// the publishing member so a flow's cross-service marker points at the code that raised the event, not
    /// an unrelated node. Idempotent: the builder dedups nodes and (from,to,kind) edges.</summary>
    public static void EmitServiceLinks(CodeGraphBuilder g, ImmutableArray<EventWire> wiring)
    {
        if (wiring.IsDefaultOrEmpty) return;
        foreach (var wire in wiring)
        {
            if (!wire.IsCrossService) continue; // integration events only — domain events are in-process
            foreach (var (from, to) in wire.CrossServicePairs)
            {
                var fromId = NodeId.ForService(from);
                var toId = NodeId.ForService(to);
                g.AddNode(new GraphNode(fromId, from, NodeKind.Service));
                g.AddNode(new GraphNode(toId, to, NodeKind.Service));

                var publisherSite = wire.Publishers
                    .FirstOrDefault(p => string.Equals(p.Service, from, StringComparison.OrdinalIgnoreCase))
                    ?.Provenance;
                var provenance = publisherSite is { Length: > 0 }
                    ? $"{publisherSite} raises {wire.EventName}"
                    : $"{from}→{to}:{wire.EventName}";

                g.AddEdge(new GraphEdge(fromId, toId, EdgeKind.ServiceLink)
                {
                    Provenance = provenance,
                    Resolution = Resolution.Join,
                    Confidence = 0.8f,
                    Tags = [ServiceLinkTags.BusPublishConsume],
                });
            }
        }
    }

    // ── F4 (backlog #35): the transport-port half of the join ────────────────────────────────────
    //
    // Book2Course's Q4: BuildCoordinator.AdvanceAsync enqueues into IJobQueue and JobRunner.RunNextAsync
    // dequeues from it — the connection is in the graph, fully verified, and seam() answered
    // "unconnected" because both sides point INTO the port. The verb evidence needed to bridge it was
    // already on GraphEdge.TargetMember; this join reads it. Verb tables are transport verbs ONLY
    // (enqueue/dequeue families) — deliberately NOT store verbs (Add/Get/Save/Read/Write): a repository
    // written by an uploader and read by a later stage is staging, not transport, and bridging it would
    // fabricate a path the repo does not have (the drive's Q4 FIRST case, which was correctly
    // found:false, stays found:false). Kin tables: EventBusExtractor's raw-transport queue verbs and
    // DispatchSeamCatalog's bus receiver verbs.
    //
    // MEASURED COUPLING TO #33 (carried openly): on Book2Course today the WRITER edge's TargetMember
    // is "ConfigureAwait", not "EnqueueAsync" — `await queue.EnqueueAsync(..).ConfigureAwait(false)`
    // lets the chained BCL call win the (from, to, kind) dedupe, the #33 extension/BCL-member defect
    // wearing a different hat. Awaiter plumbing matches no stem, so the polluted edge classifies as
    // nothing and the port stays unbridged — the join degrades to the honest miss, never a wrong
    // bridge. When #33's declares-gate lands, the verb evidence comes through clean and the wire
    // forms; the Book2Course re-measure runs on the integrated tree in that order by design.

    /// <summary>Method-name stems that WRITE into a transport port. Matched at a PascalCase word
    /// boundary ("EnqueueAsync", "PushMany" — never "Poster" or "Sender").</summary>
    private static readonly ImmutableArray<string> PortWriteStems =
        ["Enqueue", "Push", "Publish", "Send", "Produce", "Emit", "Post", "Schedule"];

    /// <summary>Method-name stems that READ from a transport port. "Claim"/"Lease" are the dequeue of
    /// lease-based job queues (Book2Course's <c>IJobQueue.ClaimAsync</c> — F4's measured case).
    /// Lifecycle verbs (Complete/Fail/Renew/Cancel) are deliberately absent: settling a lease is not
    /// receiving work, and an unclassifiable caller must never mint a bridge.</summary>
    private static readonly ImmutableArray<string> PortReadStems =
        ["Dequeue", "Pop", "Receive", "Consume", "Take", "Poll", "Pull", "Peek", "Lease", "Claim"];

    /// <summary>Classifies every in-repo port the graph holds BOTH sides of: callers that write into it
    /// and callers that read from it, split by the verb on <see cref="GraphEdge.TargetMember"/>. Only
    /// Calls edges from a member onto an in-scope TYPE participate (the DI-interface call shape). A
    /// caller type with calls in BOTH directions is the transport's own plumbing and is dropped from
    /// both sides — the same rule EventBusExtractor.EmitQueueSeams applies to raw queue transports. A
    /// port left with only writers or only readers is NOT a wire: nothing may be bridged that the
    /// graph does not hold both ends of.</summary>
    public static ImmutableArray<TransportPortWire> BuildTransportPorts(
        CodeGraph graph,
        Func<string, string?>? projectForFile = null,
        Func<string, bool>? isProduction = null)
    {
        var writes = new Dictionary<NodeId, List<EventParticipant>>();
        var reads = new Dictionary<NodeId, List<EventParticipant>>();
        var writerTypes = new Dictionary<NodeId, HashSet<string>>();
        var readerTypes = new Dictionary<NodeId, HashSet<string>>();

        foreach (var edge in graph.AllEdges)
        {
            if (edge.Kind != EdgeKind.Calls) continue;
            if (edge.From.Kind != NodeKind.Member || edge.To.Kind != NodeKind.Type) continue;
            if (edge.TargetMember is not { Length: > 0 } verb) continue;

            // In-repo ports only: a node with no declaring file is an external leaf, and a type
            // wearing store/entity tags is data, not transport.
            var port = graph.Node(edge.To);
            if (port?.FilePath is not { Length: > 0 }) continue;
            if (port.Tags.Contains(RoleTags.Entity) || port.Tags.Contains(RoleTags.Aggregate)
                || port.Tags.Contains(RoleTags.DataStore)) continue;

            var caller = graph.Node(edge.From);
            if (caller is null) continue;
            if (!IsProductionSeam(edge, caller, isProduction)) continue;

            var direction = ClassifyPortVerb(verb);
            if (direction == 0) continue;

            var side = direction > 0 ? writes : reads;
            if (!side.TryGetValue(edge.To, out var list)) side[edge.To] = list = [];
            list.Add(Participant(caller, edge, projectForFile));

            var types = direction > 0 ? writerTypes : readerTypes;
            if (!types.TryGetValue(edge.To, out var set)) types[edge.To] = set = new(StringComparer.Ordinal);
            set.Add(Graph2.SymbolCanon.OwnerTypeOf(edge.From.Key));
        }

        var wires = ImmutableArray.CreateBuilder<TransportPortWire>();
        foreach (var (portId, writers) in writes.OrderBy(kv => kv.Key.Key, StringComparer.Ordinal))
        {
            if (!reads.TryGetValue(portId, out var readers)) continue;

            // EmitQueueSeams' rule, mirrored: a type on BOTH sides of the same port is the transport's
            // implementation (a drain loop, a decorator, an in-memory bus) — never a bridge endpoint.
            var infra = new HashSet<string>(writerTypes[portId], StringComparer.Ordinal);
            infra.IntersectWith(readerTypes[portId]);

            var producers = Dedup([.. writers.Where(p => !infra.Contains(Graph2.SymbolCanon.OwnerTypeOf(p.Node.Key)))]);
            var consumers = Dedup([.. readers.Where(p => !infra.Contains(Graph2.SymbolCanon.OwnerTypeOf(p.Node.Key)))]);
            if (producers.IsEmpty || consumers.IsEmpty) continue;

            wires.Add(new TransportPortWire(portId, ShortName(portId), producers, consumers));
        }
        return wires.ToImmutable();
    }

    /// <summary>Emits the bridge each wire earns: port → consumer-member <see cref="EdgeKind.Consumes"/>
    /// edges, so a walk can route producer → port → consumer. The hop is a JOIN — <see
    /// cref="Resolution.Join"/>, confidence 0.8, tagged <see cref="RoleTags.TransportPortBridge"/> —
    /// because the builder classified it from verb evidence; it must never render as verified.
    /// Provenance anchors on the consumer's read call site, the line where delivery happens.
    /// Idempotent: the builder dedups (from, to, kind) edges.</summary>
    public static void EmitPortBridges(CodeGraphBuilder g, ImmutableArray<TransportPortWire> ports)
    {
        if (ports.IsDefaultOrEmpty) return;
        foreach (var wire in ports)
        {
            foreach (var consumer in wire.Consumers)
            {
                g.AddEdge(new GraphEdge(wire.Port, consumer.Node, EdgeKind.Consumes)
                {
                    Provenance = consumer.Provenance,
                    Resolution = Resolution.Join,
                    Confidence = 0.8f,
                    Tags = [RoleTags.TransportPortBridge],
                });
            }
        }
    }

    /// <summary>+1 write, -1 read, 0 neither/both. "Try"/"Begin" wrappers are transparent
    /// ("TryDequeue" reads); a name matching neither table — or, defensively, both — does not
    /// participate, so an unclassifiable caller can never mint a bridge.</summary>
    private static int ClassifyPortVerb(string memberName)
    {
        var name = StripVerbPrefix(memberName, "Try");
        name = StripVerbPrefix(name, "Begin");
        var w = MatchesAnyStem(name, PortWriteStems);
        var r = MatchesAnyStem(name, PortReadStems);
        return w == r ? 0 : w ? 1 : -1;
    }

    private static string StripVerbPrefix(string name, string prefix)
        => name.Length > prefix.Length && name.StartsWith(prefix, StringComparison.Ordinal)
            && char.IsUpper(name[prefix.Length])
            ? name[prefix.Length..] : name;

    private static bool MatchesAnyStem(string name, ImmutableArray<string> stems)
    {
        foreach (var stem in stems)
        {
            if (name.StartsWith(stem, StringComparison.Ordinal)
                && (name.Length == stem.Length || char.IsUpper(name[stem.Length])))
                return true;
        }
        return false;
    }

    private static Accumulator Acc(Dictionary<string, Accumulator> map, GraphNode eventNode)
    {
        var key = ShortName(eventNode.Id);
        if (!map.TryGetValue(key, out var acc))
        {
            map[key] = acc = new Accumulator { EventNode = eventNode.Id };
        }
        // Prefer an integration-event node's id as the wire's representative when available.
        if (IsIntegrationEvent(eventNode) && !acc.IsIntegration)
            acc.EventNode = eventNode.Id;
        acc.IsIntegration |= IsIntegrationEvent(eventNode);
        return acc;
    }

    private static ImmutableArray<(string From, string To)> CrossServicePairs(
        ImmutableArray<EventParticipant> publishers, ImmutableArray<EventParticipant> consumers)
    {
        var pairs = new List<(string From, string To)>();
        var seen = new HashSet<(string, string)>();
        foreach (var p in publishers)
        {
            if (p.Service is not { Length: > 0 } pub) continue;
            foreach (var c in consumers)
            {
                if (c.Service is not { Length: > 0 } con) continue;
                if (string.Equals(pub, con, StringComparison.OrdinalIgnoreCase)) continue;
                if (seen.Add((pub, con))) pairs.Add((pub, con));
            }
        }
        return [.. pairs];
    }

    private static ImmutableArray<EventParticipant> Dedup(List<EventParticipant> list)
    {
        var seen = new HashSet<NodeId>();
        var b = ImmutableArray.CreateBuilder<EventParticipant>();
        foreach (var p in list)
            if (seen.Add(p.Node)) b.Add(p);
        return b.ToImmutable();
    }

    private static EventParticipant Participant(GraphNode node, GraphEdge edge, Func<string, string?>? projectForFile)
    {
        // Prefer resolving the service from the seam's provenance file — a member node created by an
        // earlier pass (call graph, entry builder) can be missing its Project, but the edge always carries
        // the file it fired in. Fall back to the node's own Project for synthetic (scope-less) graphs.
        string? service = null;
        if (projectForFile is not null && FileFromProvenance(edge.Provenance) is { } file)
            service = projectForFile(file);
        service ??= node.Project is { Length: > 0 } p ? p : null;
        return new EventParticipant(node.Id, node.Title, service, edge.Provenance);
    }

    private static bool IsProductionSeam(GraphEdge edge, GraphNode node, Func<string, bool>? isProduction)
    {
        if (isProduction is null) return true;
        var file = FileFromProvenance(edge.Provenance) ?? node.FilePath;
        return file is null || isProduction(file);
    }

    private static string? FileFromProvenance(string? provenance)
    {
        if (string.IsNullOrEmpty(provenance)) return null;
        // "C:\path\File.cs:88" — the last ':' before an all-digit tail is the line separator.
        var colon = provenance.LastIndexOf(':');
        if (colon <= 1) return provenance; // no line suffix (or a bare drive) — treat whole thing as the file
        var tail = provenance[(colon + 1)..];
        return tail.Length > 0 && tail.All(char.IsDigit) ? provenance[..colon] : provenance;
    }

    private static bool IsMarker(GraphNode node) => MarkerTypes.Contains(ShortName(node.Id));

    private static bool IsIntegrationEvent(GraphNode node)
        => (!node.Tags.IsDefaultOrEmpty && node.Tags.Contains(RoleTags.IntegrationEvent))
        || node.Title.EndsWith("IntegrationEvent", StringComparison.Ordinal);

    // Batch A: arity-aware — a generic event type's key carries `N, which must not leak into the
    // event-board display or break the MarkerTypes filter.
    private static string ShortName(NodeId id) => Graph2.SymbolCanon.ShortNameOf(id.Key);
}
