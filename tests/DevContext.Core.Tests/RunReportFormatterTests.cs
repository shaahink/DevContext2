using DevContext.Core.Models;

namespace DevContext.Core.Tests;

public sealed class RunReportFormatterTests
{
    private static RunReport Report(int totalFiles = 10, double seconds = 1.5) => new()
    {
        Stages = [], Extractors = [], Scorers = [], Compressions = [],
        Cache = new(0, 0, 0, 0),
        Corpus = new(totalFiles, totalFiles, 2),
        Funnel = new(100, 0, 40, 0, 500, 8000),
        Parallelism = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero),
        TotalWall = TimeSpan.FromSeconds(seconds),
    };

    [Fact]
    public void Catalog_mode_reports_the_type_funnel()
    {
        var line = RunReportFormatter.Summary(Report(), renderFunnel: new TokenFunnel(100, 0, 40, 0, 500, 8000));

        Assert.Contains("40 types kept of 100", line);
        Assert.DoesNotContain("nodes", line);
    }

    [Fact]
    public void Map_reports_graph_shape_without_depth()
    {
        var line = RunReportFormatter.Summary(Report(), graphSummary: new GraphSummary(247, 7, 3, null), renderedTokens: 236);

        Assert.Contains("247 nodes · 7 edges · 3 entries", line);
        Assert.Contains("~236 tokens", line);
        Assert.DoesNotContain("types kept", line);
        Assert.DoesNotContain("depth", line);
    }

    [Fact]
    public void Trace_reports_graph_shape_with_depth()
    {
        var line = RunReportFormatter.Summary(Report(), graphSummary: new GraphSummary(247, 10, 3, 2), renderedTokens: 188);

        Assert.Contains("· depth 2 ·", line);
    }
}
