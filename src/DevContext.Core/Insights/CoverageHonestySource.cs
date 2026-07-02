using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class CoverageHonestySource : IInsightSource
{
    public string Id => "coverage.honesty";
    public InsightCategory Category => InsightCategory.Coverage;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (entries.IsDefaultOrEmpty) yield break;
        var withTarget = entries.Count(e => e.Target != null);
        var total = entries.Length;
        var ratio = total > 0 ? withTarget * 100 / total : 0;

        yield return Insight.Create(Id, Category, Severity.Info,
            $"Entry targets resolved {withTarget}/{total} ({ratio}%) — use --focus for deeper traces");
    }
}
