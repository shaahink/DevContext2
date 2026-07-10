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
        var httpEntries = entries.Where(e =>
            e.Kind == EntryPointKind.HttpEndpoint && !IsRazorPage(e, graph)).ToList();
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

        var anon = new List<(string Label, EntryPoint Entry)>();
        var unverifiable = new List<(string Label, EntryPoint Entry)>();
        foreach (var e in httpEntries)
        {
            if (e.Route == null) continue;
            var method = e.HttpMethod ?? "GET";
            if (!detsByRouteAndVerb.TryGetValue((e.Route, method), out var det)) continue;
            if (!det.AuthAttributes.IsDefaultOrEmpty && det.AuthAttributes.Length > 0)
            {
                if (det.AuthAttributes.Contains("[AllowAnonymous]"))
                    anon.Add(($"{method} {e.Route}", e));
                continue;
            }

            if (hasGlobalFallback)
                unverifiable.Add(($"{method} {e.Route}", e));
            else
                anon.Add(($"{method} {e.Route}", e));
        }

        if (anon.Count == 0 && unverifiable.Count == 0) yield break;

        if (anon.Count > 0)
        {
            var top = anon.Take(5).ToList();
            var postAnon = anon.Count(a => a.Label.StartsWith("POST") || a.Label.StartsWith("PUT") || a.Label.StartsWith("DELETE"));
            var sev = postAnon > 0 ? Severity.Warning : Severity.Notable;
            var suffix = postAnon > 0 ? $", incl. {postAnon} POST/PUT/DELETE" : "";
            if (unverifiable.Count > 0)
                suffix += $"; {unverifiable.Count} more not individually verifiable (app-wide default policy present)";
            var title = $"{anon.Count}/{httpEntries.Count} endpoints anonymous{suffix}";

            var authCoverage = httpEntries.Count > 0
                ? (double)(httpEntries.Count - anon.Count) / httpEntries.Count
                : 0;
            var evidence = top.Select(x => x.Label).ToImmutableArray();
            var evidenceActions = top.Select(x => TypedAction.Focus(x.Entry.Node.ToString()))
                .Cast<TypedAction?>().ToImmutableArray();
            yield return Insight.Create(Id, Category, sev, title, evidence,
                confidence: Math.Round(authCoverage, 2),
                confidenceBasis: $"{httpEntries.Count - anon.Count}/{httpEntries.Count} endpoints have known auth",
                whyItMatters: "Unauthenticated write endpoints are a security risk — verify each is intentionally public.",
                action: anon.FirstOrDefault().Entry is { } first ? TypedAction.Focus(first.Node.ToString()) : null,
                evidenceActions: evidenceActions);
        }
        else
        {
            var top = unverifiable.Take(5).ToList();
            var title = $"Auth present via app-wide default policy — {unverifiable.Count} endpoints not individually verifiable";
            var evidence = top.Select(x => x.Label).ToImmutableArray();
            var evidenceActions = top.Select(x => TypedAction.Focus(x.Entry.Node.ToString()))
                .Cast<TypedAction?>().ToImmutableArray();
            yield return Insight.Create(Id, Category, Severity.Notable, title, evidence,
                confidence: 0.7,
                confidenceBasis: "App-wide fallback policy detected; per-endpoint annotations may be incomplete",
                whyItMatters: "Global auth policy protects all endpoints by default — individual annotations confirm intent.",
                evidenceActions: evidenceActions);
        }
    }

    private static bool IsRazorPage(EntryPoint e, CodeGraph graph)
    {
        var node = graph.Node(e.Node);
        if (node?.FilePath is { } fp)
            return fp.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
        return false;
    }
}
