using System.Collections.Concurrent;

namespace DevContext.Core.Utilities;

public sealed class FakeFileSystem : IFileSystem
{
    private readonly ConcurrentDictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _dirs = new(StringComparer.OrdinalIgnoreCase) { "" };

    public void AddFile(string path, string content)
    {
        var full = path.Replace('/', '\\');
        _files[full] = content;
        var dir = Path.GetDirectoryName(full);
        while (!string.IsNullOrEmpty(dir))
        {
            _dirs.Add(dir);
            dir = Path.GetDirectoryName(dir);
        }
    }

    public ValueTask<string> ReadAllTextAsync(string path, CancellationToken ct = default)
        => new(_files.TryGetValue(path.Replace('/', '\\'), out var content)
            ? content : throw new FileNotFoundException($"File not found: {path}"));

    public async IAsyncEnumerable<string> EnumerateFilesAsync(
        string root, string pattern, SearchOption option, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var rootNorm = root.Replace('/', '\\').TrimEnd('\\');
        var isAll = pattern is "*" or "*.*";
        foreach (var kvp in _files)
        {
            ct.ThrowIfCancellationRequested();
            if (kvp.Key.StartsWith(rootNorm + "\\", StringComparison.OrdinalIgnoreCase))
            {
                if (isAll || MatchPattern(Path.GetFileName(kvp.Key), pattern))
                    yield return kvp.Key;
            }
        }
    }

    public bool FileExists(string path) => _files.ContainsKey(path.Replace('/', '\\'));
    public bool DirectoryExists(string path)
    {
        var norm = path.Replace('/', '\\').TrimEnd('\\');
        return _dirs.Contains(norm) || norm == "";
    }

    public string GetRelativePath(string relativeTo, string path)
    {
        var r = relativeTo.Replace('/', '\\').TrimEnd('\\');
        var p = path.Replace('/', '\\');
        if (p.StartsWith(r + "\\", StringComparison.OrdinalIgnoreCase))
            return p[(r.Length + 1)..];
        return p;
    }

    public string GetFullPath(string path) => path.Replace('/', '\\');

    public string? GetDirectoryName(string path)
    {
        var dir = Path.GetDirectoryName(path.Replace('/', '\\'));
        return string.IsNullOrEmpty(dir) ? null : dir;
    }

    public IEnumerable<string> EnumerateDirectories(string root, string pattern, SearchOption option)
    {
        var rootNorm = root.Replace('/', '\\').TrimEnd('\\');
        return _dirs.Where(d =>
            d.StartsWith(rootNorm + "\\", StringComparison.OrdinalIgnoreCase) &&
            d.Length > rootNorm.Length);
    }

    private static bool MatchPattern(string name, string pattern)
    {
        if (pattern == "*" || pattern == "*.*") return true;
        if (pattern.StartsWith("*") && pattern.EndsWith("*"))
            return name.Contains(pattern[1..^1], StringComparison.OrdinalIgnoreCase);
        if (pattern.StartsWith("*"))
            return name.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase);
        if (pattern.EndsWith("*"))
            return name.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
        return string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase);
    }
}
