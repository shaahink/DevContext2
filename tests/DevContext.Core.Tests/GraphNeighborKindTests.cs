using System.Collections.Immutable;
using System.Reflection;

using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>
/// G3.2 (R4 §1 item 9) — kind-filtered <c>neighbors</c>: "who WRITES this table", "who SENDS this
/// command".
///
/// <para>The check that would prove nothing is "the filter returned only ReadsWrites edges" — an
/// implementation that filters the wrong SET passes that just as happily. Two invariants
/// discriminate, and both are pinned here.</para>
///
/// <para>(1) THE FILTER RUNS ABOVE THE C3 ROLL-UP. After Batch A a Type node carries almost no edges
/// of its own — the wiring hangs off its members — so a filter applied to
/// <c>_graph.OutEdges(type, kind)</c> answers "nothing here" about a table the type writes on every
/// request. <see cref="The_kind_filter_sees_the_edges_that_hang_off_the_members"/> was watched
/// failing on exactly that implementation before this file was committed (the RED is in the
/// evidence artifact).</para>
///
/// <para>(2) THE NUMBERS DESCRIBE ONE SET. Once <c>Edges</c> is filtered it can no longer say what
/// was filtered out, so <c>TotalEdges</c>/<c>KindsPresent</c> exist to say it — and they are worth
/// nothing if they describe a different walk than the rows did.
/// <see cref="The_unfiltered_numbers_do_not_move_when_the_filter_moves"/> pins that they are one
/// pass, and <see cref="Filtering_above_the_roll_up_equals_filtering_inside_it"/> measures the
/// rewrite that made this possible instead of reading its doc comment.</para>
/// </summary>
public sealed class GraphNeighborKindTests
{
    private static NodeId Type(string t) => NodeId.ForType($"Ns.{t}");
    private static NodeId Member(string t, string m) => NodeId.ForMember($"Ns.{t}", m);

    /// <summary>
    /// The shape the real graph has after Batch A: every seam hangs off a MEMBER, the Type node
    /// keeps only its declared dependency. OrderService writes OrdersTable from two members, raises
    /// an event, calls Shipping, and calls itself once (intra-type — the roll-up drops it, which is
    /// the point of a "collaborators, not private helpers" answer). Billing and Reporting also touch
    /// the table, so the IN direction has a real "who writes this" question to answer.
    /// </summary>
    private static GraphQuery Orders()
    {
        var g = new CodeGraphBuilder();
        foreach (var t in new[] { "OrderService", "OrdersTable", "Shipping", "OrderPlaced", "Billing", "Reporting" })
            g.AddNode(new GraphNode(Type(t), t, NodeKind.Type) { FilePath = $"{t}.cs" });

        void Mem(string t, string m) => g.AddNode(
            new GraphNode(Member(t, m), $"{t}.{m}", NodeKind.Member) { FilePath = $"{t}.cs", LineNumber = 7 });

        Mem("OrderService", "Place");
        Mem("OrderService", "Cancel");
        Mem("Shipping", "Ship");
        Mem("Billing", "Charge");
        Mem("Reporting", "Summarise");

        void Edge(NodeId from, NodeId to, EdgeKind kind) =>
            g.AddEdge(new GraphEdge(from, to, kind) { Resolution = Resolution.Semantic });

        Edge(Member("OrderService", "Place"), Type("OrdersTable"), EdgeKind.ReadsWrites);
        Edge(Member("OrderService", "Cancel"), Type("OrdersTable"), EdgeKind.ReadsWrites);
        Edge(Member("OrderService", "Place"), Type("OrderPlaced"), EdgeKind.Raises);
        Edge(Member("OrderService", "Place"), Member("Shipping", "Ship"), EdgeKind.Calls);
        Edge(Member("OrderService", "Cancel"), Member("OrderService", "Place"), EdgeKind.Calls);  // intra-type
        Edge(Type("OrderService"), Type("Shipping"), EdgeKind.DependsOn);                          // type's own
        Edge(Member("Billing", "Charge"), Type("OrdersTable"), EdgeKind.ReadsWrites);
        Edge(Member("Reporting", "Summarise"), Type("OrdersTable"), EdgeKind.ReadsWrites);

        return new GraphQuery(g.Build(), []);
    }

    // ---- (1) the roll-up ------------------------------------------------------------------

    /// <summary>
    /// THE RED. "Which tables does OrderService write?" — the whole point of the parameter. Every
    /// ReadsWrites edge hangs off a member, so a filter applied to the Type node's own edges returns
    /// an empty list, and an empty list here is not a near-miss: it says a type writes nothing when
    /// it writes on every order.
    /// </summary>
    [Fact]
    public void The_kind_filter_sees_the_edges_that_hang_off_the_members()
    {
        var q = Orders();

        var writes = q.NeighborsView(Type("OrderService"), EdgeDirection.Out, EdgeKind.ReadsWrites);

        Assert.Equal(2, writes.Edges.Length);
        Assert.All(writes.Edges, e => Assert.Equal(EdgeKind.ReadsWrites, e.Kind));
        Assert.All(writes.Edges, e => Assert.Equal(Type("OrdersTable"), e.To));
        // The answer names WHICH members carry it — a type-level "yes it writes" is the weaker answer.
        Assert.Equal(
            [Member("OrderService", "Cancel"), Member("OrderService", "Place")],
            writes.Edges.Select(e => e.From).OrderBy(id => id.Key).ToArray());
    }

