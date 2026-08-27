using System.Text.Json;

using DevContext.Mcp;
using DevContext.Protos;

using Microsoft.Extensions.Logging.Abstractions;

namespace DevContext.Server.Tests;

/// <summary>
/// #36 (F5, Book2Course drive) — <c>usages(JobRunner)</c> answered count: 3 with three IDENTICAL
/// rows: same caller (<c>QueueDrainService.TurnAsync</c>), same kind, same provenance
/// (<c>QueueDrainService.cs:95</c>) for ONE call site.
///
/// <para>The wire is not the defect: the server keeps <c>direction:"usages"</c> as the raw
/// in-walk (one arm with <c>"in"</c>, pinned by <c>NeighborKindEndToEndTests</c>), and its rows
/// carry <c>to</c>, so the member→member and member→Type recordings of one invocation are
/// distinguishable there. THIS projection drops <c>to</c> — the focus IS the target — so rows
/// that differ only in <c>to</c> render identical. The tool must collapse them and count call
/// sites, exactly as <c>GraphQuery.FindUsages</c> does for the CLI (the parity is the point:
/// both surfaces collapse on the same (caller, kind, provenance) key).</para>
/// </summary>
public sealed class McpUsagesCallSiteTests
{
    private const string Handle = "handle-1";
    private const string Turn = "Member:Ns.QueueDrainService::TurnAsync";
    private const string Pump = "Member:Ns.QueueDrainService::PumpAsync";
    private const string Site95 = "QueueDrainService.cs:95";
    private const string Site120 = "QueueDrainService.cs:120";

    /// <summary>The JobRunner shape as the server sends it: one invocation at line 95 recorded
    /// member→Type and member→member (twice), plus a second, genuinely distinct call site.</summary>
    private static NeighborsResponse OneCallSiteThreeRecordings()
    {
        var resp = new NeighborsResponse { TotalEdges = 4 };
        void Add(string from, string to, string resolution, string provenance, string title) =>
            resp.Edges.Add(new Edge
            {
                From = from,
                To = to,
                Kind = "Calls",
                Resolution = resolution,
                Provenance = provenance,
                OtherTitle = title,
            });
        Add(Turn, "Type:Ns.JobRunner", "Syntactic", Site95, "QueueDrainService.TurnAsync");
        Add(Turn, "Member:Ns.JobRunner::RunNextAsync", "Semantic", Site95, "QueueDrainService.TurnAsync");
        Add(Turn, "Member:Ns.JobRunner::RunAsync", "Semantic", Site95, "QueueDrainService.TurnAsync");
        Add(Pump, "Member:Ns.JobRunner::RunAsync", "Semantic", Site120, "QueueDrainService.PumpAsync");
        return resp;
    }

    private static DevContextTools Tools(NeighborsResponse neighbors) => new(
        new DevContextService.DevContextServiceClient(new McpStubCallInvoker(
            rpc => rpc == "GetNeighbors" ? neighbors : null)),
        NullLogger<DevContextTools>.Instance);

    /// <summary>THE RED — before #36 this reported count: 4 with three indistinguishable rows.</summary>
    [Fact]
    public async Task Usages_counts_call_sites_not_recordings()
    {
        var tools = Tools(OneCallSiteThreeRecordings());

        var root = JsonDocument.Parse(await tools.Usages(Handle, nodeId: "Type:Ns.JobRunner")).RootElement;

        Assert.Equal(2, root.GetProperty("count").GetInt32());
        var rows = root.GetProperty("usages").EnumerateArray()
            .Select(r => (Caller: r.GetProperty("caller").GetString(),
                          Kind: r.GetProperty("kind").GetString(),
                          Provenance: r.GetProperty("provenance").GetString()))
            .ToArray();
        Assert.Equal(2, rows.Length);
        // No two rows may agree on everything the projection shows — that is the defect.
        Assert.Equal(rows.Length, rows.Distinct().Count());
        Assert.Contains(rows, r => r.Caller == Turn && r.Provenance == Site95);
        Assert.Contains(rows, r => r.Caller == Pump && r.Provenance == Site120);
    }
}
