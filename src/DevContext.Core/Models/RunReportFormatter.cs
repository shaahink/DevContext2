namespace DevContext.Core.Models;

/// <summary>Formats a RunReport into human-readable summaries. Shared by CLI and Desktop.</summary>
public static class RunReportFormatter
{
    /// <summary>Produces a compact one-line summary: "analyzed N files · M types kept of O · X/Y tokens · Zs".</summary>
    public static string Summary(RunReport report, TokenFunnel? renderFunnel = null)
    {
        var funnel = renderFunnel ?? report.Funnel;
        var files = report.Corpus.TotalFiles > 0 ? report.Corpus.TotalFiles : report.Corpus.CSharpFiles;
        var kept = funnel.TypesIncluded;
        var total = funnel.TypesDiscovered;
        var tokens = renderFunnel is not null ? renderFunnel.RenderedEstimatedTokens : funnel.RenderedEstimatedTokens;
        var budget = funnel.Budget;
        var elapsed = report.TotalWall.TotalSeconds;

        var speedup = "";
        if (report.Parallelism.Stage2CpuSum > report.Parallelism.Stage2Wall + TimeSpan.FromMilliseconds(50))
            speedup += $" stage2 ×{report.Parallelism.Stage2CpuSum.TotalMilliseconds / report.Parallelism.Stage2Wall.TotalMilliseconds:F1}";
        if (report.Parallelism.Stage3CpuSum > report.Parallelism.Stage3Wall + TimeSpan.FromMilliseconds(50))
            speedup += $" stage3 ×{report.Parallelism.Stage3CpuSum.TotalMilliseconds / report.Parallelism.Stage3Wall.TotalMilliseconds:F1}";

        return $"analyzed {files} files · {kept} types kept of {total} · {tokens}/{budget} tokens · {elapsed:F1}s{speedup}";
    }
}
