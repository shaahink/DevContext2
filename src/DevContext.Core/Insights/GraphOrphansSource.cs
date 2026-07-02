using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class GraphOrphansSource : IInsightSource
{
    public string Id => "graph.orphans";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (graph.NodeCount < 10) yield break;

        var entryIds = new HashSet<NodeId>(
            entries.Where(e => graph.Contains(e.Node)).Select(e => e.Node));
        var diTypes = model.Detections.OfType<DiRegistrationDetection>()
            .Select(d => d.ServiceType?.Split(',').FirstOrDefault()?.Trim())
            .Where(t => t is not null)
            .ToHashSet();

        var orphans = graph.Nodes
            .Where(n => n.Kind == NodeKind.Type
                && !n.Tags.Contains("framework")
                && !n.Tags.Contains("internal")
                && graph.InEdges(n.Id).Length == 0
                && !entryIds.Contains(n.Id)
                && !diTypes.Contains(n.Id.Key))
            .Take(5)
            .Select(n => n.Title)
            .ToList();

        if (orphans.Count == 0) yield break;

        var severity = orphans.Count >= 3 ? Severity.Notable : Severity.Info;
        yield return Insight.Create(Id, Category, severity,
            $"Possible dead code: {orphans.Count} public types with zero inbound references",
            orphans);
    }
}
