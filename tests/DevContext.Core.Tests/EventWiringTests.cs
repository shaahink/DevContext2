using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;

namespace DevContext.Core.Tests;

/// <summary>T2.6 — the single publisher→event→consumer projection, and the guarantee that the event board,
/// the one-pager cross-service line, and flow markers all render from it (never three disagreeing joins).</summary>
public sealed class EventWiringTests
{
    // ── graph fixture helpers ────────────────────────────────────────────────────────────────────
    private static GraphNode Svc(string name) => new(NodeId.ForService(name), name, NodeKind.Service);
    private static GraphNode EventType(string fqn, params string[] tags) =>
        new(NodeId.ForType(fqn), Short(fqn), NodeKind.Type) { Tags = [.. tags] };
    private static GraphNode Member(string fqn, string project) =>
        new(NodeId.ForType(fqn), Short(fqn), NodeKind.Member) { Project = project, FilePath = $@"C:\repo\{project}\x.cs" };
    private static string Short(string fqn) => fqn.Contains('.') ? fqn[(fqn.LastIndexOf('.') + 1)..] : fqn;

    private static GraphEdge Raises(NodeId from, NodeId to, string project) =>
        new(from, to, EdgeKind.Raises) { Provenance = $@"C:\repo\{project}\x.cs:10" };
    private static GraphEdge Consumes(NodeId evt, NodeId handler, string project) =>
        new(evt, handler, EdgeKind.Consumes) { Provenance = $@"C:\repo\{project}\y.cs:5" };

    /// <summary>An eShop-shaped bus: the same integration event is DECLARED TWICE — a publisher copy in
    /// the Ordering.API namespace and a consumer copy in the Basket.API namespace — so the two never share
    /// a node id. The projection must still join them into one wire on the short type name.</summary>
    private static CodeGraph DuplicatedEventBus()
    {
        var b = new CodeGraphBuilder();
        b.AddNode(Svc("Ordering.API"));
        b.AddNode(Svc("Basket.API"));

        var publisher = Member("eShop.Ordering.API.Application.OrderingService.Publish", "Ordering.API");
        var pubEvent = EventType("eShop.Ordering.API.IntegrationEvents.OrderStartedIntegrationEvent", RoleTags.IntegrationEvent);
        var conEvent = EventType("eShop.Basket.API.IntegrationEvents.OrderStartedIntegrationEvent", RoleTags.IntegrationEvent);
        var consumer = Member("eShop.Basket.API.IntegrationEvents.OrderStartedHandler", "Basket.API");

        b.AddNode(publisher); b.AddNode(pubEvent); b.AddNode(conEvent); b.AddNode(consumer);
        b.AddEdge(Raises(publisher.Id, pubEvent.Id, "Ordering.API"));
        b.AddEdge(Consumes(conEvent.Id, consumer.Id, "Basket.API"));
        return b.Build();
    }

    [Fact]
    public void Projection_joins_a_duplicated_event_by_short_name()
    {
        var wiring = EventWiringProjection.Build(DuplicatedEventBus());

        var wire = Assert.Single(wiring);
        Assert.Equal("OrderStartedIntegrationEvent", wire.EventName);
        Assert.True(wire.IsIntegration);
        Assert.True(wire.IsCrossService);
        Assert.Equal("Ordering.API", Assert.Single(wire.Publishers).Service);
        Assert.Equal("Basket.API", Assert.Single(wire.Consumers).Service);
        Assert.Equal(("Ordering.API", "Basket.API"), Assert.Single(wire.CrossServicePairs));
    }

    [Fact]
    public void Board_one_pager_and_flow_render_from_the_same_projection()
    {
        // Build the projection, store it and emit its ServiceLinks — exactly what GraphBuilder does.
        var seam = DuplicatedEventBus();
        var b = new CodeGraphBuilder();
        foreach (var n in seam.Nodes) b.AddNode(n);
        foreach (var e in seam.AllEdges) b.AddEdge(e);
        var wiring = EventWiringProjection.Build(seam);
        b.SetEventWiring(wiring);
        EventWiringProjection.EmitServiceLinks(b, wiring);
        var graph = b.Build();

        // (a) the event board reads graph.EventWiring
        var board = new EventFlowSource().Compute(new DiscoveryModel(), graph, []).ToList();
        var boardInsight = Assert.Single(board);
        Assert.Contains("1 cross-service", boardInsight.Title);

        // (b/c) the one-pager cross-service line and flow markers read ServiceLink(bus) edges, which are
        // emitted from the very same projection — so all three agree on the cross-service reach.
        var busLinks = graph.AllEdges
            .Where(e => e.Kind == EdgeKind.ServiceLink && e.Tags.Contains(ServiceLinkTags.BusPublishConsume))
            .ToList();
        var projectionPairs = wiring.SelectMany(w => w.CrossServicePairs).Distinct().ToList();

        Assert.Equal(projectionPairs.Count, busLinks.Count);
        Assert.Equal(wiring.Count(w => w.IsCrossService), busLinks.Count);
        Assert.Contains(busLinks, e => e.From == NodeId.ForService("Ordering.API") && e.To == NodeId.ForService("Basket.API"));
        // The flow marker points at the publishing member (audit A10c), not an unrelated node.
        Assert.Contains(busLinks, e => e.Provenance is { } p && p.Contains("Ordering.API") && p.Contains("OrderStartedIntegrationEvent"));
    }

