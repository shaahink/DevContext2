using DevContext.Core.Graph;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Insights;

public sealed class AnonymousEndpointsSource : IInsightSource
{
    public string Id => "auth.anonymous";
    public InsightCategory Category => InsightCategory.Risk;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var httpEntries = entries.Where(e => e.Kind == EntryPointKind.HttpEndpoint).ToList();
        if (httpEntries.Count == 0) yield break;

        // Merge auth info from EndpointDetections
        var detsByRoute = model.Detections.OfType<EndpointDetection>()
            .GroupBy(d => d.RouteTemplate)
            .ToDictionary(g => g.Key, g => g.First());

        var anon = new List<string>();
        foreach (var e in httpEntries)
        {
            if (e.Route == null) continue;
            if (detsByRoute.TryGetValue(e.Route, out var det))
            {
                if (det.AuthAttributes.IsDefaultOrEmpty || det.AuthAttributes.Length == 0)
                {
                    var method = e.HttpMethod ?? "GET";
                    anon.Add($"{method} {e.Route}");
                }
            }
        }

        if (anon.Count == 0) yield break;

        var postAnon = anon.Count(a => a.StartsWith("POST") || a.StartsWith("PUT") || a.StartsWith("DELETE"));
        var sev = postAnon > 0 ? Severity.Warning : Severity.Notable;
        var title = $"{anon.Count}/{httpEntries.Count} endpoints anonymous{(postAnon > 0 ? $", incl. {postAnon} POST/PUT/DELETE" : "")}";

        yield return Insight.Create(Id, Category, sev, title, anon.Take(5));
    }
}
