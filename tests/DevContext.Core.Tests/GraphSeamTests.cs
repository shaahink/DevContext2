using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>
/// G3.1 (R4 §1 item 8) — <c>seam(from, to)</c>, the path BETWEEN two symbols.
///
/// <para>The check that would prove nothing is "seam returned a path": a direct-edge BFS returns
/// paths too, and on the shapes that matter it returns the WRONG answer confidently. The two
/// discriminating invariants are pinned here.</para>
///
/// <para>(1) THE C3 ROLL-UP. A Type node carries almost no edges of its own — the wiring hangs off
/// its members — so a Type→Type seam over direct edges answers "unconnected" for two types that
/// call each other every request. <see cref="Type_to_type_connects_through_the_members_that_carry_it"/>
/// fails on a direct-edge implementation and passes on the rolled one; it was watched doing exactly
/// that before this file was committed.</para>
///
/// <para>(2) A HOP COUNT IS NOT A PAGE SIZE. The same class of defect as G1.4's find totals: a
/// number that moves when the page size moves is describing the page.
/// <see cref="The_shortest_path_length_does_not_move_when_maxPaths_moves"/> pins that the distance
/// is a fact about the graph and the path list is a window onto it.</para>
/// </summary>
public sealed class GraphSeamTests
{
    // A::Go -> B::Handle -> C::Save. The TYPE nodes A, B, C carry no edges at all: every edge in
    // this graph hangs off a member, which is what the real graph looks like after Batch A.
    private static GraphQuery MemberChain()
    {
        var g = new CodeGraphBuilder();
        foreach (var t in new[] { "Ns.A", "Ns.B", "Ns.C" })
            g.AddNode(new GraphNode(NodeId.ForType(t), t[3..], NodeKind.Type) { FilePath = $"{t[3..]}.cs" });

        g.AddNode(new GraphNode(NodeId.ForMember("Ns.A", "Go"), "A.Go", NodeKind.Member) { FilePath = "A.cs", LineNumber = 11 });
        g.AddNode(new GraphNode(NodeId.ForMember("Ns.B", "Handle"), "B.Handle", NodeKind.Member) { FilePath = "B.cs", LineNumber = 22 });
        g.AddNode(new GraphNode(NodeId.ForMember("Ns.C", "Save"), "C.Save", NodeKind.Member) { FilePath = "C.cs", LineNumber = 33 });

        g.AddEdge(new GraphEdge(NodeId.ForMember("Ns.A", "Go"), NodeId.ForMember("Ns.B", "Handle"), EdgeKind.Calls)
        { Resolution = Resolution.Semantic });
        g.AddEdge(new GraphEdge(NodeId.ForMember("Ns.B", "Handle"), NodeId.ForMember("Ns.C", "Save"), EdgeKind.ReadsWrites)
        { Resolution = Resolution.Syntactic });

        return new GraphQuery(g.Build(), []);
    }

    // Type-level graph with two 2-hop routes A->X->D and A->Y->D, plus a 3-hop detour A->X->Z->D.
    private static GraphQuery TypeDiamond()
    {
        var g = new CodeGraphBuilder();
        foreach (var t in new[] { "A", "X", "Y", "Z", "D", "Island" })
            g.AddNode(new GraphNode(NodeId.ForType($"Ns.{t}"), t, NodeKind.Type));

        void Edge(string a, string b) => g.AddEdge(
            new GraphEdge(NodeId.ForType($"Ns.{a}"), NodeId.ForType($"Ns.{b}"), EdgeKind.Calls)
            { Resolution = Resolution.Semantic });

        Edge("A", "X"); Edge("A", "Y");
        Edge("X", "D"); Edge("Y", "D");
        Edge("X", "Z"); Edge("Z", "D");

        return new GraphQuery(g.Build(), []);
    }

    private static GraphQuery Line(int length)
    {
        var g = new CodeGraphBuilder();
        for (var i = 0; i <= length; i++)
            g.AddNode(new GraphNode(NodeId.ForType($"Ns.N{i}"), $"N{i}", NodeKind.Type));
        for (var i = 0; i < length; i++)
            g.AddEdge(new GraphEdge(NodeId.ForType($"Ns.N{i}"), NodeId.ForType($"Ns.N{i + 1}"), EdgeKind.Calls));
        return new GraphQuery(g.Build(), []);
    }

    /// <summary>
    /// THE RED. Two types whose only connection runs through their members. A direct-edge search
    /// answers Direction.None here — both endpoints are bare Type nodes with zero edges — and that
    /// answer is wrong in the most damaging way available: it says "these are unconnected" about a
    /// call that happens on every request.
    /// </summary>
    [Fact]
    public void Type_to_type_connects_through_the_members_that_carry_it()
    {
        var seam = MemberChain().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.C"));

        Assert.Equal(SeamDirection.Forward, seam.Direction);
        Assert.Equal(2, seam.Hops);

        // And the hops name the MEMBERS, not the types — "which member carries the collaboration"
        // is the whole reason the roll-up keeps the true edge endpoints.
        var hops = Assert.Single(seam.Paths).Hops;
        Assert.Equal(["A.Go", "B.Handle"], hops.Select(h => h.FromTitle));
        Assert.Equal(["B.Handle", "C.Save"], hops.Select(h => h.ToTitle));
    }

