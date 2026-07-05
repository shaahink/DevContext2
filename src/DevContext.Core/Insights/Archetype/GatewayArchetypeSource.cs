using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>L4.2 — Gateway archetype composition: combines web + messaging lenses with upstream/downstream
/// routing pairs. A gateway (Ocelot/YARP) is a reverse proxy — its job is to sit between clients and
/// downstream services, routing HTTP requests and sometimes bridging message transports.</summary>
public sealed class GatewayArchetypeSource : IInsightSource
{
    public string Id => "archetype.gateway";
    public InsightCategory Category => InsightCategory.Shape;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var httpEntries = entries.Where(e => e.Kind == EntryPointKind.HttpEndpoint).ToList();
        if (httpEntries.Count == 0) yield break;

        // ── Routing surface ──
        var routes = httpEntries
            .Select(e => $"{e.HttpMethod} {e.Route}")
            .Distinct()
            .Take(8)
            .ToList();

        yield return Insight.Create("gateway.routing-surface", InsightCategory.Shape, Severity.Info,
            $"Routing surface: {routes.Count} routes exposed",
            routes,
            confidence: 0.8,
            confidenceBasis: "Gateway routes detected from HTTP endpoint inventory — reliable",
            whyItMatters: "The gateway's routing table is its contract with clients — every route represents a downstream dependency.",
            action: InsightAction.Trace,
            actionTarget: httpEntries.FirstOrDefault()?.Node.ToString());

        // ── Downstream wiring ──
        var downstreamCalls = graph.Nodes
            .SelectMany(n => graph.OutEdges(n.Id))
            .Where(e => e.Kind is EdgeKind.Calls or EdgeKind.Sends)
            .Select(e => graph.Node(e.To))
            .Where(n => n is not null)
            .Select(n => n!.Title)
            .Distinct()
            .Take(10)
            .ToList();

        if (downstreamCalls.Count > 0)
        {
            yield return Insight.Create("gateway.downstream-wiring", InsightCategory.Wiring, Severity.Notable,
                $"Downstream wiring: {downstreamCalls.Count} target services detected",
                downstreamCalls,
                confidence: 0.5,
                confidenceBasis: "Downstream detection is call-edge analysis — may miss external config-based routing",
                whyItMatters: "Every downstream service is a failure domain — understanding the gateway's reach is essential for reliability.",
                action: InsightAction.Trace,
                actionTarget: downstreamCalls.FirstOrDefault());
        }
    }
}
