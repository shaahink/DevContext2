using System.Collections.Immutable;
using System.Text.Json;

using DevContext.Core.Graph;
using DevContext.Mcp;
using DevContext.Protos;
using DevContext.Server.Mapping;

using Microsoft.Extensions.Logging.Abstractions;

// Core and the contract both have an EdgeKindCount, which is the point of this file — it is about
// the crossing between them. The engine side is the one built by hand here, so it takes the bare
// name; the wire side is only ever reached through a response property, which needs no name.
using EdgeKindCount = DevContext.Core.Graph.EdgeKindCount;

namespace DevContext.Server.Tests;

/// <summary>
/// G3.2 (R4 §1 item 9) — the kind filter at the wire and the tool.
///
/// <para>The engine half is pinned in <c>GraphNeighborKindTests</c>. What is pinned HERE is the
/// failure mode that belongs to a FILTER specifically: an unknown kind that quietly does not
/// filter. That reply is indistinguishable from a correct one — same shape, same field names, a
/// plausible list — and the caller believes an answer to a question the server never asked. It is
/// strictly worse than an error, so it is an error.</para>
///
/// <para>The second thing pinned here is R4 §3's bar: every dead-end reply names a next call that
/// WORKS. Three empty lists arrive by three different routes — the node has nothing, the kind
/// matched nothing, the kind was not a kind — and one empty list for all three is the defect this
/// item exists to remove.</para>
/// </summary>
public sealed class NeighborKindFilterTests
{
    private const string Handle = "handle-1";

    // A node with a real mix, matching the engine fixture's shape: the busiest kind first.
    private static NeighborView View(params (EdgeKind Kind, int Count)[] kinds)
    {
        var edges = ImmutableArrayOf(kinds);
        return new NeighborView(
            edges,
            edges.Length,
            [.. kinds.Select(k => new EdgeKindCount(k.Kind, k.Count))]);
    }

    private static ImmutableArray<EdgeRef> ImmutableArrayOf((EdgeKind Kind, int Count)[] kinds)
    {
        var b = ImmutableArray.CreateBuilder<EdgeRef>();
        foreach (var (kind, count) in kinds)
            for (var i = 0; i < count; i++)
                b.Add(new EdgeRef(
                    NodeId.ForMember("Ns.Caller", $"M{i}"), NodeId.ForType("Ns.OrdersTable"),
                    kind, Resolution.Semantic, "seam", "OrdersTable"));
        return b.ToImmutable();
    }

    /// <summary>The engine's answer when the filter matched nothing: no rows, but the unfiltered
    /// facts intact — that is exactly what <see cref="NeighborView"/> promises.</summary>
    private static NeighborView Matched_nothing(params (EdgeKind Kind, int Count)[] present)
    {
        var full = View(present);
        return full with { Edges = [] };
    }

    // ---- ProtoMapper: the wire ------------------------------------------------------------

    /// <summary>
    /// THE ONE THAT MATTERS. An unknown kind must not come back as the unfiltered list wearing the
    /// caller's filter name. The rows are empty ON PURPOSE even though the view holds every edge,
    /// and the note names the vocabulary — read from the enum, so it cannot drift from what the
    /// server accepts.
    /// </summary>
    [Fact]
    public void An_unknown_kind_returns_no_rows_and_says_which_kinds_exist()
    {
        var resp = ProtoMapper.ToNeighborsResponse(
            View((EdgeKind.ReadsWrites, 4), (EdgeKind.Calls, 2)), requestedKind: "writes", unknownKind: true);

        Assert.Empty(resp.Edges);                    // NOT the 6 edges the view is holding
        Assert.Equal(6, resp.TotalEdges);            // ...but the count is still true
        Assert.Contains("Unknown edge kind 'writes'", resp.Note, StringComparison.Ordinal);
        foreach (var name in GraphQuery.EdgeKindNames)
            Assert.Contains(name, resp.Note, StringComparison.Ordinal);
    }

