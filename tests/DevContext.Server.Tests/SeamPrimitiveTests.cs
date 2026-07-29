using System.Text.Json;

using DevContext.Core.Graph;
using DevContext.Mcp;
using DevContext.Protos;
using DevContext.Server.Mapping;

using Microsoft.Extensions.Logging.Abstractions;

// Core and the contract both have a SeamHop/SeamPath, which is the point — this file is about the
// crossing between them. Alias the engine side so every unqualified name here is the WIRE type.
using SeamHop = DevContext.Core.Graph.SeamHop;
using SeamPath = DevContext.Core.Graph.SeamPath;

namespace DevContext.Server.Tests;

/// <summary>
/// G3.1 (R4 §1 item 8) — <c>seam(from, to)</c> at the wire and the tool.
///
/// <para>The engine half is pinned in <c>GraphSeamTests</c>. What is pinned HERE is the half a
/// checkpoint of this program keeps losing on the floor: a fact the engine computes, maps outward,
/// and no client reads. So the tool assertions read the tool's own JSON, field by field.</para>
///
/// <para>The other thing pinned here is that a dead end names a next step (R4 §3). Three answers
/// are empty in exactly the same way — the ends did not resolve, they resolve but do not connect,
/// and they may well connect just not within the hop budget — and one word for all three is the
/// failure mode this tool exists to avoid.</para>
/// </summary>
public sealed class SeamPrimitiveTests
{
    private const string Handle = "handle-1";

    private static SeamHop Hop(string from, string to, EdgeKind kind = EdgeKind.Calls,
        Resolution res = Resolution.Semantic, string? file = "Src.cs", int? line = 7)
        => new(NodeId.ForType($"Ns.{from}"), NodeId.ForType($"Ns.{to}"), from, to, kind, res, file, line);

    private static SeamResult Forward(params SeamHop[] hops)
        => new(SeamDirection.Forward, [new SeamPath([.. hops])], hops.Length, 1, false);

    // ---- ProtoMapper: the wire ------------------------------------------------------------

    [Fact]
    public void Every_hop_reaches_the_wire_with_its_kind_resolution_and_site()
    {
        var result = Forward(
            Hop("A", "B", EdgeKind.Sends, Resolution.Semantic, "A.cs", 11),
            Hop("B", "C", EdgeKind.Handles, Resolution.Syntactic, "B.cs", 22));

        var resp = ProtoMapper.ToSeamResponse(result, "Type:Ns.A", "A", "Type:Ns.C", "C", maxDepth: 8);

        Assert.True(resp.Found);
        Assert.Equal("forward", resp.Direction);
        Assert.Equal(2, resp.Hops);
        var hops = Assert.Single(resp.Paths).Hops;
        Assert.Equal(["Sends", "Handles"], hops.Select(h => h.Kind));
        Assert.Equal(["Semantic", "Syntactic"], hops.Select(h => h.Resolution));
        Assert.Equal(["A.cs", "B.cs"], hops.Select(h => h.FilePath));
        Assert.Equal([11, 22], hops.Select(h => h.LineNumber));
        Assert.Equal(["Type:Ns.A", "Type:Ns.B"], hops.Select(h => h.FromNodeId));
    }

    /// <summary>A clean, complete answer carries no note — the note is for what the rows cannot say.</summary>
    [Fact]
    public void A_complete_forward_answer_says_nothing_extra()
    {
        var resp = ProtoMapper.ToSeamResponse(Forward(Hop("A", "B")), "Type:Ns.A", "A", "Type:Ns.B", "B", 8);

        Assert.False(resp.HasNote);
    }

