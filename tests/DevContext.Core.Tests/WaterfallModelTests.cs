using DevContext.Core.Observers;

namespace DevContext.Core.Tests;

/// <summary>K1 (Prism D3.2) — the live-waterfall aggregation: stage rows carry running extractors,
/// discovery counts, skip counts, and stage extras; the ticker streams cumulative discoveries.
/// The Spectre binder renders these strings verbatim, so they ARE the UI contract.</summary>
public sealed class WaterfallModelTests
{
    [Fact]
    public void Stage_rows_stream_running_extractors_and_bake_counts_on_completion()
    {
        var model = new WaterfallModel();
        var changed = new List<string>();
        model.RowChanged += r => changed.Add(r.Description());

        model.StageStarted(PipelineStage.SpecificExtraction);
        model.ExtractorStarted("RouteExtractor");
        model.ExtractorStarted("CallGraphExtractor");
        model.ExtractorStarted("GrpcExtractor");

        // Active row shows at most two running names + a +N overflow.
        Assert.Contains(changed, d => d.Contains("running RouteExtractor, CallGraphExtractor +1"));

        model.ExtractorCompleted("RouteExtractor", skipped: false, typesAdded: 12, detectionsAdded: 662);
        model.ExtractorCompleted("CallGraphExtractor", skipped: false, typesAdded: 0, detectionsAdded: 40);
        model.ExtractorCompleted("GrpcExtractor", skipped: true, typesAdded: 0, detectionsAdded: 0);

        model.StageCompleted(PipelineStage.SpecificExtraction, TimeSpan.FromMilliseconds(1234));

        var final = changed[^1];
        Assert.StartsWith("Extract (specific)", final);
        Assert.Contains("+12t +702d", final);
        Assert.Contains("1 skipped", final);
        Assert.DoesNotContain("running", final); // completion clears the running list
    }

    [Fact]
    public void Ticker_streams_cumulative_discoveries_across_stages()
    {
        var model = new WaterfallModel();
        string? last = null;
        model.TickerChanged += t => last = t;

        model.StageStarted(PipelineStage.GenericExtraction);
        model.ExtractorCompleted("A", skipped: false, typesAdded: 100, detectionsAdded: 5);
        model.StageCompleted(PipelineStage.GenericExtraction, TimeSpan.Zero);
        model.StageStarted(PipelineStage.SpecificExtraction);
        model.ExtractorCompleted("B", skipped: false, typesAdded: 25, detectionsAdded: 657);
        model.ExtractorCompleted("C", skipped: true, typesAdded: 0, detectionsAdded: 0);

        Assert.Equal("125 types · 662 detections · 2 extractors · 1 skipped", last);
        Assert.Equal(last, model.Ticker());
    }

    [Fact]
    public void Signals_and_compression_land_as_stage_extras()
    {
        var model = new WaterfallModel();

        model.StageStarted(PipelineStage.SignalSealing);
        model.SignalsSealed(7);
        WaterfallModel.StageRow? sealing = null;
        model.RowChanged += r => sealing ??= r;
        model.StageCompleted(PipelineStage.SignalSealing, TimeSpan.Zero);
        Assert.NotNull(sealing);
        Assert.Contains("7 signals", sealing.Description());

        WaterfallModel.StageRow? compress = null;
        model.RowChanged += r => { if (r.Stage == PipelineStage.Compression) compress = r; };
        model.StageStarted(PipelineStage.Compression);
        model.CompressionApplied(24000, 8000);
        Assert.NotNull(compress);
        Assert.Contains("~24000→~8000 tok", compress.Description());
    }

    [Fact]
    public void Events_outside_an_active_stage_are_safe_no_ops()
    {
        var model = new WaterfallModel();
        // No stage started: extractor + seal + compression events must not throw or corrupt.
        model.ExtractorStarted("Loose");
        model.ExtractorCompleted("Loose", skipped: false, typesAdded: 3, detectionsAdded: 1);
        model.SignalsSealed(2);
        model.CompressionApplied(10, 5);
        model.StageCompleted(PipelineStage.Insights, TimeSpan.Zero); // never started

        // Loose extractor counts still ride the ticker (they happened), just unattributed to a row.
        Assert.Equal("3 types · 1 detections · 1 extractors · 0 skipped", model.Ticker());
    }
}
