using DevContext.Core.Services;
using DevContext.Server.Sessions;

using Microsoft.Extensions.Logging;

namespace DevContext.Server.Tests;

/// <summary>
/// G1.4 (R4 §1 item 7) — <c>AnalyzeResult.cached</c> must be measured, not assumed.
///
/// There are three ways an analyze call can come back without analysing anything, and before this
/// checkpoint only ONE of them said so, to a progress event nobody read: the session manager's
/// repo+HEAD+sln reuse. The engine runner's two snapshot-cache branches returned an EngineResult
/// indistinguishable from a full run, so a call that rehydrated a snapshot in 200ms and a call that
/// parsed the repo for eight minutes were the same shape on the wire.
///
/// The cache root is redirected to a fresh temp directory (same reason as HostReleaseTests), which
/// is what makes "cold" here actually cold: against the developer's real cache the first call would
/// already be a hit and the false case would never be exercised.
/// </summary>
public sealed class AnalyzeCacheTruthTests : IDisposable
{
    private readonly string _cacheRoot = Path.Combine(
        Path.GetTempPath(), "devcontext-cachetruth-tests", Guid.NewGuid().ToString("N"));
    private readonly string? _priorCacheRoot;

    public AnalyzeCacheTruthTests()
    {
        _priorCacheRoot = Environment.GetEnvironmentVariable("DEVCONTEXT_CACHE_ROOT");
        Environment.SetEnvironmentVariable("DEVCONTEXT_CACHE_ROOT", _cacheRoot);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("DEVCONTEXT_CACHE_ROOT", _priorCacheRoot);
        try { if (Directory.Exists(_cacheRoot)) Directory.Delete(_cacheRoot, true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static AnalysisSessionManager CreateManager()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var hosts = new EngineHostCache(loggerFactory);
        var runner = new EngineRunner(loggerFactory, hosts, new CloneRegistry());
        return new AnalysisSessionManager(runner, hosts, new ServerOptions { SessionCapacity = 5 });
    }

    private static AnalyzeSpec Spec(string fixture) => new(
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "tests", "fixtures", fixture)),
        Focus: null, Depth: null, Detail: null, NoRoslyn: true);

    [Fact]
    public async Task All_three_paths_report_themselves()
    {
        await using var manager = CreateManager();
        var spec = Spec("ControllerApp");

        // 1. Nothing on disk, nothing open: this call analyses, and must not claim otherwise.
        var first = await manager.AnalyzeAsync(spec, progress: null, CancellationToken.None);
        Assert.False(first.Cached);

        // 2. Same repo+HEAD+sln, session still open: reuse.
        var second = await manager.AnalyzeAsync(spec, progress: null, CancellationToken.None);
        Assert.True(second.Cached);
        Assert.Same(first.Session, second.Session);

        // 3. Session gone, snapshot on disk: a NEW session, still not an analysis. This is the
        //    branch that was silent — a fresh session used to look like a fresh analysis.
        Assert.True(await manager.CloseSessionAsync(first.Session.Handle));
        var third = await manager.AnalyzeAsync(spec, progress: null, CancellationToken.None);
        Assert.True(third.Cached);
        Assert.NotSame(first.Session, third.Session);
    }

    /// <summary>A different repo is a different question — reuse must not bleed across roots.</summary>
    [Fact]
    public async Task A_second_repo_is_not_answered_from_the_firsts_cache()
    {
        await using var manager = CreateManager();

        await manager.AnalyzeAsync(Spec("ControllerApp"), progress: null, CancellationToken.None);
        var other = await manager.AnalyzeAsync(Spec("MinimalApiProject"), progress: null, CancellationToken.None);

        Assert.False(other.Cached);
    }
}
