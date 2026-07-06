using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

/// <summary>L4.2 — CLI archetype composition: command tree, parameter inventory.</summary>
public sealed class CliArchetypeSource : IInsightSource
{
    public string Id => "archetype.cli";
    public InsightCategory Category => InsightCategory.Shape;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        var cliEntries = entries.Where(e => e.Kind == EntryPointKind.CliCommand).ToList();
        if (cliEntries.Count == 0) yield break;

        var totalNonCli = entries.Count(e => e.Kind != EntryPointKind.CliCommand);
        if (totalNonCli > cliEntries.Count * 3)
            yield break;

        // ── Command tree ──
        var byDepth = cliEntries
            .GroupBy(e => e.Node)
            .Select(g => g.First())
            .OrderBy(e => e.Title)
            .ToList();

        var topLevel = byDepth.GroupBy(e =>
        {
            var parts = e.Title.Split(' ');
            return parts.Length > 0 ? parts[0] : e.Title;
        })
        .OrderByDescending(g => g.Count())
        .Take(8)
        .Select(g => $"{g.Key} ({g.Count()} commands)")
        .ToList();

        yield return Insight.Create("cli.command-tree", InsightCategory.Shape, Severity.Info,
            $"Command tree: {cliEntries.Count} CLI commands, {topLevel.Count} top-level groups",
            topLevel,
            confidence: 0.8,
            confidenceBasis: "CLI command detection requires framework base types (Spectre/System.CommandLine) — reliable once gated",
            whyItMatters: "The command tree is a CLI app's user interface — it shows what the tool can do.",
            action: TypedAction.Focus(cliEntries.FirstOrDefault()?.Node.ToString()));

        // ── Parameter/option count per command ──
        var avgParams = cliEntries.Count > 0
            ? (double)cliEntries.Count(e => e.Target is not null) / cliEntries.Count
            : 0;

        if (avgParams > 0)
        {
            yield return Insight.Create("cli.parameter-inventory", InsightCategory.Data, Severity.Info,
                $"Parameter inventory: ~{avgParams:F1} params per command (avg)",
                [$"{cliEntries.Count} commands"],
                confidence: 0.5,
                confidenceBasis: "Parameter count is approximate — some commands may have inline resolution only",
                whyItMatters: "Options and parameters define a CLI's flexibility — high param counts suggest rich functionality.");
        }
    }
}
