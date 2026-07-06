using System.Collections.Concurrent;

namespace DevContext.Server.Sessions;

public sealed class AnalysisSessionManager : IAnalysisSessionManager, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _repoToHandle = new(StringComparer.Ordinal);
    private readonly EngineHostCache _hostCache;
    private readonly ServerOptions _options;
    private readonly IEngineRunner _runner;

    public AnalysisSessionManager(IEngineRunner runner, EngineHostCache hostCache, ServerOptions options)
    {
        _runner = runner;
        _hostCache = hostCache;
        _options = options;
    }

    public async Task<AnalysisSession> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct)
    {
        await EvictIfNeededAsync().ConfigureAwait(false);

        var engine = await _runner.AnalyzeAsync(spec, progress, ct).ConfigureAwait(false);
        var handle = Guid.NewGuid().ToString("N");

        // M3.1 — resolve repo path + HEAD for session keying
        var repoPath = ResolveRepoPath(spec.Path);
        var commitSha = ResolveCommitSha(repoPath);

        var session = new AnalysisSession(handle, engine)
        {
            RepoPath = repoPath,
            CommitSha = commitSha,
        };
        _sessions[handle] = new SessionEntry { Session = session, LastAccess = DateTime.UtcNow };

        // M3.1 — index by repo+HEAD for server-of-record lookup
        var repoKey = RepoKey(repoPath, commitSha);
        _repoToHandle[repoKey] = handle;

        return session;
    }

    public AnalysisSession? Get(string handle)
    {
        if (!_sessions.TryGetValue(handle, out var entry)) return null;
        entry.LastAccess = DateTime.UtcNow;
        entry.Session.LastActivity = DateTime.UtcNow;
        entry.Session.CallCount++;
        return entry.Session;
    }

    public AnalysisSession? TryGetByRepo(string repoPath, string commitSha)
    {
        var repoKey = RepoKey(repoPath, commitSha);
        if (!_repoToHandle.TryGetValue(repoKey, out var handle)) return null;
        return Get(handle);
    }

    public IReadOnlyList<AnalysisSession> ListSessions()
    {
        return _sessions.Values
            .OrderByDescending(e => e.LastAccess)
            .Select(e => e.Session)
            .ToList();
    }

    public async Task<bool> CloseSessionAsync(string handle)
    {
        if (!_sessions.TryRemove(handle, out var entry)) return false;

        // G3 — only remove repo index entry if it still points to this handle
        var repoKey = RepoKey(entry.Session.RepoPath, entry.Session.CommitSha);
        if (_repoToHandle.TryGetValue(repoKey, out var current) && current == handle)
            _repoToHandle.TryRemove(repoKey, out _);

        await entry.Session.DisposeAsync().ConfigureAwait(false);
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (_, entry) in _sessions)
            await entry.Session.DisposeAsync().ConfigureAwait(false);
        _sessions.Clear();
        _repoToHandle.Clear();
        await _hostCache.DisposeAsync().ConfigureAwait(false);
    }

    private async Task EvictIfNeededAsync()
    {
        var capacity = _options.SessionCapacity;
        if (_sessions.Count < capacity) return;

        var idleTimeout = _options.SessionIdleTimeout;
        var now = DateTime.UtcNow;

        var expired = new List<string>();
        foreach (var (key, entry) in _sessions)
        {
            if (now - entry.LastAccess > idleTimeout)
                expired.Add(key);
        }

        foreach (var key in expired)
        {
            if (_sessions.TryRemove(key, out var entry))
            {
                var repoKey = RepoKey(entry.Session.RepoPath, entry.Session.CommitSha);
                _repoToHandle.TryRemove(repoKey, out _);
                await entry.Session.DisposeAsync().ConfigureAwait(false);
            }
        }

        if (_sessions.Count >= capacity)
        {
            var lru = _sessions.Values.OrderBy(e => e.LastAccess).First();
            if (_sessions.TryRemove(lru.Session.Handle, out var lruEntry))
            {
                var repoKey = RepoKey(lruEntry.Session.RepoPath, lruEntry.Session.CommitSha);
                _repoToHandle.TryRemove(repoKey, out _);
                await lruEntry.Session.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private static string RepoKey(string repoPath, string commitSha)
        => $"{repoPath}@{commitSha}";

    private static string ResolveRepoPath(string path)
    {
        try
        {
            var full = Path.GetFullPath(path);
            if (Directory.Exists(full)) return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (File.Exists(full)) return Path.GetDirectoryName(full)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? full;
        }
        catch { }
        return path;
    }

    private static string ResolveCommitSha(string repoPath)
    {
        try
        {
            var dir = repoPath;
            while (dir is not null && !Directory.Exists(Path.Combine(dir, ".git")))
                dir = Path.GetDirectoryName(dir);
            if (dir is null) return "";

            var headFile = Path.Combine(dir, ".git", "HEAD");
            if (!File.Exists(headFile)) return "";

            var content = File.ReadAllText(headFile).Trim();
            if (content.StartsWith("ref:", StringComparison.Ordinal))
            {
                var refPath = content[5..].Trim();
                var refFile = Path.Combine(dir, ".git", refPath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(refFile))
                    return File.ReadAllText(refFile).Trim();
            }
            return content;
        }
        catch
        {
            return "";
        }
    }

    private sealed class SessionEntry
    {
        public required AnalysisSession Session { get; init; }
        public DateTime LastAccess { get; set; }
    }
}