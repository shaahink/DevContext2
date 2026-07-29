using DevContext.Protos;

using Grpc.Core;
using Grpc.Net.Client;

namespace DevContext.Server.Tests;

/// <summary>
/// G3.2 (R4 §1 item 9) — the kind filter over the REAL service, on a real analyzed snapshot.
///
/// <para><c>NeighborKindFilterTests</c> pins the mapper and the tool with a hand-built view. Neither
/// touches <c>DevContextGrpcService.GetNeighbors</c>, and that method is where the two decisions
/// this checkpoint is actually about are made: which direction <c>"usages"</c> means now that it has
/// to accept a kind, and whether an unparseable kind is refused or falls through as "no filter". A
/// mapper test cannot see either, because in both cases the mapper is handed a view that has already
/// had the decision applied to it.</para>
///
/// <para>The expectations are derived from the snapshot at run time rather than hard-coded, so this
/// stays a test about the filter and does not turn into a golden of the ControllerApp fixture that
/// breaks whenever an extractor improves.</para>
/// </summary>
public sealed class NeighborKindEndToEndTests(ServerTestFactory factory)
    : IClassFixture<ServerTestFactory>
{
    private DevContextService.DevContextServiceClient CreateClient()
    {
        var http = factory.CreateClient();
        var channel = GrpcChannel.ForAddress(http.BaseAddress!, new GrpcChannelOptions { HttpClient = http });
        return new DevContextService.DevContextServiceClient(channel);
    }

    /// <summary>Every node the snapshot will show us. NOTE: an empty query is not a wildcard —
    /// SearchNodes("") returns 0 nodes (measured, 2026-07-29), so the seed is a substring common
    /// enough to reach most of the fixture (15 of its 18 nodes).</summary>
    private static async Task<IReadOnlyList<NodeRef>> AllNodes(
        DevContextService.DevContextServiceClient client, string handle)
        => (await client.SearchNodesAsync(new SearchRequest { Handle = handle, Query = "a", Limit = 200 })).Nodes;

    /// <summary>A node that actually has out-edges — a filter test on a leaf proves nothing.</summary>
    private static async Task<(string Handle, string NodeId, NeighborsResponse Unfiltered)>
        ANodeWithEdges(DevContextService.DevContextServiceClient client)
    {
        var handle = await AnalyzeControllerApp(client);

        foreach (var node in await AllNodes(client, handle))
        {
            var resp = await client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle,
                NodeId = node.NodeId,
                Direction = "out",
            });
            if (resp.TotalEdges > 0 && resp.KindsPresent.Count > 0)
                return (handle, node.NodeId, resp);
        }

        throw new InvalidOperationException("No node in the ControllerApp snapshot has out-edges.");
    }

    /// <summary>
    /// The filter runs server-side and the unfiltered facts survive it: rows shrink to exactly the
    /// asked-for kind, while total_edges and kinds_present stay describing the whole direction.
    /// That pairing is the entire honesty contract of this item — a count that shrinks with the rows
    /// cannot tell a caller what to ask instead.
    /// </summary>
    [Fact]
    public async Task The_filter_narrows_the_rows_and_leaves_the_totals_alone()
    {
        var client = CreateClient();
        var (handle, nodeId, unfiltered) = await ANodeWithEdges(client);
        var busiest = unfiltered.KindsPresent[0];

        var filtered = await client.GetNeighborsAsync(new NeighborsRequest
        {
            Handle = handle,
            NodeId = nodeId,
            Direction = "out",
            Kind = busiest.Kind,
        });

        Assert.Equal(busiest.Count, filtered.Edges.Count);
        Assert.All(filtered.Edges, e => Assert.Equal(busiest.Kind, e.Kind));
        Assert.Equal(unfiltered.TotalEdges, filtered.TotalEdges);
        Assert.Equal(
            unfiltered.KindsPresent.Select(k => (k.Kind, k.Count)).ToArray(),
            filtered.KindsPresent.Select(k => (k.Kind, k.Count)).ToArray());
        Assert.False(filtered.HasNote);   // it matched; there is nothing the rows cannot say
    }

    /// <summary>
    /// THE ONE A MAPPER TEST CANNOT REACH. An unparseable kind must be refused by the service, not
    /// parsed to a default and not dropped to "no filter". The proof that it was not dropped is that
    /// the row count is zero while total_edges is not — the unfiltered list was RIGHT THERE and did
    /// not come back wearing the caller's filter name.
    /// </summary>
    [Fact]
    public async Task An_unparseable_kind_is_refused_by_the_service_not_ignored()
    {
        var client = CreateClient();
        var (handle, nodeId, unfiltered) = await ANodeWithEdges(client);

        var resp = await client.GetNeighborsAsync(new NeighborsRequest
        {
            Handle = handle,
            NodeId = nodeId,
            Direction = "out",
            Kind = "writes",          // the plausible guess: the real kind is ReadsWrites
        });

        Assert.Empty(resp.Edges);
        Assert.True(unfiltered.TotalEdges > 0);
        Assert.Equal(unfiltered.TotalEdges, resp.TotalEdges);
        Assert.Contains("Unknown edge kind 'writes'", resp.Note, StringComparison.Ordinal);
        Assert.Contains("ReadsWrites", resp.Note, StringComparison.Ordinal);
    }

    /// <summary>
    /// <c>direction:"usages"</c> and <c>direction:"in"</c> are the same walk, so they must honour the
    /// kind the same way. They are one arm in the service precisely so they cannot drift — this pins
    /// that they have not, including the totals.
    /// </summary>
    [Fact]
    public async Task Usages_honours_the_kind_exactly_as_the_in_direction_does()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var compared = 0;
        foreach (var node in await AllNodes(client, handle))
        {
            var inbound = await client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle, NodeId = node.NodeId, Direction = "in",
            });
            if (inbound.KindsPresent.Count == 0) continue;

            var kind = inbound.KindsPresent[0].Kind;
            var asIn = await client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle, NodeId = node.NodeId, Direction = "in", Kind = kind,
            });
            var asUsages = await client.GetNeighborsAsync(new NeighborsRequest
            {
                Handle = handle, NodeId = node.NodeId, Direction = "usages", Kind = kind,
            });

            Assert.Equal(asIn.Edges.Select(e => (e.From, e.To, e.Kind)), asUsages.Edges.Select(e => (e.From, e.To, e.Kind)));
            Assert.Equal(asIn.TotalEdges, asUsages.TotalEdges);
            Assert.True(asUsages.Edges.Count > 0);
            compared++;
            if (compared == 5) break;
        }

        Assert.True(compared > 0, "No node in the snapshot had in-edges to compare.");
    }

    /// <summary>An existing caller that sends no kind gets exactly the list it got before G3.2 —
    /// the new fields are additive, and the rows are the same rows.</summary>
    [Fact]
    public async Task An_unfiltered_call_is_unchanged_and_its_rows_are_the_whole_set()
    {
        var client = CreateClient();
        var (_, _, unfiltered) = await ANodeWithEdges(client);

        Assert.Equal(unfiltered.TotalEdges, unfiltered.Edges.Count);
        Assert.Equal(unfiltered.TotalEdges, unfiltered.KindsPresent.Sum(k => k.Count));
        Assert.False(unfiltered.HasNote);
    }

    private static async Task<string> AnalyzeControllerApp(DevContextService.DevContextServiceClient client)
    {
        var fixture = FixturePath("ControllerApp");
        string? handle = null;
        using var call = client.Analyze(new AnalyzeRequest { Path = fixture, NoRoslyn = true });
        await foreach (var evt in call.ResponseStream.ReadAllAsync())
        {
            if (evt.EventCase == AnalyzeEvent.EventOneofCase.Result)
                handle = evt.Result.Handle;
        }
        Assert.NotNull(handle);
        return handle!;
    }

    private static string FixturePath(string name) => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures", name));
}