    /// <summary>A valid kind that matched nothing is a DIFFERENT answer, and it carries the retry:
    /// what this node does have, with counts.</summary>
    [Fact]
    public void A_kind_that_matched_nothing_names_what_is_here()
    {
        var resp = ProtoMapper.ToNeighborsResponse(
            Matched_nothing((EdgeKind.ReadsWrites, 4), (EdgeKind.Calls, 2)), "Sends", unknownKind: false);

        Assert.Empty(resp.Edges);
        Assert.Equal(6, resp.TotalEdges);
        Assert.Contains("No 'Sends' edges here", resp.Note, StringComparison.Ordinal);
        Assert.Contains("ReadsWrites 4", resp.Note, StringComparison.Ordinal);
        Assert.Contains("Calls 2", resp.Note, StringComparison.Ordinal);
        // The two dead ends must not read alike.
        Assert.DoesNotContain("Unknown edge kind", resp.Note, StringComparison.Ordinal);
    }

    /// <summary>Every kind present reaches the wire with its count — this is the field a caller
    /// retries from, and a count that never left the server is the dead-field class.</summary>
    [Fact]
    public void Kinds_present_reaches_the_wire_with_its_counts()
    {
        var resp = ProtoMapper.ToNeighborsResponse(
            View((EdgeKind.ReadsWrites, 4), (EdgeKind.Calls, 2)), requestedKind: null, unknownKind: false);

        Assert.Equal(["ReadsWrites", "Calls"], resp.KindsPresent.Select(k => k.Kind));
        Assert.Equal([4, 2], resp.KindsPresent.Select(k => k.Count));
        Assert.Equal(6, resp.Edges.Count);
    }

    /// <summary>A clean answer carries no note — the note is for what the rows cannot say. (Same
    /// rule as the seam response, one RPC over.)</summary>
    [Fact]
    public void A_complete_answer_says_nothing_extra()
    {
        var unfiltered = ProtoMapper.ToNeighborsResponse(View((EdgeKind.Calls, 2)), null, false);
        var filtered = ProtoMapper.ToNeighborsResponse(View((EdgeKind.Calls, 2)), "Calls", false);

        Assert.False(unfiltered.HasNote);
        Assert.False(filtered.HasNote);
    }

    /// <summary>A node with genuinely nothing must not be dressed up with a retry that cannot
    /// work — there is no kind to suggest, and "no edges at all" is the honest answer.</summary>
    [Fact]
    public void A_node_with_no_edges_at_all_gets_no_false_retry()
    {
        var empty = new NeighborView([], 0, []);

        var resp = ProtoMapper.ToNeighborsResponse(empty, "ReadsWrites", unknownKind: false);

        Assert.Empty(resp.Edges);
        Assert.Equal(0, resp.TotalEdges);
        Assert.Empty(resp.KindsPresent);
        Assert.False(resp.HasNote);   // nothing to retry with; the zeros say it
    }

    // ---- The MCP tool ---------------------------------------------------------------------