    /// <summary>Each hop carries the seam kind, how the edge was bound, and where to look.</summary>
    [Fact]
    public void Every_hop_names_its_seam_kind_resolution_and_site()
    {
        var seam = MemberChain().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.C"));
        var hops = Assert.Single(seam.Paths).Hops;

        Assert.Equal([EdgeKind.Calls, EdgeKind.ReadsWrites], hops.Select(h => h.Kind));
        Assert.Equal([Resolution.Semantic, Resolution.Syntactic], hops.Select(h => h.Resolution));
        Assert.Equal(["A.cs", "B.cs"], hops.Select(h => h.FilePath));
        Assert.Equal([11, 22], hops.Select(h => h.LineNumber));
    }

    /// <summary>
    /// THE SECOND RED. maxPaths is a window onto the answer; the distance is the answer. If the
    /// reported hop count tracked the number of returned paths it would be describing the page,
    /// which is exactly the defect G1.4 found in find's total.
    /// </summary>
    [Fact]
    public void The_shortest_path_length_does_not_move_when_maxPaths_moves()
    {
        var q = TypeDiamond();
        var one = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 1);
        var many = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 10);

        Assert.Equal(2, one.Hops);
        Assert.Equal(2, many.Hops);
        Assert.Single(one.Paths);
        Assert.Equal(2, many.Paths.Length);       // A->X->D and A->Y->D; A->X->Z->D is longer
        Assert.All(many.Paths, p => Assert.Equal(2, p.Hops.Length));
    }

    /// <summary>The count of shortest paths is a fact about the graph, not about the page —
    /// so a caller shown 1 of 2 can tell it was shown 1 of 2.</summary>
    [Fact]
    public void Total_paths_counts_every_shortest_path_including_the_unshown_ones()
    {
        var seam = TypeDiamond().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 1);

        Assert.Single(seam.Paths);
        Assert.Equal(2, seam.TotalPaths);
    }

    /// <summary>"B reaches A" is a fact, and returning None for it would be a false negative the
    /// caller cannot see. The reverse walk is reported AS reverse — never silently as the answer to
    /// the question that was asked.</summary>
    [Fact]
    public void A_connection_that_runs_the_other_way_is_reported_as_reverse()
    {
        var seam = MemberChain().Seam(NodeId.ForType("Ns.C"), NodeId.ForType("Ns.A"));

        Assert.Equal(SeamDirection.Reverse, seam.Direction);
        Assert.Equal(2, seam.Hops);
        Assert.Equal("A.Go", Assert.Single(seam.Paths).Hops[0].FromTitle);
    }

    /// <summary>Genuinely unconnected: the walk exhausted both ends and found nothing. This is the
    /// only case that may claim there is no path.</summary>
    [Fact]
    public void Unconnected_nodes_report_none_without_blaming_the_depth_limit()
    {
        var seam = TypeDiamond().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.Island"));

        Assert.Equal(SeamDirection.None, seam.Direction);
        Assert.Empty(seam.Paths);
        Assert.Equal(0, seam.TotalPaths);
        Assert.False(seam.StoppedAtDepthLimit);
    }

    /// <summary>"No path within 2 hops" is not "no path". A search the budget ended says so, or the
    /// caller reads a hop budget as a fact about the codebase.</summary>
    [Fact]
    public void A_search_the_depth_budget_ended_says_so()
    {
        var q = Line(5);
        var capped = q.Seam(NodeId.ForType("Ns.N0"), NodeId.ForType("Ns.N5"), maxDepth: 2);

        Assert.Equal(SeamDirection.None, capped.Direction);
        Assert.True(capped.StoppedAtDepthLimit);

        // ...and the same seam with room finds it, which is what makes the flag actionable.
        var deeper = q.Seam(NodeId.ForType("Ns.N0"), NodeId.ForType("Ns.N5"), maxDepth: 8);
        Assert.Equal(SeamDirection.Forward, deeper.Direction);
        Assert.Equal(5, deeper.Hops);
        Assert.False(deeper.StoppedAtDepthLimit);
    }

    /// <summary>Both ends the same symbol: zero hops, forward, and no invented path rows.</summary>
    [Fact]
    public void The_same_symbol_at_both_ends_is_zero_hops_not_no_path()
    {
        var seam = TypeDiamond().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.A"));

        Assert.Equal(SeamDirection.Forward, seam.Direction);
        Assert.Equal(0, seam.Hops);
        Assert.Empty(Assert.Single(seam.Paths).Hops);
    }

    /// <summary>Determinism seal: the same question returns the same paths in the same order.
    /// Path enumeration walks a predecessor DAG, and a HashSet iteration order in there would make
    /// a query layer that answers differently on two runs of the same snapshot.</summary>
    [Fact]
    public void The_same_seam_answers_identically_twice()
    {
        var q = TypeDiamond();
        var a = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 10);
        var b = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 10);

        Assert.Equal(
            a.Paths.Select(p => string.Join(" -> ", p.Hops.Select(h => h.ToTitle))),
            b.Paths.Select(p => string.Join(" -> ", p.Hops.Select(h => h.ToTitle))));
    }
}
