using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

/// <summary>Batch E (R2 §2.E item 1) — the one trace contract. These lock the properties that made the
/// spine and the tree tell different stories about the same entry.</summary>
public sealed class TracePolicyTests
{
    [Fact]
    public void Spine_and_tree_rank_seams_with_the_same_table()
    {
        // The defect: the flow spine ranked ServiceLink third (a cross-service hop is the point of a
        // distributed trace) while the trace tree left it in the catch-all bucket beside Calls — the
        // lowest rank there is. Same entry, two stories. One table now answers for both.
        Assert.True(TracePolicy.SeamPriority(EdgeKind.Sends) < TracePolicy.SeamPriority(EdgeKind.Handles));
        Assert.True(TracePolicy.SeamPriority(EdgeKind.Handles) < TracePolicy.SeamPriority(EdgeKind.ServiceLink));
        Assert.True(TracePolicy.SeamPriority(EdgeKind.ServiceLink) < TracePolicy.SeamPriority(EdgeKind.Raises));
        Assert.True(TracePolicy.SeamPriority(EdgeKind.ServiceLink) < TracePolicy.SeamPriority(EdgeKind.Calls));
        Assert.True(TracePolicy.SeamPriority(EdgeKind.ReadsWrites) < TracePolicy.SeamPriority(EdgeKind.Resolves));
        Assert.True(TracePolicy.SeamPriority(EdgeKind.Resolves) < TracePolicy.SeamPriority(EdgeKind.Calls));
    }

    [Fact]
    public void A_service_link_outranks_a_plain_call_in_the_built_tree()
    {
        // The property above, observed through the builder: with both edges available and a fan-out of
        // one, the cross-service hop is the branch that survives.
        var g = new CodeGraphBuilder();
        var entryId = NodeId.ForEntry("POST /checkout");
        var basketId = NodeId.ForService("Basket.API");
        var noiseId = NodeId.ForType("App.Helper");

        g.AddNode(new GraphNode(entryId, "POST /checkout", NodeKind.EntryPoint));
        g.AddNode(new GraphNode(basketId, "Basket.API", NodeKind.Service));
        g.AddNode(new GraphNode(noiseId, "Helper", NodeKind.Type));
        g.AddEdge(new GraphEdge(entryId, noiseId, EdgeKind.Calls));
        g.AddEdge(new GraphEdge(entryId, basketId, EdgeKind.ServiceLink));

        var trace = new TraceBuilder(g.Build()).Build(
            new EntryPoint(EntryPointKind.HttpEndpoint, "POST /checkout", entryId),
            new TraceOptions { MaxDepth = 3, MaxFanOut = 1 });

        var kept = Assert.Single(trace.Root.Children);
        Assert.Equal(basketId, kept.Node.Id);
        Assert.Equal(1, trace.Root.Omitted);
        // ...and the omitted branch is NAMED, not just counted (R2 §2.E item 3).
        Assert.Contains("Helper", trace.Root.OmittedNames);
    }

    [Fact]
    public void Framework_leaves_are_the_same_set_for_every_surface()
    {
        Assert.True(TracePolicy.IsFrameworkLeaf(new GraphNode(NodeId.ForType("x"), "ILogger", NodeKind.Type)));
        Assert.True(TracePolicy.IsFrameworkLeaf(new GraphNode(NodeId.ForType("x"), "System.String", NodeKind.Type)));
        Assert.True(TracePolicy.IsFrameworkLeaf(null));  // an edge to a node that isn't there cannot be walked
        Assert.False(TracePolicy.IsFrameworkLeaf(new GraphNode(NodeId.ForType("x"), "OrderService", NodeKind.Type)));
    }

    [Fact]
    public void Tree_budget_reserves_room_for_the_document_around_it()
    {
        // The shaper only estimates the TREE; the rendered document adds TOUCHES/EMITS, hints and the
        // diagnostics tail. Reserving for them is why a shaped trace stops overshooting the budget it
        // just enforced — and it lives here so every surface reserves the same amount.
        Assert.Equal(0, TracePolicy.TreeBudget(0));                       // 0 = unlimited, shaping off
        Assert.Equal(TracePolicy.MinTreeBudget, TracePolicy.TreeBudget(1500));  // floor, never a negative budget
        Assert.Equal(8000 - TracePolicy.RenderReserveTokens, TracePolicy.TreeBudget(8000));
    }

    [Fact]
    public void Elastic_depth_only_deepens_when_the_walk_was_cut_by_depth_and_the_budget_is_idle()
    {
        // Deepen: hit the depth limit, used a fraction of the budget.
        Assert.Equal(9, TracePolicy.ElasticDepth(6, usedTokens: 500, treeBudget: 8000, hitDepthLimit: true));
        // Do not deepen: the walk ended on its own — there is nothing deeper to find.
        Assert.Equal(6, TracePolicy.ElasticDepth(6, usedTokens: 500, treeBudget: 8000, hitDepthLimit: false));
        // Do not deepen: already spending half the budget; the shaper would only cut it back.
        Assert.Equal(6, TracePolicy.ElasticDepth(6, usedTokens: 5000, treeBudget: 8000, hitDepthLimit: true));
        // Do not deepen: no budget was given, so there is no evidence there is room.
        Assert.Equal(6, TracePolicy.ElasticDepth(6, usedTokens: 10, treeBudget: 0, hitDepthLimit: true));
        // Never past the ceiling: a trace deeper than this is a dump, whatever the budget allows.
        Assert.Equal(TracePolicy.MaxElasticDepth,
            TracePolicy.ElasticDepth(TracePolicy.MaxElasticDepth - 1, 10, 100000, true));
    }
}
