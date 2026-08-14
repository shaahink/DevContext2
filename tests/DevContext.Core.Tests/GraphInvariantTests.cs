using DevContext.Core.Graph;
using DevContext.Core.Graph2;
using DevContext.Core.Graph2.Seams;

using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Tests;

/// <summary>Pre-release V1.3 — the two STANDING graph invariants. Each was a measured defect first
/// (backlog #7's rider and #18) and each is now enforced where a node is MADE, so no producer and no
/// pass order can reach a surface with the shape. Both are refusals rather than repairs: the engine
/// does not know which type the offending node means, and inventing one is how #7 happened.
///
///   INV-A  no node carries kind Type with a MEMBER id
///   INV-B  lambda / expression TEXT never becomes a node key or title
///
/// Measured RED at 0fd1cbe over nine poles (4919 nodes / 2501 Type nodes): INV-A 4, INV-B 76.
/// Evidence: eval-results/2026-08-13/v1-invariants/.</summary>
public sealed class GraphInvariantTests
{
    // ── The one predicate pair, stated ───────────────────────────────────────────────────────

    [Theory]
    // Real Type-node keys the engine shipped, verbatim from the V1.3 probe:
    [InlineData("options => options.AddFixedWindowLimiter(\"feeds\", options => { })")]
    [InlineData("new System.IO.StringWriter()")]
    [InlineData("Config.GetApis()")]
    [InlineData("typeof(IPipelineBehavior<,>)")]
    [InlineData("\"self\"")]
    [InlineData("prior with { SnapshotCacheRoot = _cacheRoot }")]
    [InlineData("o => o.BaseAddress = new(MobileBffHost)")]
    [InlineData("options =>\n{\n    // Lifetime: Singleton is fastest per docs\n}")]
    public void Expression_text_is_recognised_as_expression_text(string shipped)
        => Assert.True(SymbolCanon.IsExpressionText(shipped));

    [Theory]
    // A name, however unusual, is not expression text. The last two are the RULE'S BOUNDARY and are
    // pinned deliberately: a queue channel is keyed by its display text on purpose (B5/Prism D1.2d),
    // so a "must look like a type name" rule would delete real event wiring. Seven such nodes exist
    // across the nine poles and the V1.3 evidence names them rather than sweeping them up.
    [InlineData("Ns.Sub.Type")]
    [InlineData("Ns.Outer.Inner`2")]
    [InlineData("IPipelineBehavior<,>")]
    [InlineData("System.Collections.Generic.List<string>")]
    [InlineData("int[]")]
    [InlineData("feed-queue [AzureStorageQueue]")]
    [InlineData("<OnModelCreating>")]
    public void A_name_is_not_expression_text(string name)
        => Assert.False(SymbolCanon.IsExpressionText(name));

    [Fact]
    public void Empty_text_is_not_a_name()
    {
        Assert.True(SymbolCanon.IsExpressionText(null));
        Assert.True(SymbolCanon.IsExpressionText("   "));
    }

    [Fact]
    public void A_member_key_is_the_one_with_the_separator()
    {
        Assert.True(SymbolCanon.IsMemberKey("Hangfire.StackTraceHtmlFragments::Type(1)"));
        Assert.True(SymbolCanon.IsMemberKey("MediatR.Mediator::Send"));
        Assert.False(SymbolCanon.IsMemberKey("Hangfire.StackTraceHtmlFragments"));
        Assert.False(SymbolCanon.IsMemberKey(""));
    }

    // ── INV-A: a Type node may not carry a member id ─────────────────────────────────────────

    [Fact]
    public void No_node_carries_kind_Type_with_a_member_id()
    {
        // Hangfire's `string IStackTraceFormatter<string>.Type(string markup)` — an explicit interface
        // implementation — shipped as Type:Hangfire.StackTraceHtmlFragments::Type(1) with 26 BCL
        // System.Type references bound onto it, ranking a dashboard formatter fragment #5 in the repo
        // by connectivity. A member wearing a type node's identity.
        var g = new CodeGraphBuilder();
        var phantom = NodeId.ForType("Hangfire.StackTraceHtmlFragments::Type(1)");
        var real = NodeId.ForType("Hangfire.StackTraceHtmlFragments");

        g.AddNode(new GraphNode(phantom, "Type", NodeKind.Type));
        g.AddNode(new GraphNode(real, "StackTraceHtmlFragments", NodeKind.Type) { FilePath = "/src/S.cs" });

        Assert.False(g.HasNode(phantom));
        Assert.True(g.HasNode(real));

        // and the edge that wanted it leaves no half behind
        Assert.False(g.AddEdge(new GraphEdge(real, phantom, EdgeKind.Calls)));
        Assert.Empty(g.Build().OutEdges(real));
    }

