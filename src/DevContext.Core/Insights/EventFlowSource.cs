using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>M2.2 / T2.6 — Event board: the published→consumed picture, rendered from the single
/// <see cref="CodeGraph.EventWiring"/> projection so the board, the one-pager Event Wiring section, and
/// flow cross-service markers all agree. One evidence row per event: its publisher→consumer services when
/// it crosses a boundary, or its publisher when it is an in-repo-orphan.</summary>
public sealed class EventFlowSource : IInsightSource
{
    public string Id => "wiring.event-flow";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var wiring = graph.EventWiring;
        if (wiring.IsDefaultOrEmpty || wiring.Length == 0) yield break;

        var integration = wiring.Count(w => w.IsIntegration);
        var crossService = wiring.Count(w => w.IsCrossService);
        var orphans = wiring.Count(w => w.IsOrphan);
        var consumed = wiring.Length - orphans;

        // Rank cross-service events first (they define the integration contract), then orphans, then the
        // rest — most-informative rows survive the report cap.
        var ranked = wiring
            .OrderByDescending(w => w.IsCrossService)
            .ThenByDescending(w => w.IsOrphan)
            .ThenBy(w => w.EventName, StringComparer.Ordinal)
            .ToList();

        var evidence = new List<string>();
        var actions = new List<TypedAction?>();
        foreach (var w in ranked.Take(12))
        {
            evidence.Add(Describe(w));
            actions.Add(TypedAction.Node(w.EventNode.ToString()));
        }

        var severity = crossService > 0 || orphans > wiring.Length / 2 ? Severity.Notable : Severity.Info;

        yield return Insight.Create(Id, Category, severity,
            $"Event wiring: {wiring.Length} events ({integration} integration), {crossService} cross-service, {orphans} orphan",
            evidence,
            confidence: 0.7,
            confidenceBasis: "Publisher→event→consumer join over Raises+Consumes seams; events matched by type name within the detected event set",
            whyItMatters: "Cross-service events are the integration contract between services; orphan events may signal dead notifications or a consumer outside the repo.",
            action: ranked.FirstOrDefault() is { } first ? TypedAction.Node(first.EventNode.ToString()) : null,
            evidenceActions: actions.ToImmutableArray());
    }

    private static string Describe(EventWire w)
    {
        if (w.IsCrossService)
        {
            var froms = w.CrossServicePairs.Select(p => p.From).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var tos = w.CrossServicePairs.Select(p => p.To).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return $"{w.EventName}: {string.Join(", ", froms)} → {string.Join(", ", tos)}";
        }
        if (w.IsOrphan)
        {
            var pub = w.Publishers.FirstOrDefault()?.Service ?? w.Publishers.FirstOrDefault()?.Title;
            return pub is { Length: > 0 } ? $"{w.EventName} ← {pub} (no consumer)" : $"{w.EventName} (no consumer)";
        }
        var consumers = w.Consumers.Select(c => c.Service ?? c.Title).Distinct(StringComparer.OrdinalIgnoreCase);
        return $"{w.EventName} → {string.Join(", ", consumers)}";
    }
}
