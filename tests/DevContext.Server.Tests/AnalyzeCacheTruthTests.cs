using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

using DevContext.Core.Analysis;
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

    /// <summary>
    /// G3.3 (R4 §1 item 10) — <c>cached</c> says an analysis was skipped; it does not say what the
    /// numbers you were handed are ABOUT. A rehydrate stops the stopwatch at ~200ms, so
    /// <c>elapsed_ms</c> describes the LOAD, and the session it opens is seconds old however old the
    /// analysis is. Every consumer therefore read a three-day-old answer as freshly computed.
    ///
    /// The persisted stamp is back-dated to 2020 before the rehydrating call, which is what makes
    /// this test able to fail: without it the two instants are milliseconds apart and
    /// <c>DateTime.UtcNow</c> (15.6ms granularity on Windows) would often pass by accident.
    /// </summary>
    [Fact]
    public async Task A_rehydrate_reports_the_originals_instant_not_its_own()
    {
        await using var manager = CreateManager();
        var spec = Spec("ControllerApp");

        var before = DateTime.UtcNow;
        var first = await manager.AnalyzeAsync(spec, progress: null, CancellationToken.None);
        Assert.False(first.Cached);
        // A run stamps the run.
        Assert.InRange(first.Session.Engine.AnalyzedAtUtc, before.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));

        var backdated = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        BackdatePersistedSnapshot(backdated);

        Assert.True(await manager.CloseSessionAsync(first.Session.Handle));
        var rehydrated = await manager.AnalyzeAsync(spec, progress: null, CancellationToken.None);

        Assert.True(rehydrated.Cached);
        Assert.True(rehydrated.Session.Engine.FromSnapshotCache);
        Assert.Equal(backdated, rehydrated.Session.Engine.AnalyzedAtUtc);
        // ...while the SESSION really is brand new, which is exactly why age_seconds could not
        // answer this question and a second field was needed.
        Assert.InRange(rehydrated.Session.CreatedAt, before, DateTime.UtcNow.AddSeconds(1));
    }

    /// <summary>The commit the numbers describe. The snapshot cache already KEYS on HEAD, so on a
    /// hit this is a guarantee rather than a guess — it was simply computed and dropped.</summary>
    [Fact]
    public async Task An_analysis_names_the_commit_it_describes()
    {
        await using var manager = CreateManager();
        var head = GitHeadReader.Read(Spec("ControllerApp").Path);
        if (head is null) return; // no git on this host — the cache falls back to manifest keying too

        var outcome = await manager.AnalyzeAsync(Spec("ControllerApp"), progress: null, CancellationToken.None);

        Assert.Equal(head, outcome.Session.Engine.GitHead);
        Assert.Equal(head, outcome.Session.CommitSha);
    }

    /// <summary>
    /// The session's own HEAD reader used to re-implement git: it required <c>.git</c> to be a
    /// DIRECTORY, so in a git WORKTREE — the layout AGENTS.md tells agents to work from — it walked
    /// off the end of the path and returned the EMPTY STRING, and the app's head-sha chip was blank.
    /// It also missed packed refs. It now asks the same reader the cache key uses.
    /// </summary>
    [Fact]
    public void A_worktree_head_is_not_the_empty_string()
    {
        var root = Path.Combine(Path.GetTempPath(), "devcontext-worktree-tests", Guid.NewGuid().ToString("N"));
        var main = Path.Combine(root, "main");
        Directory.CreateDirectory(main);
        try
        {
            if (!TryGit(main, "init -b trunk")) return; // no git on this host
            File.WriteAllText(Path.Combine(main, "a.txt"), "hello");
            Assert.True(TryGit(main, "add ."));
            Assert.True(TryGit(main, "-c user.name=t -c user.email=t@t commit -m init"));

            var linked = Path.Combine(root, "linked");
            Assert.True(TryGit(main, $"worktree add -b side \"{linked}\""));
            // The precondition that broke the old reader: in a worktree .git is a FILE, and there is
            // no .git DIRECTORY anywhere above it (temp root).
            Assert.True(File.Exists(Path.Combine(linked, ".git")));
            Assert.False(Directory.Exists(Path.Combine(linked, ".git")));

            var sha = AnalysisSessionManager.ResolveCommitSha(linked);
            Assert.Equal(GitHeadReader.Read(linked), sha);
            Assert.Equal(40, sha.Length);
        }
        finally
        {
            TryGit(main, "worktree prune");
            try { Directory.Delete(root, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>An unknown instant is the empty string on the wire, never a confident 0001-01-01 —
    /// the field-with-a-plausible-default class of lie this stage keeps finding.</summary>
    [Fact]
    public void An_unknown_instant_is_empty_not_year_one()
    {
        Assert.Equal("", DevContext.Server.Mapping.ProtoMapper.Iso(default));
        Assert.Equal("2020-01-02T03:04:05Z",
            DevContext.Server.Mapping.ProtoMapper.Iso(new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc)));
    }

    /// <summary>Rewrites the CreatedAtUtc stamp inside the one snapshot this test class has written
    /// to its private cache root. Finding the file by glob rather than recomputing the cache key
    /// keeps the test independent of how the runner happens to build ExtractionOptions.</summary>
    private void BackdatePersistedSnapshot(DateTime instant)
    {
        var files = Directory.GetFiles(_cacheRoot, "*.snap.json.gz", SearchOption.AllDirectories);
        var file = Assert.Single(files);

        string json;
        using (var read = new GZipStream(File.OpenRead(file), CompressionMode.Decompress))
        using (var reader = new StreamReader(read))
            json = reader.ReadToEnd();

        Assert.Contains("\"CreatedAtUtc\":", json);
        var stamped = new Regex("\"CreatedAtUtc\":\"[^\"]*\"").Replace(
            json, $"\"CreatedAtUtc\":\"{instant:yyyy-MM-dd'T'HH:mm:ss'Z'}\"", 1);

        using var write = new GZipStream(File.Create(file), CompressionLevel.Fastest);
        using var writer = new StreamWriter(write);
        writer.Write(stamped);
    }

    private static bool TryGit(string workDir, string args)
    {
        try
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = workDir, RedirectStandardOutput = true,
                RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true,
            };
            using var proc = Process.Start(psi);
            if (proc is null) return false;
            proc.StandardOutput.ReadToEnd();
            proc.StandardError.ReadToEnd();
            proc.WaitForExit(15000);
            return proc.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception) { return false; }
    }
}