    [Fact]
    public void A_refusal_is_COUNTED_and_named_so_it_is_not_silent()
    {
        // E1.3 (#7): the refusal drops the node AND every edge that wanted it. Silently, that hides a
        // producer regression and the lost edges together — which is exactly how Type:…::Type(1) lived
        // for months. The tally is per DISTINCT key so repeated attempts by many passes count once.
        var g = new CodeGraphBuilder();
        var phantom = NodeId.ForType("Hangfire.StackTraceHtmlFragments::Type(1)");

        g.AddNode(new GraphNode(phantom, "Type", NodeKind.Type));
        g.AddNode(new GraphNode(phantom, "Type", NodeKind.Type));                      // same key, one entry
        g.AddNode(new GraphNode(NodeId.ForType("o => o.Use()"), "o => o.Use()", NodeKind.Type));
        g.AddNode(new GraphNode(NodeId.ForType("MyApp.Real"), "Real", NodeKind.Type)); // healthy: not counted

        Assert.Equal(2, g.RefusedNodes.Count);
        Assert.Equal("INV-A", g.RefusedNodes[0].Invariant);
        Assert.Contains("::Type(1)", g.RefusedNodes[0].Key);
        Assert.Equal("INV-B", g.RefusedNodes[1].Invariant);
    }

    [Fact]
    public void The_refusal_tally_reaches_the_DIAGNOSTICS_of_a_real_build()
    {
        // The positive control for the sweep. Measuring "0 refusals" on seven poles only means
        // something if a refusal WOULD have printed — so a producer is made to hand the graph a
        // member-keyed type (the #7 shape, straight from model.Types) and the diagnostic is read back
        // off the same DiscoveryModel the pipeline prints from.
        var model = new DiscoveryModel();
        model.Projects.Add(new ProjectInfo("Engine", @"C:/repo/src/Engine/Engine.csproj", "C#", [], [], []));
        model.Types.TryAdd("Engine.Fragments::Type(1)", new TypeDiscovery
        {
            Id = "Engine.Fragments::Type(1)", Name = "Type", Namespace = "Engine",
            FilePath = @"C:/repo/src/Engine/Fragments.cs", Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public, Layer = ArchitectureLayer.Domain,
        });

        var scope = SolutionScope.FromModel(model);
        var (graph, _) = new GraphBuilder(
                new SyntacticSymbolResolver(),
                new NoiseFilter(new ProjectClassifier(model.Projects)))
            .Build(model, scope);

        Assert.Empty(graph.Nodes);
        var d = Assert.Single(model.Diagnostics, x => x.Source == "GraphInvariants");
        Assert.Contains("INV-A", d.Message);
        Assert.Contains("::Type(1)", d.Message);
    }

    [Fact]
    public void A_graph_with_no_malformed_node_reports_NO_refusal()
    {
        var g = new CodeGraphBuilder();
        g.AddNode(new GraphNode(NodeId.ForType("MyApp.Real"), "Real", NodeKind.Type));
        g.AddNode(new GraphNode(NodeId.ForMember("MyApp.Real", "Do"), "Do", NodeKind.Member));
        Assert.Empty(g.RefusedNodes);
    }

    [Fact]
    public void A_member_NODE_still_carries_its_member_id()
    {
        // The refusal is about the KIND, not the id: Member nodes are the ones that own "::".
        var g = new CodeGraphBuilder();
        var m = NodeId.ForMember("Hangfire.StackTraceHtmlFragments", "Type");
        g.AddNode(new GraphNode(m, "irrelevant", NodeKind.Member));
        Assert.True(g.HasNode(m));
        Assert.Equal("StackTraceHtmlFragments.Type", g.Build().Node(m)!.Title);   // V1.2
    }

