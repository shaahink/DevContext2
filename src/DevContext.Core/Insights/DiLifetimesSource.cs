using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class DiLifetimesSource : IInsightSource
{
    public string Id => "di.lifetimes";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var diRegs = model.Detections.OfType<DiRegistrationDetection>().ToList();
        if (diRegs.Count < 5) yield break;

        var lifetimeCounts = diRegs
            .GroupBy(d => d.Lifetime)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Count()} {g.Key}")
            .ToList();

        if (lifetimeCounts.Count == 0) yield break;

        yield return Insight.Create(Id, Category, Severity.Info,
            $"DI: {string.Join(" · ", lifetimeCounts)} ({diRegs.Count} total)");
    }
}