    /// <summary>The IN direction is the question item 9 is actually named after: who writes this
    /// table. Three types do, through three different members.</summary>
    [Fact]
    public void Who_writes_this_table_is_one_call()
    {
        var q = Orders();

        var writers = q.NeighborsView(Type("OrdersTable"), EdgeDirection.In, EdgeKind.ReadsWrites);

        Assert.Equal(4, writers.Edges.Length);
        Assert.Equal(
            ["Ns.Billing::Charge", "Ns.OrderService::Cancel", "Ns.OrderService::Place", "Ns.Reporting::Summarise"],
            writers.Edges.Select(e => e.From.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray());
    }

    /// <summary>A private helper is not a collaborator: the intra-type call is dropped by the
    /// roll-up, and the kind filter must not smuggle it back in.</summary>
    [Fact]
    public void An_intra_type_call_is_not_a_neighbour_even_when_its_kind_is_asked_for()
    {
        var q = Orders();

        var calls = q.NeighborsView(Type("OrderService"), EdgeDirection.Out, EdgeKind.Calls);

        var call = Assert.Single(calls.Edges);
        Assert.Equal(Member("Shipping", "Ship"), call.To);
    }

    // ---- (2) one set ----------------------------------------------------------------------

    /// <summary>
    /// The honesty half. <c>TotalEdges</c> and <c>KindsPresent</c> describe the UNFILTERED edges in
    /// the same direction, so they must be byte-identical whatever the filter is — including a kind
    /// that matches nothing. A pair of numbers that shrink with the rows cannot tell a caller what
    /// to ask instead, which is the only reason they are on the wire.
    /// </summary>
    [Fact]
    public void The_unfiltered_numbers_do_not_move_when_the_filter_moves()
    {
        var q = Orders();
        var id = Type("OrderService");

        var unfiltered = q.NeighborsView(id, EdgeDirection.Out);
        // .ToArray() on both sides throughout this file: Assert.Equal on two ImmutableArray<T>
        // values binds the struct overload, which is REFERENCE equality on the backing array — it
        // fails on identical contents and prints two identical lines while doing it.
        var expected = unfiltered.KindsPresent.ToArray();

        foreach (var kind in Enum.GetValues<EdgeKind>())
        {
            var view = q.NeighborsView(id, EdgeDirection.Out, kind);
            Assert.Equal(unfiltered.TotalEdges, view.TotalEdges);
            Assert.Equal(expected, view.KindsPresent.ToArray());
            // ...and the rows really are the unfiltered rows of that kind — same set, one pass.
            Assert.Equal(
                unfiltered.Edges.Where(e => e.Kind == kind).ToArray(),
                view.Edges.ToArray());
        }
    }

    /// <summary>The counts add up to the total and are ordered busiest-first — the head of the list
    /// is the retry a caller who guessed wrong most likely wanted. Ties break on enum order so the
    /// list is stable rather than dictionary-ordered.</summary>
    [Fact]
    public void Kinds_present_counts_the_unfiltered_set_busiest_first()
    {
        var q = Orders();

        var view = q.NeighborsView(Type("OrderService"), EdgeDirection.Out);

        Assert.Equal(5, view.TotalEdges);
        Assert.Equal(view.TotalEdges, view.KindsPresent.Sum(k => k.Count));
        Assert.Equal(
            new EdgeKindCount[]
            {
                new(EdgeKind.ReadsWrites, 2), new(EdgeKind.Calls, 1),
                new(EdgeKind.Raises, 1), new(EdgeKind.DependsOn, 1),
            },
            view.KindsPresent.ToArray());
    }

    /// <summary>A kind that matches nothing on a node that HAS edges is the reply that most needs
    /// the other two fields: the rows are empty and everything a caller could retry with is still
    /// there.</summary>
    [Fact]
    public void A_kind_that_matches_nothing_still_reports_what_is_there()
    {
        var q = Orders();

        var view = q.NeighborsView(Type("OrdersTable"), EdgeDirection.In, EdgeKind.Sends);

        Assert.Empty(view.Edges);
        Assert.Equal(4, view.TotalEdges);
        Assert.Equal(new EdgeKindCount(EdgeKind.ReadsWrites, 4), Assert.Single(view.KindsPresent));
    }

    /// <summary>A node with nothing at all is a different answer from a filter that matched nothing,
    /// and the difference is readable without a second call.</summary>
    [Fact]
    public void A_leaf_says_zero_of_zero_not_zero_of_something()
    {
        var q = Orders();

        var view = q.NeighborsView(Type("OrderPlaced"), EdgeDirection.Out, EdgeKind.Calls);

        Assert.Empty(view.Edges);
        Assert.Equal(0, view.TotalEdges);
        Assert.Empty(view.KindsPresent);
    }

    // ---- the rewrite, measured ------------------------------------------------------------

    /// <summary>
    /// G3.2 moved the kind filter from INSIDE <c>RolledEdges</c> (which pushed it down to
    /// <c>_graph.OutEdges(id, kind)</c>) to ABOVE it, because the unfiltered walk is what
    /// <c>KindsPresent</c> is counted from and two walks could disagree. The new doc comment asserts
    /// the two orders commute. This program has been wrong four times about what a comment asserts,
    /// so it is measured here instead: for every node, both directions, every kind, the filtered
    /// rows must be SEQUENCE-equal to what the pushed-down filter produced.
    ///
    /// <para>Reflection is deliberate — the pushed-down path is the private overload, and comparing
    /// against a re-implementation of it in the test would only prove the test agrees with itself.</para>
    /// </summary>
    [Fact]
    public void Filtering_above_the_roll_up_equals_filtering_inside_it()
    {
        var q = Orders();
        var rolled = typeof(GraphQuery).GetMethod("RolledEdges", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(rolled);

        var compared = 0;
        foreach (var node in q.Graph.Nodes)
        {
            foreach (var dir in new[] { EdgeDirection.Out, EdgeDirection.In })
            {
                foreach (var kind in Enum.GetValues<EdgeKind>())
                {
                    var pushedDown = (ImmutableArray<GraphEdge>)rolled!.Invoke(q, [node.Id, dir, kind])!;
                    var filteredAbove = q.NeighborsView(node.Id, dir, kind).Edges;

                    Assert.Equal(
                        pushedDown.Select(e => (e.From, e.To, e.Kind)).ToArray(),
                        filteredAbove.Select(e => (e.From, e.To, e.Kind)).ToArray());
                    compared++;
                }
            }
        }

        // A vacuous sweep would also pass every assertion above; this one really visited the matrix.
        Assert.Equal(q.Graph.Nodes.Length * 2 * Enum.GetValues<EdgeKind>().Length, compared);
    }

    /// <summary>The existing overload keeps its exact meaning — every caller that predates G3.2
    /// (the context pack asks for Resolves) sees the same list it saw before.</summary>
    [Fact]
    public void The_existing_Neighbors_overload_is_the_views_rows()
    {
        var q = Orders();

        foreach (var node in q.Graph.Nodes)
        {
            foreach (var dir in new[] { EdgeDirection.Out, EdgeDirection.In })
            {
                Assert.Equal(q.NeighborsView(node.Id, dir).Edges.ToArray(), q.Neighbors(node.Id, dir).ToArray());
                Assert.Equal(
                    q.NeighborsView(node.Id, dir, EdgeKind.ReadsWrites).Edges.ToArray(),
                    q.Neighbors(node.Id, dir, EdgeKind.ReadsWrites).ToArray());
            }
        }
    }

    /// <summary>The server reads <c>direction:"usages"</c> as the IN direction so that one arm
    /// accepts the kind. That fold is only safe because the two were already the same walk.</summary>
    [Fact]
    public void FindUsages_is_the_in_direction()
    {
        var q = Orders();

        foreach (var node in q.Graph.Nodes)
            Assert.Equal(q.Neighbors(node.Id, EdgeDirection.In).ToArray(), q.FindUsages(node.Id).ToArray());
    }

    // ---- the kind vocabulary --------------------------------------------------------------

    /// <summary>Every kind name a caller can be told about is one a caller can actually pass. The
    /// list and the parser come from the enum itself, so neither can drift from what the engine
    /// accepts — the drift class G2.1 took out of the tool menu.</summary>
    [Fact]
    public void Every_advertised_kind_name_parses()
    {
        Assert.Equal(Enum.GetValues<EdgeKind>().Length, GraphQuery.EdgeKindNames.Length);

        foreach (var name in GraphQuery.EdgeKindNames)
        {
            Assert.True(GraphQuery.TryParseEdgeKind(name, out var kind));
            Assert.Equal(name, kind.ToString());
        }
    }

    /// <summary>Case-insensitive, because an agent writes <c>readswrites</c>; whitespace and empties
    /// are not kinds; and the underlying NUMBER is not a kind either —
    /// <c>Enum.TryParse</c> accepts "5" and would silently filter to whatever the fifth member
    /// happens to be after the next edit to the enum.</summary>
    [Theory]
    [InlineData("readswrites", true)]
    [InlineData("READSWRITES", true)]
    [InlineData("ReadsWrites", true)]
    [InlineData("writes", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    [InlineData("5", false)]
    [InlineData("-1", false)]
    public void A_kind_name_is_parsed_against_the_enum_itself(string? name, bool expected)
        => Assert.Equal(expected, GraphQuery.TryParseEdgeKind(name, out _));
}
