using System.Collections.Concurrent;

using DevContext.Core.Observers;

namespace DevContext.Cli.Observers;

/// <summary>K1 (Prism D3.2) — the interactive analyze view: the observer stream renders as a LIVE
/// waterfall (Spectre Progress) — one ticking row per pipeline stage with elapsed, the extractors
/// currently running, and discoveries streaming into a ticker row — instead of a blind spinner.
/// Non-interactive runs never construct this (the plain line stream stays byte-stable for
/// harnesses). Diagnostics are buffered and replayed after the live display closes — writing
/// through a live region corrupts it.</summary>
public sealed class WaterfallDiscoveryObserver : IDiscoveryObserver
{
    private readonly WaterfallModel _model = new();
    private readonly ConcurrentQueue<DiagnosticEntry> _diagnostics = new();
    private ProgressContext? _ctx;
    private ProgressTask? _ticker;
    private readonly ConcurrentDictionary<PipelineStage, ProgressTask> _tasks = new();

    public WaterfallModel Model => _model;
    public IReadOnlyCollection<DiagnosticEntry> BufferedDiagnostics => [.. _diagnostics];

    /// <summary>Bind to a live Progress context. Call before AnalyzeAsync — subscriptions are made
    /// here, so events fired earlier would not render.</summary>
    public void Attach(ProgressContext ctx)
    {
        _ctx = ctx;
        _ticker = ctx.AddTask("[dim]discovered: nothing yet[/]", autoStart: true, maxValue: 1);
        _ticker.IsIndeterminate = true;

        _model.RowStarted += row =>
        {
            if (_ctx is null) return;
            var task = _ctx.AddTask(Markup.Escape(row.Description()), autoStart: true, maxValue: 1);
            task.IsIndeterminate = true;
            _tasks[row.Stage] = task;
        };
        _model.RowChanged += row =>
        {
            if (!_tasks.TryGetValue(row.Stage, out var task)) return;
            var text = Markup.Escape(row.Description());
            task.Description = row.Done ? $"[green]{text}[/]" : text;
            if (row.Done && !task.IsFinished)
            {
                task.IsIndeterminate = false;
                task.Value = task.MaxValue;
                task.StopTask();
            }
        };
        _model.TickerChanged += text =>
        {
            if (_ticker is not null)
                _ticker.Description = $"[dim]discovered: {Markup.Escape(text)}[/]";
        };
    }

    public void OnPipelineStarted(DiscoveryContext context) { }
    public void OnStageStarted(PipelineStage stage) => _model.StageStarted(stage);
    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed) => _model.StageCompleted(stage, elapsed);
    public void OnExtractorStarted(string name, ExtractorTier tier) => _model.ExtractorStarted(name);

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
        => _model.ExtractorCompleted(name, skipped, typesAdded, detectionsAdded);

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals)
        => _model.SignalsSealed(signals.Values.Count(s => s.Detected));

    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter) { }

    public void OnCompressionApplied(CompressionResult result)
        => _model.CompressionApplied(result.TokensBefore, result.TokensAfter);

    public void OnRenderCompleted(RenderedContext result) { }
    public void OnPipelineCompleted(DiscoveryModel model) { }
    public void OnDiagnostic(DiagnosticEntry entry) => _diagnostics.Enqueue(entry);
}
