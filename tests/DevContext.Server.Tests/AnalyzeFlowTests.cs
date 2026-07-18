using DevContext.Protos;

using Grpc.Core;
using Grpc.Net.Client;

namespace DevContext.Server.Tests;

/// <summary>End-to-end gRPC tests over the real composition root (in-memory test host). Validates the
/// First View contract: analyze (streamed) → map → entry points → trace → node/neighbors, all over one
/// analyzed snapshot (analyze once, query many).</summary>
public sealed class AnalyzeFlowTests(ServerTestFactory factory)
    : IClassFixture<ServerTestFactory>
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
        string? label = null;
        string? error = null;
        using (var call = client.Analyze(new AnalyzeRequest { Path = FixturePath("ControllerApp") }))
        {
            await foreach (var evt in call.ResponseStream.ReadAllAsync())
            {
                switch (evt.EventCase)
                {
                    case AnalyzeEvent.EventOneofCase.Progress: stages.Add(evt.Progress.Stage); break;
                    case AnalyzeEvent.EventOneofCase.Result:
                        handle = evt.Result.Handle;
                        label = evt.Result.Summary?.Label;
                        break;
                    case AnalyzeEvent.EventOneofCase.Error: error = evt.Error.Message; break;
                    default: break;
                }
            }
        }

        Assert.Null(error);
        Assert.False(string.IsNullOrEmpty(handle));
        Assert.NotEmpty(stages); // progress actually streamed
        // F4 (D4.5) — the session label is the scored solution name (same identity as
        // MapResponse.solution_name), not the resolver's file name ("ControllerApp.sln").
        Assert.Equal("ControllerApp", label);

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
            Focus = "GET /api/Products/{id}",
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
        // D1.5b — the product identity travels with the map, distinct from runnable hosts.
        Assert.True(map.HasSolutionName);
        Assert.Equal("ControllerApp", map.SolutionName);
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

    // T7.4 (audit B11) — the whole flow atlas in ONE call: one stat row per entry, keyed
    // exactly like the app keys its entry rows ("VERB route" or title), memoized per session.
    [Fact]
    public async Task GetFlowIndex_returns_one_stat_per_entry_in_one_call()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var entries = await client.ListEntryPointsAsync(new SessionRequest { Handle = handle });
        Assert.NotEmpty(entries.EntryPoints);

        var index = await client.GetFlowIndexAsync(new FlowIndexRequest { Handle = handle });

        Assert.NotEmpty(index.Flows);
        // No duplicate stat keys, and every row joins an entry row the app renders.
        var focuses = index.Flows.Select(f => f.Focus).ToList();
        Assert.Equal(focuses.Count, focuses.Distinct(StringComparer.Ordinal).Count());
        var entryKeys = entries.EntryPoints
            .Select(e => e.HasHttpMethod && e.HasRoute ? $"{e.HttpMethod} {e.Route}" : e.Title)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(index.Flows, f => Assert.Contains(f.Focus, entryKeys));

        // Found flows carry real stats (nodes + a score consistent with the formula).
        var found = index.Flows.Where(f => f.Found).ToList();
        Assert.NotEmpty(found);
        Assert.All(found, f =>
        {
            Assert.True(f.NodeCount >= 1, $"{f.Focus}: nodeCount={f.NodeCount}");
            Assert.Equal(f.NodeCount * (1.0 + f.BoundaryCrossings), f.Score, 3);
            Assert.Equal(f.NodeIds.Count, f.HubIds.Count + f.DataTouches);
        });

        // Second call returns the same memoized index (same rows, same order).
        var again = await client.GetFlowIndexAsync(new FlowIndexRequest { Handle = handle });
        Assert.Equal(index.Flows.Select(f => f.Focus), again.Flows.Select(f => f.Focus));
    }

    [Fact]
    public async Task GetContextPack_with_flow_card_returns_content()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var entries = await client.ListEntryPointsAsync(new SessionRequest { Handle = handle });
        Assert.NotEmpty(entries.EntryPoints);
        var firstEntryId = entries.EntryPoints[0].NodeId;

        var pack = await client.GetContextPackAsync(new ContextPackRequest
        {
            Handle = handle,
            BudgetTokens = 4000,
            Intent = "trace",
            Cards =
            {
                new ContextCardSpec
                {
                    Type = "flow",
                    Title = "Primary Flow",
                    EntryIds = { firstEntryId },
                },
            },
        });

        Assert.NotEmpty(pack.Cards);
        Assert.NotEmpty(pack.AssembledMarkdown);
        Assert.True(pack.TotalTokens > 0);
        Assert.True(pack.AllocatedTokens > 0);

        var card = pack.Cards[0];
        Assert.Equal("flow", card.Type);
        Assert.True(card.Tokens > 0);

        // Verify key sections present in assembled markdown
        Assert.Contains("## Primary Flow", pack.AssembledMarkdown, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetContextPack_multi_card_assembles_all()
    {
        var client = CreateClient();
        var handle = await AnalyzeControllerApp(client);

        var entries = await client.ListEntryPointsAsync(new SessionRequest { Handle = handle });
        Assert.NotEmpty(entries.EntryPoints);

        var entryIds = entries.EntryPoints.Take(2).Select(e => e.NodeId).ToList();
        var pack = await client.GetContextPackAsync(new ContextPackRequest
        {
            Handle = handle,
            BudgetTokens = 6000,
            Intent = "review",
            Cards =
            {
                new ContextCardSpec { Type = "flow", Title = "Flow", EntryIds = { entryIds[0] } },
                new ContextCardSpec { Type = "signatures", Title = "Signatures", EntryIds = { entryIds[1] } },
            },
        });

        Assert.NotEmpty(pack.Cards);
        Assert.True(pack.Cards.Count >= 1);
        Assert.NotEmpty(pack.AssembledMarkdown);
        // T4.1 — the header names the analyzed repo, not the tool; archetype is filled from the Map.
        Assert.Contains("— Context Pack", pack.AssembledMarkdown, StringComparison.Ordinal);
        Assert.DoesNotContain("_Archetype: _", pack.AssembledMarkdown, StringComparison.Ordinal);
    }
}
