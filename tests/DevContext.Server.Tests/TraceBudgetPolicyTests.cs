using System.Text.Json;

using DevContext.Core.Graph;
using DevContext.Mcp;
using DevContext.Protos;

using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Server.Tests;

/// <summary>
/// G2.2 (R4 §1 item 12) — ONE trace policy, read from <see cref="TracePolicy"/>, across MCP / CLI /
/// server.
///
/// <para>The divergence the audit named ("4000 in the MCP vs unlimited in <c>query --op trace</c> vs
/// shaped in the server") had a single cause: every client restated the policy's dials as its own
/// literals. The MCP tool's signature said <c>int depth = 6, int budgetTokens = 4000</c>, and a C#
/// default is not an unset field — the tool ASSIGNED both on every call, so a proto3 optional's
/// presence bit was always set and the server's own defaults were unreachable.</para>
///
/// <para>The cost was bigger than a number: <see cref="TracePolicy.ElasticDepth"/> fires only when
/// the caller left the depth to the server (<c>explicitDepth: request.HasDepth</c> is its one gate),
/// and no client has ever left it. Batch E's budget-elastic deepening was dead by construction.</para>
///
/// <para>These tests live here, not in the MCP project, for the reason G1.3 recorded:
/// <c>DevContext.Mcp</c> is a gRPC client that deliberately does not reference Core, so it cannot
/// check itself against the policy it must obey.</para>
/// </summary>
public sealed class TraceBudgetPolicyTests
{
    /// <summary>Runs a tool call against a stub server and hands back the request it put on the wire.</summary>
    private static async Task<TraceRequest> CapturedTraceRequest(Func<DevContextTools, Task<string>> call)
    {
        TraceRequest? seen = null;
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
                rpc => rpc == "GetTrace" ? new TraceResponse { Found = true } : null,
                (rpc, req) => { if (rpc == "GetTrace") seen = (TraceRequest)req; })),
            NullLogger<DevContextTools>.Instance);

        await call(tools);
        Assert.NotNull(seen);
        return seen;
    }

    /// <summary>
    /// The whole fix, at the wire. A dial the caller did not name must not be SENT — that absence is
    /// the only thing that lets the server apply its policy, and the only way ElasticDepth can run.
    /// </summary>
    [Fact]
    public async Task An_unnamed_dial_is_not_put_on_the_wire()
    {
        var req = await CapturedTraceRequest(t => t.Trace("h", "POST /api/orders"));

        Assert.False(req.HasDepth);
        Assert.False(req.HasBudgetTokens);
    }

    [Fact]
    public async Task A_named_dial_is_sent_exactly_as_given()
    {
        var req = await CapturedTraceRequest(t => t.Trace("h", "POST /api/orders", depth: 9, budgetTokens: 1500));

        Assert.True(req.HasDepth);
        Assert.Equal(9, req.Depth);
        Assert.True(req.HasBudgetTokens);
        Assert.Equal(1500, req.BudgetTokens);
    }

    /// <summary>Zero is a REQUEST for the full tree, not a missing value — the desktop sends it.</summary>
    [Fact]
    public async Task Zero_is_an_explicit_full_tree_not_an_absent_budget()
    {
        var req = await CapturedTraceRequest(t => t.Trace("h", "POST /api/orders", budgetTokens: 0));

        Assert.True(req.HasBudgetTokens);
        Assert.Equal(0, req.BudgetTokens);
    }

    /// <summary>
    /// The reply says WHOSE budget shaped it. The MCP no longer knows the number — that is the
    /// point — so it must not quote one; naming the source is the honest form.
    /// </summary>
    [Theory]
    [InlineData(null, "server trace policy")]
    [InlineData(2500, "caller")]
    public async Task The_reply_names_the_source_of_the_budget(int? budget, string expected)
    {
        var tools = new DevContextTools(
            new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
                rpc => rpc == "GetTrace" ? new TraceResponse { Found = true } : null)),
            NullLogger<DevContextTools>.Instance);

        using var doc = JsonDocument.Parse(await tools.Trace("h", "POST /api/orders", budgetTokens: budget));

        Assert.Equal(expected, doc.RootElement.GetProperty("budgetSource").GetString());
    }

    /// <summary>
    /// The policy's own rule, at the engine. Elasticity may only ever DEEPEN, only when the caller
    /// left the depth alone, and only when the first walk both hit the limit and left budget spare —
    /// a stated dial is not a suggestion (Batch E's rule).
    /// </summary>
    [Theory]
    // built, used, budget, hitLimit  ->  expected
    [InlineData(6, 400, 4000, true, 9)]     // room to spare and more to find: deepen, by 3
    [InlineData(6, 400, 4000, false, 6)]    // nothing deeper to find: stay
    [InlineData(6, 3000, 4000, true, 6)]    // already past half the budget: stay
    [InlineData(6, 400, 0, true, 6)]        // no budget at all: nothing to be elastic about
    [InlineData(11, 400, 4000, true, 12)]   // the ceiling holds: a trace deeper than 12 is a dump
    public void Elastic_depth_only_deepens_when_the_caller_left_the_depth_alone(
        int built, int used, int budget, bool hitLimit, int expected)
        => Assert.Equal(expected, TracePolicy.ElasticDepth(built, used, budget, hitLimit));

    /// <summary>One default, and it is the policy's — not a literal that happens to match.</summary>
    [Fact]
    public void The_budget_default_is_a_real_budget_and_the_reserve_fits_inside_it()
    {
        Assert.True(TracePolicy.DefaultBudgetTokens > 0, "an unspecified budget must still be a budget");
        // The tree's share of the default has to clear the floor, or the default shapes every trace
        // down to the minimum and the dial stops meaning anything.
        Assert.True(TracePolicy.TreeBudget(TracePolicy.DefaultBudgetTokens) > TracePolicy.MinTreeBudget,
            $"TreeBudget({TracePolicy.DefaultBudgetTokens}) collapsed to the floor");
    }
}
