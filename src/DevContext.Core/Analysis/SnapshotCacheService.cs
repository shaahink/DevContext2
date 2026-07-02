using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevContext.Core.Analysis;

public static class SnapshotCacheRoot
{
    public static string DefaultPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext", "cache");

    public static string EnsureDirectory()
    {
        var dir = DefaultPath;
        Directory.CreateDirectory(dir);
        return dir;
    }
}

public sealed class SnapshotCacheService
{
    private readonly string _cacheRoot;
    private readonly int _maxVersionsPerRepo = 10;
    private readonly long _maxTotalBytes = 2L * 1024 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        IncludeFields = true,
    };

    public SnapshotCacheService(string? cacheRoot = null)
    {
        _cacheRoot = cacheRoot ?? SnapshotCacheRoot.EnsureDirectory();
    }

    public static (string RepoKey, string VersionKey) ComputeKeys(string rootPath)
    {
        var normalized = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var repoKey = HashString(normalized);
        var versionKey = ComputeGitHead(normalized) ?? $"manifest-{HashManifest(normalized)}";
        return (repoKey, versionKey);
    }

    public string GetSnapshotPath(string repoKey, string versionKey)
    {
        var dir = Path.Combine(_cacheRoot, repoKey);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"{versionKey}.snap.json.gz");
    }

    public async Task<bool> SaveAsync(string repoKey, string versionKey, object data, CancellationToken ct)
    {
        var path = GetSnapshotPath(repoKey, versionKey);
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await using var gz = new GZipStream(fs, CompressionLevel.Fastest);
            await using var sw = new StreamWriter(gz, Encoding.UTF8);
            await sw.WriteAsync(json);
            await sw.FlushAsync(ct);

            UpdateMeta(repoKey, versionKey);
            EvictIfNeeded(repoKey);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<T?> TryLoadAsync<T>(string repoKey, string versionKey, CancellationToken ct) where T : class
    {
        var path = GetSnapshotPath(repoKey, versionKey);
        if (!File.Exists(path)) return null;
        try
        {
            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            await using var gz = new GZipStream(fs, CompressionMode.Decompress);
            using var sr = new StreamReader(gz, Encoding.UTF8);
            var json = await sr.ReadToEndAsync(ct);
            var data = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (data is not null)
                TouchMeta(repoKey);
            return data;
        }
        catch
        {
            return null;
        }
    }

    public bool Exists(string repoKey, string versionKey)
        => File.Exists(GetSnapshotPath(repoKey, versionKey));

    public CacheInfo[] ListCached()
    {
        if (!Directory.Exists(_cacheRoot)) return [];
        var list = new List<CacheInfo>();
        foreach (var repoDir in Directory.GetDirectories(_cacheRoot))
        {
            var repoKey = Path.GetFileName(repoDir);
            var metaPath = Path.Combine(repoDir, "meta.json");
            var (label, versionCount, lastUsed, totalBytes) = ("", 0, DateTime.MinValue, 0L);
            if (File.Exists(metaPath))
            {
                try
                {
                    var meta = JsonSerializer.Deserialize<CacheMeta>(File.ReadAllText(metaPath));
                    if (meta is not null)
                    {
                        label = meta.Label ?? repoKey[..Math.Min(8, repoKey.Length)];
                        versionCount = meta.Versions?.Length ?? 0;
                        lastUsed = meta.LastUsed;
                    }
                }
                catch { }
            }
            foreach (var f in Directory.GetFiles(repoDir, "*.snap.json.gz"))
                totalBytes += new FileInfo(f).Length;
            if (versionCount > 0)
                list.Add(new CacheInfo(repoKey, label, versionCount, totalBytes, lastUsed));
        }
        return [.. list.OrderByDescending(c => c.LastUsed)];
    }

    public void Clear(string? repoKey = null)
    {
        if (repoKey is not null)
        {
            var dir = Path.Combine(_cacheRoot, repoKey);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            return;
        }
        if (Directory.Exists(_cacheRoot)) Directory.Delete(_cacheRoot, true);
    }

    public string CacheRoot => _cacheRoot;

    private static string? ComputeGitHead(string rootPath)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", "rev-parse HEAD")
            {
                WorkingDirectory = rootPath, RedirectStandardOutput = true,
                RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc is null) return null;
            var output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit(5000);
            return proc.ExitCode == 0 && output.Length > 0 ? output : null;
        }
        catch { return null; }
    }

    private static string HashManifest(string dir)
    {
        var sb = new StringBuilder();
        foreach (var f in Directory.GetFiles(dir, "*.sln", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories))
            .OrderBy(f => f))
        {
            var fi = new FileInfo(f);
            sb.Append(f).Append(fi.LastWriteTimeUtc.Ticks).Append(fi.Length);
        }
        return HashString(sb.ToString());
    }

    private static string HashString(string input)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    private void UpdateMeta(string repoKey, string versionKey)
    {
        var metaPath = Path.Combine(_cacheRoot, repoKey, "meta.json");
        CacheMeta meta;
        if (File.Exists(metaPath))
        {
            try { meta = JsonSerializer.Deserialize<CacheMeta>(File.ReadAllText(metaPath)) ?? new(); }
            catch { meta = new(); }
        }
        else meta = new();
        meta.LastUsed = DateTime.UtcNow;
        if (meta.Versions?.Contains(versionKey) != true)
            meta.Versions = (meta.Versions ?? []).Append(versionKey).ToArray();
        File.WriteAllText(metaPath, JsonSerializer.Serialize(meta));
    }

    private void TouchMeta(string repoKey)
    {
        var path = Path.Combine(_cacheRoot, repoKey, "meta.json");
        if (!File.Exists(path)) return;
        try
        {
            var meta = JsonSerializer.Deserialize<CacheMeta>(File.ReadAllText(path));
            if (meta is not null) { meta.LastUsed = DateTime.UtcNow; File.WriteAllText(path, JsonSerializer.Serialize(meta)); }
        }
        catch { }
    }

    private void EvictIfNeeded(string repoKey)
    {
        var dir = Path.Combine(_cacheRoot, repoKey);
        if (!Directory.Exists(dir)) return;
        var snaps = Directory.GetFiles(dir, "*.snap.json.gz")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();
        if (snaps.Count > _maxVersionsPerRepo)
            foreach (var old in snaps.Skip(_maxVersionsPerRepo))
                try { File.Delete(old.FullName); } catch { }

        long totalSize = 0;
        foreach (var rdir in Directory.GetDirectories(_cacheRoot))
            foreach (var f in Directory.GetFiles(rdir, "*.snap.json.gz"))
                totalSize += new FileInfo(f).Length;
        if (totalSize <= _maxTotalBytes) return;
        foreach (var rdir in GetDirectoriesByLastUsed())
            if (totalSize <= _maxTotalBytes) break;
            else
                foreach (var old in Directory.GetFiles(rdir, "*.snap.json.gz")
                    .Select(f => new FileInfo(f)).OrderBy(f => f.LastWriteTimeUtc))
                {
                    var sz = new FileInfo(old.FullName).Length;
                    File.Delete(old.FullName);
                    totalSize -= sz;
                    if (totalSize <= _maxTotalBytes) break;
                }
    }

    private IEnumerable<string> GetDirectoriesByLastUsed()
    {
        return Directory.GetDirectories(_cacheRoot).Select(d =>
        {
            var mp = Path.Combine(d, "meta.json");
            if (!File.Exists(mp)) return (Dir: d, LastUsed: DateTime.MinValue);
            try { return (Dir: d, LastUsed: JsonSerializer.Deserialize<CacheMeta>(File.ReadAllText(mp))?.LastUsed ?? DateTime.MinValue); }
            catch { return (Dir: d, LastUsed: DateTime.MinValue); }
        }).OrderBy(x => x.LastUsed).Select(x => x.Dir);
    }

    private sealed class CacheMeta
    {
        public string? Label { get; set; }
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        public string[]? Versions { get; set; }
    }
}

public sealed record CacheInfo(
    string RepoKey,
    string Label,
    int VersionCount,
    long TotalBytes,
    DateTime LastUsed
);
