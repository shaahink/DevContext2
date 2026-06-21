namespace DevContext.Core.IO;

/// <summary>Real implementation of <see cref="IFileSystem"/> that delegates to System.IO.</summary>
public sealed class RealFileSystem : IFileSystem
{
    public async ValueTask<string> ReadAllTextAsync(string path, CancellationToken ct = default)
        => await File.ReadAllTextAsync(path, ct);

    public async IAsyncEnumerable<string> EnumerateFilesAsync(
        string root, string pattern, SearchOption option, [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var file in Directory.EnumerateFiles(root, pattern, option))
        {
            ct.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public DateTime GetLastWriteTimeUtc(string path) => File.GetLastWriteTimeUtc(path);
    public string GetRelativePath(string relativeTo, string path)
        => Path.GetRelativePath(relativeTo, path);
    public string GetFullPath(string path) => Path.GetFullPath(path);
    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    public IEnumerable<string> EnumerateDirectories(string root, string pattern, SearchOption option)
        => Directory.EnumerateDirectories(root, pattern, option);
}
