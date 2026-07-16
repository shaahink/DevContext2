using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>M2.2 — Detects HTTP endpoints that dispatch request types without corresponding
/// FluentValidation validators. Requires the FluentValidation architecture signal.</summary>
public sealed class UnvalidatedEndpointsSource : IInsightSource
{
    public string Id => "risk.unvalidated-endpoints";
    public InsightCategory Category => InsightCategory.Risk;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (!model.Architecture.Has(ArchitectureSignals.Keys.FluentValidation))
            yield break;

        var validators = FindValidators(model);

        // Writes only (T6.3, audit A11): the copy says "every write endpoint needs a
        // validator" — counting GETs inflated eShop to "43/56" with read endpoints.
        var httpEntries = entries.Where(e => e.Kind == EntryPointKind.HttpEndpoint
            && e.HttpMethod is "POST" or "PUT" or "PATCH" or "DELETE").ToList();
        if (httpEntries.Count == 0) yield break;

        var unvalidated = new List<(string Label, EntryPoint Entry)>();
        foreach (var entry in httpEntries)
        {
            if (entry.Target is null) continue;
            var targetTypeName = entry.Target.Split('.').Last();
            if (!validators.Contains(targetTypeName))
            {
                var method = entry.HttpMethod ?? "GET";
                var route = entry.Route ?? entry.Title;
                unvalidated.Add(($"{method} {route} → {targetTypeName}", entry));
            }
        }

        if (unvalidated.Count == 0) yield break;

        var severity = unvalidated.Count >= 5 ? Severity.Warning : Severity.Notable;
        var top = unvalidated.Take(5).ToList();
        var evidence = top.Select(x => x.Label).ToImmutableArray();
        var evidenceActions = top.Select(x => TypedAction.Focus(x.Entry.Node.ToString()))
            .Cast<TypedAction?>().ToImmutableArray();

        yield return Insight.Create(Id, Category, severity,
            $"Missing validation: {unvalidated.Count}/{httpEntries.Count} write endpoints have no FluentValidation validator",
            evidence,
            confidence: 0.65,
            confidenceBasis: "Validator detection from AbstractValidator<T> subclasses; target resolution from graph dispatch edges",
            whyItMatters: "Unvalidated input is the #1 injection surface — every write endpoint needs a validator.",
            action: TypedAction.Focus(unvalidated[0].Entry.Node.ToString()),
            evidenceActions: evidenceActions);
    }

    private static HashSet<string> FindValidators(DiscoveryModel model)
    {
        var validators = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            if (type.BaseTypes.IsDefaultOrEmpty) continue;
            foreach (var bt in type.BaseTypes)
            {
                if (bt.StartsWith("AbstractValidator<", StringComparison.Ordinal))
                {
                    var validatedType = bt.AsSpan()["AbstractValidator<".Length..].TrimEnd('>');
                    validators.Add(validatedType.ToString());
                    break;
                }
            }
        }
        return validators;
    }
}
