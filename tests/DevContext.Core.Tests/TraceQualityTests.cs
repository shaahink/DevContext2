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

    private static async Task<string> RunTraceAsync(string repoPath, string entry)
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
            RootPath = rootResult.RootPath ?? repoPath,
            Options = options,
            ActiveScenario = intent.Scenario,
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("Trace"),
            RoslynWorkspace = new MockRoslynProvider(),
        };

        var pipeline = BuildPipeline(loggerFactory);
        var snapshot = await pipeline.AnalyzeAsync(ctx);

        var request = new RenderRequest
        {
            Format = "markdown",
            MaxTokens = 8000,
            Entry = entry,
            Depth = 8,
            Detail = TraceDetail.Salient,
        };
        var rendered = await pipeline.RenderAsync(snapshot, request);
        return rendered.Content;
    }

    private static DiscoveryPipeline BuildPipeline(ILoggerFactory loggerFactory)
    {
        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor(),
            new DependencyExtractor(),
            new SyntaxStructureExtractor(),
            new LayerClassifier(),
            new EndpointExtractor(),
            new MediatRExtractor(),
            new ControllerActionExtractor(),
            new EfCoreExtractor(),
            new EventBusExtractor(),
            new CallGraphExtractor(),
            new SourceBodyExtractor(),
            new IndirectWiringDetector(),
            new AspireExtractor(),
            new ProgramCsFlowExtractor(),
            new DiRegistrationExtractor(),
        };

        var pruners = new List<IPruner>
        {
            new PatternRelevancePruner(),
            new TokenBudgetEnforcer(),
        };

        var compressors = new List<ICompressionStrategy>
        {
            new TrivialMemberCompressor(),
            new BoilerplateCompressor(),
            new StructuralDeduplicator(),
            new NamespaceGrouper(),
            new LlmFriendlyFormatter(),
            new AggressiveTruncator(),
        };

        var renderers = new Dictionary<string, IContextRenderer>
        {
            ["markdown"] = new MarkdownRenderer(),
            ["json"] = new JsonContextRenderer(),
        };

        return new DiscoveryPipeline(
            extractors, pruners, compressors, renderers,
            loggerFactory.CreateLogger<DiscoveryPipeline>());
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
