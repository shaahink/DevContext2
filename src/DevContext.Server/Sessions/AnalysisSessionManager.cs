using System.Collections.Concurrent;

namespace DevContext.Server.Sessions;

public sealed class AnalysisSessionManager : IAnalysisSessionManager, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new(StringComparer.Ordinal);
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
        var session = new AnalysisSession(handle, engine);
        _sessions[handle] = new SessionEntry { Session = session, LastAccess = DateTime.UtcNow };
        return session;
    }

    public AnalysisSession? Get(string handle)
    {
        if (!_sessions.TryGetValue(handle, out var entry)) return null;
        entry.LastAccess = DateTime.UtcNow;
        return entry.Session;
    }

    public async Task<bool> CloseSessionAsync(string handle)
    {
        if (!_sessions.TryRemove(handle, out var entry)) return false;
        await entry.Session.DisposeAsync().ConfigureAwait(false);
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (_, entry) in _sessions)
            await entry.Session.DisposeAsync().ConfigureAwait(false);
        _sessions.Clear();
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
                await entry.Session.DisposeAsync().ConfigureAwait(false);
        }

        if (_sessions.Count >= capacity)
        {
            var lru = _sessions.Values.OrderBy(e => e.LastAccess).First();
            if (_sessions.TryRemove(lru.Session.Handle, out var lruEntry))
                await lruEntry.Session.DisposeAsync().ConfigureAwait(false);
        }
    }

    private sealed class SessionEntry
    {
        public required AnalysisSession Session { get; init; }
        public DateTime LastAccess { get; set; }
    }
}
