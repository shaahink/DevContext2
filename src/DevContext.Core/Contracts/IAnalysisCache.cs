using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace DevContext.Core.Contracts;

/// <summary>Provides a caching layer over parsed source files, syntax trees, and XML documents.</summary>
public interface IAnalysisCache
{
    /// <summary>Retrieves the raw text content of a file, using the cache if available.</summary>
    ValueTask<string> GetTextAsync(string filePath, CancellationToken ct = default);
    /// <summary>Retrieves or parses a Roslyn <see cref="SyntaxTree"/> for the given file path.</summary>
    ValueTask<SyntaxTree> GetSyntaxTreeAsync(string filePath, CancellationToken ct = default);
    /// <summary>Retrieves or parses an <see cref="XDocument"/> for the given file path.</summary>
    ValueTask<XDocument> GetXmlAsync(string filePath, CancellationToken ct = default);
    /// <summary>Gets the list of all registered file paths known to the cache.</summary>
    IReadOnlyList<string> KnownFilePaths { get; }
    /// <summary>Registers a file path so it appears in <see cref="KnownFilePaths"/>.</summary>
    void RegisterPath(string filePath);
}
