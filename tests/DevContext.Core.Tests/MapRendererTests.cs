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

    /// <summary>R4 §1 item 1 / §3 — the map markdown is served verbatim by the CLI, the desktop AND
    /// the MCP `map` tool. Every drill-in pointer in it used to name a CLI flag, so an agent reading
    /// the map over MCP was told to run `--focus`, a flag it has no way to pass. The pointers stay —
    /// a truncated map that says nothing about the rest is worse — but they name a verb every
    /// surface has. This test renders the flag-carrying paths at once: topology overflow, per-kind
    /// entry overflow and the footer.</summary>
    [Fact]
    public async Task Map_markdown_advertises_no_CLI_flag()
    {
        var topo = Enumerable.Range(1, 80).Select(i =>
            new ProjectNode($"Proj{i:D3}", i > 1 ? [.. new[] { "Proj001" }] : [])).ToImmutableArray();
        var entries = Enumerable.Range(1, 30).Select(i =>
            new EntryPoint(EntryPointKind.HttpEndpoint, $"GET /api/item{i:D2}", NodeId.ForEntry($"GET /api/item{i:D2}")))
            .ToImmutableArray();
        var model = new DiscoveryModel { Projects = [] };
        var map = new MapModel { Archetype = Archetype.App, Topology = topo, Entries = entries };

        var ctx = new MapRenderContext(map, Snapshot(model), "markdown", Request());
        var result = await MapRenderer.RenderAsync(ctx, CancellationToken.None);

        // The pointers are still there — this is a no-flags test, not a no-pointers test.
        Assert.Contains("and 30 more projects", result.Content, StringComparison.Ordinal);
        Assert.Contains("drill in", result.Content, StringComparison.Ordinal);

        var flags = System.Text.RegularExpressions.Regex.Matches(result.Content, @"--[a-z][a-z-]+")
            .Select(m => m.Value).Distinct().ToArray();
        Assert.True(flags.Length == 0, $"map markdown advertises CLI flags: {string.Join(", ", flags)}");
    }

    /// <summary>The SCOPE line is the other flag carrier: MapBuilder stamps
    /// <see cref="SolutionScopeNote.Text"/> into the map on a multi-solution repo, and that sentence
    /// ended by naming <c>--sln</c>. Every surface can name a solution when analyzing (CLI --sln,
    /// desktop picker, MCP analyze(sln:)); naming one surface's flag misdirects the other two. The
    /// note still says a choice was made and that another can be analyzed — only the words change.</summary>
    [Fact]
    public async Task Map_scope_line_names_the_choice_without_naming_a_flag()
    {
        var note = new SolutionScopeNote(
            AnalyzedPath: @"C:\repos\GitVersion\src\GitVersion.slnx",
            AnalyzedName: "GitVersion",
            AnalyzedRelPath: "src/GitVersion.slnx",
            TotalOnDisk: 3,
            OtherPaths: ["build/CI.slnx", "new-cli/GitVersion.slnx"]);
        Assert.True(note.IsPartial);

        var model = new DiscoveryModel { Projects = [] };
        var map = new MapModel { Archetype = Archetype.App, ScopeNote = note.Text };

        var ctx = new MapRenderContext(map, Snapshot(model), "markdown", Request());
        var result = await MapRenderer.RenderAsync(ctx, CancellationToken.None);

        Assert.Contains("SCOPE", result.Content, StringComparison.Ordinal);
        Assert.Contains("src/GitVersion.slnx", result.Content, StringComparison.Ordinal);
        Assert.Contains("1 of 3 solutions", result.Content, StringComparison.Ordinal);
        Assert.Contains("analyze another", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("--sln", result.Content, StringComparison.Ordinal);
    }
}
