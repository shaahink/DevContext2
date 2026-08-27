using DevContext.Core.Graph;
using DevContext.Core.Graph2;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Tests;

/// <summary>
/// G3.1 (R4 §1 item 8) — <c>seam(from, to)</c>, the path BETWEEN two symbols.
///
/// <para>The check that would prove nothing is "seam returned a path": a direct-edge BFS returns
/// paths too, and on the shapes that matter it returns the WRONG answer confidently. The two
/// discriminating invariants are pinned here.</para>
///
/// <para>(1) THE C3 ROLL-UP. A Type node carries almost no edges of its own — the wiring hangs off
/// its members — so a Type→Type seam over direct edges answers "unconnected" for two types that
/// call each other every request. <see cref="Type_to_type_connects_through_the_members_that_carry_it"/>
/// fails on a direct-edge implementation and passes on the rolled one; it was watched doing exactly
/// that before this file was committed.</para>
///
/// <para>(2) A HOP COUNT IS NOT A PAGE SIZE. The same class of defect as G1.4's find totals: a
/// number that moves when the page size moves is describing the page.
/// <see cref="The_shortest_path_length_does_not_move_when_maxPaths_moves"/> pins that the distance
/// is a fact about the graph and the path list is a window onto it.</para>
/// </summary>
public sealed class GraphSeamTests
{
    // A::Go -> B::Handle -> C::Save. The TYPE nodes A, B, C carry no edges at all: every edge in
    // this graph hangs off a member, which is what the real graph looks like after Batch A.
    private static GraphQuery MemberChain()
    {
        var g = new CodeGraphBuilder();
        foreach (var t in new[] { "Ns.A", "Ns.B", "Ns.C" })
            g.AddNode(new GraphNode(NodeId.ForType(t), t[3..], NodeKind.Type) { FilePath = $"{t[3..]}.cs" });

        g.AddNode(new GraphNode(NodeId.ForMember("Ns.A", "Go"), "A.Go", NodeKind.Member) { FilePath = "A.cs", LineNumber = 11 });
        g.AddNode(new GraphNode(NodeId.ForMember("Ns.B", "Handle"), "B.Handle", NodeKind.Member) { FilePath = "B.cs", LineNumber = 22 });
        g.AddNode(new GraphNode(NodeId.ForMember("Ns.C", "Save"), "C.Save", NodeKind.Member) { FilePath = "C.cs", LineNumber = 33 });

        g.AddEdge(new GraphEdge(NodeId.ForMember("Ns.A", "Go"), NodeId.ForMember("Ns.B", "Handle"), EdgeKind.Calls)
        { Resolution = Resolution.Semantic });
        g.AddEdge(new GraphEdge(NodeId.ForMember("Ns.B", "Handle"), NodeId.ForMember("Ns.C", "Save"), EdgeKind.ReadsWrites)
        { Resolution = Resolution.Syntactic });

        return new GraphQuery(g.Build(), []);
    }

    // Type-level graph with two 2-hop routes A->X->D and A->Y->D, plus a 3-hop detour A->X->Z->D.
    private static GraphQuery TypeDiamond()
    {
        var g = new CodeGraphBuilder();
        foreach (var t in new[] { "A", "X", "Y", "Z", "D", "Island" })
            g.AddNode(new GraphNode(NodeId.ForType($"Ns.{t}"), t, NodeKind.Type));

        void Edge(string a, string b) => g.AddEdge(
            new GraphEdge(NodeId.ForType($"Ns.{a}"), NodeId.ForType($"Ns.{b}"), EdgeKind.Calls)
            { Resolution = Resolution.Semantic });

        Edge("A", "X"); Edge("A", "Y");
        Edge("X", "D"); Edge("Y", "D");
        Edge("X", "Z"); Edge("Z", "D");

        return new GraphQuery(g.Build(), []);
    }

    private static GraphQuery Line(int length)
    {
        var g = new CodeGraphBuilder();
        for (var i = 0; i <= length; i++)
            g.AddNode(new GraphNode(NodeId.ForType($"Ns.N{i}"), $"N{i}", NodeKind.Type));
        for (var i = 0; i < length; i++)
            g.AddEdge(new GraphEdge(NodeId.ForType($"Ns.N{i}"), NodeId.ForType($"Ns.N{i + 1}"), EdgeKind.Calls));
        return new GraphQuery(g.Build(), []);
    }

