using DevContext.Core.Graph;
using DevContext.Core.Insights;

using Microsoft.CodeAnalysis;

namespace DevContext.Core.Tests;

/// <summary>Batch E (R2 §2.E item 2) — one counting function per stat. Each test pins a number that two
/// surfaces used to compute for themselves, and states which reading is the true one.</summary>
public sealed class NumberReconciliationTests
{
    [Fact]
    public void Confidence_ledger_reports_only_countable_facts()
    {
        // The retired member was OverallConfidence = verified/total*0.7 + (total-approx)/total*0.3.
        // The home chip printed it under the label "verified" while the chip's own tooltip printed
        // VerifiedEdgePct — one word, two numbers. Everything here is now a count, or a ratio of two
        // counts that travel beside it, so the chip and its explanation cannot disagree.
        var g = new CodeGraphBuilder();
        var a = NodeId.ForType("App.A");
        var b = NodeId.ForType("App.B");
        var c = NodeId.ForType("App.C");
        g.AddNode(new GraphNode(a, "A", NodeKind.Type));
        g.AddNode(new GraphNode(b, "B", NodeKind.Type));
        g.AddNode(new GraphNode(c, "C", NodeKind.Type));
        g.AddEdge(new GraphEdge(a, b, EdgeKind.Calls) { Resolution = Resolution.Semantic });
        g.AddEdge(new GraphEdge(a, c, EdgeKind.Calls) { Resolution = Resolution.Syntactic });

        var ledger = ConfidenceLedger.Compute(g.Build(), []);

        Assert.Equal(2, ledger.TotalEdges);
        Assert.Equal(0.5, ledger.VerifiedEdgePct);   // 1 of 2 semantic
        Assert.Equal(0.5, ledger.ApproxEdgePct);     // 1 of 2 syntactic
        // Every ratio is reproducible from counts the ledger also carries.
        Assert.Equal(ledger.VerifiedEdgePct, Math.Round(1d / ledger.TotalEdges, 2));
    }

    [Fact]
    public void One_member_title_vocabulary_whoever_writes_the_node_first()
    {
        // V1.2 (backlog #17). The engine shipped TWO Member-title vocabularies: the entry builders
        // wrote "CatalogApi.GetAllItemsV1", the call-graph and seam passes wrote a bare "Send" from
        // BodyFacts.MemberName — 343 owner-qualified against 627 bare across six poles, same kind,
        // same page. Titles merge FIRST-WRITE-WINS, so which vocabulary a node got was decided by
        // pass order, not by anything about the node.
        //
        // The title is now derived from the member key in one place. This test writes the same two
        // nodes the two ways the two producer families used to, in both orders.
        var owner = "Shop.Catalog.Api.CatalogApi";
        var id = NodeId.ForMember(owner, "GetAllItemsV1");

        // 1. The rule itself: owner SHORT name (nested chain and generic arity stripped) + "." + member.
        Assert.Equal("CatalogApi.GetAllItemsV1", DevContext.Core.Graph2.SymbolCanon.MemberTitle(id.Key));
        Assert.Equal("CatalogApi.GetAllItemsV1", DevContext.Core.Graph2.SymbolCanon.MemberTitle(owner, "GetAllItemsV1"));
        Assert.Equal("Inner.Handle",
            DevContext.Core.Graph2.SymbolCanon.MemberTitle(NodeId.ForMember("Ns.Outer.Inner`2", "Handle").Key));

        // 2. A producer that spells a bare title (the seam/call-graph shape) does not get one.
        var bareFirst = new CodeGraphBuilder();
        bareFirst.AddNode(new GraphNode(id, "GetAllItemsV1", NodeKind.Member) { FilePath = "CatalogApi.cs" });
        bareFirst.AddNode(new GraphNode(id, "CatalogApi.GetAllItemsV1", NodeKind.Member));
        Assert.Equal("CatalogApi.GetAllItemsV1", Assert.Single(bareFirst.Build().Nodes).Title);

        // 3. Reverse the pass order — the same title. That is what "one vocabulary" means here:
        //    the answer is a function of the key, so no ordering can change it.
        var qualifiedFirst = new CodeGraphBuilder();
        qualifiedFirst.AddNode(new GraphNode(id, "CatalogApi.GetAllItemsV1", NodeKind.Member));
        qualifiedFirst.AddNode(new GraphNode(id, "GetAllItemsV1", NodeKind.Member) { FilePath = "CatalogApi.cs" });
        Assert.Equal("CatalogApi.GetAllItemsV1", Assert.Single(qualifiedFirst.Build().Nodes).Title);

        // 4. Nothing else is touched: a Type node's title is still whatever its producer said, and
        //    a key that is not a member key comes back verbatim (defensive, not a real shape).
        var types = new CodeGraphBuilder();
        types.AddNode(new GraphNode(NodeId.ForType(owner), "CatalogApi", NodeKind.Type));
        Assert.Equal("CatalogApi", Assert.Single(types.Build().Nodes).Title);
        Assert.Equal(owner, DevContext.Core.Graph2.SymbolCanon.MemberTitle(owner));
    }

