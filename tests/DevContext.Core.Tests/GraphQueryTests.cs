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
}
