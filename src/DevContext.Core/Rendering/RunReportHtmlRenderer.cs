using System.Text;

namespace DevContext.Core.Rendering;

/// <summary>Renders a RunReport as a standalone HTML fragment for the Desktop Stats tab.</summary>
public static class RunReportHtmlRenderer
{
    public static string Render(RunReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div class='dc-stats'>");

        // Timing waterfall
        var totalMs = report.TotalWall.TotalMilliseconds;
        sb.AppendLine("<section class='dc-stats-section'><h3 class='dc-stats-h3'>Timing</h3>");
        sb.AppendLine("<table class='dc-stats-table'><thead><tr><th>Stage</th><th>Time</th><th></th></tr></thead><tbody>");
        foreach (var stage in report.Stages)
        {
            var pct = totalMs > 0 ? stage.Elapsed.TotalMilliseconds / totalMs * 100 : 0;
            var barWidth = (int)Math.Min(pct, 100);
            sb.AppendLine($"<tr><td class='dc-stats-label'>{stage.Stage}</td><td class='dc-stats-time'>{stage.Elapsed.TotalMilliseconds:F0}ms</td><td class='dc-stats-bar-cell'><div class='dc-stats-bar' style='width:{barWidth}%'></div></td></tr>");
        }
        sb.AppendLine($"<tr class='dc-stats-total'><td><strong>Total</strong></td><td><strong>{totalMs:F0}ms</strong></td><td></td></tr>");
        sb.AppendLine("</tbody></table></section>");

        // Extractor grid
        if (report.Extractors.Length > 0)
        {
            sb.AppendLine("<section class='dc-stats-section'><h3 class='dc-stats-h3'>Extractors</h3>");
            sb.AppendLine("<table class='dc-stats-table'><thead><tr><th>Name</th><th>Time</th><th>+Types</th><th>+Dets</th><th>Status</th></tr></thead><tbody>");
            foreach (var ex in report.Extractors.Take(25))
            {
                var cls = ex.Skipped ? " class='dc-stats-skipped'" : "";
                var status = ex.Skipped
                    ? $"<span title='{System.Net.WebUtility.HtmlEncode(ex.SkipReason ?? "?")}'>skipped</span>"
                    : "<span class='dc-stats-ran'>ran</span>";
                sb.AppendLine($"<tr{cls}><td>{System.Net.WebUtility.HtmlEncode(ex.Name)}</td><td>{ex.Elapsed.TotalMilliseconds:F0}ms</td><td>{ex.TypesAdded}</td><td>{ex.DetectionsAdded}</td><td>{status}</td></tr>");
            }
            sb.AppendLine("</tbody></table></section>");
        }

        // Scorer funnel
        if (report.Scorers.Length > 0)
        {
            sb.AppendLine("<section class='dc-stats-section'><h3 class='dc-stats-h3'>Scorer Funnel</h3>");
            sb.AppendLine("<table class='dc-stats-table'><thead><tr><th>Scorer</th><th>Before</th><th>After</th><th>Delta</th></tr></thead><tbody>");
            foreach (var sc in report.Scorers)
            {
                var delta = sc.TypesBefore > 0
                    ? (sc.TypesBefore - sc.TypesAfter) * 100 / sc.TypesBefore
                    : 0;
                sb.AppendLine($"<tr><td>{sc.Name}</td><td>{sc.TypesBefore}</td><td>{sc.TypesAfter}</td><td>{delta}%</td></tr>");
            }
            sb.AppendLine("</tbody></table></section>");
        }

        // Token funnel
        if (report.Funnel.TypesDiscovered > 0)
        {
            var includedPct = report.Funnel.TypesDiscovered > 0
                ? (double)report.Funnel.TypesIncluded / report.Funnel.TypesDiscovered * 100 : 0;
            var excludedPct = report.Funnel.TypesDiscovered > 0
                ? (double)report.Funnel.TypesHardExcluded / report.Funnel.TypesDiscovered * 100 : 0;
            sb.AppendLine("<section class='dc-stats-section'><h3 class='dc-stats-h3'>Token Funnel</h3>");
            sb.AppendLine("<div class='dc-stats-funnel'>");
            sb.AppendLine($"<div class='dc-stats-funnel-bar' style='width:{includedPct:F0}%' title='{report.Funnel.TypesIncluded} included'></div>");
            sb.AppendLine($"<div class='dc-stats-funnel-exc' style='width:{excludedPct:F0}%' title='{report.Funnel.TypesHardExcluded} excluded'></div>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<p class='dc-stats-funnel-text'>{report.Funnel.TypesDiscovered} discovered → {report.Funnel.TypesIncluded} included · {report.Funnel.TypesHardExcluded} hard-excluded · ~{report.Funnel.RenderedEstimatedTokens} tokens</p>");
            sb.AppendLine("</section>");
        }

        // Cache + Corpus chips
        var cachePct = (report.Cache.TextHits + report.Cache.SyntaxTreeHits) > 0
            ? (double)(report.Cache.TextHits + report.Cache.SyntaxTreeHits) /
              Math.Max(1, report.Cache.TextHits + report.Cache.TextMisses +
                         report.Cache.SyntaxTreeHits + report.Cache.SyntaxTreeMisses) * 100
            : 0;
        sb.AppendLine("<section class='dc-stats-section'><h3 class='dc-stats-h3'>Cache & Corpus</h3>");
        sb.AppendLine($"<div class='dc-stats-chips'><span class='dc-stats-chip'>cache {cachePct:F0}% hit</span><span class='dc-stats-chip'>{report.Corpus.CSharpFiles} files</span><span class='dc-stats-chip'>{report.Corpus.Projects} projects</span>");

        // Parallelism chips
        if (report.Parallelism.Stage2CpuSum > TimeSpan.Zero)
        {
            var s2x = report.Parallelism.Stage2Wall > TimeSpan.Zero
                ? report.Parallelism.Stage2CpuSum.TotalMilliseconds / report.Parallelism.Stage2Wall.TotalMilliseconds
                : 1.0;
            if (s2x > 1.1)
                sb.AppendLine($"<span class='dc-stats-chip'>stage2 ×{s2x:F1}</span>");
        }
        if (report.Parallelism.Stage3CpuSum > TimeSpan.Zero)
        {
            var s3x = report.Parallelism.Stage3Wall > TimeSpan.Zero
                ? report.Parallelism.Stage3CpuSum.TotalMilliseconds / report.Parallelism.Stage3Wall.TotalMilliseconds
                : 1.0;
            if (s3x > 1.1)
                sb.AppendLine($"<span class='dc-stats-chip'>stage3 ×{s3x:F1}</span>");
        }

        sb.AppendLine("</div></section>");

        sb.AppendLine("</div>");
        return sb.ToString();
    }
}
