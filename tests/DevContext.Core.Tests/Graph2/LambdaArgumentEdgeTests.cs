using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

/// <summary>BUG #8 (E1.3) — a call whose ONLY site is inside a LAMBDA ARGUMENT must produce a call edge.
/// <para>The FILED mechanism ("measure whether the extractor walks lambda/anonymous-function bodies at
/// all") is REFUTED by the first two fixtures here: <see cref="BodyFactExtractor"/> walks the whole
/// member body with <c>DescendantNodes()</c>, so a lambda-body call has always produced an op — and an
/// edge — whenever its receiver could be TYPED. What actually broke Hangfire's storage write is
/// narrower and is the third and fourth fixture: the receiver is rooted at an UNTYPED lambda parameter
/// (<c>static (_, ctx) =&gt; ctx.Context.Connection.CreateExpiredJob(...)</c>,
/// CoreBackgroundJobFactory.cs:89), and an untyped parameter is in no syntactic scope — the member's
/// parameters, the type's fields and the lambda's own TYPED parameters are the only things
/// <c>ResolveFromScope</c>/<c>GetEnclosingParamType</c> can answer from.</para>
/// <para>So the tier decides it: Tier A (syntax) cannot type <c>ctx</c>, Tier B (SemanticLite) can.
/// Every fixture makes the lambda-body call the ONLY reference to its target, per the bug's own "watch
/// it go red first" note — a fixture whose lambda duplicates a call made elsewhere in the same member
/// passes on the broken state.</para></summary>
public sealed class LambdaArgumentEdgeTests
{
    // ── The refutation: lambda bodies ARE walked ─────────────────────────────────────────────

    [Fact]
    public async Task A_call_inside_a_lambda_argument_on_a_FIELD_receiver_produces_an_edge()
    {
        // The filed mechanism says this edge cannot exist. It does, on the syntax tier alone.
        var edges = await BindAsync(semantic: false, (Path, $$"""
            {{Preamble}}
            public sealed class Factory
            {
                private readonly IStore _store;
                private readonly Retryer _retryer;
                public Factory(IStore s, Retryer r) { _store = s; _retryer = r; }
                public void Create() => _retryer.Retry(() => _store.Write("job"));
            }
            """));

        Assert.Contains(edges, e => e.CallerMethod == "Create"
            && e.CalleeType == "MyApp.IStore" && e.CalleeMethod == "Write");
    }

    [Fact]
    public async Task A_call_inside_a_lambda_argument_on_a_TYPED_lambda_parameter_produces_an_edge()
    {
        // GetEnclosingParamType walks the ancestor lambdas for an explicitly typed parameter, so this
        // arm has always worked too — the shape that does NOT work is one token different (next test).
        var edges = await BindAsync(semantic: false, (Path, $$"""
            {{Preamble}}
            public sealed class Factory
            {
                private readonly Retryer _retryer;
                public Factory(Retryer r) { _retryer = r; }
                public void Create() => _retryer.Retry((IStore store) => store.Write("job"));
            }
            """));

        Assert.Contains(edges, e => e.CallerMethod == "Create"
            && e.CalleeType == "MyApp.IStore" && e.CalleeMethod == "Write");
    }

    // ── The true mechanism: an UNTYPED lambda parameter ──────────────────────────────────────

    [Fact]
    public async Task A_call_on_an_UNTYPED_lambda_parameter_produces_an_edge_through_the_semantic_tier()
    {
        // Hangfire's shape, minus the generic. `ctx` has no type anywhere in the syntax; only the
        // delegate the argument is converted to knows it, and only Tier B reads that.
        var edges = await BindAsync(semantic: true, (Path, $$"""
            {{Preamble}}
            public sealed class Factory
            {
                private readonly Retryer _retryer;
                public Factory(Retryer r) { _retryer = r; }
                public void Create() => _retryer.Retry(ctx => ctx.Write("job"));
            }
            """));

        Assert.Contains(edges, e => e.CallerMethod == "Create"
            && e.CalleeType == "MyApp.IStore" && e.CalleeMethod == "Write");
    }

