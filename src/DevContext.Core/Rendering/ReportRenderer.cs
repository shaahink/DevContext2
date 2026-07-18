using System.Text;

using DevContext.Core.Graph;
using DevContext.Core.Insights;
using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Rendering;

/// <summary>Composes the full report document — identity, stats, top flows, top-3 traces,
/// insights, architecture map, and run report — from the existing Map / Trace / insights
/// renderers. No second rendering path; just orchestration.</summary>
public static class ReportRenderer
{
    public static async ValueTask<RenderedContext> RenderAsync(
        AnalysisSnapshot snapshot,
        GraphQuery query,
        CancellationToken ct)
    {
        var sections = new List<NarrativeSection>();
        var rootPath = snapshot.RootPath;
        var entries = snapshot.Entries;
        var insights = snapshot.Insights;
        var report = snapshot.Report;

        // ── Identity ──
        sections.Add(new NarrativeSection("Report.Identity", BuildIdentity(snapshot, query)));

        // ── Stats ──
        sections.Add(new NarrativeSection("Report.Stats", BuildStats(snapshot, query)));

        // ── Top Flows ──
        sections.Add(new NarrativeSection("Report.TopFlows", BuildTopFlows(entries)));

        // ── Top-3 Traces ──
        var ranked = RankEntries(entries);
        var traceNum = 0;
        var tracesSb = new StringBuilder();
        foreach (var entry in ranked.Take(3))
        {
            var trace = query.Trace(entry.Title, depth: 5, maxFanOut: 8);
            if (trace is null) continue;
            traceNum++;
            tracesSb.AppendLine($"### Trace {traceNum}: {entry.Title}");
            tracesSb.AppendLine();
            foreach (var ts in TraceRenderer.RenderSections(trace, TraceDetail.Salient, rootPath))
                tracesSb.Append(ts.Text);
            tracesSb.AppendLine();
            tracesSb.AppendLine("---");
            tracesSb.AppendLine();
        }
        if (traceNum > 0)
            sections.Add(new NarrativeSection("Report.Traces", tracesSb.ToString()));

        // ── Insights ──
        var insightsText = BuildInsights(insights);
        if (insightsText.Length > 0)
            sections.Add(new NarrativeSection("Report.Insights", insightsText));

        // ── Architecture Map ──
        if (snapshot.Map is { } map)
        {
            var req = new RenderRequest { Format = "markdown", MaxTokens = 32_000 };
            var mapCtx = new MapRenderContext(map, snapshot, "markdown", req);
            var mapNarrative = map.Archetype == Archetype.Library
                ? await LibrarySurfaceRenderer.RenderAsync(mapCtx, ct)
                : await MapRenderer.RenderAsync(mapCtx, ct);
            if (mapNarrative.SectionFragments is { } fragments)
                foreach (var (key, text) in fragments)
                    sections.Add(new NarrativeSection($"Report.Map.{key}", text));
        }

        // ── Run Report ──
        if (report is not null)
            sections.Add(new NarrativeSection("Report.RunReport", BuildRunReport(report, query)));

        return NarrativeSections.ToRenderedContext(sections, "devcontext/report-v1");
    }

    // ── Section builders ──

