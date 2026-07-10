using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevContext.Core.Services;

public sealed record CloneEntry(
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("repo")] string Repo,
    [property: JsonPropertyName("ref")] string? Ref,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("head")] string? Head,
    [property: JsonPropertyName("clonedAt")] DateTime ClonedAt);

public sealed class CloneRegistry
{
    private readonly string _filePath;
    private readonly ReaderWriterLockSlim _rw = new();
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly string RegistryDir =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DevContext", "repos");

    public CloneRegistry() : this(System.IO.Path.Combine(RegistryDir, "registry.json")) { }

    internal CloneRegistry(string filePath)
    {
        _filePath = filePath;
    }

    public CloneEntry? Get(string owner, string repo, string? refName)
    {
        _rw.EnterReadLock();
        try
        {
            return LoadEntries().FirstOrDefault(e =>
                e.Owner.Equals(owner, StringComparison.OrdinalIgnoreCase)
                && e.Repo.Equals(repo, StringComparison.OrdinalIgnoreCase)
                && string.Equals(e.Ref, refName, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _rw.ExitReadLock();
        }
    }

    public void Set(CloneEntry entry)
    {
        _rw.EnterWriteLock();
        try
        {
            var entries = LoadEntries();
            var idx = entries.FindIndex(e =>
                e.Owner.Equals(entry.Owner, StringComparison.OrdinalIgnoreCase)
                && e.Repo.Equals(entry.Repo, StringComparison.OrdinalIgnoreCase)
                && string.Equals(e.Ref, entry.Ref, StringComparison.OrdinalIgnoreCase));

            if (idx >= 0)
                entries[idx] = entry;
            else
                entries.Add(entry);

            SaveEntries(entries);
        }
        finally
        {
            _rw.ExitWriteLock();
        }
    }

    public bool Remove(string owner, string repo, string? refName)
    {
        _rw.EnterWriteLock();
        try
        {
            var entries = LoadEntries();
            var removed = entries.RemoveAll(e =>
                e.Owner.Equals(owner, StringComparison.OrdinalIgnoreCase)
                && e.Repo.Equals(repo, StringComparison.OrdinalIgnoreCase)
                && string.Equals(e.Ref, refName, StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
                SaveEntries(entries);

            return removed > 0;
        }
        finally
        {
            _rw.ExitWriteLock();
        }
    }

    public IReadOnlyList<CloneEntry> List()
    {
        _rw.EnterReadLock();
        try
        {
            return LoadEntries().AsReadOnly();
        }
        finally
        {
            _rw.ExitReadLock();
        }
    }

    public void Clear()
    {
        _rw.EnterWriteLock();
        try
        {
            SaveEntries([]);
        }
        finally
        {
            _rw.ExitWriteLock();
        }
    }

    private List<CloneEntry> LoadEntries()
    {
        if (!File.Exists(_filePath))
            return [];

        for (var retry = 0; retry < 3; retry++)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Open,
                    FileAccess.Read, FileShare.Read);
                var json = new StreamReader(stream).ReadToEnd();
                return JsonSerializer.Deserialize<List<CloneEntry>>(json, _json) ?? [];
            }
            catch (IOException)
            {
                if (retry == 2) return [];
                Thread.Sleep(50 * (retry + 1));
            }
        }

        return [];
    }

    private void SaveEntries(List<CloneEntry> entries)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(entries, _json);

        for (var retry = 0; retry < 3; retry++)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Create,
                    FileAccess.Write, FileShare.None);
                using var writer = new StreamWriter(stream);
                writer.Write(json);
                return;
            }
            catch (IOException)
            {
                if (retry == 2) throw;
                Thread.Sleep(50 * (retry + 1));
            }
        }
    }
}
