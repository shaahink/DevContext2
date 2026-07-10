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
        var routeEntries = httpEntries
            .Where(e => e.Route is not null)
            .Take(8)
            .ToList();
        var routes = routeEntries
            .Select(e => $"{e.HttpMethod} {e.Route}")
            .ToImmutableArray();
        var routeActions = routeEntries
            .Select(e => TypedAction.Focus(e.Node.ToString()))
            .Cast<TypedAction?>()
            .ToImmutableArray();

        yield return Insight.Create("gateway.routing-surface", InsightCategory.Shape, Severity.Info,
            $"Routing surface: {routes.Length} routes exposed",
            routes,
            confidence: 0.8,
            confidenceBasis: "Gateway routes detected from HTTP endpoint inventory — reliable",
            whyItMatters: "The gateway's routing table is its contract with clients — every route represents a downstream dependency.",
            action: routeEntries.FirstOrDefault() is { } first ? TypedAction.Focus(first.Node.ToString()) : null,
            evidenceActions: routeActions);

        // ── Downstream wiring ──
        var serviceLinks = graph.AllEdges
            .Where(e => e.Kind == EdgeKind.ServiceLink)
            .ToList();

        var downstreamList = new List<(string Label, string? NodeId)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sl in serviceLinks)
        {
            var toNode = graph.Node(sl.To);
            if (toNode is null) continue;
            var tag = sl.Tags.IsDefaultOrEmpty ? "" : sl.Tags[0];
            var label = $"{toNode.Title} ← {tag}";
            if (seen.Add(label))
                downstreamList.Add((label, toNode.Id.ToString()));
        }
        downstreamList = downstreamList.Take(10).ToList();

        if (downstreamList.Count > 0)
        {
            var transferSeams = serviceLinks
                .Select(e => e.Tags.IsDefaultOrEmpty ? "unknown" : e.Tags[0])
                .Distinct()
                .ToList();
            var seamsDesc = transferSeams.Count > 0 ? string.Join(", ", transferSeams) : "unknown";
            var dwEvidence = downstreamList.Select(x => x.Label).ToImmutableArray();
            var dwActions = downstreamList.Select(x => x.NodeId is { } nid ? TypedAction.Node(nid) : null)
                .Cast<TypedAction?>()
                .ToImmutableArray();
            yield return Insight.Create("gateway.downstream-wiring", InsightCategory.Wiring, Severity.Notable,
                $"Downstream wiring: {downstreamList.Count} target services via {seamsDesc}",
                dwEvidence,
                confidence: 0.75,
                confidenceBasis: "Downstream services detected from verified ServiceLink edges (bus/gRPC/HTTP/YARP)",
                whyItMatters: "Every downstream service is a failure domain — understanding the gateway's reach is essential for reliability.",
                action: TypedAction.Focus(serviceLinks[0].From.ToString()),
                evidenceActions: dwActions);
        }
    }
}
