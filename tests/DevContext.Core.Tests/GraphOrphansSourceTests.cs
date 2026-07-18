using DevContext.Core.Graph;
using G2 = DevContext.Core.Graph2;
using DevContext.Core.Insights;
using DevContext.Core.Models;

namespace DevContext.Core.Tests;

/// <summary>I1 (Prism D2) — coverage-gated dead-code claims. Pins the audit's podcasts round:
/// EpisodeDto (constructed in a LINQ projection), IRequest (implemented by 5 classes), and
/// FeedCategory (EF join entity) were all accused dead; only genuinely unreferenced types may be.</summary>
public sealed class GraphOrphansSourceTests
{
    private static G2.SymbolRef Ref(string text) => new()
    {
        Text = text,
        Site = new G2.RefSite { File = @"C:\repo\src\App\EpisodesApi.cs", Line = 10, Project = "App" },
    };

    /// <summary>Graph passing the floor: >=30 Calls (60% semantic), 6 Handles, wired entries — plus
    /// four zero-in-degree Type nodes: one truly dead, one constructed, one implemented, one entity.</summary>
    private static (CodeGraphBuilder G, ImmutableArray<EntryPoint> Entries) BuildBaseGraph()
    {
        var g = new CodeGraphBuilder();
        var hub = new NodeId(NodeKind.Type, "App.Hub");
        g.AddNode(new GraphNode(hub, "Hub", NodeKind.Type) { FilePath = @"C:\repo\src\App\Hub.cs" });
        for (var i = 0; i < 36; i++)
        {
            var callee = new NodeId(NodeKind.Member, $"App.Svc{i}.Run");
            g.AddNode(new GraphNode(callee, $"Run{i}", NodeKind.Member));
            g.AddEdge(new GraphEdge(hub, callee, EdgeKind.Calls)
            {
                Resolution = i < 24 ? Resolution.Semantic : Resolution.Syntactic,
            });
        }
        for (var i = 0; i < 6; i++)
        {
            var handler = new NodeId(NodeKind.Type, $"App.Handler{i}");
            g.AddNode(new GraphNode(handler, $"Handler{i}", NodeKind.Type));
            g.AddEdge(new GraphEdge(hub, handler, EdgeKind.Handles));
        }

        g.AddNode(new GraphNode(new NodeId(NodeKind.Type, "App.DeadHelper"), "DeadHelper", NodeKind.Type));
        g.AddNode(new GraphNode(new NodeId(NodeKind.Type, "App.EpisodeDto"), "EpisodeDto", NodeKind.Type));
        g.AddNode(new GraphNode(new NodeId(NodeKind.Type, "App.IRequest"), "IRequest", NodeKind.Type));
        g.AddNode(new GraphNode(new NodeId(NodeKind.Type, "App.FeedCategory"), "FeedCategory", NodeKind.Type)
        {
            Tags = [RoleTags.Entity],
        });

        ImmutableArray<EntryPoint> entries =
        [
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /a", hub) { Target = "Hub.Run", Project = "App" },
            new EntryPoint(EntryPointKind.HttpEndpoint, "GET /b", hub) { Target = "Hub.Run", Project = "App" },
        ];
        return (g, entries);
    }

