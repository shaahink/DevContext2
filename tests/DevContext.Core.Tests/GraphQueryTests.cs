using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>Phase 5 query layer: inverse edges + the GraphQuery facade (neighbors / find_usages / node /
/// entrypoints). The browse UI and MCP server are clients of these, so both edge directions must be
/// correct and a node's callers must be reachable without a full scan.</summary>
public sealed class GraphQueryTests
{
    private static (CodeGraph Graph, NodeId A, NodeId B, NodeId C) BuildGraph()
    {
        var g = new CodeGraphBuilder();
        var a = NodeId.ForType("Ns.A");
        var b = NodeId.ForType("Ns.B");
        var c = NodeId.ForType("Ns.C");
        g.AddNode(new GraphNode(a, "A", NodeKind.Type) { FilePath = "A.cs" });
        g.AddNode(new GraphNode(b, "B", NodeKind.Type) { FilePath = "B.cs" });
        g.AddNode(new GraphNode(c, "C", NodeKind.Type) { FilePath = "C.cs" });
        g.AddEdge(new GraphEdge(a, b, EdgeKind.Calls) { Resolution = Resolution.Semantic });
        g.AddEdge(new GraphEdge(c, b, EdgeKind.Calls) { Resolution = Resolution.Syntactic });
        return (g.Build(), a, b, c);
    }

    [Fact]
    public void InEdges_and_OutEdges_are_consistent()
    {
        var (graph, a, b, c) = BuildGraph();
        Assert.Contains(graph.OutEdges(a), e => e.To == b);
        Assert.Empty(graph.InEdges(a));
        var inB = graph.InEdges(b);
        Assert.Equal(2, inB.Length);
        Assert.Contains(inB, e => e.From == a);
        Assert.Contains(inB, e => e.From == c);
    }

    [Fact]
    public void InEdges_filter_by_kind()
    {
        var (graph, _, b, _) = BuildGraph();
        Assert.Equal(2, graph.InEdges(b, EdgeKind.Calls).Length);
        Assert.Empty(graph.InEdges(b, EdgeKind.Sends));
    }

    [Fact]
    public void FindUsages_returns_all_callers()
    {
        var (graph, a, b, c) = BuildGraph();
        var q = new GraphQuery(graph, []);
        var usages = q.FindUsages(b);
        Assert.Equal(2, usages.Length);
        Assert.Contains(usages, u => u.From == a);
        Assert.Contains(usages, u => u.From == c);
        Assert.Empty(q.FindUsages(a)); // A has no callers
    }

    [Fact]
    public void Neighbors_out_and_in()
    {
        var (graph, a, b, _) = BuildGraph();
        var q = new GraphQuery(graph, []);
        var outA = q.Neighbors(a, EdgeDirection.Out);
        Assert.Single(outA);
        Assert.Equal("B", outA[0].OtherTitle);
        var inB = q.Neighbors(b, EdgeDirection.In);
        Assert.Equal(2, inB.Length);
        Assert.All(inB, e => Assert.Equal(b, e.To));
    }

    [Fact]
    public void Node_detail_has_both_degrees()
    {
        var (graph, _, b, _) = BuildGraph();
        var q = new GraphQuery(graph, []);
        var nb = q.Node(b);
        Assert.NotNull(nb);
        Assert.Equal("B", nb!.Title);
        Assert.Equal(2, nb.InDegree);
        Assert.Equal(0, nb.OutDegree);
        Assert.Null(q.Node(NodeId.ForType("Ns.Missing")));
    }

    [Fact]
    public void ResolveNodeId_by_short_name_fqn_and_missing()
    {
        var (graph, _, b, _) = BuildGraph();
        var q = new GraphQuery(graph, []);
        Assert.Equal(b, q.ResolveNodeId("B"));      // short name → title match
        Assert.Equal(b, q.ResolveNodeId("Ns.B"));   // exact key match
        Assert.Null(q.ResolveNodeId("Nope"));
    }

    [Fact]
    public void EntryPoints_filter_by_kind()
    {
        var (graph, _, _, _) = BuildGraph();
        var e1 = new EntryPoint(EntryPointKind.HttpEndpoint, "GET /x", NodeId.ForEntry("GET /x"));
        var e2 = new EntryPoint(EntryPointKind.ScheduledJob, "Job", NodeId.ForEntry("worker:Job"));
        var q = new GraphQuery(graph, [e1, e2]);
        Assert.Equal(2, q.EntryPoints().Length);
        Assert.Single(q.EntryPoints(EntryPointKind.HttpEndpoint));
        Assert.Single(q.EntryPoints(EntryPointKind.ScheduledJob));
    }

    [Fact]
    public void GetInterestingPoints_excludes_framework_and_store_noise() // T3.5
    {
        var g = new CodeGraphBuilder();
        var svc = NodeId.ForType("Shop.OrderService");
        var repo = NodeId.ForType("Shop.OrderRepository");
        var list = NodeId.ForType("List");                              // BCL type name → noise
        var sysList = NodeId.ForType("System.Collections.Generic.List");// System.* → noise
        var db = NodeId.ForStore("Shop.AppDbContext");                  // infra store → noise

        g.AddNode(new GraphNode(svc, "OrderService", NodeKind.Type) { FilePath = "src/OrderService.cs" });
        g.AddNode(new GraphNode(repo, "OrderRepository", NodeKind.Type) { FilePath = "src/OrderRepository.cs" });
        g.AddNode(new GraphNode(list, "List", NodeKind.Type));          // framework: no repo file
        g.AddNode(new GraphNode(sysList, "System.Collections.Generic.List", NodeKind.Type) { FilePath = "x.cs" });
        g.AddNode(new GraphNode(db, "AppDbContext", NodeKind.Store) { FilePath = "src/AppDbContext.cs", Tags = [RoleTags.DataStore] });

        // Give the noise nodes high degree so, unfiltered, they would top centrality.
        foreach (var caller in new[] { svc, repo })
        {
            g.AddEdge(new GraphEdge(caller, db, EdgeKind.ReadsWrites));
            g.AddEdge(new GraphEdge(caller, list, EdgeKind.Calls));
            g.AddEdge(new GraphEdge(caller, sysList, EdgeKind.Calls));
        }
        g.AddEdge(new GraphEdge(svc, repo, EdgeKind.Calls));

        var q = new GraphQuery(g.Build(), []);
        var titles = q.GetInterestingPoints().Select(p => p.Title).ToArray();

        Assert.Contains("OrderService", titles);                          // real repo type survives
        Assert.DoesNotContain("List", titles);                            // BCL noise filtered
        Assert.DoesNotContain("System.Collections.Generic.List", titles); // System.* filtered
        Assert.DoesNotContain("AppDbContext", titles);                    // infra store filtered
    }

    // ---- G1.4 (R4 §1 item 6) — FindPage: kind above the limit, and a total that is a fact -------

    /// <summary>Six "Order" matches: 3 Types, 2 Members, 1 Store.</summary>
    private static GraphQuery OrderGraph()
    {
        var g = new CodeGraphBuilder();
        void Node(NodeId id, string title, NodeKind kind) => g.AddNode(new GraphNode(id, title, kind));
        Node(NodeId.ForType("Ns.OrderService"), "OrderService", NodeKind.Type);
        Node(NodeId.ForType("Ns.OrderRepository"), "OrderRepository", NodeKind.Type);
        Node(NodeId.ForType("Ns.Order"), "Order", NodeKind.Type);
        Node(NodeId.ForMember("Ns.OrderService", "PlaceOrder"), "PlaceOrder", NodeKind.Member);
        Node(NodeId.ForMember("Ns.OrderService", "CancelOrder"), "CancelOrder", NodeKind.Member);
        Node(NodeId.ForStore("Ns.OrderStore"), "OrderStore", NodeKind.Store);
        Node(NodeId.ForType("Ns.Basket"), "Basket", NodeKind.Type);   // must never match
        return new GraphQuery(g.Build(), []);
    }

    /// <summary>
    /// The defect in one assertion: a total that describes the page is not a total. Ask for one row
    /// of six matches and the answer to "how many are there" is still six.
    /// </summary>
    [Fact]
    public void FindPage_total_counts_every_match_not_the_page()
    {
        var q = OrderGraph();

        var (page, total) = q.FindPage("Order", kind: null, limit: 1);

        Assert.Single(page);
        Assert.Equal(6, total);
    }

    /// <summary>The kind narrows what is COUNTED, which is only true if it runs above the limit.</summary>
    [Fact]
    public void FindPage_applies_the_kind_before_the_limit()
    {
        var q = OrderGraph();

        var (page, total) = q.FindPage("Order", kind: "Type", limit: 1);

        Assert.Equal(3, total);                       // 3 Types match, not "Types within the first 1"
        Assert.Equal(NodeKind.Type, page[0].Kind);
    }

    [Fact]
    public void FindPage_kind_is_case_insensitive()
        => Assert.Equal(3, OrderGraph().FindPage("Order", kind: "type", limit: 10).TotalMatches);

    /// <summary>An unrecognised kind is an honest zero, not an ignored filter.</summary>
    [Fact]
    public void FindPage_an_unknown_kind_matches_nothing()
    {
        var (page, total) = OrderGraph().FindPage("Order", kind: "NoSuchKind", limit: 10);

        Assert.Empty(page);
        Assert.Equal(0, total);
    }

    /// <summary>Find() is FindPage's page — the shared resolve/usages/impact path must not move.</summary>
    [Fact]
    public void Find_still_returns_the_same_page_it_always_did()
    {
        var q = OrderGraph();

        var find = q.Find("Order", limit: 4);
        var (page, _) = q.FindPage("Order", kind: null, limit: 4);

        Assert.Equal(find.Select(r => r.Title), page.Select(r => r.Title));
    }
}
