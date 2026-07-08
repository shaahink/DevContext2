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
    public void EntryTable_flows_and_stray_entries_deduplicated()
    {
        var g = new CodeGraphBuilder();
        var e1 = NodeId.ForEntry("GET /orders");
        g.AddNode(new GraphNode(e1, "GET /orders", NodeKind.EntryPoint));
        g.SetFlows([MakeFlow("GET /orders", e1, score: 1.0)]);

        // Same entry appears as both a flow entry AND a stray node — only one row.
        var graph = g.Build();
        var result = new EntryTableProjection().Project(graph, ProjectionOptions.Default);
        Assert.Single(result.Rows);
    }

    [Fact]
    public void EntryTable_stray_entry_uses_public_api_kind()
    {
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForEntry("worker"), "worker", NodeKind.EntryPoint) { Project = "Worker" });
        var graph = g.Build();

        var result = new EntryTableProjection().Project(graph, ProjectionOptions.Default);
        var row = Assert.Single(result.Rows);
        Assert.Equal("PublicApi", row.Kind);
        Assert.Equal("Worker", row.Project);
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
