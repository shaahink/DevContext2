using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>L4.2 — Messaging archetype composition: produce-consume matrix, external contracts.</summary>
public sealed class MessagingArchetypeSource : IInsightSource
{
    public string Id => "archetype.messaging";
    public InsightCategory Category => InsightCategory.Shape;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var producers = entries.Where(e =>
            graph.OutEdges(e.Node).Any(ed => ed.Kind is EdgeKind.Sends or EdgeKind.Raises)).ToList();
        var consumers = entries.Where(e =>
            e.Kind == EntryPointKind.DomainEventHandler || e.Kind == EntryPointKind.MessageConsumer).ToList();

        if (producers.Count == 0 && consumers.Count == 0) yield break;

        // ── Produce-consume matrix ──
        var producedEvents = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in producers)
        {
            foreach (var edge in graph.OutEdges(p.Node).Where(e => e.Kind is EdgeKind.Sends or EdgeKind.Raises))
            {
                var target = graph.Node(edge.To);
                if (target is not null) producedEvents.Add(target.Title);
            }
        }

        var consumedEvents = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in consumers)
        {
            foreach (var edge in graph.InEdges(c.Node).Where(e => e.Kind is EdgeKind.Consumes))
            {
                var source = graph.Node(edge.From);
                if (source is not null) consumedEvents.Add(source.Title);
            }
        }

        var evidence = new List<string>();
        if (producers.Count > 0) evidence.Add($"{producers.Count} producers → {producedEvents.Count} message types");
        if (consumers.Count > 0) evidence.Add($"{consumers.Count} consumers");

        yield return Insight.Create("msg.produce-consume", InsightCategory.Wiring, Severity.Info,
            $"Message flow: {producers.Count} producers, {consumers.Count} consumers, {producedEvents.Count} message types",
            evidence,
            confidence: 0.6,
            confidenceBasis: "Message detection is body-scan — producer edges may miss DI-resolved bus dispatches",
            whyItMatters: "Understanding message flow is essential in event-driven architectures — it shows how services communicate.",
            action: TypedAction.Focus(producers.FirstOrDefault()?.Node.ToString() ?? consumers.FirstOrDefault()?.Node.ToString()));

        // ── External contracts (consumed but never produced) ──
        var consumedNotProduced = consumedEvents.Except(producedEvents).ToList();
        if (consumedNotProduced.Count > 0)
        {
            yield return Insight.Create("msg.external-contracts", InsightCategory.Coverage, Severity.Notable,
                $"External contracts: {consumedNotProduced.Count} message types consumed but not produced in-repo",
                consumedNotProduced.Take(5),
                confidence: 0.5,
                confidenceBasis: "Repo boundary analysis — events produced by external services won't appear",
                whyItMatters: "Messages consumed but not produced here come from external services — they define your integration surface.");
        }
    }
}