    /// <summary>
    /// THE RED. Two types whose only connection runs through their members. A direct-edge search
    /// answers Direction.None here — both endpoints are bare Type nodes with zero edges — and that
    /// answer is wrong in the most damaging way available: it says "these are unconnected" about a
    /// call that happens on every request.
    /// </summary>
    [Fact]
    public void Type_to_type_connects_through_the_members_that_carry_it()
    {
        var seam = MemberChain().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.C"));

        Assert.Equal(SeamDirection.Forward, seam.Direction);
        Assert.Equal(2, seam.Hops);

        // And the hops name the MEMBERS, not the types — "which member carries the collaboration"
        // is the whole reason the roll-up keeps the true edge endpoints.
        var hops = Assert.Single(seam.Paths).Hops;
        Assert.Equal(["A.Go", "B.Handle"], hops.Select(h => h.FromTitle));
        Assert.Equal(["B.Handle", "C.Save"], hops.Select(h => h.ToTitle));
    }

    /// <summary>Each hop carries the seam kind, how the edge was bound, and where to look.</summary>
    [Fact]
    public void Every_hop_names_its_seam_kind_resolution_and_site()
    {
        var seam = MemberChain().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.C"));
        var hops = Assert.Single(seam.Paths).Hops;

        Assert.Equal([EdgeKind.Calls, EdgeKind.ReadsWrites], hops.Select(h => h.Kind));
        Assert.Equal([Resolution.Semantic, Resolution.Syntactic], hops.Select(h => h.Resolution));
        Assert.Equal(["A.cs", "B.cs"], hops.Select(h => h.FilePath));
        Assert.Equal([11, 22], hops.Select(h => h.LineNumber));
    }