    /// <summary>Unconnected and out-of-budget are DIFFERENT answers, and the second names the retry.</summary>
    [Fact]
    public void The_depth_limit_is_named_instead_of_being_reported_as_no_path()
    {
        var exhausted = ProtoMapper.ToSeamResponse(
            new SeamResult(SeamDirection.None, [], 0, 0, StoppedAtDepthLimit: false),
            "Type:Ns.A", "A", "Type:Ns.B", "B", maxDepth: 8);
        var budgeted = ProtoMapper.ToSeamResponse(
            new SeamResult(SeamDirection.None, [], 0, 0, StoppedAtDepthLimit: true),
            "Type:Ns.A", "A", "Type:Ns.B", "B", maxDepth: 8);

        Assert.False(exhausted.Found);
        Assert.Contains("unconnected", exhausted.Note, StringComparison.Ordinal);
        Assert.DoesNotContain("unconnected", budgeted.Note, StringComparison.Ordinal);
        Assert.Contains("within 8 hops", budgeted.Note, StringComparison.Ordinal);
        Assert.Contains("maxDepth: 16", budgeted.Note, StringComparison.Ordinal);   // the retry that works

        // The reply an agent reads most often is the one from a too-tight budget; it should not
        // read "within 1 hops".
        var one = ProtoMapper.ToSeamResponse(
            new SeamResult(SeamDirection.None, [], 0, 0, StoppedAtDepthLimit: true),
            "Type:Ns.A", "A", "Type:Ns.B", "B", maxDepth: 1);
        Assert.Contains("within 1 hop,", one.Note, StringComparison.Ordinal);
    }

    /// <summary>A reverse hit must never read as the answer to the question that was asked.</summary>
    [Fact]
    public void A_reverse_hit_says_which_way_round_it_runs()
    {
        var resp = ProtoMapper.ToSeamResponse(
            new SeamResult(SeamDirection.Reverse, [new SeamPath([Hop("B", "A")])], 1, 1, false),
            "Type:Ns.A", "A", "Type:Ns.B", "B", maxDepth: 8);

        Assert.True(resp.Found);
        Assert.Equal("reverse", resp.Direction);
        Assert.Contains("OTHER way", resp.Note, StringComparison.Ordinal);
    }

    /// <summary>Shown-of-total: a page that does not say it is a page is the G1.4 defect again.</summary>
    [Fact]
    public void An_answer_that_holds_some_of_the_paths_says_how_many_it_left_out()
    {
        var resp = ProtoMapper.ToSeamResponse(
            new SeamResult(SeamDirection.Forward, [new SeamPath([Hop("A", "B")])], 1, TotalPaths: 12, false),
            "Type:Ns.A", "A", "Type:Ns.B", "B", maxDepth: 8);

        Assert.Equal(12, resp.TotalPaths);
        Assert.Contains("Showing 1 of 12", resp.Note, StringComparison.Ordinal);
    }

    // ---- The MCP tool ---------------------------------------------------------------------

