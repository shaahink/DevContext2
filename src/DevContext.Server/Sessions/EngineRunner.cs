using DevContext.Core.Analysis;

namespace DevContext.Server.Sessions;

public sealed class EngineRunner(ILoggerFactory loggerFactory, EngineHostCache hostCache) : IEngineRunner
{
    private readonly RealFileSystem _fs = new();
    private readonly SnapshotCacheService _snapCache = new();

    public async Task<EngineResult> AnalyzeAsync(AnalyzeSpec spec, IProgress<AnalysisProgress>? progress, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var (inputPath, gitClonePath) = await PrepareSourceAsync(spec.Path, progress, ct).ConfigureAwait(false);

        var rootResult = await ProjectRootResolver.ResolveAsync(inputPath, _fs, ct).ConfigureAwait(false);

        var resolvedIntent = AnalysisIntentResolver.Resolve(new IntentInput
        {
            Focus = string.IsNullOrWhiteSpace(spec.Focus) ? null : spec.Focus,
            Depth = spec.Depth,
        });

        var options = new ExtractionOptions
        {
            EntryPaths = rootResult.EntryCandidates,
            Profile = resolvedIntent.Profile,
            AllowRoslyn = !spec.NoRoslyn,
            BuildFullGraph = true,
            OutputFormat = OutputFormat.Markdown,
            ExcludeExtractors = resolvedIntent.Scenario.DisableExtractors,
        };

        // I10.3 — check snapshot cache before running full analysis
        var (repoKey, versionKey) = SnapshotCacheService.ComputeKeys(rootResult.EffectiveRootPath);
        if (_snapCache.Exists(repoKey, versionKey))
        {
            var cached = await _snapCache.TryLoadAsync<AnalysisSnapshot>(repoKey, versionKey, ct)
                .ConfigureAwait(false);
            if (cached is not null)
            {
                var host = hostCache.GetOrCreate(rootResult.EffectiveRootPath);
                var rehydrated = cached with { Options = options, RootPath = rootResult.EffectiveRootPath };
                var label = Path.GetFileName(rootResult.SolutionFilePath ?? rootResult.RootPath.TrimEnd('\\', '/'));
                var projectCount = rehydrated.Map?.Topology.Length ?? 0;
                sw.Stop();
                return new EngineResult(rehydrated, host.Pipeline, label, projectCount,
                    sw.ElapsedMilliseconds, resolvedIntent.Explanation, resolvedIntent.Warnings,
                    gitClonePath, spec.Cleanup);
            }
        }

        var analysis = new SharedAnalysisContext
        {
            UnresolvedFocusPoints = resolvedIntent.FocusPoints,
            FocusPoints = resolvedIntent.FocusPoints,
        };

        var host2 = hostCache.GetOrCreate(rootResult.EffectiveRootPath);

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = resolvedIntent.Scenario,
            Observer = new StreamingProgressObserver(progress),
            FileSystem = _fs,
            Cache = host2.Cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("DevContext"),
            CancellationToken = ct,
        };

        var snapshot = await host2.Pipeline.AnalyzeAsync(ctx, ct).ConfigureAwait(false);

        // I10.3 — save snapshot to cache write-behind
        _ = _snapCache.SaveAsync(repoKey, versionKey, snapshot, ct);

        sw.Stop();

        var label2 = Path.GetFileName(rootResult.SolutionFilePath ?? rootResult.RootPath.TrimEnd('\\', '/'));
        var projectCount2 = snapshot.Map?.Topology.Length ?? 0;

        return new EngineResult(
            snapshot, host2.Pipeline, label2, projectCount2, sw.ElapsedMilliseconds,
            resolvedIntent.Explanation, resolvedIntent.Warnings, gitClonePath,
            spec.Cleanup);
    }

    private static async Task<(string InputPath, string? GitClonePath)> PrepareSourceAsync(
        string path, IProgress<AnalysisProgress>? progress, CancellationToken ct)
    {
        var repoUrl = RepoUrl.Parse(path);
        if (repoUrl is not { IsValid: true })
            return (path, null);

        progress?.Report(new AnalysisProgress("Cloning", 2, "Cloning repository…"));

        using var git = new GitCloneService();
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
