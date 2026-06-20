namespace DevContext.Core.Models;

/// <summary>Formats a RunReport into human-readable summaries. Shared by CLI and Desktop.</summary>
public static class RunReportFormatter
{
    /// <summary>Produces a compact one-line summary. Catalog mode reports the type funnel
    /// ("M types kept of O"); the Map/Trace narrative reports graph shape ("N nodes · M edges …"),
    /// for which the type funnel is meaningless (assessment G8).</summary>
    public static string Summary(RunReport report, TokenFunnel? renderFunnel = null,
        GraphSummary? graphSummary = null, int renderedTokens = 0)
    {
        var files = report.Corpus.TotalFiles > 0 ? report.Corpus.TotalFiles : report.Corpus.CSharpFiles;
        var elapsed = report.TotalWall.TotalSeconds;

        var speedup = "";
        if (report.Parallelism.Stage2CpuSum > report.Parallelism.Stage2Wall + TimeSpan.FromMilliseconds(50))
            speedup += $" stage2 ×{report.Parallelism.Stage2CpuSum.TotalMilliseconds / report.Parallelism.Stage2Wall.TotalMilliseconds:F1}";
        if (report.Parallelism.Stage3CpuSum > report.Parallelism.Stage3Wall + TimeSpan.FromMilliseconds(50))
            speedup += $" stage3 ×{report.Parallelism.Stage3CpuSum.TotalMilliseconds / report.Parallelism.Stage3Wall.TotalMilliseconds:F1}";

        if (graphSummary is { } g)
        {
            var depth = g.TraceDepth is int d ? $" · depth {d}" : "";
            return $"analyzed {files} files · {g.Nodes} nodes · {g.Edges} edges · {g.Entries} entries{depth} · ~{renderedTokens} tokens · {elapsed:F1}s{speedup}";
        }

        var funnel = renderFunnel ?? report.Funnel;
        var tokens = renderFunnel is not null ? renderFunnel.RenderedEstimatedTokens : funnel.RenderedEstimatedTokens;
        return $"analyzed {files} files · {funnel.TypesIncluded} types kept of {funnel.TypesDiscovered} · {tokens}/{funnel.Budget} tokens · {elapsed:F1}s{speedup}";
    }
}
