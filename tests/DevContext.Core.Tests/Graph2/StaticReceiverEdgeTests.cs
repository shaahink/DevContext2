using DevContext.Core.Graph;
using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

/// <summary>BUG #11 (E1.1) — a STATIC call whose receiver is a TYPE NAME must produce a call edge.
/// The receiver of <c>TextHelpers.Normalize(raw)</c> resolves from no local scope (it is not a field,
/// parameter or local), so <see cref="InvocationOp.ReceiverType"/> is null and the binder used to fall
/// off the end of <c>ResolveCallee</c> with no edge. Measured consequence on DevContext's own graph
/// (G4.2 dogfood drive, 2026-07-29): <c>BodyFactExtractor</c>, <c>RazorCodeVirtualizer</c> and
/// <c>ExtractorHelpers</c> each had ZERO in-edges — the whole static-utility layer was invisible to the
/// question "who calls this helper", answered "nobody" with the same shape as a true zero.
/// <para>Every fixture here makes the static call the ONLY reference to its target, per the bug's own
/// "watch it go red first" note: a fixture that also instantiates the target passes on the broken state.
/// The negatives pin the honesty gate — resolution must be unambiguous, in-solution, and the type must
/// DECLARE the called method, exactly like the bare-identifier self-call arm.</para></summary>
public sealed class StaticReceiverEdgeTests
{
    [Fact]
    public async Task Static_call_through_a_bare_type_name_receiver_produces_an_edge()
    {
        var edges = await BindAsync((@"C:\repo\src\MyApp\Ops.cs", """
            namespace MyApp;

            public static class TextHelpers
            {
                public static string Normalize(string s) => s.Trim();
            }

            public sealed class OrderProcessor
            {
                public string Process(string raw) => TextHelpers.Normalize(raw);
            }
            """));

        Assert.Contains(edges, e =>
            e.CallerType == "MyApp.OrderProcessor"
            && e.CallerMethod == "Process"
            && e.CalleeType == "MyApp.TextHelpers"
            && e.CalleeMethod == "Normalize");
    }

    [Fact]
    public async Task Static_call_through_a_namespace_qualified_receiver_produces_an_edge()
    {
        // The shape that hid RazorCodeVirtualizer: the ROOT identifier of the receiver is a NAMESPACE
        // segment ("Utilities"), never a scope entry — the type name is the trailing segment.
        var edges = await BindAsync((@"C:\repo\src\MyApp\Ops.cs", """
            namespace MyApp.Utilities;

            public static class TreeWalker
            {
                public static int Walk(string path) => path.Length;
            }
            """),
            (@"C:\repo\src\MyApp\Caller.cs", """
            namespace MyApp;

            public sealed class Scanner
            {
                public int Scan(string path) => Utilities.TreeWalker.Walk(path);
            }
            """));

        Assert.Contains(edges, e =>
            e.CallerType == "MyApp.Scanner"
            && e.CalleeType == "MyApp.Utilities.TreeWalker"
            && e.CalleeMethod == "Walk");
    }

    [Fact]
    public async Task Out_of_solution_static_receiver_produces_no_edge()
    {
        // Console is not in the symbol table — a name-based bind must NOT invent a node for it.
        var edges = await BindAsync((@"C:\repo\src\MyApp\Ops.cs", """
            namespace MyApp;

            public sealed class Reporter
            {
                public void Report(string raw) => System.Console.WriteLine(raw);
            }
            """));

        Assert.DoesNotContain(edges, e => e.CalleeMethod == "WriteLine");
        Assert.Empty(edges);
    }

    [Fact]
    public async Task Static_receiver_whose_type_does_not_declare_the_method_produces_no_edge()
    {
        // `Settings` is a real in-solution type, but it declares no `Load` — binding the edge anyway
        // would be a guess. (This is what keeps a same-named property/namespace hop from inventing edges.)
        var edges = await BindAsync((@"C:\repo\src\MyApp\Ops.cs", """
            namespace MyApp;

            public static class Settings
            {
                public static string Name => "x";
            }

            public sealed class Boot
            {
                public void Run() => Settings.Load();
            }
            """));

        Assert.DoesNotContain(edges, e => e.CalleeMethod == "Load");
    }

    [Fact]
    public async Task Ambiguous_static_receiver_short_name_produces_no_edge()
    {
        // Two in-solution types share the short name and both declare the method: Law R1 — skip,
        // never first-match.
        var edges = await BindAsync((@"C:\repo\src\MyApp\A.cs", """
            namespace MyApp.First;

            public static class Helpers
            {
                public static string Clean(string s) => s;
            }
            """),
            (@"C:\repo\src\MyApp\B.cs", """
            namespace MyApp.Second;

            public static class Helpers
            {
                public static string Clean(string s) => s;
            }
            """),
            (@"C:\repo\src\MyApp\C.cs", """
            namespace MyApp.Third;

            public sealed class Caller
            {
                public string Go(string s) => Helpers.Clean(s);
            }
            """));

        Assert.DoesNotContain(edges, e => e.CalleeMethod == "Clean");
    }

    /// <summary>Runs the real bind path over in-memory sources: syntax → body facts → SymbolTable →
    /// <see cref="CallGraphBinder"/>. No semantic tier, so this measures the SYNTACTIC arm only.</summary>
    private static async Task<IReadOnlyList<Core.Models.CallEdge>> BindAsync(params (string Path, string Source)[] files)
    {
        var fs = new FakeFileSystem();
        foreach (var (path, source) in files) fs.AddFile(path, source);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = files.Select(f => f.Path).ToList();
        ctx.Analysis.FocusPoints = [];

        var model = new DiscoveryModel();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);
        await new BodyFactsExtractor().ExtractAsync(ctx, model, default);

        var symbols = new SymbolTable(model.OrderedTypes, null, ctx.Analysis.AllBodyFacts);
        CallGraphBinder.Bind(ctx, model, symbols, ctx.Analysis.AllBodyFacts,
            new NoiseFilter(new ProjectClassifier(model.Projects)), default);

        return model.CallEdges.ToList();
    }
}
