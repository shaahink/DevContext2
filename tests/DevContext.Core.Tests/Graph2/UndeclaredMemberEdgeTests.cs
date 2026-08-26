using DevContext.Core.Graph;
using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

/// <summary>F1 (backlog #33, drive-2026-08-26) — <b>no node may be a member of a type that does not
/// declare it</b>. On Book2Course the engine minted <c>AppDbContext::ConfigureAwait</c> /
/// <c>::Where</c> / <c>::IgnoreQueryFilters</c> — extension and BCL methods bound to the RECEIVER's
/// type — and, ranking by degree, put them at the top of <c>overview.startHere</c>
/// (<c>::ConfigureAwait</c> 72 connections) and inside traces
/// (<c>→ Member: SourceUploads.ConfigureAwait [approx]</c>). Residual of the #7/#12 family on the
/// extension-method path: the receiver arm of <c>CallGraphBinder.ResolveCallee</c> had the
/// Kind==Type gate and the in-solution gate but no DECLARES gate — the gate both other arms
/// (bare-identifier self-call, static receiver) already carry.
/// <para>The declares oracle walks the VISIBLE hierarchy (<c>TypeDiscovery.BaseTypes</c> +
/// interfaces): a method inherited from an in-solution base keeps its edge (the naive-gate trap),
/// while an out-of-solution base ends visibility and never vouches. No BCL name lists — the
/// declares gate is what retired them.</para>
/// <para>Fixture-first evidence: every negative here was run RED against the unfixed binder
/// (see the F1 commit body for the red output).</para></summary>
public sealed class UndeclaredMemberEdgeTests
{
    // ── The Book2Course shape: an in-solution extension method call on the receiver ──────────

    private const string ExtensionShape = """
        namespace Shop;

        public sealed class AppDbContext
        {
            public void ConfigureConventions() { }
        }

        public static class QueryExtensions
        {
            public static AppDbContext IgnoreQueryFilters(this AppDbContext db) => db;
        }

        public sealed class RunQuery
        {
            private readonly AppDbContext _db;
            public RunQuery(AppDbContext db) { _db = db; }
            public void Run()
            {
                _db.IgnoreQueryFilters();
                _db.ConfigureConventions();
            }
        }
        """;

    [Fact]
    public async Task Extension_method_call_does_not_bind_to_the_receiver_type()
    {
        // `_db.IgnoreQueryFilters()` is QueryExtensions' member, not AppDbContext's. Binding it to
        // the receiver type mints `AppDbContext::IgnoreQueryFilters` — the exact Book2Course noise.
        var edges = await BindAsync(semantic: false, (@"C:\repo\src\Shop\Db.cs", ExtensionShape));

        Assert.DoesNotContain(edges, e => e.CalleeMethod == "IgnoreQueryFilters");

        // Vacuity guard: the DECLARED call on the same receiver keeps its edge.
        Assert.Contains(edges, e =>
            e.CalleeType == "Shop.AppDbContext" && e.CalleeMethod == "ConfigureConventions");
    }

    [Fact]
    public async Task Method_inherited_from_an_out_of_solution_base_does_not_bind_to_the_receiver_type()
    {
        // `_db.SaveChanges()` really is a call on the AppDbContext instance — but SaveChanges is
        // DbContext's member and DbContext is not in this solution. The invariant is literal: a
        // member node the visible hierarchy cannot vouch for is never minted. (Same rule that keeps
        // `ConfigureAwait`, `ToString`, `Where` off in-solution types.)
        var edges = await BindAsync(semantic: false, (@"C:\repo\src\Shop\Db.cs", """
            namespace Shop;

            public sealed class AppDbContext : DbContext
            {
                public void ConfigureConventions() { }
            }

            public sealed class SaveRuns
            {
                private readonly AppDbContext _db;
                public SaveRuns(AppDbContext db) { _db = db; }
                public void Save() => _db.SaveChanges();
            }
            """));

        Assert.DoesNotContain(edges, e => e.CalleeMethod == "SaveChanges");
    }

    // ── The naive-gate trap, pinned: inherited IN-solution members keep their edges ──────────

