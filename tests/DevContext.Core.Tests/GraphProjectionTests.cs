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
}
