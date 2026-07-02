using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class EntryMixSource : IInsightSource
{
    public string Id => "shape.entry-mix";
    public InsightCategory Category => InsightCategory.Shape;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (entries.IsDefaultOrEmpty) yield break;
        var groups = entries
            .GroupBy(e => e.Kind)
            .OrderByDescending(g => g.Count())
            .Take(4)
            .Select(g => $"{g.Count()} {Label(g.Key)}")
            .ToList();
        if (groups.Count < 2) yield break;

        yield return Insight.Create(Id, Category, Severity.Info,
            $"Entry surface: {string.Join(" · ", groups)}", groups);
    }

    private static string Label(EntryPointKind k) => k switch
    {
        EntryPointKind.HttpEndpoint => "HTTP",
        EntryPointKind.MessageConsumer => "Bus",
        EntryPointKind.ScheduledJob => "scheduled",
        EntryPointKind.HostedService => "hosted",
        EntryPointKind.UiEntry => "UI",
        EntryPointKind.CliCommand => "CLI",
        _ => k.ToString(),
    };
}
