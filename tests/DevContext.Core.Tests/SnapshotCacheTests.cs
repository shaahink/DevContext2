using System.Diagnostics;

namespace DevContext.Core.Tests;

/// <summary>J2 (Prism D2.0b) — the snapshot cache must actually WORK. The audit found every cache
/// dir 0 bytes: the save serialized unserializable state, threw, and a bare catch swallowed it,
/// while a read-only Exists probe littered empty dirs. These tests pin the resurrection: a REAL
/// analyzed snapshot round-trips and renders byte-identically, failures surface as errors, probes
/// create nothing, and a dirty working tree never collides with its clean-HEAD snapshot.</summary>
public sealed class SnapshotCacheTests : IDisposable
{
    private readonly string _cacheRoot = Path.Combine(
        Path.GetTempPath(), "devcontext-snapcache-tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_cacheRoot)) Directory.Delete(_cacheRoot, true);
        }
        catch (IOException) { /* temp dir, OS cleans up */ }
        catch (UnauthorizedAccessException) { /* same */ }
    }

    // One shared analyze per test run (the fixture is small but real — same one the pack tests use).
    private static readonly SemaphoreSlim AnalyzeGate = new(1, 1);
    private static AnalysisSnapshot? _sharedSnapshot;
    private static DiscoveryPipeline? _sharedPipeline;

    private static async Task<(AnalysisSnapshot Snapshot, DiscoveryPipeline Pipeline)> AnalyzedFixtureAsync()
    {
        await AnalyzeGate.WaitAsync();
        try
        {
            if (_sharedSnapshot is not null) return (_sharedSnapshot, _sharedPipeline!);

            var repoPath = RepoPath(Path.Combine("tests", "fixtures", "CompositionApp"));
            Assert.True(Directory.Exists(repoPath), $"fixture missing: {repoPath}");

            var fs = new RealFileSystem();
            var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);
            var loggerFactory = LoggerFactory.Create(_ => { });
            var ctx = new DiscoveryContext
            {
                RootPath = rootResult.EffectiveRootPath,
                ScopedProjectDirs = rootResult.ScopeProjectDirs,
                Options = new ExtractionOptions { MaxOutputTokens = 8000, OutputFormat = OutputFormat.Markdown, AllowRoslyn = true },
                ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
                Observer = new NullDiscoveryObserver(),
                FileSystem = fs,
                Cache = new AnalysisCache(fs),
                Analysis = new SharedAnalysisContext(),
                Logger = loggerFactory.CreateLogger("SnapshotCache"),
            };
            _sharedPipeline = TestPipeline.Build(loggerFactory);
            _sharedSnapshot = await _sharedPipeline.AnalyzeAsync(ctx);
            return (_sharedSnapshot, _sharedPipeline);
        }
        finally
        {
            AnalyzeGate.Release();
        }
    }

    [Fact]
    public async Task Save_then_load_roundtrips_and_renders_byte_identical()
    {
        var (snapshot, pipeline) = await AnalyzedFixtureAsync();
        var svc = new SnapshotCacheService(_cacheRoot);

        var saved = await svc.SaveAsync("repo1", "v1", snapshot, CancellationToken.None);
        Assert.True(saved.Success, $"save failed: {saved.Error}");
        var file = svc.GetSnapshotPath("repo1", "v1");
        Assert.True(File.Exists(file), "snapshot file missing after successful save");
        Assert.True(new FileInfo(file).Length > 0, "snapshot file is empty");

        var loaded = await svc.TryLoadAsync("repo1", "v1", CancellationToken.None);
        Assert.NotNull(loaded);
        // What every load site does before rendering:
        loaded = loaded with { Options = snapshot.Options, RootPath = snapshot.RootPath };

        // Structural parity of everything render/query paths consume.
        Assert.Equal(snapshot.Model.Types.Count, loaded.Model.Types.Count);
        Assert.Equal(snapshot.Model.Detections.Count, loaded.Model.Detections.Count);
        Assert.Equal(snapshot.Model.DetectedStyle, loaded.Model.DetectedStyle);
        Assert.Equal(snapshot.Model.Archetype, loaded.Model.Archetype);
        Assert.NotNull(loaded.Graph);
        Assert.Equal(snapshot.Graph!.NodeCount, loaded.Graph!.NodeCount);
        Assert.Equal(snapshot.Graph.EdgeCount, loaded.Graph.EdgeCount);
        Assert.Equal(snapshot.Graph.Flows.Length, loaded.Graph.Flows.Length);
        Assert.Equal(snapshot.Entries.Length, loaded.Entries.Length);
        Assert.Equal(snapshot.Analysis.CallGraph?.Edges.Count, loaded.Analysis.CallGraph?.Edges.Count);
        Assert.Equal(snapshot.Analysis.ProjectGraph?.AdjacencyList.Count, loaded.Analysis.ProjectGraph?.AdjacencyList.Count);
        Assert.Equal(
            snapshot.Model.Architecture.All.Keys.Order().ToArray(),
            loaded.Model.Architecture.All.Keys.Order().ToArray());

        // The proof the persisted subset is COMPLETE for rendering: fresh vs rehydrated renders
        // are byte-identical in both formats (diagnostics tail stripped — it may carry run-local
        // numbers, same as BudgetIndependenceTests).
        foreach (var format in new[] { "markdown", "json" })
        {
            var request = new RenderRequest { Format = format, MaxTokens = 8000 };
            var fresh = Normalize((await pipeline.RenderAsync(snapshot, request)).Content);
            var cached = Normalize((await pipeline.RenderAsync(loaded, request)).Content);
            Assert.False(string.IsNullOrWhiteSpace(fresh));
            Assert.Equal(fresh, cached);
        }
    }

    [Fact]
    public void Exists_probe_creates_no_directories()
    {
        var svc = new SnapshotCacheService(_cacheRoot);
        Assert.False(svc.Exists("deadbeef", "v1"));
        Assert.False(Directory.Exists(_cacheRoot),
            "a read-only Exists probe must not litter cache directories (the audit's 0-byte dirs)");
    }

    [Fact]
    public async Task Save_failure_surfaces_the_reason()
    {
        var (snapshot, _) = await AnalyzedFixtureAsync();
        // The cache root path IS a file — directory creation must fail and the reason must travel.
        Directory.CreateDirectory(_cacheRoot);
        var fileAsRoot = Path.Combine(_cacheRoot, "not-a-dir");
        File.WriteAllText(fileAsRoot, "occupied");

        var svc = new SnapshotCacheService(fileAsRoot);
        var result = await svc.SaveAsync("repo1", "v1", snapshot, CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public async Task Dry_run_snapshots_are_refused_not_cached()
    {
        var (snapshot, _) = await AnalyzedFixtureAsync();
        var svc = new SnapshotCacheService(_cacheRoot);

        var result = await svc.SaveAsync("repo1", "v1", snapshot with { IsDryRun = true }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("dry-run", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(svc.GetSnapshotPath("repo1", "v1")));
    }

    [Fact]
    public void Dirty_working_tree_gets_its_own_version_key()
    {
        // A cache keyed on bare HEAD would serve yesterday's map after an uncommitted edit.
        Directory.CreateDirectory(_cacheRoot);
        var repo = Path.Combine(_cacheRoot, "gitrepo");
        Directory.CreateDirectory(repo);
        if (!TryGit(repo, "init")) return; // no git on this host — the key falls back to manifest hashing, covered elsewhere
        File.WriteAllText(Path.Combine(repo, "a.cs"), "class A { }");
        Assert.True(TryGit(repo, "add ."));
        Assert.True(TryGit(repo, "-c user.name=t -c user.email=t@t commit -m init"));

        var (_, cleanKey) = SnapshotCacheService.ComputeKeys(repo);
        Assert.DoesNotContain("-dirty-", cleanKey);
        Assert.DoesNotContain("manifest-", cleanKey);

        File.WriteAllText(Path.Combine(repo, "a.cs"), "class A { int X; }");
        var (_, dirtyKey) = SnapshotCacheService.ComputeKeys(repo);
        Assert.NotEqual(cleanKey, dirtyKey);
        Assert.StartsWith(cleanKey + "-dirty-", dirtyKey);

        // A different edit is a different key (fingerprint covers content change via mtime+length).
        File.WriteAllText(Path.Combine(repo, "a.cs"), "class A { int X; int Y; }");
        var (_, dirtyKey2) = SnapshotCacheService.ComputeKeys(repo);
        Assert.NotEqual(dirtyKey, dirtyKey2);
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

    /// <summary>Removes render-run-local content before comparing fresh vs cached: the diagnostics
    /// tail (as BudgetIndependenceTests does) and the JSON renderers' <c>GeneratedAt = UtcNow</c>
    /// stamp. Everything else must be byte-identical.</summary>
    private static string Normalize(string content)
    {
        var idx = content.IndexOf("\nDIAGNOSTICS", StringComparison.Ordinal);
        if (idx >= 0) content = content[..idx];
        return System.Text.RegularExpressions.Regex.Replace(
            content, "\"generatedAt\"\\s*:\\s*\"[^\"]+\"", "\"generatedAt\":\"<run-local>\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string RepoPath(string relativePath)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "DevContext.slnx")))
        {
            var parent = Path.GetDirectoryName(dir);
            if (parent == dir) break;
            dir = parent;
        }
        return Path.GetFullPath(Path.Combine(dir ?? Environment.CurrentDirectory, relativePath));
    }
}
