using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>
/// L4.3 projection regression: the consumers (Home hero, Atlas, MCP overview/top_flows)
/// switch to reading these projections instead of ad-hoc walks. This pins the projection
/// contract so a consumer never has to re-derive service names, runnable filtering, or flow
/// ranking client-side.
/// </summary>
public sealed class GraphProjectionTests
{
    // ── Unit: ServiceMap only surfaces runnable Service nodes with FULL names ──

    [Fact]
    public void ServiceMap_only_service_nodes_full_names_no_libraries()
    {
        var g = new CodeGraphBuilder();
        // Two runnables (Service nodes) + a class library (Type node) that must NOT appear.
        g.AddNode(new GraphNode(NodeId.ForService("Basket.API"), "Basket.API", NodeKind.Service) { Project = "Basket.API", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(NodeId.ForService("Ordering.API"), "Ordering.API", NodeKind.Service) { Project = "Ordering.API", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(NodeId.ForType("BuildingBlocks.Messaging.Event"), "Event", NodeKind.Type)
        {
            Project = "BuildingBlocks",
            Layer = "Infrastructure",
        });
        var graph = g.Build();

        var result = new ServiceMapProjection().Project(graph, ProjectionOptions.Default);

        Assert.Equal(2, result.Services.Length);
        // Full names, not tail-truncated segments — the projection carries DisplayName so the
        // UI never does name.split('.').pop().
        Assert.Contains(result.Services, s => s.DisplayName == "Basket.API");
        Assert.Contains(result.Services, s => s.DisplayName == "Ordering.API");
        Assert.DoesNotContain(result.Services, s => s.DisplayName == "BuildingBlocks");
        // Every service DisplayName keeps its dotted qualifier (no truncation to "API").
        Assert.DoesNotContain(result.Services, s => s.DisplayName == "API");
    }

    [Fact]
    public void ServiceMap_excludes_non_runnable_library_service_nodes()
    {
        var g = new CodeGraphBuilder();
        // A runnable + a Service node synthesized for a library that only appears as a bus consumer
        // (no Runnable tag) — the library must NOT surface as a service card (audit Claim 3 / E3).
        g.AddNode(new GraphNode(NodeId.ForService("Basket.API"), "Basket.API", NodeKind.Service) { Project = "Basket.API", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(NodeId.ForService("Ordering.Application"), "Ordering.Application", NodeKind.Service) { Project = "Ordering.Application" });
        var graph = g.Build();

        var runnablesOnly = new ServiceMapProjection().Project(graph, ProjectionOptions.Default);
        Assert.Single(runnablesOnly.Services);
        Assert.Equal("Basket.API", runnablesOnly.Services[0].DisplayName);

        // IncludeLibraries opt-in surfaces both.
        var withLibs = new ServiceMapProjection().Project(graph, new ProjectionOptions { IncludeLibraries = true });
        Assert.Equal(2, withLibs.Services.Length);
    }

    [Fact]
    public void ServiceMap_transports_come_from_servicelink_edges()
    {
        var g = new CodeGraphBuilder();
        var from = NodeId.ForService("Basket.API");
        var to = NodeId.ForService("Ordering.API");
        g.AddNode(new GraphNode(from, "Basket.API", NodeKind.Service) { Project = "Basket.API" });
        g.AddNode(new GraphNode(to, "Ordering.API", NodeKind.Service) { Project = "Ordering.API" });
        g.AddEdge(new GraphEdge(from, to, EdgeKind.ServiceLink)
        {
            Tags = ["bus"],
            Provenance = "BasketCheckoutEvent",
        });
        var graph = g.Build();

        var result = new ServiceMapProjection().Project(graph, ProjectionOptions.Default);

        var link = Assert.Single(result.Transports);
        Assert.Equal("Basket.API", link.FromService);
        Assert.Equal("Ordering.API", link.ToService);
        Assert.Equal("bus", link.Transport);
        Assert.Equal("BasketCheckoutEvent", link.Evidence);
    }

    // ── R3 D-B: the service kind is evidence, not a layer name ──

    [Fact]
    public void ServiceMap_kind_comes_from_the_entry_surfaces_a_service_owns()
    {
        // The old classifier read the service node's Layer, which AddServiceNodes never sets — so
        // every service on every repo came back "Service" and every canvas kind glyph was empty.
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForService("Catalog.API"), "Catalog.API", NodeKind.Service) { Project = "Catalog.API", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(NodeId.ForService("Basket.API"), "Basket.API", NodeKind.Service) { Project = "Basket.API", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(NodeId.ForService("OrderProcessor"), "OrderProcessor", NodeKind.Service) { Project = "OrderProcessor", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(NodeId.ForService("WebApp"), "WebApp", NodeKind.Service) { Project = "WebApp", Tags = [RoleTags.Runnable] });
        g.SetEntries(
        [
            Entry(EntryPointKind.HttpEndpoint, "GET /api/catalog/items", "Catalog.API"),
            Entry(EntryPointKind.HttpEndpoint, "PUT /api/catalog/items", "Catalog.API"),
            Entry(EntryPointKind.GrpcService, "Basket.GetBasket", "Basket.API"),
            Entry(EntryPointKind.MessageConsumer, "OrderStartedConsumer", "OrderProcessor"),
            Entry(EntryPointKind.HostedService, "GracePeriodWorker", "OrderProcessor"),
            Entry(EntryPointKind.UiEntry, "Cart.razor", "WebApp"),
        ]);
        var graph = g.Build();

        var byName = new ServiceMapProjection().Project(graph, ProjectionOptions.Default)
            .Services.ToDictionary(s => s.DisplayName, s => s.Kind);

        Assert.Equal("Web API", byName["Catalog.API"]);
        Assert.Equal("gRPC", byName["Basket.API"]);
        Assert.Equal("Worker", byName["OrderProcessor"]);
        Assert.Equal("UI", byName["WebApp"]);
    }

    [Fact]
    public void ServiceMap_kind_ignores_domain_event_handlers_and_falls_back_honestly()
    {
        // A domain reaction is something a service does internally, not something it offers: three of
        // them must not outvote the one endpoint the service exists to serve. And a runnable that
        // owns no nameable surface stays "Service" rather than being guessed into a kind.
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForService("Ordering.API"), "Ordering.API", NodeKind.Service) { Project = "Ordering.API", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(NodeId.ForService("Silent"), "Silent", NodeKind.Service) { Project = "Silent", Tags = [RoleTags.Runnable] });
        g.SetEntries(
        [
            Entry(EntryPointKind.DomainEventHandler, "OrderStartedHandler", "Ordering.API"),
            Entry(EntryPointKind.DomainEventHandler, "OrderPaidHandler", "Ordering.API"),
            Entry(EntryPointKind.DomainEventHandler, "OrderShippedHandler", "Ordering.API"),
            Entry(EntryPointKind.HttpEndpoint, "POST /api/orders/", "Ordering.API"),
        ]);
        var graph = g.Build();

        var byName = new ServiceMapProjection().Project(graph, ProjectionOptions.Default)
            .Services.ToDictionary(s => s.DisplayName, s => s.Kind);

        Assert.Equal("Web API", byName["Ordering.API"]);
        Assert.Equal("Service", byName["Silent"]);
    }

    [Fact]
    public void ServiceMap_carries_the_stores_a_service_depends_on()
    {
        // Batch B has emitted these Store nodes since the Aspire topology landed; until D-B nothing
        // carried them out of the graph, so no renderer could draw a repo's stores.
        var g = new CodeGraphBuilder();
        var svc = NodeId.ForService("Basket.API");
        var redis = NodeId.ForStore("basketdb");
        g.AddNode(new GraphNode(svc, "Basket.API", NodeKind.Service) { Project = "Basket.API", Tags = [RoleTags.Runnable] });
        g.AddNode(new GraphNode(redis, "basketdb", NodeKind.Store) { Tags = [RoleTags.DataStore] });
        g.AddEdge(new GraphEdge(svc, redis, EdgeKind.DependsOn) { Tags = ["redis"] });
        var graph = g.Build();

        var card = Assert.Single(new ServiceMapProjection().Project(graph, ProjectionOptions.Default).Services);
        var store = Assert.Single(card.Stores);
        Assert.Equal("basketdb", store.Name);
        Assert.Equal("redis", store.ResourceType);
    }

    [Fact]
    public void ServiceMap_transport_carries_the_resolution_tier()
    {
        // Without this the topology canvas cannot make the verified/inferred distinction the trace
        // tree has drawn since T6.2 — a deployment-derived guess looked as certain as a verified call.
        var g = new CodeGraphBuilder();
        var from = NodeId.ForService("WebApp");
        var to = NodeId.ForService("Catalog.API");
        g.AddNode(new GraphNode(from, "WebApp", NodeKind.Service) { Project = "WebApp" });
        g.AddNode(new GraphNode(to, "Catalog.API", NodeKind.Service) { Project = "Catalog.API" });
        g.AddEdge(new GraphEdge(from, to, EdgeKind.ServiceLink) { Tags = ["http-direct"], Resolution = Resolution.Semantic });
        var graph = g.Build();

        var link = Assert.Single(new ServiceMapProjection().Project(graph, ProjectionOptions.Default).Transports);
        Assert.Equal(Resolution.Semantic, link.Resolution);
    }

    private static EntryPoint Entry(EntryPointKind kind, string title, string project)
        => new(kind, title, NodeId.ForEntry(title)) { Project = project };

    [Fact]
    public void FlowList_ranks_by_score_and_caps_at_max_flows()
    {
        var g = new CodeGraphBuilder();
        var e1 = NodeId.ForEntry("GET /a");
        var e2 = NodeId.ForEntry("GET /b");
        var e3 = NodeId.ForEntry("GET /c");
        g.AddNode(new GraphNode(e1, "GET /a", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(e2, "GET /b", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(e3, "GET /c", NodeKind.EntryPoint));
        g.SetFlows(
        [
            MakeFlow("GET /a", e1, score: 1.0),
            MakeFlow("GET /b", e2, score: 3.0),
            MakeFlow("GET /c", e3, score: 2.0),
        ]);
        var graph = g.Build();

        var result = new FlowListProjection().Project(graph, new ProjectionOptions { MaxFlows = 2 });

        Assert.Equal(3, result.TotalFlows);
        Assert.Equal(2, result.Flows.Length);
        Assert.Equal("GET /b", result.Flows[0].Title); // highest score first
        Assert.Equal("GET /c", result.Flows[1].Title);
    }

    private static Flow MakeFlow(string title, NodeId node, double score)
    {
        var entry = new EntryPoint(EntryPointKind.HttpEndpoint, title, node) { Score = score };
        return new Flow(title, entry, []);
    }

    // ── Edge cases (L4 audit) ────────────────────────────────────────────

    [Fact]
    public void ServiceMap_empty_graph_returns_empty()
    {
        var graph = new CodeGraphBuilder().Build();
        var result = new ServiceMapProjection().Project(graph, ProjectionOptions.Default);
        Assert.Empty(result.Services);
        Assert.Empty(result.Transports);
    }

    [Fact]
    public void ServiceMap_no_servicelink_edges_empty_transports()
    {
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForService("Api"), "Api", NodeKind.Service) { Project = "Api", Tags = [RoleTags.Runnable] });
        var graph = g.Build();
        var result = new ServiceMapProjection().Project(graph, ProjectionOptions.Default);
        Assert.Single(result.Services);
        Assert.Empty(result.Transports);
    }

    [Fact]
    public void ServiceMap_service_kind_gateway_from_tags()
    {
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForService("Gateway"), "Gateway", NodeKind.Service)
        {
            Project = "Gateway",
            Tags = [RoleTags.Runnable, "gateway"],
            Layer = "Api",
        });
        var graph = g.Build();
        var result = new ServiceMapProjection().Project(graph, ProjectionOptions.Default);
        Assert.Single(result.Services);
        Assert.Equal("Gateway", result.Services[0].Kind);
    }

    [Fact]
    public void FlowList_empty_graph_returns_empty()
    {
        var graph = new CodeGraphBuilder().Build();
        var result = new FlowListProjection().Project(graph, ProjectionOptions.Default);
        Assert.Empty(result.Flows);
        Assert.Equal(0, result.TotalFlows);
    }

    [Fact]
    public void FlowList_max_flows_zero_clamps_empty()
    {
        var g = new CodeGraphBuilder();
        var entry = NodeId.ForEntry("GET /x");
        g.AddNode(new GraphNode(entry, "GET /x", NodeKind.EntryPoint));
        g.SetFlows([MakeFlow("GET /x", entry, score: 1.0)]);
        var graph = g.Build();

        var result = new FlowListProjection().Project(graph, new ProjectionOptions { MaxFlows = 0 });
        Assert.Equal(1, result.TotalFlows);
        Assert.Empty(result.Flows);
    }

    [Fact]
    public void EntryTable_duplicate_entry_records_deduplicated_by_node()
    {
        var g = new CodeGraphBuilder();
        var e1 = NodeId.ForEntry("GET /orders");
        g.AddNode(new GraphNode(e1, "GET /orders", NodeKind.EntryPoint));
        // T1.8 — the same node appearing twice in the inventory collapses to one row.
        g.SetEntries([
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /orders", e1) { Score = 1.0 },
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /orders", e1) { Score = 1.0 },
        ]);
        var graph = g.Build();
        var result = new EntryTableProjection().Project(graph, ProjectionOptions.Default);
        Assert.Single(result.Rows);
    }

    [Fact]
    public void EntryTable_kind_comes_from_entry_record_not_node_tag()
    {
        // T1.8 — the row kind is the builder-stamped EntryPointKind (single-sourced), even when the
        // node carries no `kind:` tag. A gRPC entry reads "GrpcService", never a PublicApi default.
        var g = new CodeGraphBuilder();
        var node = NodeId.ForEntry("Basket/GetBasket");
        g.AddNode(new GraphNode(node, "Basket.GetBasket", NodeKind.EntryPoint) { Project = "Basket.API" });
        g.SetEntries([new EntryPoint(EntryPointKind.GrpcService, "Basket.GetBasket", node) { Project = "Basket.API" }]);
        var graph = g.Build();

        var result = new EntryTableProjection().Project(graph, ProjectionOptions.Default);
        var row = Assert.Single(result.Rows);
        Assert.Equal("GrpcService", row.Kind);
        Assert.Equal("Basket.API", row.Project);
    }

    [Fact]
    public void EntryTable_entrypoint_node_without_entry_record_is_omitted()
    {
        // T1.8 — a bare EntryPoint node with no matching Entry record is NOT invented as a PublicApi
        // row (the deleted DeriveEntryKind default); the inventory is the single source of truth.
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForEntry("worker"), "worker", NodeKind.EntryPoint) { Project = "Worker" });
        var graph = g.Build();

