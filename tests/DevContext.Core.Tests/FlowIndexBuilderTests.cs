using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>
/// T7.4 — server-side flow atlas. The builder must mirror the retired client indexer's stat
/// semantics (focus key, hub-vs-data split, score), fix its boundary-seam counting bug (the
/// client compared 'consumes'/'raises'/'handler' against the wire's 'consume'/'raise'/'handle',
/// so only send-hops ever counted), and return the top-hub degrees so the app needs no
/// per-hub GetNode calls.
/// </summary>
public sealed class FlowIndexBuilderTests
{
    private static (GraphQuery Query, ImmutableArray<EntryPoint> Entries) BuildFixture()
    {
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /orders");
        var requestId = NodeId.ForType("App.CreateOrder");
        var handlerId = NodeId.ForType("App.CreateOrderHandler");
        var storeId = NodeId.ForStore("App.OrdersDb");
        var entry2Id = NodeId.ForEntry("GET /orders");

        g.AddNode(new GraphNode(entryId, "POST /orders", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(requestId, "CreateOrder", NodeKind.Type) { Tags = [RoleTags.Command] });
        g.AddNode(new GraphNode(handlerId, "CreateOrderHandler", NodeKind.Type) { Tags = [RoleTags.Handler] });
        g.AddNode(new GraphNode(storeId, "OrdersDb", NodeKind.Store));
        g.AddNode(new GraphNode(entry2Id, "GET /orders", NodeKind.EntryPoint));

        g.AddEdge(new GraphEdge(entryId, requestId, EdgeKind.Sends));
        g.AddEdge(new GraphEdge(requestId, handlerId, EdgeKind.Handles) { Resolution = Resolution.Semantic });
        g.AddEdge(new GraphEdge(handlerId, storeId, EdgeKind.ReadsWrites));
        // Second flow shares the handler (makes it a >1-flow hub for the radar).
        g.AddEdge(new GraphEdge(entry2Id, handlerId, EdgeKind.Calls));

        var graph = g.Build();
        var entries = ImmutableArray.Create(
            new EntryPoint(EntryPointKind.HttpEndpoint, "POST /orders", entryId) { HttpMethod = "POST", Route = "/orders" },
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /orders", entry2Id) { HttpMethod = "GET", Route = "/orders" });
        return (new GraphQuery(graph, entries), entries);
    }

    [Fact]
    public void Build_produces_one_stat_per_entry_with_boundary_and_data_counts()
    {
        var (query, entries) = BuildFixture();

        var index = FlowIndexBuilder.Build(query, entries);

        Assert.Equal(2, index.Flows.Length);
        var post = index.Flows.Single(f => f.Focus == "POST /orders");
        Assert.True(post.Found);
        Assert.Equal(4, post.NodeCount);                 // entry + command + handler + store
        // Send + Handle both count (the client's seam-name bug counted only sends).
        Assert.Equal(2, post.BoundaryCrossings);
        Assert.Equal(1, post.DataTouches);               // the ReadsWrites hop
        Assert.Equal(3, post.HubIds.Length);             // data-seam node excluded from hubs
        Assert.DoesNotContain(post.HubIds, id => id.Contains("OrdersDb"));
        Assert.Contains(post.NodeIds, id => id.Contains("OrdersDb"));
        Assert.Equal(post.NodeCount * (1.0 + post.BoundaryCrossings), post.Score);
        Assert.Equal(25, post.VerifiedPct);              // 1 semantic hop of 4 nodes
    }

    [Fact]
    public void Build_returns_degrees_for_hubs_on_more_than_one_flow()
    {
        var (query, entries) = BuildFixture();

        var index = FlowIndexBuilder.Build(query, entries);

        var hub = Assert.Single(index.HubDegrees);       // only the shared handler is on 2 flows
        Assert.Contains("CreateOrderHandler", hub.NodeId);
        Assert.Equal(2, hub.InDegree);                   // Handles + Calls into it
        Assert.Equal(1, hub.OutDegree);                  // ReadsWrites out of it
    }

    [Fact]
    public void Build_keys_flows_by_verb_route_exactly_like_the_app()
    {
        var (query, entries) = BuildFixture();

        var index = FlowIndexBuilder.Build(query, entries);

        // The app derives its stat key as `${httpMethod} ${route}` — the server must match
        // or every stat row orphans from its entry row.
        Assert.All(index.Flows, f => Assert.Matches("^(POST|GET) /orders$", f.Focus));
    }
}
