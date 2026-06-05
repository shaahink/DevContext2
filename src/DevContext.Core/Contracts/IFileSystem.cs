namespace DevContext.Core.Contracts;

/// <summary>Abstraction over the file system to enable testing with fake implementations.</summary>
public interface IFileSystem
{
    /// <summary>Reads all text content from a file asynchronously.</summary>
    ValueTask<string> ReadAllTextAsync(string path, CancellationToken ct = default);
    /// <summary>Enumerates files matching a pattern under the given root directory.</summary>
    IAsyncEnumerable<string> EnumerateFilesAsync(
        string root, string pattern, SearchOption option, CancellationToken ct = default);
    /// <summary>Returns true if the specified file exists.</summary>
    bool FileExists(string path);
    /// <summary>Returns true if the specified directory exists.</summary>
    bool DirectoryExists(string path);
    /// <summary>Computes a relative path from one path to another.</summary>
    string GetRelativePath(string relativeTo, string path);
    /// <summary>Gets the absolute path for a given path.</summary>
    string GetFullPath(string path);
    /// <summary>Gets the parent directory of a path, or null if none.</summary>
    string? GetDirectoryName(string path);
    /// <summary>Enumerates directories matching a pattern under the given root.</summary>
    IEnumerable<string> EnumerateDirectories(string root, string pattern, SearchOption option);
}
