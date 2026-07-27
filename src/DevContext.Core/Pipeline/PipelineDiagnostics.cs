using System.Collections.Concurrent;

using DevContext.Core.Models;

namespace DevContext.Core.Pipeline;

/// <summary>
/// J1 (Prism D2) — the silent-failure amnesty channel. Every former bare catch-swallow in Core
/// reports here instead of vanishing: <c>Swallowed(source, category, ex)</c> increments a
/// (source × category) counter and keeps the FIRST sample exception. The pipeline opens a scope per
/// run (AsyncLocal — flows into parallel extractor tasks, isolates concurrent server analyses) and
/// drains the counters into <see cref="DiscoveryModel.ExtractionFailures"/> at the end, where stats
/// and the analyze waterfall surface them (J3). Outside a scope the channel no-ops — same swallow
/// semantics as before, so call sites never change behavior, only visibility.
/// </summary>
public static class PipelineDiagnostics
{
    private sealed class Sink
    {
        public readonly ConcurrentDictionary<(string Source, string Category), Counter> Counts = new();
    }

    private sealed class Counter
    {
        public int Count;
        public string? Sample;
    }

    private static readonly AsyncLocal<Sink?> _scope = new();

    /// <summary>Opens a per-run counting scope. Dispose to close (drain first).</summary>
    public static IDisposable BeginScope()
    {
        var sink = new Sink();
        _scope.Value = sink;
        return new ScopeEnd(sink);
    }

    /// <summary>Counts a swallowed failure. <paramref name="source"/> is the component (extractor,
    /// builder, resolver), <paramref name="category"/> what failed (e.g. "semantic-bind"). The first
    /// exception per (source, category) is kept as the sample. No-op outside a scope.</summary>
    public static void Swallowed(string source, string category, Exception? ex = null)
    {
        if (_scope.Value is not { } sink) return;
        var counter = sink.Counts.GetOrAdd((source, category), _ => new Counter());
        Interlocked.Increment(ref counter.Count);
        if (ex is not null && counter.Sample is null)
        {
            var msg = ex.Message is { Length: > 160 } m ? m[..160] : ex.Message;
            Interlocked.CompareExchange(ref counter.Sample, $"{ex.GetType().Name}: {msg}", null);
        }
    }

    /// <summary>Snapshots the current scope's counters, largest first. Empty outside a scope.</summary>
    public static ImmutableArray<SwallowedFailure> Drain()
    {
        if (_scope.Value is not { } sink) return [];
        return [.. sink.Counts
            .Select(kv => new SwallowedFailure(kv.Key.Source, kv.Key.Category, kv.Value.Count, kv.Value.Sample))
            .OrderByDescending(f => f.Count)
            .ThenBy(f => f.Source, StringComparer.Ordinal)
            .ThenBy(f => f.Category, StringComparer.Ordinal)];
    }

    private sealed class ScopeEnd(Sink sink) : IDisposable
    {
        public void Dispose()
        {
            if (ReferenceEquals(_scope.Value, sink)) _scope.Value = null;
        }
    }
}
