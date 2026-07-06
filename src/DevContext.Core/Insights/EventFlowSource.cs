using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>M2.2 — Event flow map: published events with/without consumers, cross-service event wiring.
/// Uses existing Raises/Consumes edges and ServiceLink edges to build a per-event flow picture.</summary>
public sealed class EventFlowSource : IInsightSource
{
    public string Id => "wiring.event-flow";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var published = new HashSet<string>(StringComparer.Ordinal);
        var consumed = new HashSet<string>(StringComparer.Ordinal);
        var publisherMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var consumerMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var eventKeyToNodeId = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var e in graph.AllEdges)
        {
            if (e.Kind == EdgeKind.Raises)
            {
                var eventNode = graph.Node(e.To);
                if (eventNode is null) continue;
                published.Add(eventNode.Id.Key);
                eventKeyToNodeId[eventNode.Id.Key] = eventNode.Id.ToString();
                if (!publisherMap.TryGetValue(eventNode.Id.Key, out var pubs))
                    publisherMap[eventNode.Id.Key] = pubs = [];
                var fromNode = graph.Node(e.From);
                if (fromNode is not null) pubs.Add(fromNode.Title);
            }
            else if (e.Kind == EdgeKind.Consumes)
            {
                var consumerNode = graph.Node(e.To);
                if (consumerNode is null) continue;
                consumed.Add(e.From.Key);
                eventKeyToNodeId[e.From.Key] = e.From.ToString();
                if (!consumerMap.TryGetValue(e.From.Key, out var cons))
                    consumerMap[e.From.Key] = cons = [];
                cons.Add(consumerNode.Title);
            }
        }

        var orphanEvents = published.Except(consumed).ToList();
        var crossServiceEvents = graph.AllEdges
            .Where(e => e.Kind == EdgeKind.ServiceLink && e.Tags.Contains(ServiceLinkTags.BusPublishConsume))
            .ToList();

        if (published.Count > 0)
        {
            var evidence = new List<string>();
            var actions = new List<TypedAction?>();

            if (orphanEvents.Count > 0)
            {
                evidence.Add($"{orphanEvents.Count} orphan events (published, no internal consumer)");
                actions.Add(null);
                foreach (var oe in orphanEvents.Take(3))
                {
                    var label = publisherMap.TryGetValue(oe, out var pubs) && pubs.Count > 0
                        ? $"{oe} ← {pubs[0]}"
                        : oe;
                    evidence.Add(label);
                    actions.Add(eventKeyToNodeId.TryGetValue(oe, out var nid) ? TypedAction.Node(nid) : null);
                }
            }
            if (crossServiceEvents.Count > 0)
            {
                evidence.Add($"{crossServiceEvents.Count} cross-service event flows");
                actions.Add(null);
            }
            evidence.Add($"{consumed.Count}/{published.Count} events consumed");
            actions.Add(null);

            var severity = orphanEvents.Count > published.Count / 2 ? Severity.Notable : Severity.Info;

            yield return Insight.Create(Id, Category, severity,
                $"Event flow: {published.Count} published, {consumed.Count} consumed, {orphanEvents.Count} orphan",
                evidence,
                confidence: 0.65,
                confidenceBasis: "Event publisher/consumer mapping derived from Raises+Consumes edges — body-scan based, may miss indirect publish patterns",
                whyItMatters: "Orphan events may signal incomplete wiring or dead notifications — cross-service events define the integration contract.",
                action: orphanEvents.FirstOrDefault() is { } first && eventKeyToNodeId.TryGetValue(first, out var fnid) ? TypedAction.Node(fnid) : null,
                evidenceActions: actions.ToImmutableArray());
        }
    }
}