    [Fact]
    public async Task Method_inherited_from_an_in_solution_base_keeps_its_edge()
    {
        // The oracle MUST walk TypeDiscovery.BaseTypes: OrderRepository declares nothing, but its
        // in-solution base declares Save — a declared-members-only gate would drop a real call.
        var edges = await BindAsync(semantic: false, (@"C:\repo\src\Shop\Repo.cs", """
            namespace Shop;

            public class RepositoryBase
            {
                public void Save() { }
            }

            public sealed class OrderRepository : RepositoryBase { }

            public sealed class Checkout
            {
                private readonly OrderRepository _repo;
                public Checkout(OrderRepository repo) { _repo = repo; }
                public void Commit() => _repo.Save();
            }
            """));

        Assert.Contains(edges, e =>
            e.CalleeType == "Shop.OrderRepository" && e.CalleeMethod == "Save");
    }

    [Fact]
    public async Task Method_declared_on_a_base_interface_keeps_its_edge()
    {
        // Interface hierarchies walk the same way: IRepo declares nothing, its base interface does.
        var edges = await BindAsync(semantic: false, (@"C:\repo\src\Shop\Store.cs", """
            namespace Shop;

            public interface IStore
            {
                string Get(string key);
            }

            public interface IRepo : IStore { }

            public sealed class Reader
            {
                private readonly IRepo _repo;
                public Reader(IRepo repo) { _repo = repo; }
                public string Read(string key) => _repo.Get(key);
            }
            """));

        Assert.Contains(edges, e =>
            e.CalleeType == "Shop.IRepo" && e.CalleeMethod == "Get");
    }

    // ── The chain half of the mechanism: a call result's type is not the root identifier's ───

    [Fact]
    public async Task Chained_call_result_receiver_is_not_bound_to_the_root_identifiers_type()
    {
        // `_session.Begin().Commit()` — Commit's receiver is the Batch that Begin() RETURNS, not
        // `_session`. RootIdentifier used to walk THROUGH the invocation, root the receiver at
        // `_session`, and — because Session also declares a Commit — mint Session::Commit for a
        // call that never touches it. This is the `_db.SaveChangesAsync().ConfigureAwait(false)`
        // shape with the declares gate held constant, so it discriminates the RootIdentifier fix.
        var edges = await BindAsync(semantic: false, (@"C:\repo\src\Shop\Session.cs", """
            namespace Shop;

            public sealed class Batch
            {
                public void Commit() { }
            }

            public sealed class Session
            {
                public Batch Begin() => new Batch();
                public void Commit() { }
            }

            public sealed class Worker
            {
                private readonly Session _session;
                public Worker(Session session) { _session = session; }
                public void Run() => _session.Begin().Commit();
            }
            """));

        Assert.DoesNotContain(edges, e =>
            e.CalleeType == "Shop.Session" && e.CalleeMethod == "Commit");

        // Vacuity guard: the INNER call still binds — `_session` really is a Session.
        Assert.Contains(edges, e =>
            e.CalleeType == "Shop.Session" && e.CalleeMethod == "Begin"
            && e.CallerType == "Shop.Worker");
    }

    // ── The smoking gun: a contradicting Tier-B bind must not leave the wrong guess standing ──

    [Fact]
    public async Task A_contradicting_semantic_bind_unresolves_the_receiver_instead_of_keeping_the_guess()
    {
        // The member scope says `item` is the FIELD (a Db); the local declaration shadows it with
        // Factory.Make()'s Widget, which Tier A cannot infer (another type's return). Roslyn binds
        // the receiver to Widget — CONTRADICTING the syntactic guess. MergeSemantic used to return
        // null on disagreement, so the wrong guess survived and — Db declaring its own Run — the
        // binder minted Caller.M → Db.Run for a call that runs on a Widget. A contradicted ref is
        // UNRESOLVED (Law R1's spirit: when two witnesses disagree, say unknown — never guess), and
        // deliberately NOT re-pointed at this tier.
        var edges = await BindAsync(semantic: true, (@"C:\repo\src\Shop\Shadow.cs", """
            namespace Demo;

            public sealed class Db
            {
                public void Run() { }
            }

            public sealed class Widget
            {
                public void Run() { }
            }

            public sealed class Factory
            {
                public Widget Make() => new Widget();
            }

            public sealed class Caller
            {
                private readonly Db item = new Db();
                private readonly Factory _factory = new Factory();
                public void M()
                {
                    var item = _factory.Make();
                    item.Run();
                }
            }
            """));

        Assert.DoesNotContain(edges, e => e.CalleeType == "Demo.Db" && e.CalleeMethod == "Run");
        // Design pin: unresolve, never re-point — the Widget edge is NOT invented either.
        Assert.DoesNotContain(edges, e => e.CalleeType == "Demo.Widget" && e.CalleeMethod == "Run");
        // Vacuity guard: the factory call itself stays bound.
        Assert.Contains(edges, e => e.CalleeType == "Demo.Factory" && e.CalleeMethod == "Make");
    }