        var result = new EntryTableProjection().Project(graph, ProjectionOptions.Default);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void EntryTable_empty_graph_returns_empty()
    {
        var graph = new CodeGraphBuilder().Build();
        var result = new EntryTableProjection().Project(graph, ProjectionOptions.Default);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void LayerBand_empty_graph_returns_empty()
    {
        var graph = new CodeGraphBuilder().Build();
        var result = new LayerBandProjection().Project(graph, ProjectionOptions.Default);
        Assert.Empty(result.NodeBands);
        Assert.Empty(result.Layers);
        Assert.Empty(result.Features);
    }

    [Fact]
    public void LayerBand_collects_unique_layer_and_feature_sets()
    {
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForType("MyApp.Orders.Order"), "Order", NodeKind.Type)
            { Layer = "Domain", Feature = "Orders" });
        g.AddNode(new GraphNode(NodeId.ForType("MyApp.Orders.OrderController"), "OrderController", NodeKind.Type)
            { Layer = "Api", Feature = "Orders" });
        g.AddNode(new GraphNode(NodeId.ForType("MyApp.Customers.Customer"), "Customer", NodeKind.Type)
            { Layer = "Domain", Feature = "Customers" });
        var graph = g.Build();

        var result = new LayerBandProjection().Project(graph, ProjectionOptions.Default);
        Assert.Equal(3, result.NodeBands.Length);
        Assert.Equal(2, result.Layers.Length);
        Assert.Contains("Api", result.Layers);
        Assert.Contains("Domain", result.Layers);
        Assert.Equal(2, result.Features.Length);
        Assert.Contains("Orders", result.Features);
        Assert.Contains("Customers", result.Features);
    }
}
