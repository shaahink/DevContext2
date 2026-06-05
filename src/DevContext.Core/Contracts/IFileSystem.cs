namespace DevContext.Core.Contracts;

public interface IFileSystem
{
    ValueTask<string> ReadAllTextAsync(string path, CancellationToken ct = default);
    IAsyncEnumerable<string> EnumerateFilesAsync(
        string root, string pattern, SearchOption option, CancellationToken ct = default);

    bool FileExists(string path);
    bool DirectoryExists(string path);
    string GetRelativePath(string relativeTo, string path);
    string GetFullPath(string path);
    string? GetDirectoryName(string path);
    IEnumerable<string> EnumerateDirectories(string root, string pattern, SearchOption option);
}
