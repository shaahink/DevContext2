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
        var httpEntries = entries.Where(e => e.Kind == EntryPointKind.HttpEndpoint).ToList();
        if (httpEntries.Count == 0) yield break;

        // ── Auth surface ──
        var authEntries = httpEntries.Count(e => !e.AuthAttributes.IsDefaultOrEmpty);
        if (authEntries > 0 || httpEntries.Count > 0)
        {
            var protectedCount = httpEntries.Count(e =>
                !e.AuthAttributes.IsDefaultOrEmpty && e.AuthAttributes.Any(a => a.Contains("Authorize")));
            var publicCount = httpEntries.Count(e =>
                !e.AuthAttributes.IsDefaultOrEmpty && e.AuthAttributes.Any(a => a.Contains("AllowAnonymous")));
            var unprotected = httpEntries.Count - (protectedCount + publicCount);

            var evidence = new List<string>();
            if (protectedCount > 0) evidence.Add($"{protectedCount} protected");
            if (publicCount > 0) evidence.Add($"{publicCount} explicitly public");
            if (unprotected > 0) evidence.Add($"{unprotected} no auth annotation");

            yield return Insight.Create("web.auth-surface", InsightCategory.Risk,
                unprotected > httpEntries.Count * 0.5 ? Severity.Warning : Severity.Notable,
                $"Auth surface: {protectedCount} protected, {unprotected} unannotated of {httpEntries.Count} endpoints",
                evidence,
                confidence: httpEntries.Count > 0 ? (double)Math.Max(protectedCount, publicCount) / httpEntries.Count : 0.5,
                confidenceBasis: $"{protectedCount + publicCount}/{httpEntries.Count} endpoints have auth annotations",
                whyItMatters: "Every unauthenticated write endpoint may be a security gap — verify intent.",
                action: InsightAction.Trace,
                actionTarget: httpEntries.FirstOrDefault(e =>
                    !e.AuthAttributes.IsDefaultOrEmpty && e.AuthAttributes.Any(a => a.Contains("Authorize")))
                    ?.Node.ToString());
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
}
