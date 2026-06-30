using Xunit;

namespace DevContext.Core.Tests;

/// <summary>
/// Validates the *library* product — the capability-grouped PUBLIC SURFACE — the way
/// <see cref="TraceQualityTests"/> validates the app Trace. The JSON expectation suite
/// (<see cref="EvalExpectationTests"/>) measures the detection substrate over a Map run and would not
/// notice a flat, useless library dump; this tier focuses one real library per shape and asserts the
/// surface artifact actually renders (archetype routing, capability header, entry API, abstractions).
///
/// Ratchet (mirrors ACCEPTANCE.md): a surface fact lands here as a hard, blocking check only once its
/// engine delta is ENFORCED (test + implementation in the same step). Benchmark deltas not yet
/// implemented live as soft, non-blocking checks in <c>eval/expectations/{fluentvalidation,polly}.json</c>
/// (status <c>aspirational</c>) and flip to <c>expected</c> when the engine produces them. Adding a new
/// library = clone it (eval/README.md), register it (eval-repos.json), then add its rows here + a
/// per-repo expectations file — no harness changes.
///
/// Seeded with the Iteration-0 acceptance bar: a real OSS library (FluentValidation, Polly) is detected
/// as the <c>Library</c> archetype and rendered through the library surface renderer (not the app
/// entry-point Map). These skip silently until the repos are cloned into <c>eval-repos/</c>.
/// </summary>
[Trait("Category", "Eval")]
public sealed class SurfaceQualityTests
{
    [Theory]
    [InlineData("eval-repos/FluentValidation")]
    [InlineData("eval-repos/Polly")]
    [InlineData("eval-repos/CommunityToolkit.Mvvm")]
    public async Task Library_repo_is_detected_as_library_archetype(string repoRel)
    {
        var repoPath = RepoPath(repoRel);
        if (!Directory.Exists(repoPath))
            return; // eval repo not cloned in this environment — skip silently

        var json = await RunJsonAsync(repoPath);

        Assert.Contains("\"archetype\": \"Library\"", json);
    }

    [Theory]
    [InlineData("eval-repos/FluentValidation")]
    [InlineData("eval-repos/Polly")]
    [InlineData("eval-repos/CommunityToolkit.Mvvm")]
    public async Task Library_surface_renders_capability_header(string repoRel)
    {
        var repoPath = RepoPath(repoRel);
        if (!Directory.Exists(repoPath))
            return; // eval repo not cloned in this environment — skip silently

        var map = await RunMapAsync(repoPath);

        // Routed to the library surface renderer, not the app entry-point Map.
        Assert.Contains("LIBRARY", map);
        Assert.Contains("PUBLIC SURFACE", map);
    }

    private static Task<string> RunMapAsync(string repoPath) => RunAsync(repoPath, "markdown");

    private static Task<string> RunJsonAsync(string repoPath) => RunAsync(repoPath, "json");

    private static async Task<string> RunAsync(string repoPath, string format)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);

        // No focus → the Map / library-surface artifact (the CLI's no-`--focus` behaviour).
        var intent = AnalysisIntentResolver.Resolve(new IntentInput());

        var options = new ExtractionOptions
        {
            MaxOutputTokens = 8000,
            OutputFormat = OutputFormat.Markdown,
            AllowRoslyn = true,
            Profile = intent.Profile,
        };

        var loggerFactory = LoggerFactory.Create(_ => { });
        var analysis = new SharedAnalysisContext
        {
            UnresolvedFocusPoints = intent.FocusPoints,
            FocusPoints = intent.FocusPoints,
        };

        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = intent.Scenario,
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("Surface"),
        };

        var pipeline = TestPipeline.Build(loggerFactory);
        var snapshot = await pipeline.AnalyzeAsync(ctx);

        var request = new RenderRequest
        {
            Format = format,
            MaxTokens = 8000,
        };
        var rendered = await pipeline.RenderAsync(snapshot, request);
        return rendered.Content;
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
