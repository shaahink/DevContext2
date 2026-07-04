using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>Runs all registered insight sources after GraphAssembly, ranks by severity,
/// caps per category and globally, and stores the ranked list on the snapshot.</summary>
public sealed class InsightsBuilder
{
    private readonly ImmutableArray<IInsightSource> _sources;

    private const int MaxPerCategory = 3;
    private const int MaxTotal = 10;

    public InsightsBuilder(IEnumerable<IInsightSource> sources)
    {
        _sources = sources.OrderBy(s => s.Id).ToImmutableArray();
    }

    public ImmutableArray<Insight> Compute(DiscoveryModel model, CodeGraph graph,
        ImmutableArray<EntryPoint> entries)
    {
        var all = new List<Insight>();
        foreach (var source in _sources)
        {
            try
            {
                foreach (var insight in source.Compute(model, graph, entries))
                {
                    // Global honesty invariant (E2): no insight may surface an unresolved-name
                    // placeholder ("? (16 impls)") — the source should have filtered it out itself, but
                    // this is the last line of defense before it reaches a human.
                    if (HasUnresolvedPlaceholder(insight))
                    {
                        model.AddDiagnostic(DiagnosticLevel.Warning, nameof(InsightsBuilder),
                            $"Dropped insight '{insight.Id}' — unresolved '?' placeholder in output");
                        continue;
                    }
                    all.Add(insight);
                }
            }
            catch
            {
                // An insight source must never crash the pipeline — skip silently
            }
        }

        // Rank: Severity desc → Category round-robin → source Id
        var ranked = all
            .OrderByDescending(i => i.Severity)
            .ThenBy(i => ((int)i.Category) % MaxPerCategory)
            .ThenBy(i => i.Id)
            .ToList();

        // Cap per category
        var byCategory = new Dictionary<InsightCategory, int>();
        var capped = new List<Insight>();
        foreach (var i in ranked)
        {
            if (!byCategory.TryGetValue(i.Category, out var count))
                count = 0;
            if (count >= MaxPerCategory) continue;
            byCategory[i.Category] = count + 1;
            capped.Add(i);
            if (capped.Count >= MaxTotal) break;
        }

        return capped.ToImmutableArray();
    }

    /// <summary>
    /// True when the title or any evidence line carries a bare "?" token — the shape an unresolved
    /// name leaves behind ("? (16 impls)", "options (7 impls)" is fine, "? (9 impls)" is not). Trims
    /// surrounding punctuation so "? (16 impls)" matches but a genuine question ("...ready?") doesn't,
    /// since its "?" is glued to a word.
    /// </summary>
    private static bool HasUnresolvedPlaceholder(Insight insight)
    {
        static bool HasBareQuestionMark(string text) => text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(tok => tok.Trim('(', ')', ',', ':', ';', '.') == "?");

        if (HasBareQuestionMark(insight.Title)) return true;
        return insight.Evidence.Any(HasBareQuestionMark);
    }
}
