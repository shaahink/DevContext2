using Xunit;

namespace DevContext.Core.Tests;

/// <summary>
/// Locks the Iteration-1 / PRODUCT-DIRECTION §8 invariant: <b>token budgeting is out of the kernel</b>.
/// The CodeGraph + Map/Trace are assembled before the pruners run and never read <c>model.Budget</c>,
/// so the narrative (markdown) Map and Trace must be <b>byte-identical across different --max-tokens</b>.
/// If someone re-couples the token budget to graph assembly or the narrative renderers, these fail.
/// (The legacy JSON/HTML catalog path is intentionally still budget-driven and is not covered here.)
/// </summary>
[Trait("Category", "Eval")]
public sealed class BudgetIndependenceTests
{
    [Fact]
    public async Task Map_is_invariant_across_token_budgets()
    {
        var repo = RepoPath("eval-repos/eShop/src/Ordering.API");
        if (!Directory.Exists(repo)) return; // eval repo not cloned — skip silently

        var small = await RenderAsync(repo, entry: null, maxTokens: 2000);
        var large = await RenderAsync(repo, entry: null, maxTokens: 20000);

        Assert.False(string.IsNullOrWhiteSpace(small));
        Assert.Equal(large, small);
    }

    [Fact]
    public async Task Trace_is_invariant_across_token_budgets()
    {
        var repo = RepoPath("eval-repos/eShop/src/Ordering.API");
        if (!Directory.Exists(repo)) return; // eval repo not cloned — skip silently

        var small = await RenderAsync(repo, entry: "POST /api/orders/", maxTokens: 2000);
        var large = await RenderAsync(repo, entry: "POST /api/orders/", maxTokens: 20000);

        Assert.Contains("TRACE", small, StringComparison.Ordinal);
        Assert.Equal(large, small);
    }

    private static async Task<string> RenderAsync(string repoPath, string? entry, int maxTokens)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);
        var intent = AnalysisIntentResolver.Resolve(new IntentInput { Focus = entry });

        var options = new ExtractionOptions
        {
            MaxOutputTokens = maxTokens,
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
            Logger = loggerFactory.CreateLogger("Budget"),
        };

        var pipeline = TestPipeline.Build(loggerFactory);
        var snapshot = await pipeline.AnalyzeAsync(ctx);

        var request = new RenderRequest
        {
            Format = "markdown",
            MaxTokens = maxTokens,
            Entry = entry,
            Depth = 8,
            Detail = TraceDetail.Salient,
        };
        var rendered = await pipeline.RenderAsync(snapshot, request);

        // The Diagnostics tail can carry budget-derived numbers; the assertion targets the artifact body.
        return StripDiagnostics(rendered.Content);
    }

    private static string StripDiagnostics(string content)
    {
        var idx = content.IndexOf("\nDIAGNOSTICS", StringComparison.Ordinal);
        return idx >= 0 ? content[..idx] : content;
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
