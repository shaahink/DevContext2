using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>L4.2 — Web archetype composition: auth surface, data map, middleware pipeline.</summary>
public sealed class WebArchetypeSource : IInsightSource
{
    public string Id => "archetype.web";
    public InsightCategory Category => InsightCategory.Shape;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var httpEntries = entries.Where(e =>
            e.Kind == EntryPointKind.HttpEndpoint && !IsRazorPage(e, graph)).ToList();
        if (httpEntries.Count == 0) yield break;

        // ── Auth surface ──
        // Only when the repo HAS auth annotations to describe — with zero annotations this
        // card and auth.anonymous stated the same fact twice (T6.3: shamshir rendered
        // "130/130 endpoints anonymous" AND "Auth surface: 0 protected, 130 unannotated").
        var authEntries = httpEntries.Count(e => !e.AuthAttributes.IsDefaultOrEmpty);
        if (authEntries > 0)
        {
            var protectedEntries = httpEntries.Where(e =>
                !e.AuthAttributes.IsDefaultOrEmpty && e.AuthAttributes.Any(a => a.Contains("Authorize"))).ToList();
            var publicEntries = httpEntries.Where(e =>
                !e.AuthAttributes.IsDefaultOrEmpty && e.AuthAttributes.Any(a => a.Contains("AllowAnonymous"))).ToList();
            var unannotated = httpEntries.Where(e =>
                e.AuthAttributes.IsDefaultOrEmpty || e.AuthAttributes.Length == 0).ToList();

            var evidence = new List<string>();
            var actions = new List<TypedAction?>();

            if (protectedEntries.Count > 0)
            {
                evidence.Add($"{protectedEntries.Count} protected");
                actions.Add(null);
                foreach (var pe in protectedEntries.Take(2))
                {
                    evidence.Add($"{pe.HttpMethod ?? "GET"} {pe.Route}");
                    actions.Add(TypedAction.Focus(pe.Node.ToString()));
                }
            }
            if (publicEntries.Count > 0)
            {
                evidence.Add($"{publicEntries.Count} explicitly public");
                actions.Add(null);
            }
            if (unannotated.Count > 0)
            {
                evidence.Add($"{unannotated.Count} no auth annotation");
                actions.Add(null);
            }

            yield return Insight.Create("web.auth-surface", InsightCategory.Risk,
                unannotated.Count > httpEntries.Count * 0.5 ? Severity.Warning : Severity.Notable,
                $"Auth surface: {protectedEntries.Count} protected, {unannotated.Count} unannotated of {httpEntries.Count} API endpoints",
                evidence,
                confidence: httpEntries.Count > 0 ? (double)Math.Max(protectedEntries.Count, publicEntries.Count) / httpEntries.Count : 0.5,
                confidenceBasis: $"{protectedEntries.Count + publicEntries.Count}/{httpEntries.Count} API endpoints have auth annotations",
                whyItMatters: "Every unauthenticated write endpoint may be a security gap — verify intent.",
                action: protectedEntries.FirstOrDefault() is { } first ? TypedAction.Focus(first.Node.ToString()) : null,
                evidenceActions: actions.ToImmutableArray());
        }

        // ── Data map ──
        var entities = graph.Nodes.Where(n =>
            n.Tags.Contains(RoleTags.Entity) || n.Tags.Contains(RoleTags.Aggregate)).ToList();
        if (entities.Count > 0)
        {
            var byProject = entities
                .Where(n => n.FilePath is not null)
                .GroupBy(n => n.Project ?? Path.GetFileNameWithoutExtension(n.FilePath!))
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => $"{g.Key} ({g.Count()} entities)")
                .ToList();

            yield return Insight.Create("web.data-map", InsightCategory.Data, Severity.Info,
                $"Data map: {entities.Count} entities across {byProject.Count} scopes",
                byProject,
                confidence: 0.6,
                confidenceBasis: "Entity detection is name/heuristic-based — may miss some code-first models",
                whyItMatters: "Entities are the core domain model — knowing where they cluster helps orient a new reader.");
        }

        // ── Middleware pipeline ──
        var pipelineNodes = graph.Nodes.Where(n => n.Tags.Contains(RoleTags.Pipeline)).ToList();
        if (pipelineNodes.Count > 0)
        {
            var names = pipelineNodes.Select(n => n.Title).Take(5).ToList();
            yield return Insight.Create("web.middleware-pipeline", InsightCategory.Shape, Severity.Info,
                $"Middleware pipeline: {pipelineNodes.Count} behaviours",
                names,
                confidence: 0.8,
                confidenceBasis: "Pipeline/handler detection is structural — reliable",
                whyItMatters: "Pipeline behaviours apply cross-cutting concerns to every request — they affect all endpoints.");
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