    [Fact]
    public void Domain_event_across_layer_projects_is_not_cross_service()
    {
        // eShop's Ordering.Domain raises a domain event that Ordering.API handles — a PROJECT hop inside the
        // one Ordering service, not a bus link. It must not become a cross-service ServiceLink.
        var b = new CodeGraphBuilder();
        b.AddNode(Svc("Ordering.Domain"));
        b.AddNode(Svc("Ordering.API"));
        var raiser = Member("eShop.Ordering.Domain.Order.SetPaid", "Ordering.Domain");
        var evt = EventType("eShop.Ordering.Domain.Events.OrderPaidDomainEvent", RoleTags.DomainEvent);
        var handler = Member("eShop.Ordering.API.Handlers.OrderPaidHandler", "Ordering.API");
        b.AddNode(raiser); b.AddNode(evt); b.AddNode(handler);
        b.AddEdge(Raises(raiser.Id, evt.Id, "Ordering.Domain"));
        b.AddEdge(Consumes(evt.Id, handler.Id, "Ordering.API"));

        var wiring = EventWiringProjection.Build(b.Build());
        var wire = Assert.Single(wiring);
        Assert.False(wire.IsIntegration);
        Assert.False(wire.IsCrossService);   // domain event → in-process, never a bus hop

        EventWiringProjection.EmitServiceLinks(b, wiring);
        Assert.DoesNotContain(b.Build().AllEdges, e => e.Kind == EdgeKind.ServiceLink);
    }

    [Fact]
    public void Same_service_event_is_not_cross_service()
    {
        var b = new CodeGraphBuilder();
        b.AddNode(Svc("Catalog.API"));
        var pub = Member("Catalog.API.PublishSvc.Emit", "Catalog.API");
        var evt = EventType("Catalog.API.Events.PriceChangedIntegrationEvent", RoleTags.IntegrationEvent);
        var con = Member("Catalog.API.Handlers.PriceHandler", "Catalog.API");
        b.AddNode(pub); b.AddNode(evt); b.AddNode(con);
        b.AddEdge(Raises(pub.Id, evt.Id, "Catalog.API"));
        b.AddEdge(Consumes(evt.Id, con.Id, "Catalog.API"));

        var wire = Assert.Single(EventWiringProjection.Build(b.Build()));
        Assert.True(wire.IsIntegration);
        Assert.False(wire.IsCrossService);   // same service on both ends
    }

    [Fact]
    public void Base_marker_types_are_not_events()
    {
        var b = new CodeGraphBuilder();
        b.AddNode(Svc("EventBusRabbitMQ"));
        var pub = Member("eShop.EventBusRabbitMQ.RabbitMQEventBus.PublishAsync", "EventBusRabbitMQ");
        var marker = EventType("eShop.EventBus.Events.IntegrationEvent", RoleTags.IntegrationEvent);
        b.AddNode(pub); b.AddNode(marker);
        b.AddEdge(Raises(pub.Id, marker.Id, "EventBusRabbitMQ"));

        Assert.Empty(EventWiringProjection.Build(b.Build()));   // "IntegrationEvent" base class is not a wire
    }

    [Fact]
    public void Orphan_event_is_published_with_no_consumer()
    {
        var b = new CodeGraphBuilder();
        b.AddNode(Svc("Notifications.API"));
        var pub = Member("Notifications.API.Svc.Emit", "Notifications.API");
        var evt = EventType("Notifications.API.Events.WidgetPokedIntegrationEvent", RoleTags.IntegrationEvent);
        b.AddNode(pub); b.AddNode(evt);
        b.AddEdge(Raises(pub.Id, evt.Id, "Notifications.API"));

        var wire = Assert.Single(EventWiringProjection.Build(b.Build()));
        Assert.True(wire.IsOrphan);
        Assert.False(wire.IsCrossService);
    }
}
