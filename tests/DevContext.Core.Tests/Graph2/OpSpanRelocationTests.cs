using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

/// <summary>E1.2 (backlog #12) — the semantic-lite upgrade relocates an op's syntax node before it can
/// bind it. Relocating by the op's LINE and then walking ANCESTORS finds the enclosing STATEMENT, so an
/// invocation whose statement fits on one line is a DESCENDANT and is never found; and when two ops share
/// a line the line span widens to their common parent, so every op on that line misses (or, where a
/// descendant fallback exists, binds the FIRST match — the wrong node, silently).
///
/// <para>Every fixture here is a ONE-LINE statement, which is what makes it discriminating: the backlog's
/// case D (a call whose statement STARTS at the invocation and wraps onto a second line) binds even on the
/// broken build, so a multi-line fixture proves nothing. <see cref="Multi_line_call_statement_still_binds"/>
/// pins case D so the fix cannot trade one shape for the other.</para></summary>
public sealed class OpSpanRelocationTests
{
    [Fact]
    public async Task One_line_instance_call_binds_its_receiver_semantically()
    {
        // Backlog case A (`widget.Do();`) and case B (`this.widget.Do();`). Both already produce a
        // SYNTACTIC receiver type from the member scope; what neither gets today is the Semantic tier,
        // and every consumer keyed on receiver type (dispatch detection, DI iface→impl, HopThroughProperty,
        // the Resolution tag on the Calls edge) reads that tier.
        var ops = await InvocationsAsync("""
            namespace Demo;
            public interface IWidget { void Do(); void Poke(); }
            public class C
            {
                private readonly IWidget widget;
                public C(IWidget w) { widget = w; }
                public void M()
                {
                    widget.Do();
                    this.widget.Poke();
                }
            }
            """);

        var bare = ops.Single(o => o.MethodName == "Do");
        var qualified = ops.Single(o => o.MethodName == "Poke");
        Assert.Equal(ResolutionTier.Semantic, bare.ReceiverType?.Tier);
        Assert.Equal("Demo.IWidget", bare.ReceiverType?.Resolved?.Canonical);
        Assert.Equal(ResolutionTier.Semantic, qualified.ReceiverType?.Tier);
        Assert.Equal("Demo.IWidget", qualified.ReceiverType?.Resolved?.Canonical);
    }

    [Fact]
    public async Task One_line_static_call_binds_its_type_name_receiver_semantically()
    {
        // The E1.1 (#11) shape seen from the other end: a TYPE-NAME receiver resolves from no local
        // scope, so ReceiverType is null and the semantic upgrade is the ONLY path that can fill it.
        var ops = await InvocationsAsync("""
            namespace Demo;
            public static class TextHelpers { public static string Normalize(string s) => s; }
            public class C
            {
                public void M(string x)
                {
                    TextHelpers.Normalize(x);
                }
            }
            """);

        var call = ops.Single(o => o.MethodName == "Normalize");
        Assert.Equal(ResolutionTier.Semantic, call.ReceiverType?.Tier);
        Assert.Equal("Demo.TextHelpers", call.ReceiverType?.Resolved?.Canonical);
    }

    [Fact]
    public async Task One_line_generic_call_binds_its_type_argument_semantically()
    {
        // Sister site: TryBindGenericArg, same relocate-by-line + ascend shape.
        var ops = await InvocationsAsync("""
            namespace Demo;
            public class Widget { }
            public interface IRegistry { void Register<T>(); }
            public class C
            {
                private readonly IRegistry registry;
                public C(IRegistry r) { registry = r; }
                public void M()
                {
                    this.registry.Register<Widget>();
                }
            }
            """);

        var call = ops.Single(o => o.MethodName == "Register");
        Assert.Equal(ResolutionTier.Semantic, call.GenericArgs[0].Tier);
        Assert.Equal("Demo.Widget", call.GenericArgs[0].Resolved?.Canonical);
    }

    [Fact]
    public async Task Two_same_named_calls_on_one_line_bind_their_own_arguments()
    {
        // Sister site: TryBindArgType. This one does NOT merely miss — it has a descendant fallback that
        // disambiguates by METHOD NAME, so when the same method is called twice on one line BOTH ops bind
        // the FIRST call's argument. A wrong bind is worse than a null: it makes the dispatched contract
        // read as verified while naming the other command.
        var ops = await InvocationsAsync("""
            namespace Demo;
            public class ACommand { }
            public class BCommand { }
            public interface ISender { void Send(object o); }
            public class C
            {
                private readonly ISender sender;
                public C(ISender s) { sender = s; }
                public void M()
                {
                    sender.Send(new ACommand()); sender.Send(new BCommand());
                }
            }
            """);

        var sends = ops.Where(o => o.MethodName == "Send").ToList();
        Assert.Equal(2, sends.Count);
        Assert.Equal("Demo.ACommand", sends[0].Args[0].Type?.Resolved?.Canonical);
        Assert.Equal(ResolutionTier.Semantic, sends[0].Args[0].Type?.Tier);
        Assert.Equal("Demo.BCommand", sends[1].Args[0].Type?.Resolved?.Canonical);
        Assert.Equal(ResolutionTier.Semantic, sends[1].Args[0].Type?.Tier);
    }

