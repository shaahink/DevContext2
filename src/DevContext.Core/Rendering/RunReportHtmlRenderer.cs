using System.Text;

namespace DevContext.Core.Rendering;

/// <summary>Renders a RunReport as a standalone HTML fragment for the Desktop Stats tab.</summary>
public static class RunReportHtmlRenderer
{
    public static string Render(RunReport report, GraphSummary? graphSummary = null,
        int renderedTokens = 0, TokenFunnel? renderFunnel = null)
    {
        var funnel = renderFunnel ?? report.Funnel;
        var totalMs = report.TotalWall.TotalMilliseconds;
        var sb = new StringBuilder();

        sb.AppendLine("<div class='dc-stats-grid'>");

        // ── Card 1: Timing Waterfall (full-width left col) ──
        RenderTimingCard(sb, report, totalMs);

        // ── Card 2: Extractor Grid (full-width left col) ──
        RenderExtractorCard(sb, report);

        // ── Card 3: Scorer Funnel (right col top) ──
        RenderScorerCard(sb, report, funnel);

        // ── Card 4: Cache & Corpus + Parallelism + Graph (right col bottom) ──
        RenderSummaryCard(sb, report, graphSummary, renderedTokens);

        sb.AppendLine("</div>");
        return sb.ToString();
    }

