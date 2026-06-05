using System.Collections.Concurrent;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DevContext.Core.Analysis;

public sealed class FakeAnalysisCache : IAnalysisCache
{
    private readonly IFileSystem _fs;
    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> _textCache = new();
    private readonly ConcurrentDictionary<string, Lazy<Task<SyntaxTree>>> _syntaxCache = new();
    private readonly ConcurrentDictionary<string, Lazy<Task<XDocument>>> _xmlCache = new();
    private readonly ConcurrentBag<string> _knownPaths = [];

    public FakeAnalysisCache(IFileSystem fs)
    {
        _fs = fs;
    }

    public IReadOnlyList<string> KnownFilePaths => _knownPaths.ToList();

    public void RegisterPath(string filePath) => _knownPaths.Add(filePath);

    public ValueTask<string> GetTextAsync(string filePath, CancellationToken ct = default)
    {
        var task = _textCache.GetOrAdd(filePath, _ => new Lazy<Task<string>>(
            () => _fs.ReadAllTextAsync(filePath, ct).AsTask()));
        return new ValueTask<string>(task.Value);
    }

    public ValueTask<SyntaxTree> GetSyntaxTreeAsync(string filePath, CancellationToken ct = default)
    {
        var task = _syntaxCache.GetOrAdd(filePath, _ => new Lazy<Task<SyntaxTree>>(async () =>
        {
            var text = await GetTextAsync(filePath, ct);
            return CSharpSyntaxTree.ParseText(text, path: filePath);
        }));
        return new ValueTask<SyntaxTree>(task.Value);
    }

    public ValueTask<XDocument> GetXmlAsync(string filePath, CancellationToken ct = default)
    {
        var task = _xmlCache.GetOrAdd(filePath, _ => new Lazy<Task<XDocument>>(async () =>
        {
            var text = await GetTextAsync(filePath, ct);
            return XDocument.Parse(text);
        }));
        return new ValueTask<XDocument>(task.Value);
    }
}
