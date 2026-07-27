namespace DevContext.Core.Observers;

/// <summary>K1 (Prism D3.2) — pure state behind the live analyze waterfall. Consumers (the CLI's
/// Spectre binder today) subscribe to the events and render rows; keeping the aggregation
/// console-free makes the row/ticker text unit-testable without a TTY. Thread-safe: extractor
/// events arrive from parallel stage workers.</summary>
public sealed class WaterfallModel
{
    public sealed class StageRow
    {
        public required PipelineStage Stage { get; init; }
        public required string Label { get; init; }
        public bool Done { get; private set; }
        public double ElapsedMs { get; private set; }
        public int ExtractorsDone { get; internal set; }
        public int TypesAdded { get; internal set; }
        public int DetectionsAdded { get; internal set; }
        public int Skipped { get; internal set; }
        public string? Extra { get; internal set; }
        internal readonly List<string> Running = [];

        internal void Complete(TimeSpan elapsed)
        {
            Done = true;
            ElapsedMs = elapsed.TotalMilliseconds;
            Running.Clear();
        }

        /// <summary>The task-description text for this row in its current state.</summary>
        public string Description()
        {
            var parts = new List<string> { Label };
            if (TypesAdded > 0 || DetectionsAdded > 0)
                parts.Add($"+{TypesAdded}t +{DetectionsAdded}d");
            if (Skipped > 0)
                parts.Add($"{Skipped} skipped");
            if (Extra is not null)
                parts.Add(Extra);
            if (!Done && Running.Count > 0)
            {
                var shown = Running.Take(2).ToArray();
                var more = Running.Count > shown.Length ? $" +{Running.Count - shown.Length}" : "";
                parts.Add($"running {string.Join(", ", shown)}{more}");
            }
            return string.Join(" · ", parts);
        }
    }

    private readonly object _lock = new();
    private readonly Dictionary<PipelineStage, StageRow> _rows = [];
    private StageRow? _active;
    private int _types, _dets, _ran, _skipped;

    public event Action<StageRow>? RowChanged;
    public event Action<StageRow>? RowStarted;
    public event Action<string>? TickerChanged;

    private static string LabelFor(PipelineStage stage) => stage switch
    {
        PipelineStage.ProjectRootResolution => "Resolve root",
        PipelineStage.DiscoveryAndCacheWarmup => "Discover files",
        PipelineStage.GenericExtraction => "Extract (generic)",
        PipelineStage.SignalSealing => "Seal signals",
        PipelineStage.SpecificExtraction => "Extract (specific)",
        PipelineStage.SemanticLite => "Semantic upgrade",
        PipelineStage.GraphAssembly => "Assemble graph",
        PipelineStage.Insights => "Insights",
        PipelineStage.Scoring => "Score",
        PipelineStage.Compression => "Compress",
        PipelineStage.Snapshot => "Snapshot",
        PipelineStage.Rendering => "Render",
        _ => stage.ToString(),
    };

    public string Ticker()
    {
        lock (_lock)
            return $"{_types} types · {_dets} detections · {_ran} extractors · {_skipped} skipped";
    }

    public void StageStarted(PipelineStage stage)
    {
        StageRow row;
        lock (_lock)
        {
            if (_rows.ContainsKey(stage)) return; // defensive: stages fire once
            row = new StageRow { Stage = stage, Label = LabelFor(stage) };
            _rows[stage] = row;
            _active = row;
        }
        RowStarted?.Invoke(row);
    }

    public void StageCompleted(PipelineStage stage, TimeSpan elapsed)
    {
        StageRow? row;
        lock (_lock)
        {
            if (!_rows.TryGetValue(stage, out row)) return;
            row.Complete(elapsed);
            if (ReferenceEquals(_active, row)) _active = null;
        }
        RowChanged?.Invoke(row);
    }

    public void ExtractorStarted(string name)
    {
        StageRow? row;
        lock (_lock)
        {
            row = _active;
            row?.Running.Add(name);
        }
        if (row is not null) RowChanged?.Invoke(row);
    }

    public void ExtractorCompleted(string name, bool skipped, int typesAdded, int detectionsAdded)
    {
        StageRow? row;
        string ticker;
        lock (_lock)
        {
            row = _active;
            if (row is not null)
            {
                row.Running.Remove(name);
                row.ExtractorsDone++;
                row.TypesAdded += typesAdded;
                row.DetectionsAdded += detectionsAdded;
                if (skipped) row.Skipped++;
            }
            if (skipped) _skipped++; else _ran++;
            _types += typesAdded;
            _dets += detectionsAdded;
            ticker = $"{_types} types · {_dets} detections · {_ran} extractors · {_skipped} skipped";
        }
        if (row is not null) RowChanged?.Invoke(row);
        TickerChanged?.Invoke(ticker);
    }

    public void SignalsSealed(int detectedCount)
    {
        StageRow? row;
        lock (_lock)
        {
            row = _active is { Stage: PipelineStage.SignalSealing } ? _active : null;
            if (row is not null) row.Extra = $"{detectedCount} signals";
        }
        if (row is not null) RowChanged?.Invoke(row);
    }

    public void CompressionApplied(int tokensBefore, int tokensAfter)
    {
        StageRow? row;
        lock (_lock)
        {
            row = _active is { Stage: PipelineStage.Compression } ? _active : null;
            if (row is not null) row.Extra = $"~{tokensBefore}→~{tokensAfter} tok";
        }
        if (row is not null) RowChanged?.Invoke(row);
    }
}
