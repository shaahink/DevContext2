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

        // Surface-neutral phrasing (T6.3): "use --focus" is a CLI flag — this line also
        // renders inside the desktop Insights page and MCP, where no such flag exists.
        yield return Insight.Create(Id, Category, Severity.Info,
            $"Entry targets resolved {withTarget}/{total} ({ratio}%) — trace any entry for its full path");
    }
}
