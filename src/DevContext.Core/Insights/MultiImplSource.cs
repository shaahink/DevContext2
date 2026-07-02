using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class MultiImplSource : IInsightSource
{
    public string Id => "wiring.multi-impl";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var diRegs = model.Detections.OfType<DiRegistrationDetection>().ToList();
        if (diRegs.Count < 10) yield break;

        var multiImpl = diRegs
            .Where(d => !string.IsNullOrWhiteSpace(d.ServiceType))
            .GroupBy(d => d.ImplementationType ?? d.ServiceType!)
            .Where(g => g.Count() >= 2)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => $"{TypeName(g.Key!)} ({g.Count()} impls)")
            .ToList();

        if (multiImpl.Count == 0) yield break;

        yield return Insight.Create(Id, Category, Severity.Notable,
            $"Multi-implementation interfaces: {string.Join(" · ", multiImpl)}", multiImpl);
    }

    private static string TypeName(string fqn)
    {
        var lastDot = fqn.LastIndexOf('.');
        return lastDot >= 0 ? fqn[(lastDot + 1)..] : fqn;
    }
}
