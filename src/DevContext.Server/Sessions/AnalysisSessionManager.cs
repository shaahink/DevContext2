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

    public async Task<AnalysisOutcome> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct)
    {
        await EvictIfNeededAsync().ConfigureAwait(false);

        var repoPath = ResolveRepoPath(spec.Path);
        var commitSha = ResolveCommitSha(repoPath);

        // L5.1 — idempotent by repo+HEAD+scope. TryGetByRepo already routes through Get(),
        // which stamps LastAccess/LastActivity and increments CallCount exactly once;
        // don't repeat those mutations here or the reuse double-counts the call.
        // R3 D-D: the scope belongs in the key. Without it, asking for GitVersion's new-cli solution
        // was answered in 2ms with the src solution already in hand — idempotence turned into a
        // silent refusal, which is exactly what the snapshot cache's flavor suffix exists to prevent.
        var existing = TryGetByRepo(repoPath, commitSha, spec.Sln);
        if (existing is not null)
        {
            progress?.Report(new AnalysisProgress("cached", 100, "Reusing existing analysis for this repo"));
            return new AnalysisOutcome(existing, Cached: true);
        }

        var engine = await _runner.AnalyzeAsync(spec, progress, ct).ConfigureAwait(false);
        var handle = Guid.NewGuid().ToString("N");

        var session = new AnalysisSession(handle, engine)
        {
            RepoPath = repoPath,
            CommitSha = commitSha,
            Sln = spec.Sln,
        };
        _sessions[handle] = new SessionEntry { Session = session, LastAccess = DateTime.UtcNow };

        var repoKey = RepoKey(repoPath, commitSha, spec.Sln);
        _repoToHandle[repoKey] = handle;

        // A brand-new session is still a cached ANSWER when the runner rehydrated it from disk
        // instead of analyzing — the caller asked "did this take minutes", not "is this session new".
        return new AnalysisOutcome(session, engine.FromSnapshotCache);
    }

    public AnalysisSession? Get(string handle)
    {
        if (!_sessions.TryGetValue(handle, out var entry)) return null;
        entry.LastAccess = DateTime.UtcNow;
        entry.Session.LastActivity = DateTime.UtcNow;
        entry.Session.CallCount++;
        return entry.Session;
    }

    public AnalysisSession? TryGetByRepo(string repoPath, string commitSha, string? sln = null)
    {
        var repoKey = RepoKey(repoPath, commitSha, sln);
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
        var repoKey = RepoKey(entry.Session.RepoPath, entry.Session.CommitSha, entry.Session.Sln);
        if (_repoToHandle.TryGetValue(repoKey, out var current) && current == handle)
            _repoToHandle.TryRemove(repoKey, out _);

        await entry.Session.DisposeAsync().ConfigureAwait(false);
        await ReleaseHostIfOrphanedAsync(entry.Session).ConfigureAwait(false);
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
                var repoKey = RepoKey(entry.Session.RepoPath, entry.Session.CommitSha, entry.Session.Sln);
                _repoToHandle.TryRemove(repoKey, out _);
                await entry.Session.DisposeAsync().ConfigureAwait(false);
                await ReleaseHostIfOrphanedAsync(entry.Session).ConfigureAwait(false);
            }
        }

        if (_sessions.Count >= capacity)
        {
            var lru = _sessions.Values.OrderBy(e => e.LastAccess).First();
            if (_sessions.TryRemove(lru.Session.Handle, out var lruEntry))
            {
                var repoKey = RepoKey(lruEntry.Session.RepoPath, lruEntry.Session.CommitSha, lruEntry.Session.Sln);
                _repoToHandle.TryRemove(repoKey, out _);
                await lruEntry.Session.DisposeAsync().ConfigureAwait(false);
                await ReleaseHostIfOrphanedAsync(lruEntry.Session).ConfigureAwait(false);
            }
        }
    }

    /// <summary>D5.3 laden-server — a removed session takes its EngineHost with it when no other
    /// live session shares the root, so the host cache (which pins the repo's parsed trees) stays
    /// bounded by the session cap instead of growing per analyzed root for the server's lifetime.
    /// The host key is the ANALYZED root (<see cref="Core.Pipeline.AnalysisSnapshot.RootPath"/> =
    /// ProjectRootResolver's EffectiveRootPath), which can differ from the session's RepoPath.
    /// A concurrent analyze on the same root recreates the host via GetOrCreate — worst case a
    /// cold re-parse, never a wrong result.</summary>
    private async Task ReleaseHostIfOrphanedAsync(AnalysisSession closed)
    {
        var root = closed.Snapshot.RootPath;
        if (string.IsNullOrEmpty(root)) return;
        foreach (var (_, e) in _sessions)
        {
            if (string.Equals(e.Session.Snapshot.RootPath, root, StringComparison.OrdinalIgnoreCase))
                return;
        }
        await _hostCache.ReleaseAsync(root).ConfigureAwait(false);
    }

    /// <summary>Session identity: the tree, the commit, and the slice of it that was analyzed. The
    /// empty scope (the scorer's pick) is its own key rather than an alias of whatever it picked —
    /// naming a solution is a different request from letting the tool choose it.</summary>
    private static string RepoKey(string repoPath, string commitSha, string? sln)
        => $"{repoPath}@{commitSha}#{sln ?? ""}";

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

    /// <summary>R4 item 10 — session identity's notion of HEAD, now the SAME one the snapshot cache
    /// keys on (<see cref="Core.Analysis.GitHeadReader"/> = <c>git rev-parse HEAD</c>). The hand-rolled
    /// walk below is kept only for a machine with no git on PATH; as the primary reader it returned
    /// the EMPTY STRING for two ordinary layouts, measured on this repo's own second worktree:
    /// a git WORKTREE (<c>.git</c> is a FILE there, so <c>Directory.Exists</c> walks off the end of
    /// the path — and AGENTS.md tells agents to work from worktrees), and a packed ref (after
    /// <c>git gc</c> there is no <c>.git/refs/heads/&lt;branch&gt;</c> file to read).</summary>
    internal static string ResolveCommitSha(string repoPath)
    {
        if (Core.Analysis.GitHeadReader.Read(repoPath) is { Length: > 0 } head) return head;

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