    private static string BuildIdentity(AnalysisSnapshot snapshot, GraphQuery query)
    {
        var sb = new StringBuilder();
        var model = snapshot.Model;
        var map = snapshot.Map;
        var sln = model.Solution?.Name ?? Path.GetFileName(snapshot.RootPath.TrimEnd('/', '\\'));

        sb.AppendLine("# REPORT");
        sb.AppendLine($"**{sln}**");
        sb.AppendLine();

        // Architecture style
        if (map is { Style.Length: > 0 })
            sb.AppendLine($"Style: {map.Style}");

        // Identity line
        var parts = new List<string>();
        var projCount = map?.Topology.Length ?? model.Projects.Length;
        if (projCount > 0) parts.Add($"{projCount} project{(projCount != 1 ? "s" : "")}");

        var entryGroups = snapshot.Entries
            .GroupBy(e => e.Kind).OrderBy(g => g.Key)
            .Select(g => $"{g.Count()} {g.Key}").ToList();
        if (entryGroups.Count > 0)
            parts.Add(string.Join(", ", entryGroups));

        // Stack: TFM + detected signals
        var stackParts = new List<string>();
        var tfms = model.Projects
            .SelectMany(p => p.TargetFrameworks)
            .Where(f => !f.Contains("$(", StringComparison.Ordinal))
            .Distinct().OrderBy(f => f).ToList();
        if (tfms.Count > 0)
            stackParts.Add(string.Join(", ", tfms.Take(2)));

        foreach (var (_, sig) in model.Architecture.All)
            if (sig is { Detected: true, Key: not "style" })
                stackParts.Add(sig.Key);

        if (stackParts.Count > 0)
            parts.Add(string.Join(" + ", stackParts));

        if (parts.Count > 0)
            sb.AppendLine($"_{string.Join("  ·  ", parts)}_");

        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildStats(AnalysisSnapshot snapshot, GraphQuery query)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Stats");
        sb.AppendLine();

        var graph = query.Graph;
        var entries = snapshot.Entries;
        var (seams, withTarget, entriesWithDeepSpine, deepSpineRatio) = query.Stats();
        var report = snapshot.Report;
        var model = snapshot.Model;

        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        if (report is not null) sb.AppendLine($"| Files | {report.Corpus.CSharpFiles} |");
        sb.AppendLine($"| Projects | {model.Projects.Length} |");
        sb.AppendLine($"| Nodes | {graph.NodeCount} |");
        sb.AppendLine($"| Edges | {graph.EdgeCount} |");
        var serviceLinkCount = graph.AllEdges.Count(e => e.Kind == EdgeKind.ServiceLink);
        if (serviceLinkCount > 0)
            sb.AppendLine($"| ServiceLinks | {serviceLinkCount} |");
        sb.AppendLine($"| Entries | {entries.Length} |");
        sb.AppendLine($"| With target | {withTarget}/{entries.Length} |");

        if (entries.Length > 0)
            sb.AppendLine($"| Deep spine (>=2) | {entriesWithDeepSpine}/{entries.Length} ({(int)Math.Round(deepSpineRatio * 100)}%) |");

        var totalEdges = seams.Length > 0 ? seams.Sum(s => s.Count) : graph.EdgeCount;
        var approx = seams.Length > 0 ? seams.Sum(s => s.Approx) : 0;
        var verifiedPct = totalEdges > 0 ? (totalEdges - approx) * 100.0 / totalEdges : 0;
        sb.AppendLine($"| Verified edges | {verifiedPct:F0}% |");

        if (report is not null)
            sb.AppendLine($"| Analyzed in | {report.TotalWall.TotalSeconds:F1}s |");

        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildTopFlows(ImmutableArray<EntryPoint> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Top Flows");
        sb.AppendLine();

        var ranked = RankEntries(entries);
        var count = 0;
        foreach (var entry in ranked.Take(10))
        {
            count++;
            var label = entry switch
            {
                { HttpMethod: not null, Route: not null } => $"{entry.HttpMethod} {entry.Route}",
                _ => entry.Title
            };
            var target = entry.Target is { Length: > 0 } t
                ? $" \u2192 `{t}`"
                : "";
            sb.AppendLine($"{count}. **{label}**{target} *({entry.Kind})*");
        }

        if (ranked.Length == 0)
            sb.AppendLine("_No entries found._");

        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>L3.2 — Ranks entries by graph-aware composite score (reach × seams × entities ×
    /// cross-project depth), falling back to has-target + kind priority when scores are identical.
    /// The scores are pre-computed during graph construction so this method is a pure sort.</summary>
    public static ImmutableArray<EntryPoint> RankEntries(ImmutableArray<EntryPoint> entries)
        => entries
            .OrderByDescending(e => e.Score)
            .ThenByDescending(e => e.Target is not null)
            .ThenBy(e => KindPriority(e.Kind))
            .ThenBy(e => e.Title)
            .ToImmutableArray();

    private static int KindPriority(EntryPointKind kind) => kind switch
    {
        EntryPointKind.HttpEndpoint => 0,
        EntryPointKind.GrpcService => 1,
        EntryPointKind.SignalRHub => 2,
        EntryPointKind.GraphQlField => 3,
        EntryPointKind.MessageConsumer => 4,
        EntryPointKind.DomainEventHandler => 5,
        EntryPointKind.PublicApi => 6,
        EntryPointKind.CliCommand => 7,
        EntryPointKind.HostedService => 8,
        EntryPointKind.ScheduledJob => 9,
        EntryPointKind.UiEntry => 10,
        EntryPointKind.FunctionEntry => 11,
        EntryPointKind.GrainMethod => 12,
        _ => 99,
    };

    private static string BuildInsights(ImmutableArray<Insight> insights)
    {
        if (insights.IsDefaultOrEmpty) return "";

        var sb = new StringBuilder();
        sb.AppendLine("## Insights");
        sb.AppendLine();

        var counts = insights
            .GroupBy(i => i.Severity)
            .Select(g => (sev: g.Key, count: g.Count()))
            .Where(x => x.count > 0)
            .OrderBy(x => x.sev)
            .Select(x => x.sev switch
            {
                Severity.Warning => $"{x.count} warning",
                Severity.Notable => $"{x.count} notable",
                _ => $"{x.count} info"
            });

        sb.AppendLine($"_{string.Join(" · ", counts)}_");
        sb.AppendLine();

        foreach (var i in insights)
        {
            var sev = i.Severity switch
            {
                Severity.Warning => "**WARNING**",
                Severity.Notable => "**NOTABLE**",
                _ => "_INFO_"
            };
            sb.AppendLine($"### {sev}: {i.Title}");
            sb.AppendLine($"*({i.Category})*");
            if (i.Evidence.Length > 0)
            {
                sb.AppendLine();
                foreach (var e in i.Evidence.Take(5))
                    sb.AppendLine($"- {e}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string BuildRunReport(RunReport report, GraphQuery query)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Run Report");
        sb.AppendLine();

        sb.AppendLine("### Stages");
        sb.AppendLine();
        sb.AppendLine("| Stage | Time |");
        sb.AppendLine("|-------|------|");
        foreach (var s in report.Stages)
            sb.AppendLine($"| {s.Stage} | {s.Elapsed.TotalMilliseconds:F0}ms |");
        sb.AppendLine($"| **Total** | **{report.TotalWall.TotalMilliseconds:F0}ms** |");
        sb.AppendLine();

        if (report.Extractors.Length > 0)
        {
            sb.AppendLine("### Extractors");
            sb.AppendLine();
            sb.AppendLine("| Name | Time | +Types | +Dets |");
            sb.AppendLine("|------|------|--------|-------|");
            foreach (var ex in report.Extractors
                .OrderByDescending(e => e.Elapsed)
                .Take(15))
                sb.AppendLine($"| {ex.Name} | {ex.Elapsed.TotalMilliseconds:F0}ms | {ex.TypesAdded} | {ex.DetectionsAdded} |");
            sb.AppendLine();
        }

        var (seams, withTarget, _, _) = query.Stats();
        if (seams.Length > 0)
        {
            sb.AppendLine("### Graph Seams");
            sb.AppendLine();
            sb.AppendLine("| Seam | Edges | Approx |");
            sb.AppendLine("|------|-------|--------|");
            foreach (var s in seams)
                sb.AppendLine($"| {s.Seam} | {s.Count} | {s.Approx} |");
            sb.AppendLine();
        }

        sb.Append($"_{report.Corpus.CSharpFiles} files · {report.Corpus.Projects} projects_");
        sb.AppendLine();

        return sb.ToString();
    }
}
