using DevContext.Core.Graph;
using DevContext.Core.Pipeline;
using DevContext.Core.Rendering;

namespace DevContext.Core.Tests;

public sealed class MapRendererTests
{
    private static AnalysisSnapshot Snapshot(DiscoveryModel model) => new()
    {
        Model = model,
        RootPath = "/root",
        Analysis = new SharedAnalysisContext(),
        Scenario = ScenarioRegistry.BuiltIn["overview"],
        Options = new ExtractionOptions(),
        Report = new RunReport
        {
            Stages = [],
            Extractors = [],
            Scorers = [],
            Compressions = [],
            Cache = new CacheStats(0, 0, 0, 0),
            Corpus = new CorpusStats(0, 0, 0),
            Funnel = new TokenFunnel(0, 0, 0, 0, 0, 8000),
            Parallelism = new ParallelismStats(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
            TotalWall = TimeSpan.Zero,
        },
    };

    private static RenderRequest Request() => new() { Format = "markdown", MaxTokens = 8000 };

    [Fact]
    public async Task Map_topo_capped_at_50_with_disclosure()
    {
        // 80 projects: all reference Project1, ranking it first. With alphabetical string
        // sorting on tie, exactly 50 are shown + "… and 30 more" disclosure.
        var topo = Enumerable.Range(1, 80).Select(i =>
            new ProjectNode($"Proj{i:D3}",
                i > 1 ? [.. new[] { "Proj001" }] : [])).ToImmutableArray();
        var model = new DiscoveryModel { Projects = [] };
        var map = new MapModel { Archetype = Archetype.App, Topology = topo };

        var ctx = new MapRenderContext(map, Snapshot(model), "markdown", Request());
        var result = await MapRenderer.RenderAsync(ctx, CancellationToken.None);

        Assert.Contains("TOPOLOGY (depends-on)", result.Content, StringComparison.Ordinal);
        Assert.Contains("and 30 more projects", result.Content, StringComparison.Ordinal);
        // 50 projects + "TOPOLOGY" header + disclosure line = 52 lines in the section
        var topologyLines = result.Content.Split('\n').Count(l => l.StartsWith("   Proj"));
        Assert.True(topologyLines == 50, $"Expected 50 topology lines, got {topologyLines}");
    }

    [Fact]
    public async Task Map_entries_capped_at_20_per_kind_with_disclosure()
    {
        var entries = Enumerable.Range(1, 30).Select(i =>
            new EntryPoint(EntryPointKind.HttpEndpoint, $"GET /api/item{i:D2}", NodeId.ForEntry($"GET /api/item{i:D2}")))
            .ToImmutableArray();
        var model = new DiscoveryModel { Projects = [] };
        var map = new MapModel { Archetype = Archetype.App, Entries = entries };

        var ctx = new MapRenderContext(map, Snapshot(model), "markdown", Request());
        var result = await MapRenderer.RenderAsync(ctx, CancellationToken.None);

        Assert.Contains("HTTP (30)", result.Content, StringComparison.Ordinal);
        Assert.Contains("and 10 more", result.Content, StringComparison.Ordinal);
        // 20 entries shown out of 30, disclosure present
        var entryLines = result.Content.Split('\n').Count(l => l.TrimStart().StartsWith("GET /api/"));
        Assert.True(entryLines == 20, $"Expected 20 entry lines, got {entryLines}");
    }

    [Fact]
    public async Task Map_no_truncation_when_under_cap()
    {
        var topo = new[] { new ProjectNode("App", ["Core"]), new ProjectNode("Core", []) }.ToImmutableArray();
        var entries = new[] { new EntryPoint(EntryPointKind.HttpEndpoint, "GET /", NodeId.ForEntry("GET /")) }.ToImmutableArray();
        var model = new DiscoveryModel { Projects = [] };
        var map = new MapModel { Archetype = Archetype.App, Topology = topo, Entries = entries };

        var ctx = new MapRenderContext(map, Snapshot(model), "markdown", Request());
        var result = await MapRenderer.RenderAsync(ctx, CancellationToken.None);

        Assert.DoesNotContain("more projects", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("more http", result.Content, StringComparison.OrdinalIgnoreCase);
    }
}