    // ── INV-B: lambda / expression text never becomes a node ─────────────────────────────────

    [Theory]
    [InlineData("options =>\n{\n    options.PermitLimit = 5;\n}")]
    [InlineData("new MediatR.Tests.PipelineTests.Logger()")]
    [InlineData("typeof(GenericPipelineBehavior<,>)")]
    [InlineData("Config.GetClients(builder.Configuration)")]
    [InlineData("\"self\"")]
    public void Lambda_or_expression_text_never_becomes_a_Type_node(string shipped)
    {
        var g = new CodeGraphBuilder();
        var id = NodeId.ForType(shipped);
        g.AddNode(new GraphNode(id, shipped, NodeKind.Type) { Layer = "Infrastructure" });
        Assert.False(g.HasNode(id));
        Assert.Empty(g.Build().Nodes);
    }

    [Fact]
    public void A_queue_channel_node_survives_the_rule()
    {
        // The boundary, pinned: the channel's key IS its display text and it is not expression text.
        // Prism D1.2d's FeedsApi -> [feed-queue] -> Worker wire depends on this node existing.
        var g = new CodeGraphBuilder();
        var channel = NodeId.ForType("feed-queue [AzureStorageQueue]");
        g.AddNode(new GraphNode(channel, "feed-queue [AzureStorageQueue]", NodeKind.Type));
        Assert.True(g.HasNode(channel));
    }

    [Fact]
    public void A_named_node_with_an_expression_TITLE_keeps_the_node_and_loses_the_title()
    {
        // Key is a name, title is not — the node is real, so the title is DERIVED from the key
        // (V1.2's move for members) rather than the node being thrown away.
        var g = new CodeGraphBuilder();
        var id = NodeId.ForType("Ns.Sub.Widget`1");
        g.AddNode(new GraphNode(id, "new Ns.Sub.Widget<int>()", NodeKind.Type));
        Assert.Equal("Widget", g.Build().Node(id)!.Title);
    }

    private static readonly RefSite Site = new() { File = "/src/C.cs", Line = 0, Project = "Shop" };

    // ── The producer the invariant was measured on ───────────────────────────────────────────

    [Fact]
    public void A_call_whose_receiver_resolves_to_a_MEMBER_is_not_a_call_to_a_type()
    {
        // eShop's WebNavigatingEventArgsConverter::Convert(4), reproduced from its shape: a class
        // declares a method named Convert, the repo declares no TYPE called Convert, and the body
        // calls BCL Convert.ToInt32. SymbolTable's member tier fires only when no type candidate
        // exists — so the receiver "Convert" resolved onto the class's OWN method, and PlainCall
        // emitted a seam whose target was a member id. It became a Type node with kind Type.
        const string src = """
            namespace Shop.Converters;
            public class WebNavigatingEventArgsConverter
            {
                public object Convert(object value, object t, object p, object c)
                {
                    return System.Convert.ToInt32(value);
                }
            }
            """;
        var facts = BodyFactExtractor.Extract(CSharpSyntaxTree.ParseText(src), "/src/C.cs", "Shop");
        var owner = new TypeDiscovery
        {
            Id = "Shop.Converters.WebNavigatingEventArgsConverter",
            Name = "WebNavigatingEventArgsConverter",
            Namespace = "Shop.Converters",
            FilePath = "/src/C.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Presentation,
        };
        var symbols = new SymbolTable([owner], _ => "Shop", facts);

        // The premise, measured rather than assumed: the short name DOES resolve, to a MEMBER.
        var asName = symbols.Resolve(new DevContext.Core.Graph2.SymbolRef { Text = "Convert", Site = Site });
        Assert.NotNull(asName.Resolved);
        Assert.Equal(SymbolKind.Member, asName.Resolved!.Value.Kind);

        // So the detector must decline: a member answer is not a type answer.
        var ctx = new SeamContext { Symbols = symbols };
        var matches = facts.SelectMany(b => new PlainCallDetector().Detect(b, ctx)).ToList();
        Assert.DoesNotContain(matches, m => m.Target.Text == "Convert");
    }
}
