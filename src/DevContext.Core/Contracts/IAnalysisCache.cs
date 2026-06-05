using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace DevContext.Core.Contracts;

public interface IAnalysisCache
{
    ValueTask<string> GetTextAsync(string filePath, CancellationToken ct = default);
    ValueTask<SyntaxTree> GetSyntaxTreeAsync(string filePath, CancellationToken ct = default);
    ValueTask<XDocument> GetXmlAsync(string filePath, CancellationToken ct = default);
    IReadOnlyList<string> KnownFilePaths { get; }
    void RegisterPath(string filePath);
}