    private static DiscoveryModel ModelWithImplementor()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("App.CreateFeedRequest", new TypeDiscovery
        {
            Id = "App.CreateFeedRequest", Name = "CreateFeedRequest", Namespace = "App",
            FilePath = @"C:\repo\src\App\CreateFeedRequest.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Application,
            ImplementedInterfaces = ["IRequest"],
        });
        return model;
    }

    private static SharedAnalysisContext AnalysisWithDtoCreation() => new()
    {
        AllBodyFacts =
        [
            new G2.BodyFacts(new G2.SymbolId(default, "App.EpisodesApi.GetEpisodeById"), "GetEpisodeById",
                [new G2.CreationOp(12, Ref("EpisodeDto"))]),
        ],
    };

    [Fact]
    public void Live_types_are_excluded_and_the_claim_carries_its_coverage_basis()
    {
        var (g, entries) = BuildBaseGraph();
        g.SetEntries(entries);
        var graph = g.Build();
        var insight = new GraphOrphansSource()
            .Compute(ModelWithImplementor(), graph, entries, AnalysisWithDtoCreation())
            .SingleOrDefault();

        Assert.NotNull(insight);
        Assert.Contains("DeadHelper", insight.Evidence);          // genuinely unreferenced — still claimed
        Assert.DoesNotContain("EpisodeDto", insight.Evidence);    // body-constructed (LINQ projection)
        Assert.DoesNotContain("IRequest", insight.Evidence);      // implemented by CreateFeedRequest
        Assert.DoesNotContain("FeedCategory", insight.Evidence);  // entity-indexed
        Assert.Contains("coverage basis", insight.ConfidenceBasis);
        Assert.Contains("entries wired", insight.ConfidenceBasis);
    }

    [Fact]
    public void No_claim_below_the_verified_edge_floor()
    {
        // Same shape but Calls edges all syntactic — "zero inbound references" would only mean
        // "zero edges we captured", so the insight must stay silent.
        var g = new CodeGraphBuilder();
        var hub = new NodeId(NodeKind.Type, "App.Hub");
        g.AddNode(new GraphNode(hub, "Hub", NodeKind.Type));
        for (var i = 0; i < 36; i++)
        {
            var callee = new NodeId(NodeKind.Member, $"App.Svc{i}.Run");
            g.AddNode(new GraphNode(callee, $"Run{i}", NodeKind.Member));
            g.AddEdge(new GraphEdge(hub, callee, EdgeKind.Calls) { Resolution = Resolution.Syntactic });
        }
        for (var i = 0; i < 6; i++)
        {
            var handler = new NodeId(NodeKind.Type, $"App.Handler{i}");
            g.AddNode(new GraphNode(handler, $"Handler{i}", NodeKind.Type));
            g.AddEdge(new GraphEdge(hub, handler, EdgeKind.Handles));
        }
        g.AddNode(new GraphNode(new NodeId(NodeKind.Type, "App.DeadHelper"), "DeadHelper", NodeKind.Type));
        ImmutableArray<EntryPoint> entries =
            [new EntryPoint(EntryPointKind.HttpEndpoint, "GET /a", hub) { Target = "Hub.Run" }];
        g.SetEntries(entries);
        var graph = g.Build();

        var result = new GraphOrphansSource()
            .Compute(new DiscoveryModel(), graph, entries, new SharedAnalysisContext());

        Assert.Empty(result);
    }

    [Fact]
    public void Library_archetype_makes_no_dead_code_claim_at_all()
    {
        // wolverine P7 catch: a library's public types are consumed EXTERNALLY — zero inbound graph
        // references is their normal state, not evidence of death.
        var (g, entries) = BuildBaseGraph();
        g.SetEntries(entries);
        var graph = g.Build();
        var model = ModelWithImplementor();
        model.Archetype = "Library";

        Assert.Empty(new GraphOrphansSource().Compute(model, graph, entries, AnalysisWithDtoCreation()));
    }

    [Fact]
    public void Type_live_only_as_a_generic_argument_of_a_base_type_is_not_accused()
    {
        // ValidationOutcome : List<WolverineValidationResult> — the element type never appears as a
        // bare base/interface name, only inside the generic argument list.
        var (g, entries) = BuildBaseGraph();
        g.AddNode(new GraphNode(new NodeId(NodeKind.Type, "App.WolverineValidationResult"),
            "WolverineValidationResult", NodeKind.Type));
        g.SetEntries(entries);
        var graph = g.Build();

        var model = ModelWithImplementor();
        model.Types.TryAdd("App.ValidationOutcome", new TypeDiscovery
        {
            Id = "App.ValidationOutcome", Name = "ValidationOutcome", Namespace = "App",
            FilePath = @"C:\repo\src\App\ValidationOutcome.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Domain,
            BaseTypes = ["List<WolverineValidationResult>"],
        });

        var insight = new GraphOrphansSource()
            .Compute(model, graph, entries, AnalysisWithDtoCreation())
            .SingleOrDefault();

        Assert.NotNull(insight);
        Assert.DoesNotContain("WolverineValidationResult", insight.Evidence);
        Assert.Contains("DeadHelper", insight.Evidence); // the genuine orphan is still claimed
    }

    [Fact]
    public void Fallback_overload_without_analysis_makes_no_claim()
    {
        var (g, entries) = BuildBaseGraph();
        g.SetEntries(entries);
        var graph = g.Build();
        Assert.Empty(new GraphOrphansSource().Compute(ModelWithImplementor(), graph, entries));
    }
}
