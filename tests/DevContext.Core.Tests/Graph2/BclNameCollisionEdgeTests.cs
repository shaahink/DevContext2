using DevContext.Core.Graph;
using DevContext.Core.Graph2;

namespace DevContext.Core.Tests.Graph2;

/// <summary>BUG #7 (E1.3) — a BCL type name that a repo member happens to share must not become a call
/// target. Hangfire's <c>string IStackTraceFormatter&lt;string&gt;.Type(string markup)</c> — an explicit
/// interface implementation — shipped as <c>Type:Hangfire.StackTraceHtmlFragments::Type(1)</c> with
/// <b>26 in-edges</b>, every one of them a reference to BCL <c>System.Type</c> from a member that never
/// touches the dashboard's stack-trace formatter. 4.2% of that repo's whole call graph was phantom, and
/// the node ranked FIFTH in <c>stats</c> wiring hubs.
/// <para>This is the bug's own discriminating fixture, built from the source it was measured on: the
/// colliding member is an explicit interface implementation, the references are spelled BARE
/// (<c>using System;</c>) in the three shapes the repo used — a generic argument, a parameter, and
/// <c>typeof</c> — and NOTHING in the fixture legitimately calls the formatter, so any edge to it is
/// the defect. It asserts at graph level, which covers BOTH call-edge producers.</para>
/// <para>The last assertion is the strong one: the V1.3 invariant would REFUSE such a node at the door,
/// so "no phantom node" alone could mean "the producers still mint it and the graph deletes it, losing
/// whatever real edge came with it". No refusal diagnostic means no producer even tried.</para>
/// <para><b>Stated honestly: this fixture is a STANDING GUARD, not a reproduction.</b> It was run at
/// 0fd1cbe (pre-V1.3) and at 8f0ce0a (post-#11, pre-afee44b) and passes on both — so the 26-edge
/// mis-binding does not fall out of a one-file rendering of the shape, and a future regression of the
/// EXACT 2026-07-29 path could slip past it. The red-first evidence for this family is elsewhere and
/// stays cited rather than re-implied here: V1.3's producer-level test (a member answer is not a type
/// answer, measured red on eShop's <c>Convert</c>) and the Hangfire graph measured before/after in
/// eval-results/2026-08-13/e1-typenode/E1.3-EVIDENCE.md.</para></summary>
public sealed class BclNameCollisionEdgeTests
{
    private const string Source = """
        using System;
        using System.Collections.Concurrent;

        namespace Hangfire.Dashboard;

        public interface IStackTraceFormatter<T>
        {
            T Type(T markup);
        }

        public sealed class StackTraceHtmlFragments : IStackTraceFormatter<string>
        {
            string IStackTraceFormatter<string>.Type(string markup) => "<span>" + markup + "</span>";
        }

        public sealed class JobDisplayNameAttribute
        {
            private readonly ConcurrentDictionary<Type, string> _cache = new ConcurrentDictionary<Type, string>();

            public string InitResourceManager(Type type)
            {
                return _cache.GetOrAdd(type, static t => t.FullName);
            }
        }

        public sealed class ExceptionTypeHelper
        {
            private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);

            public bool IsCatchableExceptionType(Exception ex)
            {
                Type type = ex.GetType();
                return type != OutOfMemoryType;
            }

            public string ResolveByName(string name)
            {
                // The arm that can actually MINT the edge on the current engine: a static call whose
                // receiver is the bare type name `Type`. E1.1 taught both producers to bind exactly
                // this shape, so from #11 onward the collision has a path to travel — it is the
                // out-of-solution gate (afee44b) that has to refuse it, and this is what watches that.
                return Type.GetType(name).FullName;
            }
        }
        """;

    [Fact]
    public async Task A_BCL_type_reference_does_not_bind_to_a_same_named_member()
    {
        var (graph, edges, model) = await BuildAsync();

        // The declaring type is real and present — the fixture is not vacuously empty.
        Assert.Contains(graph.Nodes, n => n.Id.Key == "Hangfire.Dashboard.StackTraceHtmlFragments");

        // (1) no node wears the member's identity, in either kind
        Assert.DoesNotContain(graph.Nodes, n => n.Id.Key.Contains("::Type", StringComparison.Ordinal));

        // (2) no edge lands on the formatter fragment — the 26 in-edges have no shape to come back in
        Assert.DoesNotContain(graph.AllEdges, e =>
            e.To.Key.Contains("StackTraceHtmlFragments", StringComparison.Ordinal)
            && e.From.Key.Contains("StackTraceHtmlFragments", StringComparison.Ordinal) == false
            && e.Kind == EdgeKind.Calls);
        Assert.DoesNotContain(edges, e => e.CalleeMethod == "Type");

        // (3) and no producer even TRIED: a refusal would have been counted (E1.3's tally).
        Assert.DoesNotContain(model.Diagnostics, d => d.Source == "GraphInvariants");
    }

    private static async Task<(CodeGraph Graph, IReadOnlyList<Core.Models.CallEdge> Edges, DiscoveryModel Model)> BuildAsync()
    {
        const string path = @"C:\repo\src\Hangfire.Core\Dashboard\StackTraceFormatter.cs";
        var fs = new FakeFileSystem();
        fs.AddFile(path, Source);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [path];
        ctx.Analysis.FocusPoints = [];

        var model = new DiscoveryModel();
        await new SyntaxStructureExtractor().ExtractAsync(ctx, model, default);
        await new BodyFactsExtractor().ExtractAsync(ctx, model, default);

        var symbols = new SymbolTable(model.OrderedTypes, null, ctx.Analysis.AllBodyFacts);
        var noise = new NoiseFilter(new ProjectClassifier(model.Projects));
        CallGraphBinder.Bind(ctx, model, symbols, ctx.Analysis.AllBodyFacts, noise, default);

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(new SyntacticSymbolResolver(), noise)
            .Build(model, scope, ctx.Analysis.AllBodyFacts, symbols);

        return (graph, model.CallEdges.ToList(), model);
    }
}
