using DevContext.Core.Pipeline;

namespace DevContext.Core.Tests;

/// <summary>J1 (Prism D2) — the silent-failure amnesty channel. These pin the contract every
/// converted bare-catch site now relies on: counted in-scope, no-op outside, first sample kept.</summary>
public class PipelineDiagnosticsTests
{
    [Fact]
    public void Swallowed_outside_a_scope_is_a_noop_and_drain_is_empty()
    {
        PipelineDiagnostics.Swallowed("X", "y", new InvalidOperationException("boom"));
        Assert.Empty(PipelineDiagnostics.Drain());
    }

    [Fact]
    public void Counts_aggregate_per_source_and_category_keeping_the_first_sample()
    {
        using var scope = PipelineDiagnostics.BeginScope();
        PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-bind", new InvalidOperationException("first"));
        PipelineDiagnostics.Swallowed("SemanticLitePopulator", "semantic-bind", new InvalidOperationException("second"));
        PipelineDiagnostics.Swallowed("SemanticLitePopulator", "syntax-parse");
        PipelineDiagnostics.Swallowed("GraphBuilder", "seam-detector", new ArgumentException("other"));

        var drained = PipelineDiagnostics.Drain();
        Assert.Equal(3, drained.Length);

        var bind = Assert.Single(drained, f => f.Category == "semantic-bind");
        Assert.Equal("SemanticLitePopulator", bind.Source);
        Assert.Equal(2, bind.Count);
        Assert.Equal("InvalidOperationException: first", bind.SampleException);

        var parse = Assert.Single(drained, f => f.Category == "syntax-parse");
        Assert.Equal(1, parse.Count);
        Assert.Null(parse.SampleException);

        // Largest count first — the render shows the worst offender at the top.
        Assert.Equal("semantic-bind", drained[0].Category);
    }

    [Fact]
    public void Scope_dispose_clears_the_channel()
    {
        using (PipelineDiagnostics.BeginScope())
        {
            PipelineDiagnostics.Swallowed("X", "y");
            Assert.Single(PipelineDiagnostics.Drain());
        }
        Assert.Empty(PipelineDiagnostics.Drain());
    }

    [Fact]
    public void Parallel_swallows_sum_exactly()
    {
        using var scope = PipelineDiagnostics.BeginScope();
        Parallel.For(0, 500, _ => PipelineDiagnostics.Swallowed("P", "hot", new Exception("e")));
        var row = Assert.Single(PipelineDiagnostics.Drain());
        Assert.Equal(500, row.Count);
    }
}
