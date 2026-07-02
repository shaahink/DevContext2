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
                all.AddRange(source.Compute(model, graph, entries));
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
}