    // ── Graph level: the standing-guard triple + startHere, the surfaces the drive measured ───

    [Fact]
    public async Task No_undeclared_member_node_no_edge_and_no_invariant_refusal()
    {
        var (graph, model) = await BuildAsync(ExtensionShape);

        // The declaring types are real and present — the fixture is not vacuously empty.
        Assert.Contains(graph.Nodes, n => n.Id.Key == "Shop.AppDbContext");

        // (1) no node wears the undeclared member's identity
        Assert.DoesNotContain(graph.Nodes,
            n => n.Id.Key.Contains("::IgnoreQueryFilters", StringComparison.Ordinal));

        // (2) no edge lands on it — the degree that out-ranked real startHere rows cannot come back
        Assert.DoesNotContain(graph.AllEdges,
            e => e.To.Key.Contains("::IgnoreQueryFilters", StringComparison.Ordinal));

        // (3) and no producer even TRIED: an INV-C refusal would have been counted and reported.
        Assert.DoesNotContain(model.Diagnostics, d => d.Source == "GraphInvariants");
    }

    [Fact]
    public async Task Start_here_offers_no_member_of_a_type_that_does_not_declare_it()
    {
        // The drive's headline symptom: `overview.startHere` — the FIRST line an agent reads —
        // led with AppDbContext.ConfigureAwait/.Where/.IgnoreQueryFilters. startHere ranks by
        // degree over the graph's nodes, so the fix must hold at the node level: the phantom
        // member never exists, so no query-layer name list is needed (house policy retired those).
        var (graph, _) = await BuildAsync(ExtensionShape);

        var points = new GraphQuery(graph, []).GetInterestingPoints();

        Assert.DoesNotContain(points, p => p.Title == "AppDbContext.IgnoreQueryFilters");
    }

    // ── Harnesses (the real bind/build paths over in-memory sources) ─────────────────────────

    /// <summary>Syntax → BodyFacts → SymbolTable → <see cref="CallGraphBinder"/>; with
    /// <paramref name="semantic"/>, the Tier-B populator upgrades the facts first (same harness
    /// shape as LambdaArgumentEdgeTests).</summary>
    private static async Task<IReadOnlyList<Core.Models.CallEdge>> BindAsync(
        bool semantic, params (string Path, string Source)[] files)
    {
        var (model, _, _) = await ExtractAsync(semantic, files);
        return model.CallEdges.ToList();
    }

    /// <summary>Runs the full graph assembly so BOTH call-edge producers and the INV-C invariant
    /// are on the path (same harness shape as BclNameCollisionEdgeTests).</summary>
    private static async Task<(CodeGraph Graph, DiscoveryModel Model)> BuildAsync(string source)
    {
        const string path = @"C:\repo\src\Shop\Db.cs";
        var (model, symbols, facts) = await ExtractAsync(semantic: false, (path, source));

        var scope = SolutionScope.FromModel(model);
        var noise = new NoiseFilter(new ProjectClassifier(model.Projects));
        var (graph, _) = new GraphBuilder(new SyntacticSymbolResolver(), noise)
            .Build(model, scope, facts, symbols);

        return (graph, model);
    }

    private static async Task<(DiscoveryModel Model, SymbolTable Symbols, ImmutableArray<BodyFacts> Facts)> ExtractAsync(
        bool semantic, params (string Path, string Source)[] files)
    {
        var fs = new FakeFileSystem();
        foreach (var (path, source) in files) fs.AddFile(path, source);
        fs.AddFile(@"C:\repo\src\Shop\Shop.csproj", "<Project />");

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
            foreach (var (path, _) in files) ((FakeAnalysisCache)ctx.Cache).RegisterPath(path);
            var projects = new[] { new ProjectInfo("Shop", @"C:\repo\src\Shop\Shop.csproj", "C#", [], [], []) };
            var upgraded = SemanticLitePopulator.Populate(projects, facts, ctx.Cache, @"C:\repo");
            Assert.True(upgraded.CompilationBuilt, "Tier B did not build a compilation — the fixture proves nothing.");
            facts = upgraded.UpgradedBodyFacts;
        }

        var symbols = new SymbolTable(model.OrderedTypes, null, facts);
        CallGraphBinder.Bind(ctx, model, symbols, facts,
            new NoiseFilter(new ProjectClassifier(model.Projects)), default);

        return (model, symbols, facts);
    }
}
