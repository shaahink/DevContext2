using DevContext.Protos;

using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.AspNetCore.Mvc.Testing;

namespace DevContext.Server.Tests;

/// <summary>End-to-end gRPC tests over the real composition root (in-memory test host). Validates the
/// First View contract: analyze (streamed) → map → entry points → trace → node/neighbors, all over one
/// analyzed snapshot (analyze once, query many).</summary>
public sealed class AnalyzeFlowTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private DevContextService.DevContextServiceClient CreateClient()
    {
        var http = factory.CreateClient();
        var channel = GrpcChannel.ForAddress(http.BaseAddress!, new GrpcChannelOptions { HttpClient = http });
        return new DevContextService.DevContextServiceClient(channel);
    }

    [Fact]
    public async Task Analyze_streams_progress_then_map_entries_trace_and_node()
    {
        var client = CreateClient();

        // 1) Analyze (server-streaming): collect progress, capture the handle.
        var stages = new List<string>();
        string? handle = null;
        string? error = null;
        using (var call = client.Analyze(new AnalyzeRequest { Path = FixturePath("ControllerApp") }))
        {
            await foreach (var evt in call.ResponseStream.ReadAllAsync())
            {
                switch (evt.EventCase)
                {
                    case AnalyzeEvent.EventOneofCase.Progress: stages.Add(evt.Progress.Stage); break;
                    case AnalyzeEvent.EventOneofCase.Result: handle = evt.Result.Handle; break;
                    case AnalyzeEvent.EventOneofCase.Error: error = evt.Error.Message; break;
                    default: break;
                }
            }
        }

        Assert.Null(error);
        Assert.False(string.IsNullOrEmpty(handle));
        Assert.NotEmpty(stages); // progress actually streamed

        // 2) Map renders from the snapshot.
        var map = await client.GetMapAsync(new SessionRequest { Handle = handle });
        Assert.Contains("MAP", map.Markdown, StringComparison.Ordinal);
        Assert.True(map.ProjectCount > 0);

        // 3) Entry points exist.
        var entries = await client.ListEntryPointsAsync(new SessionRequest { Handle = handle });
        Assert.NotEmpty(entries.EntryPoints);

        // 4) A known controller endpoint traces to a structured tree — no re-analysis.
        var trace = await client.GetTraceAsync(new TraceRequest
        {
            Handle = handle,
            Focus = "GET /api/Products",
            Depth = 4,
        });
        Assert.True(trace.Found);
        Assert.NotNull(trace.Root);
        var titles = CollectTitles(trace.Root);
        Assert.Contains(titles, t => t.Contains("ProductService", StringComparison.Ordinal));

        // 5) Node + neighbors browse the same snapshot by id.
        var node = await client.GetNodeAsync(new NodeRequest { Handle = handle, NodeId = trace.Root.NodeId });
        Assert.True(node.Found);

        var neighbors = await client.GetNeighborsAsync(new NeighborsRequest
        {
            Handle = handle,
            NodeId = trace.Root.NodeId,
            Direction = "out",
        });
        Assert.NotNull(neighbors);
    }

    [Fact]
    public async Task Ping_reports_ready()
    {
        var client = CreateClient();
        var pong = await client.PingAsync(new PingRequest());
        Assert.True(pong.Ready);
        Assert.False(string.IsNullOrEmpty(pong.Version));
    }

    [Fact]
    public async Task Unknown_handle_is_not_found()
    {
        var client = CreateClient();
        var ex = await Assert.ThrowsAsync<RpcException>(
            () => client.ListEntryPointsAsync(new SessionRequest { Handle = "does-not-exist" }).ResponseAsync);
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task SearchNodes_returns_matching_nodes()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var result = await client.SearchNodesAsync(new SearchRequest { Handle = handle, Query = "Product", Limit = 10 });
        Assert.NotEmpty(result.Nodes);
        Assert.Contains(result.Nodes, n => n.Title.Contains("Product", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetStats_returns_report()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var stats = await client.GetStatsAsync(new SessionRequest { Handle = handle });
        Assert.True(stats.Graph.Nodes > 0);
        Assert.True(stats.Graph.Edges > 0);
        Assert.NotEmpty(stats.Seams);
    }

    [Fact]
    public async Task GetMap_returns_structured_fields()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var map = await client.GetMapAsync(new SessionRequest { Handle = handle });
        Assert.NotEmpty(map.Markdown);
        Assert.True(map.ProjectCount > 0);
        Assert.NotEmpty(map.Topology);
    }

    [Fact]
    public async Task Render_returns_content()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var rendered = await client.RenderAsync(new RenderRequest
        {
            Handle = handle,
            Format = "markdown",
            IncludeDiagnostics = false,
        });
        Assert.NotEmpty(rendered.Content);
        Assert.True(rendered.EstimatedTokens > 0);
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

    private static List<string> CollectTitles(TraceNode node)
    {
        var titles = new List<string> { node.Title };
        foreach (var child in node.Children)
            titles.AddRange(CollectTitles(child));
        return titles;
    }

    private static string FixturePath(string name) => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures", name));
}