    [Fact]
    public void One_definition_of_a_verified_edge_across_every_counting_surface()
    {
        // V1.1 (backlog #25). The engine shipped TWO answers to "is this edge verified?".
        // GraphStats/SeamStat called only Syntactic approximate, so the MCP stats tool, the CLI
        // `query stats` and the report all computed `verified = count - approx` — which counts
        // Resolution.Join, the enum's DEFAULT, as Roslyn-verified. GraphOrphansSource,
        // ConfidenceLedger and FlowIndexBuilder counted Semantic only, and the desktop explorer
        // rendered the same edge "approx" that the CLI called "verified".
        //
        // The middle edge below is the one that decides it: it is added with NO Resolution at all,
        // which is exactly how most of the graph's edges arrive.
        var g = new CodeGraphBuilder();
        var a = NodeId.ForType("App.A");
        var b = NodeId.ForType("App.B");
        var c = NodeId.ForType("App.C");
        var d = NodeId.ForType("App.D");
        foreach (var (id, title) in new[] { (a, "A"), (b, "B"), (c, "C"), (d, "D") })
            g.AddNode(new GraphNode(id, title, NodeKind.Type));
        g.AddEdge(new GraphEdge(a, b, EdgeKind.Calls) { Resolution = Resolution.Semantic });
        g.AddEdge(new GraphEdge(a, c, EdgeKind.Calls));                                  // unlabelled → Join
        g.AddEdge(new GraphEdge(a, d, EdgeKind.Calls) { Resolution = Resolution.Syntactic });
        var graph = g.Build();

        // 1. The tiers are an exhaustive partition, and they come from ONE helper.
        Assert.Equal(EdgeTier.Verified, EdgeConfidence.TierOf(Resolution.Semantic));
        Assert.Equal(EdgeTier.Approximate, EdgeConfidence.TierOf(Resolution.Syntactic));
        Assert.Equal(EdgeTier.Joined, EdgeConfidence.TierOf(default(Resolution))); // the DEFAULT is Join

        // 2. The seam row carries the split instead of leaving it to be inferred.
        var seam = Assert.Single(GraphStats.Compute(graph, []).Seams);
        Assert.Equal(3, seam.Count);
        Assert.Equal(seam.Count, seam.Verified + seam.Joined + seam.Approx);
        Assert.Equal((1, 1, 1), (seam.Verified, seam.Joined, seam.Approx));

        // 3. The retired inference, pinned so it cannot come back: `Count - Approx` reads TWO
        //    verified edges on this graph, and one of the two is an edge nobody labelled.
        Assert.Equal(2, seam.Count - seam.Approx);
        Assert.NotEqual(seam.Count - seam.Approx, seam.Verified);

        // 4. The ledger — the other surface that answers the same question — now answers with the
        //    same number. Its approx used to be `Syntactic || Confidence < 1.0f`, a fourth spelling
        //    that could put one edge in two buckets at once.
        var ledger = ConfidenceLedger.Compute(graph, []);
        var ledgerSeam = Assert.Single(ledger.PerSeam);
        Assert.Equal(seam.Verified, ledgerSeam.Verified);
        Assert.Equal(seam.Joined, ledgerSeam.Joined);
        Assert.Equal(seam.Approx, ledgerSeam.Approx);
        Assert.Equal(seam.Verified, (int)Math.Round(ledger.VerifiedEdgePct * ledger.TotalEdges));
        Assert.Equal(seam.Approx, (int)Math.Round(ledger.ApproxEdgePct * ledger.TotalEdges));

        // 5. Confidence is a SEPARATE axis and may not leak into the tier: a semantically resolved
        //    seam target ships at 0.95, and it is still verified.
        var lowConfidence = new GraphEdge(a, b, EdgeKind.Calls)
        {
            Resolution = Resolution.Semantic, Confidence = 0.95f,
        };
        Assert.True(EdgeConfidence.IsVerified(lowConfidence));
        Assert.False(EdgeConfidence.IsApproximate(lowConfidence));
    }

    [Fact]
    public void Public_type_count_excludes_the_librarys_own_tests_and_samples()
    {
        // The library page counted the real surface while the lib.public-surface INSIGHT counted every
        // public type in the model — tests and samples included — and both appeared on the same screen.
        // One definition now answers for both; this pins WHICH definition won and why: a library's
        // contract is not what its own tests declare.
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("Lib", @"C:/repo/src/Lib/Lib.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("Lib.Tests", @"C:/repo/tests/Lib.Tests/Lib.Tests.csproj", "C#", ["net10.0"], [],
                    [new PackageReferenceInfo("xunit", "2.9.0")]),
            ],
        };
        AddType(model, "Lib.PublicApi", @"C:/repo/src/Lib/PublicApi.cs");
        AddType(model, "Lib.Tests.PublicApiTests", @"C:/repo/tests/Lib.Tests/PublicApiTests.cs");
        AddType(model, "Lib.Samples.DemoApp", @"C:/repo/samples/Demo/DemoApp.cs");

        var surface = LibrarySurfaceBuilder.PublicSurfaceTypes(model);

        Assert.Equal(["Lib.PublicApi"], surface.Select(t => t.Id));
    }

    private static void AddType(DiscoveryModel model, string id, string filePath)
        => model.Types[id] = new TypeDiscovery
        {
            Id = id,
            Name = id[(id.LastIndexOf('.') + 1)..],
            Namespace = id[..id.LastIndexOf('.')],
            FilePath = filePath,
            Kind = DevContext.Core.Models.TypeKind.Class,
            Accessibility = Accessibility.Public,
            Layer = ArchitectureLayer.Unknown,
        };
}
