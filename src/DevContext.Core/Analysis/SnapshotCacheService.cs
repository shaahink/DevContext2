using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using DevContext.Core.Pipeline;

namespace DevContext.Core.Analysis;

/// <summary>Increment this when the AnalysisSnapshot schema changes incompatibly.
/// Stale snapshots with a different version are rejected on load.</summary>
public static class SnapshotSchema
{
    /// <summary>v2 (J2, Prism D2.0b): payload is <see cref="PersistedSnapshot"/> — v1 never
    /// produced a valid file (the save always threw and was swallowed), so no migration exists.
    /// v3 (C1, Prism D2): razor @code virtualization changed analysis output for Blazor repos —
    /// a v2 snapshot of an UNCHANGED repo would render the pre-C1 map. Discipline until J2 grows an
    /// engine-version key: bump this whenever a change alters persisted analysis semantics.
    /// v4 (C5+J1, Prism D2): Resolves edges carry RegistrationSites (a v3 snapshot would trace with
    /// pre-C5 arbitrary DI provenance) and the model carries ExtractionFailures (J1 health rows).
    /// v5 (I1 fix at D2 close): orphans insight coverage-gated + library-exempt — a v4 snapshot can
    /// serve a persisted dead-code claim the fixed engine would never make (wolverine P7 catch).</summary>
    public const int Version = 5;
}

/// <summary>Outcome of a snapshot save. The save is best-effort but NEVER silent (J2): a failure
/// carries the reason so callers surface it instead of quietly shipping a cache that never fills.</summary>
public sealed record SnapshotSaveResult(bool Success, string? Error)
{
    public static SnapshotSaveResult Ok { get; } = new(true, null);
    public static SnapshotSaveResult Fail(string error) => new(false, error);
}

