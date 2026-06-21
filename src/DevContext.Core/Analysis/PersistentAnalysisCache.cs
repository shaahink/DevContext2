using System.Collections.Concurrent;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Analysis;

/// <summary>An <see cref="IAnalysisCache"/> that is safe to reuse <b>across analysis runs</b>: each cached
/// entry records the file's last-write time, and a re-parse/re-read is forced only when the file changes
/// on disk. Reusing a single instance across desktop analyses (focus/option changes that re-run the same
/// project) skips re-reading and re-parsing unchanged files — the dominant interactive cost — while a real
/// edit invalidates exactly the touched file, so output stays correct (P3). The per-run <see cref="AnalysisCache"/>
/// remains the right choice for one-shot CLI runs.</summary>
public sealed class PersistentAnalysisCache : IAnalysisCache, ICacheStatsSource
{
    private readonly IFileSystem _fs;
    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _knownPaths = new();

    private int _textHits;
    private int _textMisses;
    private int _syntaxHits;
    private int _syntaxMisses;

    /// <summary>Creates a persistent cache backed by the given file system.</summary>
    public PersistentAnalysisCache(IFileSystem fs)
    {
        _fs = fs;
    }

    private sealed class Entry
    {
        public required DateTime Mtime { get; init; }
        public required Lazy<Task<string>> Text { get; init; }
        public required Lazy<Task<SyntaxTree>> Syntax { get; init; }
        public required Lazy<Task<XDocument>> Xml { get; init; }
    }

    /// <summary>Gets the list of all registered file paths known to the cache.</summary>
    public IReadOnlyList<string> KnownFilePaths => _knownPaths.Keys.ToList();

    /// <summary>Registers a file path so it appears in <see cref="KnownFilePaths"/>.</summary>
    public void RegisterPath(string filePath) => _knownPaths.TryAdd(filePath, 0);

    /// <summary>Returns the entry for a path, rebuilding it if the file's mtime changed since it was cached.</summary>
    private Entry GetEntry(string filePath)
    {
        DateTime mtime;
        try { mtime = _fs.GetLastWriteTimeUtc(filePath); }
        catch { mtime = DateTime.MinValue; }

        return _entries.AddOrUpdate(
            filePath,
            static (path, state) => NewEntry(state.fs, path, state.mtime),
            static (path, existing, state) => existing.Mtime == state.mtime
                ? existing
                : NewEntry(state.fs, path, state.mtime),
            (fs: _fs, mtime));
    }

    private static Entry NewEntry(IFileSystem fs, string filePath, DateTime mtime)
    {
        // CancellationToken.None on purpose: the entry is shared across callers/runs, so a single caller's
        // cancellation must not poison the cached read/parse for everyone else.
        var text = new Lazy<Task<string>>(() => fs.ReadAllTextAsync(filePath).AsTask());
        var syntax = new Lazy<Task<SyntaxTree>>(async () =>
            CSharpSyntaxTree.ParseText(await text.Value, path: filePath));
        var xml = new Lazy<Task<XDocument>>(async () => XDocument.Parse(await text.Value));
        return new Entry { Mtime = mtime, Text = text, Syntax = syntax, Xml = xml };
    }

    public ValueTask<string> GetTextAsync(string filePath, CancellationToken ct = default)
    {
        var entry = GetEntry(filePath);
        if (entry.Text.IsValueCreated) Interlocked.Increment(ref _textHits);
        else Interlocked.Increment(ref _textMisses);
        return new ValueTask<string>(entry.Text.Value);
    }

    public ValueTask<SyntaxTree> GetSyntaxTreeAsync(string filePath, CancellationToken ct = default)
    {
        var entry = GetEntry(filePath);
        if (entry.Syntax.IsValueCreated) Interlocked.Increment(ref _syntaxHits);
        else Interlocked.Increment(ref _syntaxMisses);
        return new ValueTask<SyntaxTree>(entry.Syntax.Value);
    }

    public ValueTask<XDocument> GetXmlAsync(string filePath, CancellationToken ct = default)
        => new(GetEntry(filePath).Xml.Value);

    /// <summary>Returns cache hit/miss statistics.</summary>
    public CacheStats GetStats() => new(_textHits, _textMisses, _syntaxHits, _syntaxMisses);
}
