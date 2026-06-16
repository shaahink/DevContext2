using System.Collections.Concurrent;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Analysis;

/// <summary>Real implementation of <see cref="IAnalysisCache"/> that caches file reads, syntax trees, and XML documents.</summary>
public sealed class AnalysisCache : IAnalysisCache
{
    private readonly IFileSystem _fs;
    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> _textCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Lazy<Task<SyntaxTree>>> _syntaxCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Lazy<Task<XDocument>>> _xmlCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _knownPaths = new(StringComparer.Ordinal);

    private int _textHits;
    private int _textMisses;
    private int _syntaxHits;
    private int _syntaxMisses;
    private int _xmlHits;
    private int _xmlMisses;

    /// <summary>Creates an analysis cache backed by the given file system.</summary>
    public AnalysisCache(IFileSystem fs)
    {
        _fs = fs;
    }

    /// <summary>Gets the list of all registered file paths known to the cache.</summary>
    public IReadOnlyList<string> KnownFilePaths => _knownPaths.Keys.ToList();

    /// <summary>Registers a file path so it appears in <see cref="KnownFilePaths"/>.</summary>
    public void RegisterPath(string filePath)
    {
        _knownPaths.TryAdd(filePath, 0);
    }

    public ValueTask<string> GetTextAsync(string filePath, CancellationToken ct = default)
    {
        if (_textCache.TryGetValue(filePath, out var existing))
        {
            Interlocked.Increment(ref _textHits);
            return new ValueTask<string>(existing.Value);
        }

        Interlocked.Increment(ref _textMisses);
        var task = _textCache.GetOrAdd(filePath, static (path, state) => new Lazy<Task<string>>(
            () => state._fs.ReadAllTextAsync(path, state._ct).AsTask()), (_fs, _ct: ct));
        return new ValueTask<string>(task.Value);
    }

    public ValueTask<SyntaxTree> GetSyntaxTreeAsync(string filePath, CancellationToken ct = default)
    {
        if (_syntaxCache.TryGetValue(filePath, out var existing))
        {
            Interlocked.Increment(ref _syntaxHits);
            return new ValueTask<SyntaxTree>(existing.Value);
        }

        Interlocked.Increment(ref _syntaxMisses);
        var task = _syntaxCache.GetOrAdd(filePath, static (path, state) => new Lazy<Task<SyntaxTree>>(async () =>
        {
            var text = await state._self.GetTextAsync(path, state._ct).ConfigureAwait(false);
            return CSharpSyntaxTree.ParseText(text, path: path);
        }), (_self: this, _ct: ct));
        return new ValueTask<SyntaxTree>(task.Value);
    }

    public ValueTask<XDocument> GetXmlAsync(string filePath, CancellationToken ct = default)
    {
        if (_xmlCache.TryGetValue(filePath, out var existing))
        {
            Interlocked.Increment(ref _xmlHits);
            return new ValueTask<XDocument>(existing.Value);
        }

        Interlocked.Increment(ref _xmlMisses);
        var task = _xmlCache.GetOrAdd(filePath, static (path, state) => new Lazy<Task<XDocument>>(async () =>
        {
            var text = await state._self.GetTextAsync(path, state._ct).ConfigureAwait(false);
            return XDocument.Parse(text);
        }), (_self: this, _ct: ct));
        return new ValueTask<XDocument>(task.Value);
    }

    /// <summary>Returns cache hit/miss statistics.</summary>
    public CacheStats GetStats() => new(_textHits, _textMisses, _syntaxHits, _syntaxMisses);
}