public static class SnapshotCacheRoot
{
    /// <summary>J2 — <c>DEVCONTEXT_CACHE_ROOT</c> overrides the cache location. Exists so test
    /// hosts (ServerTestFactory) and CI redirect writes away from the user's real cache — and so
    /// an unchanged-tree re-run of AnalyzeFlowTests can't cache-HIT into asserting on progress
    /// events that a hit never streams.</summary>
    public static string DefaultPath =>
        Environment.GetEnvironmentVariable("DEVCONTEXT_CACHE_ROOT") is { Length: > 0 } overridden
            ? overridden
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { SnapshotPersistence.AddDetectionPolymorphism },
        },
    };

    public SnapshotCacheService(string? cacheRoot = null)
    {
        _cacheRoot = cacheRoot ?? SnapshotCacheRoot.EnsureDirectory();
    }

    public static (string RepoKey, string VersionKey) ComputeKeys(string rootPath)
        => ComputeKeys(rootPath, null);

    /// <summary>D3.1 — the version key carries the analysis FLAVOR when it deviates from the
    /// default full-fidelity run: a <c>--fast</c>/<c>--lite</c>/<c>--no-roslyn</c>/custom-excludes
    /// analysis extracts genuinely less, and saving it under the default key would let a later full
    /// run HIT a degraded snapshot. Default-flavor runs keep the unsuffixed key, so CLI analyze,
    /// CLI query, and the server all share one slot per (repo, tree).</summary>
    public static (string RepoKey, string VersionKey) ComputeKeys(string rootPath, ExtractionOptions? options)
    {
        var normalized = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var repoKey = HashString(normalized);
        var head = GitHeadReader.Read(normalized);
        string versionKey;
        if (head is null)
        {
            versionKey = $"manifest-{HashManifest(normalized)}";
        }
        else
        {
            // J2 — a dirty working tree must not collide with the clean-HEAD snapshot, or every
            // uncommitted edit would render yesterday's map as "from cache". The key gains a
            // fingerprint over git's changed-file list (path + mtime + length per file).
            var dirty = GitHeadReader.ReadDirtyFingerprint(normalized);
            versionKey = dirty is null ? head : $"{head}-dirty-{dirty}";
        }
        if (options is not null && ComputeFlavorSuffix(options) is { } flavor)
            versionKey = $"{versionKey}-opt-{flavor}";
        return (repoKey, versionKey);
    }

    /// <summary>Null for the default full-fidelity flavor (the shared slot); otherwise a short hash
    /// over every option that changes what extraction PRODUCES. Render-only options (format, budget,
    /// profile with full graph on, provenance, focus) deliberately don't key — the persisted snapshot
    /// is render-complete and load sites re-render per request.</summary>
    private static string? ComputeFlavorSuffix(ExtractionOptions o)
    {
        var defaultFlavor = o is { AllowRoslyn: true, BuildFullGraph: true, Fast: false, ExcludeExtractors.Length: 0 }
            && o.ExcludePatterns.SequenceEqual(ExtractionOptions.DefaultExcludePatterns);
        if (defaultFlavor) return null;
        var canonical = $"roslyn:{o.AllowRoslyn}|graph:{o.BuildFullGraph}|fast:{o.Fast}"
            + $"|excl:{string.Join(",", o.ExcludeExtractors.Sort(StringComparer.Ordinal))}"
            + $"|pat:{string.Join(",", o.ExcludePatterns.Sort(StringComparer.Ordinal))}";
        return HashString(canonical)[..12];
    }

    /// <summary>Pure path computation — creates nothing. (The pre-J2 form did CreateDirectory here,
    /// so even a read-only <see cref="Exists"/> probe littered empty cache dirs — the audit's
    /// "all cache dirs 0 bytes".)</summary>
    public string GetSnapshotPath(string repoKey, string versionKey)
        => Path.Combine(_cacheRoot, repoKey, $"{versionKey}.snap.json.gz");

    public async Task<SnapshotSaveResult> SaveAsync(string repoKey, string versionKey, AnalysisSnapshot snapshot, CancellationToken ct)
    {
        try
        {
            if (snapshot.IsDryRun)
                return SnapshotSaveResult.Fail("dry-run analyses are not cached");
            Directory.CreateDirectory(Path.Combine(_cacheRoot, repoKey));
            var path = GetSnapshotPath(repoKey, versionKey);
            var envelope = new SnapshotEnvelope
            {
                SchemaVersion = SnapshotSchema.Version,
                Payload = SnapshotPersistence.FromSnapshot(snapshot),
            };
            await using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            await using (var gz = new GZipStream(fs, CompressionLevel.Fastest))
            {
                await JsonSerializer.SerializeAsync(gz, envelope, JsonOptions, ct);
            }

            UpdateMeta(repoKey, versionKey);
            EvictIfNeeded(repoKey);
            return SnapshotSaveResult.Ok;
        }
        catch (Exception ex)
        {
            return SnapshotSaveResult.Fail($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    public async Task<AnalysisSnapshot?> TryLoadAsync(string repoKey, string versionKey, CancellationToken ct)
    {
        var path = GetSnapshotPath(repoKey, versionKey);
        if (!File.Exists(path)) return null;
        try
        {
            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            await using var gz = new GZipStream(fs, CompressionMode.Decompress);
            var envelope = await JsonSerializer.DeserializeAsync<SnapshotEnvelope>(gz, JsonOptions, ct);
            if (envelope is null) return null;
            if (envelope.SchemaVersion != SnapshotSchema.Version) return null;
            if (envelope.Payload is null) return null;
            var snapshot = SnapshotPersistence.ToSnapshot(envelope.Payload);
            TouchMeta(repoKey);
            return snapshot;
        }
        catch (Exception)
        {
            // A corrupt or schema-drifted snapshot is a MISS, not an error — the caller
            // re-analyzes and the fresh save overwrites the bad file.
            return null;
        }
    }

    private sealed record SnapshotEnvelope
    {
        public int SchemaVersion { get; init; }
        public PersistedSnapshot? Payload { get; init; }
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
                catch (Exception ex) { Pipeline.PipelineDiagnostics.Swallowed("SnapshotCache", "meta-read", ex); }
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
        catch (Exception ex) { Pipeline.PipelineDiagnostics.Swallowed("SnapshotCache", "meta-touch", ex); }
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
                try { File.Delete(old.FullName); } catch (Exception ex) { Pipeline.PipelineDiagnostics.Swallowed("SnapshotCache", "evict", ex); }

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
