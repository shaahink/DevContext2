using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>M2.2 — Cross-service single-point-of-failure detection. Identifies services whose
/// failure would break multiple downstream consumers, using ServiceLink edge topology.</summary>
public sealed class SpofSource : IInsightSource
{
    public string Id => "wiring.spof";
    public InsightCategory Category => InsightCategory.Risk;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var serviceLinks = graph.AllEdges.Where(e => e.Kind == EdgeKind.ServiceLink).ToList();
        if (serviceLinks.Count < 2) yield break;

        var fanIn = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var fanOut = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var nameToNodeId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sl in serviceLinks)
        {
            var fromNode = graph.Node(sl.From);
            var toNode = graph.Node(sl.To);
            if (fromNode is null || toNode is null) continue;

            var fromName = fromNode.Title;
            var toName = toNode.Title;

            fanOut[fromName] = fanOut.GetValueOrDefault(fromName) + 1;
            fanIn[toName] = fanIn.GetValueOrDefault(toName) + 1;
            if (!nameToNodeId.ContainsKey(toName))
                nameToNodeId[toName] = toNode.Id.ToString();
        }

        var spofs = fanIn
            .Where(kv => kv.Value >= 2)
            .OrderByDescending(kv => kv.Value)
            .Take(5)
            .ToList();

        if (spofs.Count == 0) yield break;

        var evidence = spofs
            .Select(s => $"{s.Key}: {s.Value} downstream {fanOut.GetValueOrDefault(s.Key)} upstream")
            .ToImmutableArray();
        var evidenceActions = spofs
            .Select(s => nameToNodeId.TryGetValue(s.Key, out var nid) ? TypedAction.Node(nid) : null)
            .Cast<TypedAction?>()
            .ToImmutableArray();

        yield return Insight.Create(Id, Category, Severity.Notable,
            $"Potential SPOFs: {spofs.Count} services are sole providers for ≥2 consumers",
            evidence,
            confidence: 0.7,
            confidenceBasis: "SPOF analysis from verified ServiceLink edges — reflects runtime dependency topology",
            whyItMatters: "A single point of failure is a service whose failure cascades — hardening or monitoring should be prioritised.",
            action: TypedAction.Node(spofs[0].Key),
            evidenceActions: evidenceActions);
    }
}
