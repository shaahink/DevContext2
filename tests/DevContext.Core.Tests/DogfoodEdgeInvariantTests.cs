using DevContext.Core.Graph;
using Xunit.Abstractions;

namespace DevContext.Core.Tests;

/// <summary>
/// E1.1 — THE DOGFOOD EDGE INVARIANT. DevContext analyses its own <c>src/</c>, and its own static
/// helper layer must have in-edges in its own graph.
/// <para>Why this is a standing gate and not a one-off assertion: bug #11 was found by driving the MCP
/// against DevContext itself (G4.2, 2026-07-29) and asking "who calls this helper". The answer was
/// "nobody" — <c>BodyFactExtractor</c>, <c>RazorCodeVirtualizer</c> and <c>ExtractorHelpers</c> each had
/// ZERO in-edges, with the same confident shape a true zero has. Every one of them is called by name
/// from ordinary statements in this repo, several times. A zero here is never correct, and it was
/// wrong for months with every other gate green — because no gate ever asked the engine about itself.</para>
/// <para>Read a failure literally: either the call-edge path stopped binding a whole class of call,
/// or one of these types was renamed/deleted (the node-exists assertion tells you which). Never relax
/// the bar to match the output — that is the one move this program forbids.</para>
/// </summary>
[Trait("Category", "Eval")]
[Trait("Category", "Truth")]
public sealed class DogfoodEdgeInvariantTests
{
    private readonly ITestOutputHelper _output;

    public DogfoodEdgeInvariantTests(ITestOutputHelper output) => _output = output;

    /// <summary>The trio measured at 0 in-edges by the G4.2 dogfood drive. All three are static utility
    /// classes called through a TYPE-NAME receiver — the discriminating shape of bug #11.</summary>
    private static readonly string[] StaticHelperLayer =
    [
        "DevContext.Core.Graph2.BodyFactExtractor",
        "DevContext.Core.Utilities.RazorCodeVirtualizer",
        "DevContext.Core.Utilities.ExtractorHelpers",
    ];

    [SkippableFact]
    public async Task DevContexts_own_static_helper_layer_has_in_edges_in_its_own_graph()
    {
        var srcPath = RepoPath("src");
        Skip.IfNot(Directory.Exists(Path.Combine(srcPath, "DevContext.Core")),
            $"not running inside the DevContext repo (not a pass): {srcPath}");

        var graph = await AnalyzeOwnSourceAsync(srcPath);
        Assert.NotNull(graph);

        var missing = new List<string>();
        foreach (var fqn in StaticHelperLayer)
        {
            var id = NodeId.ForType(fqn);
            Assert.True(graph!.Contains(id),
                $"{fqn} is not a node in DevContext's own graph — the type was renamed or deleted, "
                + "or type discovery regressed. Update this invariant's list only after checking which.");

            var inEdges = graph.InEdges(id).Length;
            _output.WriteLine($"{fqn}: {inEdges} in-edges");
            if (inEdges == 0) missing.Add(fqn);
        }

        Assert.True(missing.Count == 0,
            "Static helper types with ZERO in-edges in DevContext's own graph (bug #11 shape — "
            + $"'who calls this helper' answered 'nobody'): {string.Join(", ", missing)}");
    }

    /// <summary>Runs the real analysis pipeline over the engine's own sources and returns the graph.</summary>
    private static async Task<CodeGraph?> AnalyzeOwnSourceAsync(string srcPath)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(srcPath, fs, CancellationToken.None);

        var options = new ExtractionOptions
        {
            MaxOutputTokens = 8000,
            OutputFormat = OutputFormat.Markdown,
            AllowRoslyn = true,
            BuildFullGraph = true,
        };

        using var loggerFactory = LoggerFactory.Create(_ => { });
        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = new SharedAnalysisContext(),
            Logger = loggerFactory.CreateLogger("DogfoodInvariant"),
        };

        var pipeline = TestPipeline.Build(loggerFactory);
        var snapshot = await pipeline.AnalyzeAsync(ctx);
        return snapshot.Graph;
    }

    private static string RepoPath(string relative)
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "DevContext.slnx")))
                return Path.GetFullPath(Path.Combine(dir, relative));
            var parent = Path.GetDirectoryName(dir);
            if (parent is null || parent == dir) break;
            dir = parent;
        }
        return Path.GetFullPath(relative);
    }
}
