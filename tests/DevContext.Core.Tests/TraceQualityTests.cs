using Xunit;

namespace DevContext.Core.Tests;

/// <summary>
/// Validates the *product* the catalog→trace pivot delivered — the entry-rooted Trace — which the
/// JSON expectation suite (<see cref="EvalExpectationTests"/>) never exercises. Those checks measure
/// detection substrate (counts, signals, arch-style); none of them would notice a trace that came out
/// empty. These tests focus one entry per archetype and assert the indirection-bridged story actually
/// renders — handler joins, data/event seams, semantic resolution — so the empty-trace class of
/// regression fails the gate instead of sailing through it green.
/// </summary>
[Trait("Category", "Eval")]
public sealed class TraceQualityTests
{
    // repo (relative to root) · entry to focus · substrings the trace MUST contain.
    [Theory]
    [InlineData("eval-repos/TodoApi", "POST /todos/", new[] { "TodoDbContext" })]
    [InlineData("eval-repos/VerticalSlice/MinimalClean", "POST /Products", new[] { "CreateEndpoint", "Product" })]
    [InlineData("eval-repos/eShop/src/Ordering.API", "POST /api/orders/",
        new[] { "send", "CreateOrderCommand", "handler", "CreateOrderCommandHandler" })]
    public async Task Trace_bridges_indirection(string repoRel, string entry, string[] expected)
    {
        var repoPath = RepoPath(repoRel);
        if (!Directory.Exists(repoPath))
            return; // eval repo not cloned in this environment — skip silently

        var trace = await RunTraceAsync(repoPath, entry);

        // 1. Must be a Trace (not a Map fallback) and reach BEYOND the ENTRY line — this is the direct
        //    regression guard for the empty-trace bug that shipped "working e2e" but rendered nothing.
        Assert.Contains("TRACE", trace);
        Assert.Contains("ENTRY", trace);
        var seamHops = trace.Split('\n').Count(l =>
            l.Contains("call ") || l.Contains("send ") || l.Contains("handler ")
            || l.Contains("raises ") || l.Contains("data ") || l.Contains("di "));
        Assert.True(seamHops >= 1, $"Trace for '{entry}' bridged no seams:\n{trace}");

        // 2. Must contain the archetype's expected wiring.
        foreach (var sub in expected)
            Assert.True(trace.Contains(sub, StringComparison.Ordinal),
                $"Trace for '{entry}' missing '{sub}':\n{trace}");
    }

    [Fact]
    public async Task Trace_can_keep_arch_sections_alongside_the_call_stack()
    {
        var repoPath = RepoPath("eval-repos/eShop/src/Ordering.API");
        if (!Directory.Exists(repoPath))
            return; // eval repo not cloned in this environment — skip silently

        // Default: a focus produces a trace-only render (the CLI behaviour).
        var traceOnly = await RunTraceAsync(repoPath, "POST /api/orders/");
        Assert.Contains("TRACE", traceOnly, StringComparison.Ordinal);
        Assert.DoesNotContain("MAP  ", traceOnly, StringComparison.Ordinal);

        // IncludeMapWithTrace: the architecture/Map sections stay on, with the trace appended — so the
        // desktop can drill a call stack from a node without losing the orientation view.
        var combined = await RunTraceAsync(repoPath, "POST /api/orders/", includeMap: true);
        Assert.Contains("MAP  ", combined, StringComparison.Ordinal);     // arch header kept
        Assert.Contains("ENTRY POINTS", combined, StringComparison.Ordinal);
        Assert.Contains("TRACE", combined, StringComparison.Ordinal);     // plus the drill-down
    }

    private static async Task<string> RunTraceAsync(string repoPath, string entry, bool includeMap = false)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);

        // Derive scenario/profile/focus from the entry exactly as the CLI does (focus → deep-dive → Debug).
        var intent = AnalysisIntentResolver.Resolve(new IntentInput { Focus = entry });

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
            Logger = loggerFactory.CreateLogger("Trace"),
        };

        var pipeline = TestPipeline.Build(loggerFactory);
        var snapshot = await pipeline.AnalyzeAsync(ctx);

        var request = new RenderRequest
        {
            Format = "markdown",
            MaxTokens = 8000,
            Entry = entry,
            Depth = 8,
            Detail = TraceDetail.Salient,
            IncludeMapWithTrace = includeMap,
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