    [Fact]
    public async Task A_call_on_an_untyped_lambda_parameter_of_a_GENERIC_helper_produces_an_edge()
    {
        // Hangfire's shape verbatim: RetryOnException<TContext>(ref int, Action<int, TContext>, TContext)
        // — the lambda parameter's type is a TYPE PARAMETER inferred from a LATER argument, so no
        // delegate-signature lookup on the syntax tier could ever type it either. Tier B must.
        var edges = await BindAsync(semantic: true, (Path, $$"""
            {{Preamble}}
            public sealed class Factory
            {
                private readonly IStore _store;
                public Factory(IStore s) { _store = s; }
                public void Create() => Retry(static (_, ctx) => ctx.Write("job"), _store);
                private void Retry<TContext>(System.Action<int, TContext> action, TContext context)
                    => action(0, context);
            }
            """));

        Assert.Contains(edges, e => e.CallerMethod == "Create"
            && e.CalleeType == "MyApp.IStore" && e.CalleeMethod == "Write");
    }

    // ── The tier boundary, pinned so the next session does not re-hunt it ────────────────────

    [Fact]
    public async Task Tier_A_alone_cannot_type_an_untyped_lambda_parameter()
    {
        // DOCUMENTS the residual, it does not bless it: on a project whose assets.json is missing,
        // SemanticLitePopulator degrades to Tier A and this call has no edge. That is the honest
        // remainder of #8 and the reason its BUG-BACKLOG entry stays open at MEDIUM. If a later
        // session teaches Tier A to read the delegate parameter of an in-solution helper, this
        // assertion flips to Assert.Contains — an improvement, not a regression.
        var edges = await BindAsync(semantic: false, (Path, $$"""
            {{Preamble}}
            public sealed class Factory
            {
                private readonly Retryer _retryer;
                public Factory(Retryer r) { _retryer = r; }
                public void Create() => _retryer.Retry(ctx => ctx.Write("job"));
            }
            """));

        Assert.DoesNotContain(edges, e => e.CallerMethod == "Create"
            && e.CalleeType == "MyApp.IStore" && e.CalleeMethod == "Write");
    }

    // ── Harness ──────────────────────────────────────────────────────────────────────────────

    private const string Path = @"C:\repo\src\MyApp\Factory.cs";

    /// <summary>The callee and the helper that takes the lambda. IStore.Write is called from NOWHERE
    /// but the lambda in each fixture, so any edge to it is the edge under test.</summary>
    private const string Preamble = """
        namespace MyApp;

        public interface IStore { void Write(string s); }

        public sealed class Retryer
        {
            public void Retry(System.Action a) => a();
            public void Retry(System.Action<IStore> a) => a(null!);
        }
        """;

    private static async Task<IReadOnlyList<Core.Models.CallEdge>> BindAsync(
        bool semantic, params (string Path, string Source)[] files)
    {
        var fs = new FakeFileSystem();
        foreach (var (path, source) in files) fs.AddFile(path, source);
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", "<Project />");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = files.Select(f => f.Path).ToList();
        ctx.Analysis.FocusPoints = [];

        var model = new DiscoveryModel();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);
        await new BodyFactsExtractor().ExtractAsync(ctx, model, default);

        var facts = ctx.Analysis.AllBodyFacts.ToImmutableArray();
        if (semantic)
        {
            // Tier B collects its trees from the cache's KNOWN paths, not from the file system —
            // the real pipeline registers them while reading; the fake cache must be told.
            foreach (var (path, _) in files) ((FakeAnalysisCache)ctx.Cache).RegisterPath(path);
            var projects = new[] { new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", [], [], []) };
            var upgraded = SemanticLitePopulator.Populate(projects, facts, ctx.Cache, @"C:\repo");
            Assert.True(upgraded.CompilationBuilt, "Tier B did not build a compilation — the fixture proves nothing.");
            facts = upgraded.UpgradedBodyFacts;
        }

        var symbols = new SymbolTable(model.OrderedTypes, null, facts);
        CallGraphBinder.Bind(ctx, model, symbols, facts,
            new NoiseFilter(new ProjectClassifier(model.Projects)), default);

        return model.CallEdges.ToList();
    }
}