    private static (DevContextTools Tools, List<SeamRequest> Sent) ToolsReturning(SeamResponse seam)
    {
        var sent = new List<SeamRequest>();
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
                rpc => rpc == "GetSeam" ? seam : null,
                (rpc, req) => { if (rpc == "GetSeam" && req is SeamRequest s) sent.Add(s.Clone()); })),
            NullLogger<DevContextTools>.Instance);
        return (tools, sent);
    }

    private static SeamResponse WireAnswer()
        => ProtoMapper.ToSeamResponse(
            Forward(Hop("A", "B", EdgeKind.Sends, Resolution.Semantic, "A.cs", 11),
                    Hop("B", "C", EdgeKind.Handles, Resolution.Syntactic, "B.cs", 22)) with { TotalPaths = 4 },
            "Type:Ns.A", "A", "Type:Ns.C", "C", maxDepth: 8);

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    /// <summary>
    /// Field by field: everything the server computes has to arrive at the agent. Seven fields have
    /// shipped dead in this repo before the contract sweep existed, and the sweep cannot see a field
    /// that a client reads with the wrong key (S10's Insight.Severity) — so read them here.
    /// </summary>
    [Fact]
    public async Task The_tool_renders_every_field_the_server_filled_in()
    {
        var (tools, _) = ToolsReturning(WireAnswer());

        var root = Parse(await tools.Seam(Handle, "A", "C"));

        Assert.True(root.GetProperty("found").GetBoolean());
        Assert.Equal("forward", root.GetProperty("direction").GetString());
        Assert.Equal(2, root.GetProperty("hops").GetInt32());
        Assert.Equal(4, root.GetProperty("totalPaths").GetInt32());
        Assert.Equal("Type:Ns.A", root.GetProperty("from").GetProperty("nodeId").GetString());
        Assert.Equal("C", root.GetProperty("to").GetProperty("title").GetString());
        Assert.Contains("Showing 1 of 4", root.GetProperty("note").GetString()!, StringComparison.Ordinal);

        var hops = root.GetProperty("paths")[0].EnumerateArray().ToArray();
        Assert.Equal(2, hops.Length);
        Assert.Equal("Sends", hops[0].GetProperty("kind").GetString());
        Assert.Equal("Semantic", hops[0].GetProperty("resolution").GetString());
        Assert.Equal("A.cs:11", hops[0].GetProperty("site").GetString());
        Assert.Equal("B", hops[0].GetProperty("to").GetString());
        Assert.Equal("Handles", hops[1].GetProperty("kind").GetString());
    }

    /// <summary>The dials must travel on the REQUEST — a depth the server never saw cannot have
    /// bounded the walk it did (the shape of R4 item 6's defect, one tool over).</summary>
    [Fact]
    public async Task The_dials_go_out_on_the_request()
    {
        var (tools, sent) = ToolsReturning(WireAnswer());

        await tools.Seam(Handle, "A", "C", maxDepth: 12, maxPaths: 5);

        var req = Assert.Single(sent);
        Assert.Equal("A", req.From);
        Assert.Equal("C", req.To);
        Assert.Equal(12, req.MaxDepth);
        Assert.Equal(5, req.MaxPaths);
    }

    /// <summary>An end that resolved to nothing is not "no path" — it gets candidates and a retry.</summary>
    [Fact]
    public async Task An_unresolved_end_returns_candidates_not_a_false_negative()
    {
        var unresolved = new SeamResponse
        {
            Found = false,
            Direction = "none",
            FromNodeId = "Type:Ns.A",
            FromTitle = "A",
            ToNodeId = "",
            ToTitle = "",
            Note = "'Zzz' did not resolve to a node — that is not the same as no path.",
        };
        var sent = new List<string>();
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
                rpc => rpc switch
                {
                    "GetSeam" => unresolved,
                    "SearchNodes" => Suggestions(),
                    _ => null,
                },
                (rpc, _) => sent.Add(rpc))),
            NullLogger<DevContextTools>.Instance);

        var root = Parse(await tools.Seam(Handle, "A", "Zzz"));

        Assert.Contains("did not resolve", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
        Assert.Contains("not the same as no path", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
        Assert.NotEmpty(root.GetProperty("candidates").EnumerateArray());
        Assert.Contains("SearchNodes", sent);      // it went and found them
    }

    /// <summary>Both ends are required, and saying so costs no round trip.</summary>
    [Fact]
    public async Task One_end_is_not_a_seam()
    {
        var (tools, sent) = ToolsReturning(WireAnswer());

        var root = Parse(await tools.Seam(Handle, "A"));

        Assert.Contains("Missing", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
        Assert.Empty(sent);
    }

    private static SearchResponse Suggestions()
    {
        var resp = new SearchResponse { TotalMatches = 2 };
        resp.Nodes.Add(new NodeRef { NodeId = "Type:Ns.Zebra", Title = "Zebra", Kind = "Type" });
        resp.Nodes.Add(new NodeRef { NodeId = "Type:Ns.Zoo", Title = "Zoo", Kind = "Type" });
        return resp;
    }
}
