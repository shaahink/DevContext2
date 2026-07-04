using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;

namespace DevContext.Core.Tests;

public sealed class InsightsBuilderTests
{
    private static readonly CodeGraph EmptyGraph = new(
        new Dictionary<NodeId, GraphNode>(),
        new Dictionary<NodeId, ImmutableArray<GraphEdge>>());

    private sealed class FakeSource(string id, params Insight[] insights) : IInsightSource
    {
        public string Id => id;
        public InsightCategory Category => InsightCategory.Shape;
        public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
            => insights;
    }

    [Fact]
    public void DropsInsight_WithBareQuestionMarkTitle()
    {
        var bad = Insight.Create("bad.one", InsightCategory.Wiring, Severity.Notable, "? (16 impls)");
        var builder = new InsightsBuilder([new FakeSource("bad.one", bad)]);
        var model = new DiscoveryModel();

        var result = builder.Compute(model, EmptyGraph, []);

        Assert.Empty(result);
        Assert.Contains(model.Diagnostics, d => d.Message.Contains("bad.one"));
    }

    [Fact]
    public void DropsInsight_WithBareQuestionMarkInEvidence()
    {
        var bad = Insight.Create("bad.two", InsightCategory.Wiring, Severity.Notable,
            "Some finding", ["? (9 impls)", "options (7 impls)"]);
        var builder = new InsightsBuilder([new FakeSource("bad.two", bad)]);

        var result = builder.Compute(new DiscoveryModel(), EmptyGraph, []);

        Assert.Empty(result);
    }

    [Fact]
    public void KeepsInsight_WithGenuineQuestionMarkPunctuation()
    {
        // A trailing "?" glued to a word (a real question) must not trip the bare-token check.
        var good = Insight.Create("good.one", InsightCategory.Shape, Severity.Info, "Is this ready?");
        var builder = new InsightsBuilder([new FakeSource("good.one", good)]);

        var result = builder.Compute(new DiscoveryModel(), EmptyGraph, []);

        Assert.Single(result);
    }
}
