using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>L4.2 — Desktop archetype composition: module map, ViewModel-View wiring.</summary>
public sealed class DesktopArchetypeSource : IInsightSource
{
    public string Id => "archetype.desktop";
    public InsightCategory Category => InsightCategory.Shape;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var uiEntries = entries.Where(e =>
            e.Kind == EntryPointKind.UiEntry || e.Kind == EntryPointKind.GrainMethod
            || e.Kind == EntryPointKind.FunctionEntry).ToList();

        // Check for desktop UI patterns via type naming
        var hasDesktopPatterns = model.Types.Values.Any(t =>
            t.ImplementedInterfaces.Any(i => i.Contains("ICommand", StringComparison.Ordinal))
            || t.Name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase)
            || t.Name.EndsWith("Window", StringComparison.OrdinalIgnoreCase));

        if (uiEntries.Count == 0 && !hasDesktopPatterns) yield break;

        // ── Module map ──
        var byGroup = entries
            .Where(e => e.GroupPath is not null)
            .GroupBy(e => e.GroupPath!)
            .OrderByDescending(g => g.Count())
            .Take(8)
            .Select(g => $"{g.Key} ({g.Count()} entries)")
            .ToList();

        if (byGroup.Count > 0)
        {
            yield return Insight.Create("desktop.module-map", InsightCategory.Shape, Severity.Info,
                $"Module map: {byGroup.Count} feature areas",
                byGroup,
                confidence: 0.7,
                confidenceBasis: "GroupPath is namespace-derived — reliable within project conventions",
                whyItMatters: "Desktop apps are organised in feature areas — this map shows the high-level structure.",
                action: InsightAction.Trace,
                actionTarget: entries.FirstOrDefault(e => e.GroupPath is not null)?.Node.ToString());
        }

        // ── ViewModel-View wiring ──
        var vmTypes = model.Types.Values.Where(t =>
            t.Name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase)
            || t.Name.EndsWith("VM", StringComparison.Ordinal)).ToList();
        var viewTypes = model.Types.Values.Where(t =>
            t.Name.EndsWith("View", StringComparison.OrdinalIgnoreCase)
            || t.Name.EndsWith("Page", StringComparison.OrdinalIgnoreCase)
            || t.Name.EndsWith("Window", StringComparison.OrdinalIgnoreCase)).ToList();

        if (vmTypes.Count > 0 || viewTypes.Count > 0)
        {
            var evidence = new List<string>();
            if (vmTypes.Count > 0) evidence.Add($"{vmTypes.Count} ViewModels");
            if (viewTypes.Count > 0) evidence.Add($"{viewTypes.Count} Views");

            var total = vmTypes.Count + viewTypes.Count;
            var boundCount = model.CallEdges.Count(ce =>
                vmTypes.Any(vm => ce.CallerType.EndsWith(vm.Name, StringComparison.Ordinal))
                && viewTypes.Any(v => ce.CalleeType.EndsWith(v.Name, StringComparison.Ordinal)));

            yield return Insight.Create("desktop.vm-view-wiring", InsightCategory.Wiring,
                boundCount > 0 ? Severity.Info : Severity.Notable,
                $"ViewModel-View: {vmTypes.Count} VMs + {viewTypes.Count} Views ({boundCount} call edges)",
                evidence,
                confidence: 0.5,
                confidenceBasis: "Naming-convention detection — some VMs/Views may not follow naming patterns",
                whyItMatters: "VM-View wiring is the desktop app's connective tissue — understanding it helps navigate the UI layer.");
        }

        // ── Command inventory ──
        var commands = model.Types.Values.Where(t =>
            t.ImplementedInterfaces.Any(i => i.Contains("ICommand", StringComparison.Ordinal))
            || t.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase)).ToList();

        if (commands.Count > 0)
        {
            var names = commands.Take(5).Select(c => c.Name).ToList();
            yield return Insight.Create("desktop.command-inventory", InsightCategory.Wiring, Severity.Info,
                $"Command inventory: {commands.Count} ICommand implementations",
                names,
                confidence: 0.8,
                confidenceBasis: "ICommand interface detection + naming convention — reliable",
                whyItMatters: "Commands are the user-facing actions in desktop apps — they define what the user can do.");
        }
    }
}
