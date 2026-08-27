using DevContext.Core.Graph;
using DevContext.Core.Graph2;

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

        var graph = (await AnalyzeOwnSourceAsync(srcPath)).Graph;
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

    /// <summary>F1 (backlog #33) — THE DOGFOOD MEMBER INVARIANT: no node in DevContext's own graph
    /// may be a member of a type that does not visibly declare it. The Book2Course drive measured
    /// the violation on an unseen repo (`AppDbContext.ConfigureAwait` as startHere's FIRST row, the
    /// same phantoms inside traces); this sweep asks the engine the same question about itself, so
    /// the class of defect cannot come back silently. The oracle is the shipping one
    /// (<see cref="SymbolTable.DeclaresMemberInHierarchy"/>, tri-state, in-solution base walk), and
    /// the zero-refusals assertion carries the same weight as BclNameCollisionEdgeTests' third leg:
    /// nodes clean AND refusals zero means no producer even TRIED to mint the shape.</summary>
    [SkippableFact]
    public async Task No_member_node_of_a_type_that_does_not_visibly_declare_it()
    {
        var srcPath = RepoPath("src");
        Skip.IfNot(Directory.Exists(Path.Combine(srcPath, "DevContext.Core")),
            $"not running inside the DevContext repo (not a pass): {srcPath}");

        var snapshot = await AnalyzeOwnSourceAsync(srcPath);
        var graph = snapshot.Graph;
        Assert.NotNull(graph);

        // The GRAPH-TIME symbol table, not one re-derived from snapshot.Model: the legacy catalog's
        // TrivialMemberCompressor strips ToString/Equals/GetHashCode from TypeDiscovery AFTER the
        // graph is assembled, so a post-compression oracle would flag legitimately declared members
        // (measured: NodeId::ToString) and disagree with the INV-C decisions the build actually made.
        var symbols = snapshot.Analysis.GraphSymbols;
        Assert.NotNull(symbols);

        var offenders = new List<string>();
        foreach (var node in graph!.Nodes)
        {
            if (node.Kind != NodeKind.Member) continue;
            var key = node.Id.Key;
            var sep = key.IndexOf("::", StringComparison.Ordinal);
            if (sep <= 0 || sep + 2 >= key.Length) continue;
            var owner = key[..sep];
            var member = key[(sep + 2)..];
            var paren = member.IndexOf('(');
            if (paren > 0 && member.EndsWith(')')) member = member[..paren];
            if (member.Length == 0 || member.Any(c => !char.IsLetterOrDigit(c) && c != '_')) continue;

            if (symbols!.DeclaresMemberInHierarchy(owner, member) == false)
                offenders.Add(key);
        }

        foreach (var o in offenders) _output.WriteLine($"undeclared member node: {o}");
        Assert.True(offenders.Count == 0,
            "Member nodes of types that do not visibly declare them in DevContext's own graph "
            + $"(F1/#33 shape — extension/BCL methods minted on the receiver type): first "
            + $"{Math.Min(offenders.Count, 10)} of {offenders.Count}: "
            + string.Join(", ", offenders.Take(10)));

        // No producer even tried: an INV-C refusal would be counted in the GraphInvariants tally.
        // Deliberately scoped to INV-C — the dogfood graph carries a handful of INV-B refusals
        // (expression-text keys from DI-lambda shapes), which is that invariant's machinery doing
        // its counted-refusal job, not this sweep's subject.
        foreach (var d in snapshot.Model.Diagnostics.Where(d => d.Source == "GraphInvariants"))
        {
            _output.WriteLine(d.Message);
            var m = System.Text.RegularExpressions.Regex.Match(
                d.Message, @"INV-C \(member of a type that does not declare it\): (\d+)");
            if (m.Success)
                Assert.Equal("0", m.Groups[1].Value);
        }
    }

    /// <summary>Runs the real analysis pipeline over the engine's own sources and returns the snapshot.</summary>
    private static async Task<AnalysisSnapshot> AnalyzeOwnSourceAsync(string srcPath)
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
        return await pipeline.AnalyzeAsync(ctx);
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
