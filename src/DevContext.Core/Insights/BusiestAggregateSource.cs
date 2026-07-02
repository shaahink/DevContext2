using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class BusiestAggregateSource : IInsightSource
{
    public string Id => "data.busiest-aggregate";
    public InsightCategory Category => InsightCategory.Data;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var aggregates = model.Detections.OfType<EfEntityDetection>()
            .Where(d => d.IsAggregate)
            .Select(d => d.EntityType)
            .ToHashSet();

        if (aggregates.Count == 0) yield break;

        var aggregateRaises = graph.Nodes
            .Where(n => n.Kind == NodeKind.Type && aggregates.Contains(n.Id.Key))
            .Select(n => (
                Label: n.Title,
                RaiseCount: graph.OutEdges(n.Id).Count(e => e.Kind == EdgeKind.Raises)
            ))
            .OrderByDescending(x => x.RaiseCount)
            .Take(3)
            .Where(x => x.RaiseCount > 0)
            .Select(x => $"{x.Label} ({x.RaiseCount} events)")
            .ToList();

        if (aggregateRaises.Count == 0) yield break;

        yield return Insight.Create(Id, Category, Severity.Info,
            $"Busiest aggregates: {string.Join(" · ", aggregateRaises)}");
    }
}
