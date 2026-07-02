using System.Collections.Immutable;

using DevContext.Core.Extractors.Specific;
using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class ExternalEventsSource : IInsightSource
{
    public string Id => "wiring.external-events";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var eventFlows = model.Detections.OfType<EventFlowDetection>().ToList();
        if (eventFlows.Count == 0) yield break;

        var consumed = eventFlows
            .Where(e => e.Kind == "Subscribe" || e.Kind == "Handler")
            .Select(e => e.EventType)
            .ToHashSet();

        var produced = eventFlows
            .Where(e => e.Kind == "Publish")
            .Select(e => e.EventType)
            .ToHashSet();

        var external = consumed.Except(produced).ToList();
        if (external.Count == 0) yield break;

        yield return Insight.Create(Id, Category, Severity.Notable,
            $"External event contracts: {external.Count} consumed but never produced internally",
            external.Take(5));
    }
}