    [Fact]
    public async Task Two_calls_on_one_line_bind_their_own_receivers()
    {
        var ops = await InvocationsAsync("""
            namespace Demo;
            public interface IAlpha { void Go(); }
            public interface IBeta { void Stop(); }
            public class C
            {
                private readonly IAlpha alpha;
                private readonly IBeta beta;
                public C(IAlpha a, IBeta b) { alpha = a; beta = b; }
                public void M()
                {
                    alpha.Go(); beta.Stop();
                }
            }
            """);

        Assert.Equal("Demo.IAlpha", ops.Single(o => o.MethodName == "Go").ReceiverType?.Resolved?.Canonical);
        Assert.Equal("Demo.IBeta", ops.Single(o => o.MethodName == "Stop").ReceiverType?.Resolved?.Canonical);
    }

    [Fact]
    public async Task Two_local_declarations_on_one_line_both_bind()
    {
        // Sister site: TryBindLocalDeclType. A single local on its own line DOES bind today (the line
        // span's innermost node IS the LocalDeclarationStatement, so ascending finds it as self) — so
        // the discriminating fixture is two declarations sharing a line, where the span widens to the
        // enclosing block and neither is found.
        var facts = await FactsAsync("""
            namespace Demo;
            public class Alpha { }
            public class Beta { }
            public class C
            {
                public void M()
                {
                    var a = new Alpha(); var b = new Beta();
                    a.ToString(); b.ToString();
                }
            }
            """);

        var locals = facts.SelectMany(f => f.Ops).OfType<LocalDeclOp>().ToList();
        Assert.Equal("Demo.Alpha", locals.Single(l => l.Name == "a").InferredFrom?.Resolved?.Canonical);
        Assert.Equal(ResolutionTier.Semantic, locals.Single(l => l.Name == "a").InferredFrom?.Tier);
        Assert.Equal("Demo.Beta", locals.Single(l => l.Name == "b").InferredFrom?.Resolved?.Canonical);
        Assert.Equal(ResolutionTier.Semantic, locals.Single(l => l.Name == "b").InferredFrom?.Tier);
    }

    [Fact]
    public async Task Two_object_creations_on_one_line_both_bind()
    {
        // Sister site: TryBindCreationType — same ascend shape, and two creations in one argument list
        // share a line by construction, not by formatting accident.
        var facts = await FactsAsync("""
            namespace Demo;
            public class Alpha { }
            public class Beta { }
            public class Sink { public static void Take(object a, object b) { } }
            public class C
            {
                public void M()
                {
                    Sink.Take(new Alpha(), new Beta());
                }
            }
            """);

        var creations = facts.SelectMany(f => f.Ops).OfType<CreationOp>().ToList();
        Assert.Equal(2, creations.Count);
        Assert.Contains(creations, c => c.Type.Resolved?.Canonical == "Demo.Alpha" && c.Type.Tier == ResolutionTier.Semantic);
        Assert.Contains(creations, c => c.Type.Resolved?.Canonical == "Demo.Beta" && c.Type.Tier == ResolutionTier.Semantic);
    }

    [Fact]
    public async Task Multi_line_call_statement_still_binds()
    {
        // The backlog's case D — the one shape that binds on the BROKEN build. Pinned so the fix is
        // strictly additive and cannot trade the wrapped spelling for the one-line spelling.
        var ops = await InvocationsAsync("""
            namespace Demo;
            public interface IWidget { void Do(int a, int b); }
            public class C
            {
                private readonly IWidget widget;
                public C(IWidget w) { widget = w; }
                public void M()
                {
                    this.widget.Do(
                        1, 2);
                }
            }
            """);

        Assert.Equal("Demo.IWidget", ops.Single(o => o.MethodName == "Do").ReceiverType?.Resolved?.Canonical);
    }

    private static async Task<List<InvocationOp>> InvocationsAsync(string src)
        => (await FactsAsync(src)).SelectMany(b => b.Ops).OfType<InvocationOp>().ToList();

    private static async Task<IReadOnlyList<BodyFacts>> FactsAsync(string src)
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
        Assert.True(result.CompilationBuilt, "the fixture compilation must build or every tier assertion below is vacuous");
        return result.UpgradedBodyFacts;
    }
}
