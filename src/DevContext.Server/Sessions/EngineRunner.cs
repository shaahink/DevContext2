using System.Diagnostics;
using DevContext.Core.Analysis;
using DevContext.Core.Configuration;
using DevContext.Core.Services;

namespace DevContext.Server.Sessions;

public sealed class EngineRunner(ILoggerFactory loggerFactory, EngineHostCache hostCache, CloneRegistry cloneRegistry) : IEngineRunner
{
    private readonly RealFileSystem _fs = new();
    private readonly SnapshotCacheService _snapCache = new();

    public async Task<EngineResult> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        var repoUrl = RepoUrl.Parse(spec.Path);

        // L1.2 — snapshot-first open: if the input is a known GitHub URL, check the clone registry
        // and snapshot cache BEFORE any network I/O. A cache hit renders instantly; the clone +
        // full analysis are skipped entirely.
        if (repoUrl is { IsValid: true })
        {
            var registryEntry = cloneRegistry.Get(repoUrl.Owner, repoUrl.Repo, repoUrl.Ref);
            if (registryEntry is not null && Directory.Exists(registryEntry.Path))
            {
                progress?.Report(new AnalysisProgress("Checking", 1, "Found cached clone, checking snapshots…"));

                var rootResult = await ProjectRootResolver.ResolveAsync(registryEntry.Path, _fs, ct)
                    .ConfigureAwait(false);

                var (repoKey, versionKey) = SnapshotCacheService.ComputeKeys(rootResult.EffectiveRootPath);
                if (_snapCache.Exists(repoKey, versionKey))
                {
                    var cached = await _snapCache.TryLoadAsync(repoKey, versionKey, ct)
                        .ConfigureAwait(false);
                    if (cached is not null)
                    {
                        var (stale, staleMessage) = await ProbeStalenessAsync(registryEntry.Path, ct)
                            .ConfigureAwait(false);

                        var resolvedIntent = ResolveIntent(spec);
                        var options = BuildOptions(rootResult, resolvedIntent, spec);

                        var host = hostCache.GetOrCreate(rootResult.EffectiveRootPath);
                        var rehydrated = cached with { Options = options, RootPath = rootResult.EffectiveRootPath };
                        var label = Path.GetFileName(rootResult.SolutionFilePath ?? rootResult.RootPath.TrimEnd('\\', '/'));
                        var projectCount = rehydrated.Map?.Topology.Length ?? 0;
                        sw.Stop();
                        return new EngineResult(rehydrated, host.Pipeline, label, projectCount,
                            sw.ElapsedMilliseconds, resolvedIntent.Explanation, resolvedIntent.Warnings,
                            registryEntry.Path, spec.Cleanup, stale, stale ? staleMessage : null);
                    }
                }
            }
        }

        // Registry miss or snapshot miss — full clone + analyze path
        var (inputPath, gitClonePath) = await PrepareSourceAsync(spec.Path, progress, ct).ConfigureAwait(false);

        var rootResult2 = await ProjectRootResolver.ResolveAsync(inputPath, _fs, ct).ConfigureAwait(false);

        var resolvedIntent2 = ResolveIntent(spec);
        var options2 = BuildOptions(rootResult2, resolvedIntent2, spec);

        var (repoKey2, versionKey2) = SnapshotCacheService.ComputeKeys(rootResult2.EffectiveRootPath);
        if (_snapCache.Exists(repoKey2, versionKey2))
        {
            var cached2 = await _snapCache.TryLoadAsync(repoKey2, versionKey2, ct)
                .ConfigureAwait(false);
            if (cached2 is not null)
            {
                var host2 = hostCache.GetOrCreate(rootResult2.EffectiveRootPath);
                var rehydrated2 = cached2 with { Options = options2, RootPath = rootResult2.EffectiveRootPath };
                var label2 = Path.GetFileName(rootResult2.SolutionFilePath ?? rootResult2.RootPath.TrimEnd('\\', '/'));
                var projectCount2 = rehydrated2.Map?.Topology.Length ?? 0;
                sw.Stop();
                return new EngineResult(rehydrated2, host2.Pipeline, label2, projectCount2,
                    sw.ElapsedMilliseconds, resolvedIntent2.Explanation, resolvedIntent2.Warnings,
                    gitClonePath, spec.Cleanup);
            }
        }

