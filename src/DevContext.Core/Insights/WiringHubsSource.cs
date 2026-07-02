using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class WiringHubsSource : IInsightSource
{
    public string Id => "wiring.hubs";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (graph.NodeCount < 10) yield break;

        var hubs = graph.Nodes
            .Where(n => n.Kind == NodeKind.Type && !n.Tags.Contains("framework"))
            .Select(n => (Node: n, Degree: graph.OutEdges(n.Id).Length + graph.InEdges(n.Id).Length))
            .OrderByDescending(x => x.Degree)
            .Take(5)
            .Where(x => x.Degree >= 5)
            .Select(x => $"{x.Node.Title} ({x.Degree})")
            .ToList();

        if (hubs.Count == 0) yield break;

        yield return Insight.Create(Id, Category, Severity.Info,
            $"Wiring hubs: {string.Join(" · ", hubs)}", hubs);
    }
}
