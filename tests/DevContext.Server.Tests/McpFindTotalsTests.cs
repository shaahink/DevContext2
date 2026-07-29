using System.Text.Json;

using DevContext.Mcp;
using DevContext.Protos;

using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Server.Tests;

/// <summary>
/// G1.4 (R4 §1 item 6) — what <c>find</c>'s total and hasMore describe.
///
/// The defect: the MCP over-fetched (limit + cursor + 20), filtered THAT page by kind in LINQ, and
/// reported the survivors as <c>total</c>. A filter downstream of a truncation can only describe the
/// window it was handed, so on eShop find("Order", limit:100) answered total=120 — which is
/// limit+20, the fetch size, not a fact about the repo — and find("Order", kind:"Type", limit:5)
/// answered 22, meaning "Types among the first 25 matches"
/// (eval-results/2026-07-29/mcp-r4-g14-before/find-kind-eshop.json).
///
/// The tell is that both numbers MOVED WITH THE PAGE SIZE, which is what these tests pin: the page
/// comes from the response, the total comes from the server, and the kind goes out on the request.
/// </summary>
public sealed class McpFindTotalsTests
{
    private const string Handle = "handle-1";

    private static SearchResponse Page(int nodesInPage, int totalMatches, string kind = "Type")
    {
        var resp = new SearchResponse { TotalMatches = totalMatches };
        for (var i = 0; i < nodesInPage; i++)
            resp.Nodes.Add(new NodeRef { NodeId = $"n{i}", Title = $"Order{i}", Kind = kind });
        return resp;
    }

    private static (DevContextTools Tools, List<SearchRequest> Sent) ToolsReturning(SearchResponse page)
    {
        var sent = new List<SearchRequest>();
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
                rpc => rpc == "SearchNodes" ? page : null,
                (rpc, req) => { if (rpc == "SearchNodes" && req is SearchRequest s) sent.Add(s.Clone()); })),
            NullLogger<DevContextTools>.Instance);
        return (tools, sent);
    }

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    /// <summary>The total is the server's count of everything that matched, not the page length.</summary>
    [Fact]
    public async Task Total_is_the_servers_match_count_not_the_page()
    {
        var (tools, _) = ToolsReturning(Page(nodesInPage: 5, totalMatches: 137));

        var root = Parse(await tools.Find(Handle, "Order", limit: 5));

        Assert.Equal(5, root.GetProperty("count").GetInt32());
        Assert.Equal(137, root.GetProperty("total").GetInt32());
        Assert.True(root.GetProperty("hasMore").GetBoolean());
    }

    /// <summary>hasMore is derived from that total — false exactly when one page held everything.</summary>
    [Fact]
    public async Task HasMore_is_false_when_the_page_holds_every_match()
    {
        var (tools, _) = ToolsReturning(Page(nodesInPage: 3, totalMatches: 3));

        var root = Parse(await tools.Find(Handle, "Order", limit: 20));

        Assert.Equal(3, root.GetProperty("total").GetInt32());
        Assert.False(root.GetProperty("hasMore").GetBoolean());
    }

    /// <summary>
    /// The kind must travel ON THE REQUEST. This is the whole item: a kind the server never saw
    /// cannot have narrowed the set the server counted.
    /// </summary>
    [Fact]
    public async Task Kind_goes_out_on_the_request()
    {
        var (tools, sent) = ToolsReturning(Page(nodesInPage: 2, totalMatches: 9));

        await tools.Find(Handle, "Order", kind: "Type", limit: 5);

        var req = Assert.Single(sent);
        Assert.True(req.HasKind);
        Assert.Equal("Type", req.Kind);
        Assert.Equal(Handle, req.Handle);
    }

    /// <summary>No kind means no kind — an absent filter must not become an empty-string one.</summary>
    [Fact]
    public async Task No_kind_leaves_the_request_unfiltered()
    {
        var (tools, sent) = ToolsReturning(Page(nodesInPage: 2, totalMatches: 9));

        await tools.Find(Handle, "Order", limit: 5);

        Assert.False(Assert.Single(sent).HasKind);
    }

    /// <summary>
    /// The old over-fetch (limit + cursor + 20) existed only to make a client-side total plausible.
    /// With the server counting, asking for more rows than the page needs is dead weight on every
    /// call — and it was never enough rows to page past 20 anyway.
    /// </summary>
    [Fact]
    public async Task It_asks_for_the_rows_the_page_needs_and_no_more()
    {
        var (tools, sent) = ToolsReturning(Page(nodesInPage: 15, totalMatches: 60));

        await tools.Find(Handle, "Order", limit: 10, cursor: 5);

        Assert.Equal(15, Assert.Single(sent).Limit);
    }

    /// <summary>A cursor past the end is a true empty page over a real total, not "no matches".</summary>
    [Fact]
    public async Task A_cursor_past_the_end_says_so_instead_of_denying_the_matches()
    {
        var (tools, _) = ToolsReturning(Page(nodesInPage: 4, totalMatches: 4));

        var root = Parse(await tools.Find(Handle, "Order", limit: 5, cursor: 99));

        Assert.Equal(0, root.GetProperty("count").GetInt32());
        Assert.Equal(4, root.GetProperty("total").GetInt32());
        Assert.False(root.GetProperty("hasMore").GetBoolean());
        Assert.Contains("past the last", root.GetProperty("hint").GetString()!, StringComparison.Ordinal);
    }

    /// <summary>Zero matches keeps the did-you-mean envelope — the dead-end must name a next step.</summary>
    [Fact]
    public async Task Zero_matches_still_returns_the_envelope()
    {
        var (tools, _) = ToolsReturning(Page(nodesInPage: 0, totalMatches: 0));

        var root = Parse(await tools.Find(Handle, "Zzz", kind: "Type"));

        Assert.Contains("No nodes match", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
        Assert.Contains("Type", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
    }
}