    private static void RenderTimingCard(StringBuilder sb, RunReport report, double totalMs)
    {
        sb.AppendLine("<div class='dc-stats-card'>");
        sb.AppendLine("<div class='dc-stats-card-header'>Timing Waterfall</div>");
        sb.AppendLine("<div class='dc-stats-card-body'>");

        foreach (var stage in report.Stages)
        {
            var pct = totalMs > 0 ? stage.Elapsed.TotalMilliseconds / totalMs * 100 : 0;
            var barWidth = Math.Max(1, (int)Math.Min(pct * 2, 100));
            var label = stage.Stage switch
            {
                "DiscoveryAndCacheWarmup" => "Discovery",
                "GenericExtraction" => "GenericExtract",
                "SpecificExtraction" => "SpecificExtract",
                _ => stage.Stage
            };
            sb.AppendLine("<div class='dc-stats-timing-row'>");
            sb.AppendLine($"<span class='dc-stats-timing-label'>{System.Net.WebUtility.HtmlEncode(label)}</span>");
            sb.AppendLine($"<span class='dc-stats-timing-ms'>{stage.Elapsed.TotalMilliseconds:F0}ms</span>");
            sb.AppendLine($"<div class='dc-stats-timing-track'><div class='dc-stats-timing-bar' style='width:{barWidth}%'></div></div>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("<div class='dc-stats-timing-total'>");
        sb.AppendLine($"<span class='dc-stats-timing-label'>Total</span><span class='dc-stats-timing-ms'>{totalMs / 1000:F1}s</span>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div></div>");
    }

    private static void RenderExtractorCard(StringBuilder sb, RunReport report)
    {
        sb.AppendLine("<div class='dc-stats-card'>");
        sb.AppendLine("<div class='dc-stats-card-header'>Extractors</div>");
        sb.AppendLine("<div class='dc-stats-card-body dc-stats-extractors-scroll'>");
        sb.AppendLine("<table class='dc-stats-table'><thead><tr><th>Name</th><th>Time</th><th>+Types</th><th>+Dets</th><th></th></tr></thead><tbody>");

        foreach (var ex in report.Extractors)
        {
            var name = ex.Name.Length > 24
                ? System.Net.WebUtility.HtmlEncode(ex.Name[..22] + "..")
                : System.Net.WebUtility.HtmlEncode(ex.Name);
            var cls = ex.Skipped ? " class='dc-stats-skipped'" : "";
            var status = ex.Skipped
                ? $"<span class='dc-stats-skip-chip' title='{System.Net.WebUtility.HtmlEncode(ex.SkipReason ?? "?")}'>off</span>"
                : "";

            if (ex.Skipped)
            {
                sb.AppendLine($"<tr{cls}><td>{name}</td><td>&mdash;</td><td>&mdash;</td><td>&mdash;</td><td>{status}</td></tr>");
            }
            else
            {
                var timeFrag = ex.Elapsed.TotalMilliseconds >= 1
                    ? $"{ex.Elapsed.TotalMilliseconds:F0}ms"
                    : $"{(ex.Elapsed.TotalMilliseconds * 1000):F0}&micro;s";
                sb.AppendLine($"<tr><td>{name}</td><td class='dc-stats-time'>{timeFrag}</td><td>{ex.TypesAdded}</td><td>{ex.DetectionsAdded}</td><td></td></tr>");
            }
        }

        sb.AppendLine("</tbody></table>");
        sb.AppendLine("</div></div>");
    }

    private static void RenderScorerCard(StringBuilder sb, RunReport report, TokenFunnel funnel)
    {
        sb.AppendLine("<div class='dc-stats-card'>");
        sb.AppendLine("<div class='dc-stats-card-header'>Scorer &amp; Token Funnel</div>");
        sb.AppendLine("<div class='dc-stats-card-body'>");

        // Scorer mini-table
        if (report.Scorers.Length > 0)
        {
            sb.AppendLine("<table class='dc-stats-table'><thead><tr><th>Scorer</th><th>Before</th><th>After</th><th>&Delta;</th></tr></thead><tbody>");
            foreach (var sc in report.Scorers)
            {
                var delta = sc.TypesBefore > 0
                    ? (sc.TypesBefore - sc.TypesAfter) * 100 / sc.TypesBefore
                    : 0;
                sb.AppendLine($"<tr><td>{System.Net.WebUtility.HtmlEncode(sc.Name)}</td><td>{sc.TypesBefore}</td><td>{sc.TypesAfter}</td><td>&minus;{delta}%</td></tr>");
            }
            sb.AppendLine("</tbody></table>");
        }

        // Token funnel bar
        if (funnel.TypesDiscovered > 0)
        {
            var includedPct = (double)funnel.TypesIncluded / Math.Max(1, funnel.TypesDiscovered) * 100;
            var excludedPct = (double)funnel.TypesHardExcluded / Math.Max(1, funnel.TypesDiscovered) * 100;
            sb.AppendLine("<div class='dc-stats-funnel-label'>Types</div>");
            sb.AppendLine("<div class='dc-stats-funnel'>");
            sb.AppendLine($"<div class='dc-stats-funnel-bar' style='flex:{includedPct:F1}%' title='{funnel.TypesIncluded} included'></div>");
            sb.AppendLine($"<div class='dc-stats-funnel-exc' style='flex:{excludedPct:F1}%' title='{funnel.TypesHardExcluded} hard-excluded'></div>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class='dc-stats-funnel-text'>{funnel.TypesDiscovered:N0} discovered &rarr; {funnel.TypesIncluded:N0} included &middot; {funnel.TypesHardExcluded:N0} excluded &middot; ~{funnel.RenderedEstimatedTokens:N0} tokens</div>");
        }

        sb.AppendLine("</div></div>");
    }

    private static void RenderSummaryCard(StringBuilder sb, RunReport report, GraphSummary? graphSummary, int renderedTokens)
    {
        sb.AppendLine("<div class='dc-stats-card'>");
        sb.AppendLine("<div class='dc-stats-card-header'>Cache &amp; Corpus</div>");
        sb.AppendLine("<div class='dc-stats-card-body'>");

        // Cache hit-rate gauge
        var totalCacheOps = report.Cache.TextHits + report.Cache.TextMisses
            + report.Cache.SyntaxTreeHits + report.Cache.SyntaxTreeMisses;
        var totalCacheHits = report.Cache.TextHits + report.Cache.SyntaxTreeHits;
        var cachePct = totalCacheOps > 0
            ? (double)totalCacheHits / totalCacheOps * 100
            : 0;

        sb.AppendLine("<div class='dc-stats-gauge-row'>");
        sb.AppendLine($"<span class='dc-stats-gauge'>{cachePct:F0}<span class='dc-stats-gauge-unit'>%</span></span>");
        sb.AppendLine("<span class='dc-stats-gauge-label'>cache hit</span>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class='dc-stats-gauge-track'>");
        sb.AppendLine($"<div class='dc-stats-gauge-fill' style='width:{cachePct:F0}%'></div>");
        sb.AppendLine("</div>");

        // Corpus chips
        sb.AppendLine("<div class='dc-stats-chips'>");
        var files = report.Corpus.CSharpFiles > 0 ? report.Corpus.CSharpFiles : report.Corpus.TotalFiles;
        sb.AppendLine($"<span class='dc-stats-chip'>{files} files</span>");
        if (report.Corpus.Projects > 0)
            sb.AppendLine($"<span class='dc-stats-chip'>{report.Corpus.Projects} projects</span>");
        sb.AppendLine($"<span class='dc-stats-chip'>{report.TotalWall.TotalSeconds:F1}s wall</span>");
        if (renderedTokens > 0)
            sb.AppendLine($"<span class='dc-stats-chip'>~{renderedTokens:N0} tokens</span>");
        sb.AppendLine("</div>");

        // Parallelism speedup
        var hasSpeedup = false;
        if (report.Parallelism.Stage2CpuSum > report.Parallelism.Stage2Wall + TimeSpan.FromMilliseconds(50))
        {
            var s2x = report.Parallelism.Stage2CpuSum.TotalMilliseconds / Math.Max(1, report.Parallelism.Stage2Wall.TotalMilliseconds);
            sb.AppendLine($"<span class='dc-stats-badge'>stage2 &times;{s2x:F1}</span>");
            hasSpeedup = true;
        }
        if (report.Parallelism.Stage3CpuSum > report.Parallelism.Stage3Wall + TimeSpan.FromMilliseconds(50))
        {
            var s3x = report.Parallelism.Stage3CpuSum.TotalMilliseconds / Math.Max(1, report.Parallelism.Stage3Wall.TotalMilliseconds);
            sb.AppendLine($"<span class='dc-stats-badge'>stage3 &times;{s3x:F1}</span>");
            hasSpeedup = true;
        }
        if (hasSpeedup)
            sb.AppendLine("<br>");

        // Graph summary (Map/Trace mode)
        if (graphSummary is { } g)
        {
            sb.AppendLine("<div class='dc-stats-graph-summary'>");
            sb.AppendLine($"<span class='dc-stats-graph-stat'>{g.Nodes}<span class='dc-stats-graph-label'>nodes</span></span>");
            sb.AppendLine($"<span class='dc-stats-graph-stat'>{g.Edges}<span class='dc-stats-graph-label'>edges</span></span>");
            sb.AppendLine($"<span class='dc-stats-graph-stat'>{g.Entries}<span class='dc-stats-graph-label'>entries</span></span>");
            if (g.TraceDepth is { } d)
                sb.AppendLine($"<span class='dc-stats-graph-stat'>{d}<span class='dc-stats-graph-label'>depth</span></span>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</div></div>");
    }
}
