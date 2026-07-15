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

/// <summary>Builds the single event-wiring projection from an assembled graph. Pure over the graph's
/// Raises/Consumes edges — no re-parsing, no second detection pass — so the board, one-pager, and flow
/// surfaces cannot disagree.</summary>
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

    private static string ShortName(NodeId id)
    {
        var key = id.Key;
        var dot = key.LastIndexOf('.');
        return dot >= 0 && dot < key.Length - 1 ? key[(dot + 1)..] : key;
    }
}