        var analysis = new SharedAnalysisContext
        {
            UnresolvedFocusPoints = resolvedIntent2.FocusPoints,
            FocusPoints = resolvedIntent2.FocusPoints,
        };

        var host3 = hostCache.GetOrCreate(rootResult2.EffectiveRootPath);

        var collector = new RunReportCollector();
        collector.SetBudget(options2.MaxOutputTokens);
        var observer = new CompositeDiscoveryObserver(new StreamingProgressObserver(progress), collector);

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult2.EffectiveRootPath,
            ScopedProjectDirs = rootResult2.ScopeProjectDirs,
            Options = options2,
            ActiveScenario = resolvedIntent2.Scenario,
            Observer = observer,
            FileSystem = _fs,
            Cache = host3.Cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("DevContext"),
            CancellationToken = ct,
        };

        var snapshot = await host3.Pipeline.AnalyzeAsync(ctx, ct).ConfigureAwait(false);

        // J2 — awaited save (the fire-and-forget form could die with the request scope) with the
        // failure surfaced in the server log instead of swallowed.
        var saveResult = await _snapCache.SaveAsync(repoKey2, versionKey2, snapshot, ct).ConfigureAwait(false);
        if (!saveResult.Success)
            loggerFactory.CreateLogger<EngineRunner>().LogWarning(
                "Snapshot cache save failed for {Root}: {Error}", rootResult2.EffectiveRootPath, saveResult.Error);

        sw.Stop();

        var label3 = Path.GetFileName(rootResult2.SolutionFilePath ?? rootResult2.RootPath.TrimEnd('\\', '/'));
        var projectCount3 = snapshot.Map?.Topology.Length ?? 0;

        return new EngineResult(
            snapshot, host3.Pipeline, label3, projectCount3, sw.ElapsedMilliseconds,
            resolvedIntent2.Explanation, resolvedIntent2.Warnings, gitClonePath,
            spec.Cleanup);
    }

    private static ResolvedIntent ResolveIntent(AnalyzeSpec spec)
    {
        return AnalysisIntentResolver.Resolve(new IntentInput
        {
            Focus = string.IsNullOrWhiteSpace(spec.Focus) ? null : spec.Focus,
            Depth = spec.Depth,
        });
    }

    private static ExtractionOptions BuildOptions(ProjectRootResult rootResult, ResolvedIntent resolvedIntent, AnalyzeSpec spec)
    {
        return new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = resolvedIntent.Profile,
            AllowRoslyn = !spec.NoRoslyn,
            BuildFullGraph = true,
            OutputFormat = OutputFormat.Markdown,
            ExcludeExtractors = resolvedIntent.Scenario.DisableExtractors,
        };
    }

    /// <summary>
    /// Quick staleness probe: runs <c>git fetch --dry-run</c> to learn the remote HEAD without
    /// downloading any objects, then compares against the local HEAD. Returns (stale, message).
    /// </summary>
    private static async Task<(bool Stale, string? Message)> ProbeStalenessAsync(string repoPath, CancellationToken ct)
    {
        try
        {
            var remoteHead = await FetchRemoteHeadAsync(repoPath, ct).ConfigureAwait(false);
            if (remoteHead is null) return (false, null);

            var localHead = await ReadLocalHeadAsync(repoPath).ConfigureAwait(false);
            if (localHead is null) return (false, null);

            if (!string.Equals(remoteHead, localHead, StringComparison.Ordinal))
            {
                // Count how many commits ahead the remote is
                var aheadCount = await CountRemoteAheadAsync(repoPath, remoteHead, ct)
                    .ConfigureAwait(false);
                var msg = aheadCount > 0
                    ? $"Repo moved ahead {aheadCount} commit{(aheadCount > 1 ? "s" : "")} — Re-analyze?"
                    : "Repo has diverged — Re-analyze?";
                return (true, msg);
            }

            return (false, null);
        }
        catch
        {
            // Network down, no git, etc. — not stale; just can't tell.
            return (false, null);
        }
    }

    private static async Task<string?> FetchRemoteHeadAsync(string repoPath, CancellationToken ct)
    {
        using var p = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "fetch --dry-run origin",
            WorkingDirectory = repoPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        });
        if (p is null) return null;

        var stderr = new System.Text.StringBuilder();
        p.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
        p.BeginErrorReadLine();
        await p.WaitForExitAsync(ct).ConfigureAwait(false);

        if (p.ExitCode != 0) return null;

        // After fetch --dry-run, read the remote HEAD
        using var p2 = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-parse origin/HEAD",
            WorkingDirectory = repoPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        });
        if (p2 is null) return null;

        var head = await p2.StandardOutput.ReadToEndAsync(ct).ConfigureAwait(false);
        await p2.WaitForExitAsync(ct).ConfigureAwait(false);
        return head.Trim();
    }

    private static async Task<string?> ReadLocalHeadAsync(string repoPath)
    {
        try
        {
            var headFile = Path.Combine(repoPath, ".git", "HEAD");
            if (!File.Exists(headFile)) return null;
            return (await File.ReadAllTextAsync(headFile).ConfigureAwait(false)).Trim();
        }
        catch
        {
            return null;
        }
    }

    private static async Task<int> CountRemoteAheadAsync(string repoPath, string remoteHead, CancellationToken ct)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"rev-list --count HEAD..{remoteHead}",
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });
            if (p is null) return 0;

            var output = await p.StandardOutput.ReadToEndAsync(ct).ConfigureAwait(false);
            await p.WaitForExitAsync(ct).ConfigureAwait(false);
            return int.TryParse(output.Trim(), out var count) ? count : 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<(string InputPath, string? GitClonePath)> PrepareSourceAsync(
        string path, IProgress<AnalysisProgress>? progress, CancellationToken ct)
    {
        var repoUrl = RepoUrl.Parse(path);
        if (repoUrl is not { IsValid: true })
            return (path, null);

        progress?.Report(new AnalysisProgress("Cloning", 2, "Cloning repository…"));

        using var git = new GitCloneService(cloneRegistry);
        if (!git.IsGitAvailable)
            throw new AnalysisException("GitNotInstalled",
                "Git is not installed. Install Git to clone GitHub repositories.");

        var status = await git.ValidateAsync(repoUrl, ct).ConfigureAwait(false);
        if (status != RepoStatus.Valid)
            throw new AnalysisException(status.ToString(), DescribeRepoStatus(status));

        var clonePath = repoUrl.ClonePath;
        var cloneProgress = progress is null ? null : new CloneToProgress(progress);
        var cloneResult = await git.CloneAsync(repoUrl, clonePath, repoUrl.Ref, cloneProgress, ct).ConfigureAwait(false);
        if (cloneResult is null)
            throw new AnalysisException("CloneFailed", "Clone failed.");

        return (clonePath, clonePath);
    }

    private static string DescribeRepoStatus(RepoStatus status) => status switch
    {
        RepoStatus.NotFound => "Repository not found. Check the URL or ensure the repo is public.",
        RepoStatus.Private => "Private repositories require authentication. Clone the repo locally and analyze the local path.",
        RepoStatus.NetworkError => "Network error — check your connection or try again later.",
        RepoStatus.RateLimited => "GitHub API rate limit exceeded. Wait a few minutes or use a local path.",
        _ => "Unknown error validating the repository.",
    };

    private sealed class CloneToProgress(IProgress<AnalysisProgress> progress) : IProgress<CloneProgress>
    {
        public void Report(CloneProgress value)
            => progress.Report(new AnalysisProgress(value.Phase, value.PercentComplete, value.Message));
    }
}
