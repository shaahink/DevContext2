using System.Collections.Concurrent;

namespace DevContext.Core.IO;

/// <summary>In-memory implementation of <see cref="IFileSystem"/> for testing, backed by concurrent dictionaries.
/// Paths are canonicalized to '/' on EVERY platform (H1): tests write Windows-style (`C:\x` or `src/x`)
/// paths, consumers call System.IO.Path over what this fake returns, and '/' is the one form both
/// Windows and Unix System.IO split correctly — a hardcoded '\' canonical made every fake-backed test
/// fail off-Windows, while a platform-dependent canonical would make expectations OS-flavored. This
/// way the fake's world (and thus rendered output and goldens) is byte-identical across OSes.</summary>
public sealed class FakeFileSystem : IFileSystem
{
    private const char Sep = '/';
    private readonly ConcurrentDictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DateTime> _mtimes = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _dirs = new(StringComparer.OrdinalIgnoreCase) { "" };
    private static readonly DateTime Epoch = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private long _seq;

    private static string Norm(string path) => path.Replace('\\', Sep);

    /// <summary>Adds a file with the given path and content to the fake file system. Re-adding a path
    /// bumps its last-write time monotonically, so tests can simulate an edit for cache invalidation.</summary>
    public void AddFile(string path, string content)
    {
        var full = Norm(path);
        _files[full] = content;
        _mtimes[full] = Epoch.AddTicks(Interlocked.Increment(ref _seq));
        var dir = DirName(full);
        while (!string.IsNullOrEmpty(dir))
        {
            _dirs.Add(dir);
            dir = DirName(dir);
        }
    }

    /// <summary>Directory-name over the canonical form, platform-independent w.r.t. Windows drive
    /// roots: on Unix, System.IO.Path reads "C:" as a plain file-name segment and would loop forever
    /// on the drive-rooted paths tests use, so the walk is done by hand.</summary>
    private static string? DirName(string path)
    {
        var i = path.LastIndexOf(Sep);
        if (i < 0) return null;
        if (i == 0) return null;                              // "/foo" → root reached
        if (i == path.Length - 1) return DirName(path[..i]);  // trailing separator
        if (path[i - 1] == ':') return null;                  // "C:\foo" → drive root reached
        return path[..i];
    }

    public ValueTask<string> ReadAllTextAsync(string path, CancellationToken ct = default)
        => new(_files.TryGetValue(Norm(path), out var content)
            ? content : throw new FileNotFoundException($"File not found: {path}"));

    public async IAsyncEnumerable<string> EnumerateFilesAsync(
        string root, string pattern, SearchOption option, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var rootNorm = Norm(root).TrimEnd(Sep);
        var isAll = pattern is "*" or "*.*";
        foreach (var kvp in _files)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(rootNorm) || kvp.Key.StartsWith(rootNorm + Sep, StringComparison.OrdinalIgnoreCase))
            {
                if (isAll || MatchPattern(FileName(kvp.Key), pattern))
                    yield return kvp.Key;
            }
        }
    }

    public bool FileExists(string path) => _files.ContainsKey(Norm(path));
    public bool DirectoryExists(string path)
    {
        var norm = Norm(path).TrimEnd(Sep);
        return _dirs.Contains(norm) || norm == "";
    }

    public DateTime GetLastWriteTimeUtc(string path)
        => _mtimes.TryGetValue(Norm(path), out var t) ? t : DateTime.MinValue;

    public string GetRelativePath(string relativeTo, string path)
    {
        var r = Norm(relativeTo).TrimEnd(Sep);
        var p = Norm(path);
        if (p.StartsWith(r + Sep, StringComparison.OrdinalIgnoreCase))
            return p[(r.Length + 1)..];
        return p;
    }

    public string GetFullPath(string path) => Norm(path);

    public string? GetDirectoryName(string path)
    {
        var dir = DirName(Norm(path));
        return string.IsNullOrEmpty(dir) ? null : dir;
    }

    public IEnumerable<string> EnumerateDirectories(string root, string pattern, SearchOption option)
    {
        var rootNorm = Norm(root).TrimEnd(Sep);
        return _dirs.Where(d =>
            d.StartsWith(rootNorm + Sep, StringComparison.OrdinalIgnoreCase) &&
            d.Length > rootNorm.Length);
    }

    private static string FileName(string path)
    {
        var i = path.LastIndexOf(Sep);
        return i < 0 ? path : path[(i + 1)..];
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