    private static (DevContextTools Tools, List<NeighborsRequest> Sent) ToolsReturning(NeighborsResponse neighbors)
    {
        var sent = new List<NeighborsRequest>();
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
                rpc => rpc == "GetNeighbors" ? neighbors : null,
                (rpc, req) => { if (rpc == "GetNeighbors" && req is NeighborsRequest n) sent.Add(n.Clone()); })),
            NullLogger<DevContextTools>.Instance);
        return (tools, sent);
    }

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    /// <summary>
    /// Field by field. The contract sweep catches a field NO client reads; it cannot catch a field
    /// read under the wrong key (S10's <c>Insight.Severity</c> shipped three spellings and the
    /// Insights page never rendered). So the tool's own JSON is read here, by the names an agent
    /// actually receives.
    /// </summary>
    [Fact]
    public async Task The_tool_renders_every_field_the_server_filled_in()
    {
        var wire = ProtoMapper.ToNeighborsResponse(
            View((EdgeKind.ReadsWrites, 2), (EdgeKind.Calls, 1)), "ReadsWrites", unknownKind: false);
        var (tools, _) = ToolsReturning(wire);

        var root = Parse(await tools.Neighbors(Handle, "Ns.OrdersTable", "in", kind: "ReadsWrites"));

        Assert.Equal("ReadsWrites", root.GetProperty("kind").GetString());
        Assert.Equal(3, root.GetProperty("count").GetInt32());
        Assert.Equal(3, root.GetProperty("totalEdges").GetInt32());
        var kinds = root.GetProperty("kindsPresent").EnumerateArray().ToArray();
        Assert.Equal(["ReadsWrites", "Calls"], kinds.Select(k => k.GetProperty("kind").GetString()));
        Assert.Equal([2, 1], kinds.Select(k => k.GetProperty("count").GetInt32()));
    }

    /// <summary>The filter must travel on the REQUEST. A kind the server never saw cannot have
    /// filtered the walk it did — this is R4 item 6's defect (find's client-side kind filter,
    /// fixed in G1.4) one RPC over, and it is the reason that item existed at all.</summary>
    [Fact]
    public async Task The_kind_goes_out_on_the_request()
    {
        var (tools, sent) = ToolsReturning(
            ProtoMapper.ToNeighborsResponse(View((EdgeKind.ReadsWrites, 1)), "ReadsWrites", false));

        await tools.Neighbors(Handle, "Ns.OrdersTable", "in", kind: "ReadsWrites");

        var req = Assert.Single(sent);
        Assert.True(req.HasKind);
        Assert.Equal("ReadsWrites", req.Kind);
        Assert.Equal("in", req.Direction);
    }

    /// <summary>No kind asked for means no kind on the wire — an optional field that is always set
    /// is not optional, and the server would read "" as a filter request.</summary>
    [Fact]
    public async Task No_kind_asked_for_puts_no_kind_on_the_request()
    {
        var (tools, sent) = ToolsReturning(
            ProtoMapper.ToNeighborsResponse(View((EdgeKind.Calls, 1)), null, false));

        await tools.Neighbors(Handle, "Ns.OrdersTable", "out");

        Assert.False(Assert.Single(sent).HasKind);
    }

    /// <summary>
    /// A filter that matched nothing is NOT a missing node. Before G3.2 the tool read an empty edge
    /// list as "this node may not exist" and spent a resolve round-trip proving otherwise; with a
    /// filter on, that path would fire on every pointed question that happens to have no hits and
    /// report "not found" about a node the server just described. The unfiltered count settles it
    /// without a second call.
    /// </summary>
    [Fact]
    public async Task A_filter_that_matched_nothing_is_not_reported_as_a_missing_node()
    {
        var wire = ProtoMapper.ToNeighborsResponse(
            Matched_nothing((EdgeKind.ReadsWrites, 4)), "Sends", unknownKind: false);
        var (tools, _) = ToolsReturning(wire);

        var root = Parse(await tools.Neighbors(Handle, "Ns.OrdersTable", "in", kind: "Sends"));

        Assert.Equal(0, root.GetProperty("count").GetInt32());
        Assert.Equal(4, root.GetProperty("totalEdges").GetInt32());
        Assert.Contains("No 'Sends' edges here", root.GetProperty("note").GetString()!, StringComparison.Ordinal);
        // The not-found envelope has these instead; none of them may appear.
        Assert.False(root.TryGetProperty("error", out _));
        Assert.False(root.TryGetProperty("suggestion", out _));
    }

    /// <summary>The unknown-kind note has to survive the whole way to the agent — the server saying
    /// it and the tool dropping it is the same silent filter, one layer up.</summary>
    [Fact]
    public async Task The_unknown_kind_note_reaches_the_agent()
    {
        var wire = ProtoMapper.ToNeighborsResponse(
            View((EdgeKind.ReadsWrites, 4)), "writes", unknownKind: true);
        var (tools, _) = ToolsReturning(wire);

        var root = Parse(await tools.Neighbors(Handle, "Ns.OrdersTable", "in", kind: "writes"));

        Assert.Equal(0, root.GetProperty("count").GetInt32());
        Assert.Contains("Unknown edge kind 'writes'", root.GetProperty("note").GetString()!, StringComparison.Ordinal);
        Assert.Contains("ReadsWrites", root.GetProperty("note").GetString()!, StringComparison.Ordinal);
    }

    /// <summary>A clean answer carries no note field at all — an always-present null is noise in
    /// every reply an agent reads.</summary>
    [Fact]
    public async Task A_clean_answer_carries_no_note_field()
    {
        var (tools, _) = ToolsReturning(
            ProtoMapper.ToNeighborsResponse(View((EdgeKind.Calls, 2)), null, false));

        var root = Parse(await tools.Neighbors(Handle, "Ns.OrdersTable", "out"));

        Assert.False(root.TryGetProperty("note", out _));
    }
}
