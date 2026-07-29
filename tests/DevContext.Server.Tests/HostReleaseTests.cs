using DevContext.Core.Services;
using DevContext.Server.Sessions;

using Microsoft.Extensions.Logging;

namespace DevContext.Server.Tests;

/// <summary>D5.3 laden-server — an EngineHost pins every parsed tree of its repo, so the host
/// cache must stay bounded by the live sessions instead of accumulating one host per analyzed
/// root for the server's lifetime (the "unresponsive after ~36 analyses" class; only a restart
/// cured it because restart was the only host eviction).</summary>
public sealed class HostReleaseTests : IDisposable
{
    // Same J2 redirect as ServerTestFactory: never touch (or serve from) the user's real snapshot
    // cache. G5 s18 — handed to the runner rather than written to DEVCONTEXT_CACHE_ROOT; the
    // save-and-restore form could not survive a concurrent collection doing the same thing.
    private readonly string _cacheRoot = Path.Combine(
        Path.GetTempPath(), "devcontext-server-tests-cache", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { if (Directory.Exists(_cacheRoot)) Directory.Delete(_cacheRoot, true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private (AnalysisSessionManager Manager, EngineHostCache Hosts) CreateManager(int capacity)
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var hosts = new EngineHostCache(loggerFactory);
        var options = new ServerOptions { SessionCapacity = capacity, SnapshotCacheRoot = _cacheRoot };
        var runner = new EngineRunner(loggerFactory, hosts, new CloneRegistry(), options);
        var manager = new AnalysisSessionManager(runner, hosts, options);
        return (manager, hosts);
    }

    private static AnalyzeSpec Spec(string fixture) => new(
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "tests", "fixtures", fixture)),
        Focus: null, Depth: null, Detail: null, NoRoslyn: true);

    [Fact]
    public async Task Closing_the_last_session_on_a_root_releases_its_host()
    {
        var (manager, hosts) = CreateManager(capacity: 5);
        await using var _ = manager;

        var (session, _) = await manager.AnalyzeAsync(Spec("ControllerApp"), progress: null, CancellationToken.None);
        Assert.Equal(1, hosts.Count);

        Assert.True(await manager.CloseSessionAsync(session.Handle));
        Assert.Equal(0, hosts.Count);
    }

    [Fact]
    public async Task Capacity_eviction_releases_the_evicted_roots_host()
    {
        var (manager, hosts) = CreateManager(capacity: 1);
        await using var _ = manager;

        await manager.AnalyzeAsync(Spec("ControllerApp"), progress: null, CancellationToken.None);
        Assert.Equal(1, hosts.Count);

        // Second, different root: the LRU eviction that admits it must take ControllerApp's
        // host down with the session — not just the session.
        await manager.AnalyzeAsync(Spec("MinimalApiProject"), progress: null, CancellationToken.None);
        Assert.Equal(1, hosts.Count);

        var remaining = manager.ListSessions();
        var single = Assert.Single(remaining);
        Assert.EndsWith("MinimalApiProject", single.RepoPath, StringComparison.OrdinalIgnoreCase);
    }
}