    /// <summary>
    /// THE SECOND RED. maxPaths is a window onto the answer; the distance is the answer. If the
    /// reported hop count tracked the number of returned paths it would be describing the page,
    /// which is exactly the defect G1.4 found in find's total.
    /// </summary>
    [Fact]
    public void The_shortest_path_length_does_not_move_when_maxPaths_moves()
    {
        var q = TypeDiamond();
        var one = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 1);
        var many = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 10);

        Assert.Equal(2, one.Hops);
        Assert.Equal(2, many.Hops);
        Assert.Single(one.Paths);
        Assert.Equal(2, many.Paths.Length);       // A->X->D and A->Y->D; A->X->Z->D is longer
        Assert.All(many.Paths, p => Assert.Equal(2, p.Hops.Length));
    }

    /// <summary>The count of shortest paths is a fact about the graph, not about the page —
    /// so a caller shown 1 of 2 can tell it was shown 1 of 2.</summary>
    [Fact]
    public void Total_paths_counts_every_shortest_path_including_the_unshown_ones()
    {
        var seam = TypeDiamond().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 1);

        Assert.Single(seam.Paths);
        Assert.Equal(2, seam.TotalPaths);
    }

    /// <summary>"B reaches A" is a fact, and returning None for it would be a false negative the
    /// caller cannot see. The reverse walk is reported AS reverse — never silently as the answer to
    /// the question that was asked.</summary>
    [Fact]
    public void A_connection_that_runs_the_other_way_is_reported_as_reverse()
    {
        var seam = MemberChain().Seam(NodeId.ForType("Ns.C"), NodeId.ForType("Ns.A"));

        Assert.Equal(SeamDirection.Reverse, seam.Direction);
        Assert.Equal(2, seam.Hops);
        Assert.Equal("A.Go", Assert.Single(seam.Paths).Hops[0].FromTitle);
    }

    /// <summary>Genuinely unconnected: the walk exhausted both ends and found nothing. This is the
    /// only case that may claim there is no path.</summary>
    [Fact]
    public void Unconnected_nodes_report_none_without_blaming_the_depth_limit()
    {
        var seam = TypeDiamond().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.Island"));

        Assert.Equal(SeamDirection.None, seam.Direction);
        Assert.Empty(seam.Paths);
        Assert.Equal(0, seam.TotalPaths);
        Assert.False(seam.StoppedAtDepthLimit);
    }

    /// <summary>"No path within 2 hops" is not "no path". A search the budget ended says so, or the
    /// caller reads a hop budget as a fact about the codebase.</summary>
    [Fact]
    public void A_search_the_depth_budget_ended_says_so()
    {
        var q = Line(5);
        var capped = q.Seam(NodeId.ForType("Ns.N0"), NodeId.ForType("Ns.N5"), maxDepth: 2);

        Assert.Equal(SeamDirection.None, capped.Direction);
        Assert.True(capped.StoppedAtDepthLimit);

        // ...and the same seam with room finds it, which is what makes the flag actionable.
        var deeper = q.Seam(NodeId.ForType("Ns.N0"), NodeId.ForType("Ns.N5"), maxDepth: 8);
        Assert.Equal(SeamDirection.Forward, deeper.Direction);
        Assert.Equal(5, deeper.Hops);
        Assert.False(deeper.StoppedAtDepthLimit);
    }

    /// <summary>Both ends the same symbol: zero hops, forward, and no invented path rows.</summary>
    [Fact]
    public void The_same_symbol_at_both_ends_is_zero_hops_not_no_path()
    {
        var seam = TypeDiamond().Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.A"));

        Assert.Equal(SeamDirection.Forward, seam.Direction);
        Assert.Equal(0, seam.Hops);
        Assert.Empty(Assert.Single(seam.Paths).Hops);
    }

    // ── F4 (backlog #35) — the transport-port cell: producer → port ← consumer ──────────────────
    //
    // The fourth factory runs the REAL assembly pipeline (BodyFacts → seam detectors → GraphBuilder),
    // not a hand-built graph: the defect lives in what the builder emits, so a hand-assembled fixture
    // would only ever test the assertion's mirror. Shape (Book2Course's Q4, minimized): a producer
    // enqueues into an in-repo port interface, a consumer dequeues from it and drives the next stage.
    // Both callers land as IN-edges on the port Type (a call through a DI interface has no member body
    // to land on), so without a join the port is a sink — in-degree 2, out-degree 0.
    private static (CodeGraph Graph, GraphQuery Query) BuildZoo(params (string Fqn, TypeKind Kind, string Source)[] types)
        => BuildZoo([], types);

    private static (CodeGraph Graph, GraphQuery Query) BuildZoo(
        IReadOnlyList<Detection> detections, (string Fqn, TypeKind Kind, string Source)[] types)
    {
        var pi = new ProjectInfo("Zoo", @"C:\repo\Zoo\Zoo.csproj", "C#", ["net10.0"], [], []);
        var model = new DiscoveryModel { Projects = [pi] };
        foreach (var detection in detections) model.Detections.Add(detection);
        var facts = new List<DevContext.Core.Graph2.BodyFacts>();
        var parseOpts = CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG");

        foreach (var (fqn, kind, source) in types)
        {
            var name = fqn[(fqn.LastIndexOf('.') + 1)..];
            var filePath = $@"C:\repo\Zoo\{name}.cs";
            model.Types.TryAdd(fqn, new TypeDiscovery
            {
                Id = fqn,
                Name = name,
                Namespace = "Zoo",
                FilePath = filePath,
                Kind = kind,
                Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Application,
                SourceBody = source,
                Methods = [],
            });

            var tree = CSharpSyntaxTree.ParseText($"namespace Zoo {{ {source} }}", parseOpts, path: filePath);
            // F1 (#33): the model must DECLARE what the source declares — INV-C refuses a member
            // node of a type whose model entry does not vouch for it, fixture models included.
            model.Types[fqn].Methods = TestMethodSignatures.DeclaredIn(tree);
            // #37: the model must also DECLARE what the source declares about its BASE LIST — the
            // scan join's implementor oracle reads ImplementedInterfaces, and a fixture that omits
            // what its source spells would hide the very fact under test (the same rule that gave
            // Methods its DeclaredIn oracle).
            model.Types[fqn].ImplementedInterfaces =
                [.. tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                    .SelectMany(t => t.BaseList?.Types ?? default)
                    .Select(t => t.Type.ToString())];
            facts.AddRange(BodyFactExtractor.Extract(tree, filePath, "Zoo"));
        }

        var builder = new GraphBuilder(new SyntacticSymbolResolver(), new NoiseFilter(new ProjectClassifier([pi])));
        var (graph, entries) = builder.Build(model, SolutionScope.FromModel(model), facts);
        return (graph, new GraphQuery(graph, entries));
    }

    private const string PortSource = """
        public interface IJobQueue
        {
            Task EnqueueAsync(string job);
            Task<string?> DequeueAsync();
        }
        """;

    private const string ProducerSource = """
        public class BuildCoordinator
        {
            private readonly IJobQueue _queue;
            public BuildCoordinator(IJobQueue queue) => _queue = queue;
            public Task AdvanceAsync() => _queue.EnqueueAsync("ingest");
        }
        """;

    private const string StageSource = """
        public class IngestStage
        {
            public Task ExecuteAsync(string job) => Task.CompletedTask;
        }
        """;

    private const string ConsumerSource = """
        public class JobRunner
        {
            private readonly IJobQueue _queue;
            private readonly IngestStage _stage;
            public JobRunner(IJobQueue queue, IngestStage stage) { _queue = queue; _stage = stage; }
            public async Task RunNextAsync()
            {
                var job = await _queue.DequeueAsync();
                if (job is not null) await _stage.ExecuteAsync(job);
            }
        }
        """;

    private static (CodeGraph Graph, GraphQuery Query) TransportPortChain() => BuildZoo(
        ("Zoo.IJobQueue", TypeKind.Interface, PortSource),
        ("Zoo.BuildCoordinator", TypeKind.Class, ProducerSource),
        ("Zoo.IngestStage", TypeKind.Class, StageSource),
        ("Zoo.JobRunner", TypeKind.Class, ConsumerSource));

    // ── #37 — the registration-is-a-scan cell: AddSingleton(typeof(I), t) in an assembly scan ───
    //
    // Book2Course's residual behind F4: stages registered by reflection (AddStages() scans the
    // assembly for IStage implementors), dispatched dynamically through a registry. No spelling in
    // source names any stage as a registration or a callee, so the implementor sits at in-degree 0
    // and the seam walk exhausts honestly one hop short of it.

    private const string StageContractSource = """
        public interface IStage
        {
            Task ExecuteAsync(string job);
        }
        """;

    private const string IngestStageImplSource = """
        public class IngestStage : IStage
        {
            public Task ExecuteAsync(string job) => Task.CompletedTask;
        }
        """;

    private const string OutlineStageImplSource = """
        public class OutlineStage : IStage
        {
            public Task ExecuteAsync(string job) => Task.CompletedTask;
        }
        """;

    // Minimized from the real registry shape (IEnumerable<IStage> + a lookup): the binding
    // mechanism under test is a call through the INTERFACE, and a field-typed receiver is how the
    // zoo's resolver binds one (the F4 consumer's _queue rule; a foreach variable does not bind).
    private const string StageRegistrySource = """
        public class StageRegistry
        {
            private readonly IStage _current;
            public StageRegistry(IStage current) => _current = current;
            public Task RunAsync(string job) => _current.ExecuteAsync(job);
        }
        """;

    private const string StageModuleSource = """
        public static class StageModule
        {
            public static void AddStages(IServiceCollection services)
            {
                foreach (var stage in typeof(IStage).Assembly.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(IStage).IsAssignableFrom(t)))
                {
                    services.AddSingleton(typeof(IStage), stage);
                }
            }
        }
        """;

    private static (CodeGraph Graph, GraphQuery Query) ScanRegistrationChain() => BuildZoo(
        [new DiRegistrationDetection("IStage", "*", "Singleton", [], DiRegistrationShape.ScanRegistration)
        {
            ExtractorName = "DiRegistrationExtractor",
            SourceFile = @"C:\repo\Zoo\StageModule.cs",
            LineNumber = 7,
        }],
        [("Zoo.IStage", TypeKind.Interface, StageContractSource),
         ("Zoo.IngestStage", TypeKind.Class, IngestStageImplSource),
         ("Zoo.OutlineStage", TypeKind.Class, OutlineStageImplSource),
         ("Zoo.StageRegistry", TypeKind.Class, StageRegistrySource),
         ("Zoo.StageModule", TypeKind.Class, StageModuleSource)]);

    /// <summary>
    /// THE RED (#37, Book2Course F4's residual). Today `neighbors(IngestStage, in)` measures 0 —
    /// NOTHING in the graph lands on a reflection-registered stage, so the walk from the bridged
    /// queue exhausts honestly one hop short. The fix joins the scanned interface to each
    /// in-solution implementor: Resolution.Join, rendered joined, NEVER verified — the same
    /// truthfulness discipline the F4 port bridge established.
    /// </summary>
    [Fact]
    public void A_scan_registration_joins_the_interface_to_each_implementor()
    {
        var (graph, query) = ScanRegistrationChain();

        // Precondition — the dispatch call must actually land on the interface Type, or the seam
        // assertion below would pass/fail for the wrong reason after an extractor regression.
        Assert.Contains(graph.InEdges(NodeId.ForType("Zoo.IStage")),
            e => e.Kind == EdgeKind.Calls && e.TargetMember == "ExecuteAsync");

        var ingestIn = graph.InEdges(NodeId.ForType("Zoo.IngestStage"), EdgeKind.Resolves);
        var join = Assert.Single(ingestIn);
        Assert.Equal(NodeId.ForType("Zoo.IStage"), join.From);
        Assert.Equal(Resolution.Join, join.Resolution);            // joined on the wire, never verified
        Assert.Equal(2, join.MultiImplCount);                      // the scan's candidate set is honest
        Assert.Contains(RoleTags.ScanRegistrationDi, join.Tags);
        Assert.EndsWith("StageModule.cs:7", join.Provenance);      // the scan site is the provenance

        // BOTH implementors join — a scan registers the set, not a chosen one.
        Assert.Single(graph.InEdges(NodeId.ForType("Zoo.OutlineStage"), EdgeKind.Resolves));

        // And the seam routes: registry -> IStage (dispatch call) -> Resolves join -> stage.
        var seam = query.Seam(NodeId.ForType("Zoo.StageRegistry"), NodeId.ForType("Zoo.IngestStage"));
        Assert.Equal(SeamDirection.Forward, seam.Direction);
    }

    /// <summary>An interface the scan names but nothing in the solution implements joins NOTHING —
    /// a scan over externally-provided implementors must not fabricate an edge to guess at.</summary>
    [Fact]
    public void A_scan_over_an_interface_with_no_implementors_joins_nothing()
    {
        var (graph, _) = BuildZoo(
            [new DiRegistrationDetection("IStage", "*", "Singleton", [], DiRegistrationShape.ScanRegistration)
            {
                ExtractorName = "DiRegistrationExtractor",
                SourceFile = @"C:\repo\Zoo\StageModule.cs",
                LineNumber = 7,
            }],
            [("Zoo.IStage", TypeKind.Interface, StageContractSource),
             ("Zoo.StageModule", TypeKind.Class, StageModuleSource)]);

        Assert.Empty(graph.OutEdges(NodeId.ForType("Zoo.IStage")).Where(e => e.Kind == EdgeKind.Resolves));
    }

    /// <summary>
    /// THE RED (F4, backlog #35). Today's answer is <see cref="SeamDirection.None"/> — "unconnected"
    /// — about a producer and a stage that are wired through a queue on every single job. Both the
    /// enqueue and the dequeue land as IN-edges on the port interface's Type node, so the walk
    /// (out-edges only) can never route through it: the graph holds the truth and the query cannot
    /// reach it. The fix classifies the port's callers by verb (Enqueue* writes, Dequeue* reads —
    /// the evidence is already on <see cref="GraphEdge.TargetMember"/>) and emits a JOINED bridge
    /// port → consumer, so the path producer → port → consumer → stage exists and every joined hop
    /// says it was joined, never verified.
    /// </summary>
    [Fact]
    public void A_seam_crosses_an_in_repo_transport_port_through_a_joined_hop()
    {
        var (graph, query) = TransportPortChain();

        // Preconditions — the defect's shape must actually be in the graph, or this test would pass
        // for the wrong reason (no edges at all) after an extractor regression.
        var port = NodeId.ForType("Zoo.IJobQueue");
        var producerCall = graph.InEdges(port).Where(e => e.Kind == EdgeKind.Calls && e.TargetMember == "EnqueueAsync");
        var consumerCall = graph.InEdges(port).Where(e => e.Kind == EdgeKind.Calls && e.TargetMember == "DequeueAsync");
        Assert.Single(producerCall);
        Assert.Single(consumerCall);

        var seam = query.Seam(NodeId.ForType("Zoo.BuildCoordinator"), NodeId.ForType("Zoo.IngestStage"));

        Assert.Equal(SeamDirection.Forward, seam.Direction);
        Assert.Equal(3, seam.Hops);

        var hops = Assert.Single(seam.Paths).Hops;
        Assert.Equal(["BuildCoordinator.AdvanceAsync", "IJobQueue", "JobRunner.RunNextAsync"], hops.Select(h => h.FromTitle));

        // The bridge hop is the port: it renders as a JOINED consume — a classification the builder
        // made from verb evidence — never as a verified call. Truthfulness is the product.
        var bridge = hops[1];
        Assert.Equal(EdgeKind.Consumes, bridge.Kind);
        Assert.Equal(Resolution.Join, bridge.Resolution);
    }

    /// <summary>
    /// THE NEGATIVE TWIN (the drive's Q4 first case, which was CORRECT as found:false). A port with
    /// only writers is staging, not transport: nothing in the graph reads it, so bridging it would
    /// fabricate a path the repo does not have. The uploader stages work into the port; the stage
    /// runs off something else the graph cannot see — and the honest answer stays "unconnected".
    /// </summary>
    [Fact]
    public void A_port_with_only_writers_does_not_bridge()
    {
        var (graph, query) = BuildZoo(
            ("Zoo.IJobQueue", TypeKind.Interface, PortSource),
            ("Zoo.BuildCoordinator", TypeKind.Class, ProducerSource),
            ("Zoo.IngestStage", TypeKind.Class, StageSource),
            ("Zoo.SourceUploader", TypeKind.Class, """
                public class SourceUploader
                {
                    private readonly IJobQueue _queue;
                    public SourceUploader(IJobQueue queue) => _queue = queue;
                    public Task UploadAsync() => _queue.EnqueueAsync("staged");
                }
                """));

        // No reader in the graph → no bridge out of the port, and the seam stays honest.
        Assert.DoesNotContain(graph.OutEdges(NodeId.ForType("Zoo.IJobQueue")), e => e.Kind == EdgeKind.Consumes);

        var seam = query.Seam(NodeId.ForType("Zoo.BuildCoordinator"), NodeId.ForType("Zoo.IngestStage"));
        Assert.Equal(SeamDirection.None, seam.Direction);
        Assert.False(seam.StoppedAtDepthLimit);
    }

    /// <summary>
    /// A type calling BOTH directions on the same port is the transport's own plumbing (a decorator,
    /// a drain loop, an in-memory bus), not a producer or a consumer — the same rule
    /// EventBusExtractor's queue seams apply to raw transports. Its calls must not mint a bridge.
    /// </summary>
    [Fact]
    public void A_type_calling_both_directions_is_infrastructure_not_a_bridge_endpoint()
    {
        var (graph, query) = BuildZoo(
            ("Zoo.IJobQueue", TypeKind.Interface, PortSource),
            ("Zoo.IngestStage", TypeKind.Class, StageSource),
            ("Zoo.QueueDrain", TypeKind.Class, """
                public class QueueDrain
                {
                    private readonly IJobQueue _queue;
                    private readonly IngestStage _stage;
                    public QueueDrain(IJobQueue queue, IngestStage stage) { _queue = queue; _stage = stage; }
                    public Task RequeueAsync() => _queue.EnqueueAsync("again");
                    public async Task DrainAsync()
                    {
                        var job = await _queue.DequeueAsync();
                        if (job is not null) await _stage.ExecuteAsync(job);
                    }
                }
                """));

        Assert.DoesNotContain(graph.OutEdges(NodeId.ForType("Zoo.IJobQueue")), e => e.Kind == EdgeKind.Consumes);

        var seam = query.Seam(NodeId.ForType("Zoo.IJobQueue"), NodeId.ForType("Zoo.IngestStage"));
        Assert.NotEqual(SeamDirection.Forward, seam.Direction);
    }

    /// <summary>Determinism seal: the same question returns the same paths in the same order.
    /// Path enumeration walks a predecessor DAG, and a HashSet iteration order in there would make
    /// a query layer that answers differently on two runs of the same snapshot.</summary>
    [Fact]
    public void The_same_seam_answers_identically_twice()
    {
        var q = TypeDiamond();
        var a = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 10);
        var b = q.Seam(NodeId.ForType("Ns.A"), NodeId.ForType("Ns.D"), maxPaths: 10);

        Assert.Equal(
            a.Paths.Select(p => string.Join(" -> ", p.Hops.Select(h => h.ToTitle))),
            b.Paths.Select(p => string.Join(" -> ", p.Hops.Select(h => h.ToTitle))));
    }
}
