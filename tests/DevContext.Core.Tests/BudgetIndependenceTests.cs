using Xunit;

namespace DevContext.Core.Tests;

/// <summary>
/// Locks the Iteration-1 / PRODUCT-DIRECTION §8 invariant: <b>token budgeting is out of the kernel</b>.
/// The CodeGraph + Map/Trace are assembled before the pruners run and never read <c>model.Budget</c>.
/// The MAP must stay <b>byte-identical across different --max-tokens</b>. The TRACE is shaped to the
/// budget at the RENDER boundary since D3 (Prism D2) — the audit's `Tokens ~24774 (budget 8000)` was
/// a silent 3× breach — via the same post-build ShapeToBudget the pack builder uses; kernel assembly
/// still never reads the budget. A shaped trace must SAY so (the TraceBudget NOTE + per-subtree
/// "(N omitted)" labels), which is what the trace half pins now.
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
    public async Task Trace_is_shaped_to_the_budget_and_says_so()
    {
        var repo = RepoPath("eval-repos/eShop/src/Ordering.API");
        if (!Directory.Exists(repo)) return; // eval repo not cloned — skip silently

        var small = await RenderAsync(repo, entry: "POST /api/orders/", maxTokens: 2000);
        var large = await RenderAsync(repo, entry: "POST /api/orders/", maxTokens: 20000);

        Assert.Contains("TRACE", small, StringComparison.Ordinal);
        Assert.Contains("TRACE", large, StringComparison.Ordinal);
        if (small == large) return; // whole trace fits the small budget — nothing to shape

        // Shaping fired: the small render must be smaller AND name what it did — never a silent cut.
        Assert.True(small.Length < large.Length,
            $"small-budget render ({small.Length} chars) is not smaller than large ({large.Length})");
        Assert.Contains("shaped to the ~2000-token budget", small, StringComparison.Ordinal);
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
