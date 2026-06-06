using System.Collections.Concurrent;

namespace DevContext.Cli.Observers;

public sealed class SpectreDiscoveryObserver : IDiscoveryObserver
{
    private readonly StatusContext? _status;
    private readonly ConcurrentQueue<string> _log = new();
    private readonly ConcurrentQueue<string> _pendingLines = new();
    private int _indent;
    private bool _inParallelStage;

    public SpectreDiscoveryObserver(StatusContext? status = null)
    {
        _status = status;
    }

    public void OnPipelineStarted(DiscoveryContext context)
    {
        WriteLine("Resolving root");
    }

    public void OnStageStarted(PipelineStage stage)
    {
        _inParallelStage = stage == PipelineStage.GenericExtraction;
        _indent++;
        WriteLine($"Stage: {stage}");
    }

    public void OnExtractorStarted(string name, ExtractorTier tier)
    {
        _indent++;
        var line = $"  ∟ {name}...";
        if (_inParallelStage)
            _pendingLines.Enqueue(line);
        else
            WriteLine(line);
    }

    public void OnExtractorCompleted(string name, TimeSpan elapsed, bool skipped, string? skipReason,
        int typesAdded = 0, int detectionsAdded = 0)
    {
        _indent--;
        var ms = elapsed.TotalMilliseconds;
        var note = skipped ? $" (skipped: {skipReason})" : "";
        var impact = (typesAdded > 0 || detectionsAdded > 0)
            ? $" (+{typesAdded}t +{detectionsAdded}d)" : "";
        var line = $"  ∟ [dim]{name}[/] ✓ {ms:F0}ms{note}{impact}";
        if (_inParallelStage)
            _pendingLines.Enqueue(line);
        else
            WriteLine(line);
    }

    public void OnSignalsSealed(IReadOnlyDictionary<string, FeatureSignal> signals)
    {
        var detected = signals.Values.Where(s => s.Detected).Select(s => s.Key);
        WriteLine($"Signals sealed: {string.Join(", ", detected)}");
    }

    public void OnPrunerCompleted(string name, int itemsBefore, int itemsAfter)
    {
        var pct = itemsBefore > 0 ? (itemsBefore - itemsAfter) * 100 / itemsBefore : 0;
        WriteLine($"{name}: {itemsBefore} -> {itemsAfter} types ({pct}% pruned)");
    }

    public void OnCompressionApplied(CompressionResult result)
    {
        var pct = result.TokensBefore > 0
            ? (result.TokensBefore - result.TokensAfter) * 100 / result.TokensBefore
            : 0;
        WriteLine($"{result.StrategyName}: ~{result.TokensBefore} -> ~{result.TokensAfter} tokens (-{pct}%)");
    }

    public void OnStageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        if (_inParallelStage)
        {
            while (_pendingLines.TryDequeue(out var line))
                WriteLine(line);
            _inParallelStage = false;
        }
        _indent--;
        WriteLine($"Stage completed [{elapsed.TotalMilliseconds:F0}ms]");
    }

    public void OnRenderCompleted(RenderedContext result)
    {
        WriteLine($"Rendering: {result.EstimatedTokens} tokens");
    }

    public void OnPipelineCompleted(DiscoveryModel model)
    {
        var pruned = model.Types.Count(t => !t.Value.IsPruned);
        var total = model.Types.Count;
        var pct = total > 0 ? (total - pruned) * 100 / total : 0;

        var table = new Table();
        table.AddColumn("Metric");
        table.AddColumn("Value");
        table.AddRow("Types found", total.ToString());
        table.AddRow("Types in output", pruned.ToString());
        table.AddRow("Pruned", $"{pct}%");
        table.AddRow("Detections", model.Detections.Count.ToString());
        AnsiConsole.Write(table);
    }

    public void OnDiagnostic(DiagnosticEntry entry)
    {
        WriteLine($"[{entry.Level}] {entry.Source}: {entry.Message}");
    }

    private void WriteLine(string message)
    {
        var indented = new string(' ', _indent * 2) + message;
        _log.Enqueue(indented);
        if (_status != null)
            _status.Status(Spectre.Console.Markup.Escape(indented));
        else
            AnsiConsole.WriteLine(indented);
    }

    public IEnumerable<string> GetLog() => _log;
}
