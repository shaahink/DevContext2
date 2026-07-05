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

        // Merge auth info from EndpointDetections, keyed by (route, verb) — a route with mixed-auth
        // verbs (GET public, POST protected) previously collapsed to whichever verb's detection was
        // enumerated first, silently misreporting the other (E1 verb-collision bug).
        var detsByRouteAndVerb = model.Detections.OfType<EndpointDetection>()
            .GroupBy(d => (d.RouteTemplate, d.HttpMethod))
            .ToDictionary(g => g.Key, g => g.First());

        // An app-wide fallback policy requiring authentication protects any endpoint that carries no
        // other auth metadata — such an endpoint is NOT anonymous, just not individually annotated (E1).
        var hasGlobalFallback = model.Detections.OfType<GlobalAuthPolicyDetection>()
            .Any(d => d.HasFallbackPolicy);

        var anon = new List<string>();
        var unverifiable = new List<string>();
        foreach (var e in httpEntries)
        {
            if (e.Route == null) continue;
            var method = e.HttpMethod ?? "GET";
            if (!detsByRouteAndVerb.TryGetValue((e.Route, method), out var det)) continue;
            if (!det.AuthAttributes.IsDefaultOrEmpty && det.AuthAttributes.Length > 0)
            {
                if (det.AuthAttributes.Contains("[AllowAnonymous]"))
                    anon.Add($"{method} {e.Route}");
                continue;
            }

            // No auth metadata at all: genuinely anonymous unless an app-wide default covers it.
            if (hasGlobalFallback)
                unverifiable.Add($"{method} {e.Route}");
            else
                anon.Add($"{method} {e.Route}");
        }

        if (anon.Count == 0 && unverifiable.Count == 0) yield break;

        if (anon.Count > 0)
        {
            var postAnon = anon.Count(a => a.StartsWith("POST") || a.StartsWith("PUT") || a.StartsWith("DELETE"));
            var sev = postAnon > 0 ? Severity.Warning : Severity.Notable;
            var suffix = postAnon > 0 ? $", incl. {postAnon} POST/PUT/DELETE" : "";
            if (unverifiable.Count > 0)
                suffix += $"; {unverifiable.Count} more not individually verifiable (app-wide default policy present)";
            var title = $"{anon.Count}/{httpEntries.Count} endpoints anonymous{suffix}";

            var authCoverage = httpEntries.Count > 0
                ? (double)(httpEntries.Count - anon.Count) / httpEntries.Count
                : 0;
            yield return Insight.Create(Id, Category, sev, title, anon.Take(5),
                confidence: Math.Round(authCoverage, 2),
                confidenceBasis: $"{httpEntries.Count - anon.Count}/{httpEntries.Count} endpoints have known auth",
                whyItMatters: "Unauthenticated write endpoints are a security risk — verify each is intentionally public.",
                action: InsightAction.Trace,
                actionTarget: httpEntries.FirstOrDefault(e => anon.Contains($"{e.HttpMethod ?? "GET"} {e.Route}"))?.Node.ToString());
        }
        else
        {
            // Never claim "anonymous" when a fallback policy exists and no endpoint overrides it (E1).
            var title = $"Auth present via app-wide default policy — {unverifiable.Count} endpoints not individually verifiable";
            yield return Insight.Create(Id, Category, Severity.Notable, title, unverifiable.Take(5),
                confidence: 0.7,
                confidenceBasis: "App-wide fallback policy detected; per-endpoint annotations may be incomplete",
                whyItMatters: "Global auth policy protects all endpoints by default — individual annotations confirm intent.");
        }
    }
}
