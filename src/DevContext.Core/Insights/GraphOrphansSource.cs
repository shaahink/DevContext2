using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class GraphOrphansSource : IInsightSource
{
    public string Id => "graph.orphans";
    public InsightCategory Category => InsightCategory.Wiring;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (graph.NodeCount < 10) yield break;

        var handlesCount = graph.AllEdges.Count(e => e.Kind == EdgeKind.Handles);
        var sendsCount = graph.AllEdges.Count(e => e.Kind == EdgeKind.Sends);
        if (handlesCount < 5 && sendsCount < 10) yield break;

        var entryIds = new HashSet<NodeId>(
            entries.Where(e => graph.Contains(e.Node)).Select(e => e.Node));
        var diTypes = model.Detections.OfType<DiRegistrationDetection>()
            .Select(d => d.ServiceType?.Split(',').FirstOrDefault()?.Trim())
            .Where(t => t is not null)
            .ToHashSet();

        var conventionDiTypes = FindConventionDiTypes(model);

        var orphans = graph.Nodes
            .Where(n => n.Kind == NodeKind.Type
                && !n.Tags.Contains("framework")
                && !n.Tags.Contains("internal")
                && graph.InEdges(n.Id).Length == 0
                && !entryIds.Contains(n.Id)
                && !diTypes.Contains(n.Id.Key)
                && !conventionDiTypes.Contains(n.Id.Key)
                // DI/startup extension classes are invoked via extension-method syntax the call
                // graph doesn't attribute to the class (T6.3 rider: "Extensions" classes were
                // classic dead-code false positives on eShop and MediatR).
                && !n.Title.EndsWith("Extensions", StringComparison.Ordinal))
            .Take(5)
            .Select(n => n.Title)
            .ToList();

        if (orphans.Count == 0) yield break;

        var severity = orphans.Count >= 3 ? Severity.Notable : Severity.Info;
        yield return Insight.Create(Id, Category, severity,
            $"Possible dead code: {orphans.Count} public types with zero inbound references",
            orphans,
            confidence: 0.4,
            confidenceBasis: "Dead-code detection is conservative — convention-scanned types (MediatR handlers, validators, etc.) are excluded");
    }

    private static HashSet<string> FindConventionDiTypes(DiscoveryModel model)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in model.Types.Values)
        {
            var ifaceNames = type.ImplementedInterfaces;
            if (ifaceNames.IsDefaultOrEmpty) continue;
            foreach (var iface in ifaceNames)
            {
                if (IsDiConventionInterface(iface))
                {
                    result.Add(type.Id);
                    break;
                }
            }
        }

        foreach (var type in model.Types.Values)
        {
            var baseTypes = type.BaseTypes;
            if (baseTypes.IsDefaultOrEmpty) continue;
            foreach (var bt in baseTypes)
            {
                if (bt.StartsWith("AbstractValidator", StringComparison.OrdinalIgnoreCase)
                    || bt.StartsWith("DbContext", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(type.Id);
                    break;
                }
            }
        }

        return result;
    }

    private static bool IsDiConventionInterface(string iface)
    {
        return iface.StartsWith("IRequestHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("INotificationHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IValidator<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IConsumer<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IEventHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("ICommandHandler<", StringComparison.OrdinalIgnoreCase)
            || iface.StartsWith("IQueryHandler<", StringComparison.OrdinalIgnoreCase)
            // EF applies these by assembly scan — zero inbound references is their normal state
            // (T6.3 rider: OrderItemEntityTypeConfiguration headlined eShop's dead-code card).
            || iface.StartsWith("IEntityTypeConfiguration<", StringComparison.OrdinalIgnoreCase);
    }
}
