using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;

namespace DevContext.Core.Tests.Graph2;

public sealed class SemanticLitePopulatorTests
{
    [Fact]
    public void Arg_demand_verbs_cover_every_detector_catalog()
    {
        // The arg-bind demand gate must never fall behind what a seam detector can consume. The
        // union is built from the detectors' own sets at runtime; this pins the contract if that
        // construction is ever replaced with a literal list.
        foreach (var v in MediatRDispatchDetector.Verbs)
            Assert.Contains(v, SemanticLitePopulator.ArgDemandVerbs);
        foreach (var v in BusPublishDetector.Verbs)
            Assert.Contains(v, SemanticLitePopulator.ArgDemandVerbs);
        foreach (var v in DomainEventRaiseDetector.RaiseVerbs)
            Assert.Contains(v, SemanticLitePopulator.ArgDemandVerbs);
    }

    [Fact]
    public async Task Binds_dispatch_args_and_skips_out_of_demand_args()
    {
        const string src = """
            namespace Demo;
            public class XCommand { }
            public interface ISender { void Send(object o); }
            public class C
            {
                private readonly ISender sender;
                public C(ISender s) { sender = s; }
                public void M()
                {
                    sender.Send(new XCommand());
                    Helper(new XCommand());
                }
                private void Helper(object o) { }
            }
            """;
        var (result, ops) = await PopulateSingleFileAsync(src);

        Assert.True(result.CompilationBuilt);
        var send = ops.Single(o => o.MethodName == "Send");
        var helper = ops.Single(o => o.MethodName == "Helper");
        // Send is dispatch demand: its Args[0] is what ResolveArgTarget reads — bound Semantic.
        Assert.Equal(ResolutionTier.Semantic, send.Args[0].Type?.Tier);
        // Helper is out of demand: no detector reads its args, so no semantic bind is spent on them.
        Assert.NotEqual(ResolutionTier.Semantic, helper.Args[0].Type?.Tier);
        Assert.Equal(1, result.ArgTypesResolved);
    }

    [Fact]
    public async Task Parallel_upgrade_preserves_order_and_is_repeatable()
    {
        var fs = new FakeFileSystem();
        var cache = new FakeAnalysisCache(fs);
        var facts = new List<BodyFacts>();
        for (var i = 0; i < 12; i++)
        {
            var path = $@"repo\App\C{i}.cs";
            fs.AddFile(path, $$"""
                namespace Demo;
                public class T{{i}} { }
                public class C{{i}}
                {
                    public void M()
                    {
                        var x = new T{{i}}();
                        x.ToString();
                    }
                }
                """);
            cache.RegisterPath(path);
            var tree = await cache.GetSyntaxTreeAsync(path);
            facts.AddRange(BodyFactExtractor.Extract(tree, path, "App"));
        }
        fs.AddFile(@"repo\App\App.csproj", "<Project />");
        var projects = new[] { new ProjectInfo("App", @"repo\App\App.csproj", "C#", [], [], []) };

        var first = SemanticLitePopulator.Populate(projects, facts, cache, "repo");
        var second = SemanticLitePopulator.Populate(projects, facts, cache, "repo");

        // Order in == order out (parallel tasks write disjoint original indices).
        Assert.Equal(facts.Select(f => (f.File, f.Member)), first.UpgradedBodyFacts.Select(f => (f.File, f.Member)));
        // Same input, same upgrades — the parallel pass is deterministic.
        Assert.Equal(first.VarDeclsResolved, second.VarDeclsResolved);
        Assert.Equal(first.ReceiversResolved, second.ReceiversResolved);
        Assert.Equal(first.ArgTypesResolved, second.ArgTypesResolved);
        Assert.True(first.VarDeclsResolved > 0, "expected at least one var-decl semantic upgrade");
    }

    private static async Task<(SemanticLiteResult Result, List<InvocationOp> Ops)> PopulateSingleFileAsync(string src)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"repo\App\C.cs", src);
        fs.AddFile(@"repo\App\App.csproj", "<Project />");
        var cache = new FakeAnalysisCache(fs);
        cache.RegisterPath(@"repo\App\C.cs");
        var tree = await cache.GetSyntaxTreeAsync(@"repo\App\C.cs");
        var facts = BodyFactExtractor.Extract(tree, @"repo\App\C.cs", "App");
        var projects = new[] { new ProjectInfo("App", @"repo\App\App.csproj", "C#", [], [], []) };

        var result = SemanticLitePopulator.Populate(projects, facts, cache, "repo");
        var ops = result.UpgradedBodyFacts.SelectMany(b => b.Ops).OfType<InvocationOp>().ToList();
        return (result, ops);
    }

    [Theory]
    [InlineData("lib/net10.0/Foo.dll", 100)]
    [InlineData("lib/net9.0/Foo.dll", 90)]
    [InlineData("lib/net8.0/Foo.dll", 80)]
    [InlineData("lib/net7.0/Foo.dll", 70)]
    [InlineData("lib/net6.0/Foo.dll", 60)]
    [InlineData("lib/net5.0/Foo.dll", 50)]
    [InlineData("lib/net11.0/Foo.dll", 110)]
    public void Scores_modern_tfm_by_major(string path, int expected)
    {
        Assert.Equal(expected, SemanticLitePopulator.TfmScore(path));
    }

    [Theory]
    [InlineData("lib/netcoreapp3.1/Foo.dll", 40)]
    [InlineData("lib/netstandard2.1/Foo.dll", 31)]
    [InlineData("lib/netstandard2.0/Foo.dll", 30)]
    [InlineData("lib/netstandard1.0/Foo.dll", 20)]
    [InlineData("lib/net45/Foo.dll", 10)]
    [InlineData("lib/net48/Foo.dll", 10)]
    public void Scores_legacy_tfm_by_fallback(string path, int expected)
    {
        Assert.Equal(expected, SemanticLitePopulator.TfmScore(path));
    }

    [Fact]
    public void Unknown_tfm_scores_minimum()
    {
        Assert.Equal(1, SemanticLitePopulator.TfmScore("lib/nosuch/Foo.dll"));
    }

    [Fact]
    public void Multiple_segments_picks_last_net()
    {
        Assert.Equal(100, SemanticLitePopulator.TfmScore("lib/net8.0/subdir/net10.0/Foo.dll"));
    }

    [Fact]
    public void NetX_without_dot_scores_fallback()
    {
        // net45, net48 etc. have digits but no dot — should fall through to fallback
        Assert.Equal(10, SemanticLitePopulator.TfmScore("lib/net45/Foo.dll"));
    }

    [Fact]
    public void Higher_major_ranks_above_lower()
    {
        var score10 = SemanticLitePopulator.TfmScore("lib/net10.0/a.dll");
        var score9 = SemanticLitePopulator.TfmScore("lib/net9.0/b.dll");
        Assert.True(score10 > score9);
    }

    [Fact]
    public void Multi_digit_minor_is_parsed()
    {
        Assert.Equal(10 * 10 + 10, SemanticLitePopulator.TfmScore("lib/net10.10/Foo.dll"));
    }
}
