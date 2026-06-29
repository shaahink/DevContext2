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
    [InlineData("tests/fixtures/ControllerApp", "GET /api/Products", new[] { "ProductService", "GetByIdAsync" })]
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

    /// <summary>Phase 1 (member-origin) divergence guard — the decisive fix. Two sibling methods of the
    /// SAME class must produce DIFFERENT traces: before member-origin, <c>CatalogApi:CreateItem</c> and
    /// <c>:UpdateItem</c> rendered byte-identical traces headed <c>ENTRY CatalogApi</c> (the type) because
    /// every edge attached to the Type node and the Member inherited all of them. The fabrication this
    /// catches: CreateItem (which only adds an item) showing UpdateItem's
    /// <c>ProductPriceChangedIntegrationEvent</c> raise. This Fact is the negative/divergence guard the
    /// JSON suite cannot express.</summary>
    [Fact]
    public async Task Sibling_methods_produce_divergent_traces_no_fabricated_edges()
    {
        var repoPath = RepoPath("eval-repos/eShop/src/Catalog.API");
        if (!Directory.Exists(repoPath))
            return; // eval repo not cloned in this environment — skip silently

        var create = await RunTraceAsync(repoPath, "CatalogApi:CreateItem");
        var update = await RunTraceAsync(repoPath, "CatalogApi:UpdateItem");

        // Each trace renders and its header names the method it was anchored on (not the bare type).
        Assert.Contains("CatalogApi.CreateItem", create, StringComparison.Ordinal);
        Assert.Contains("CatalogApi.UpdateItem", update, StringComparison.Ordinal);

        // Divergence: the two sibling traces must NOT be identical.
        Assert.NotEqual(create, update);

        // The fabrication class: CreateItem must NOT inherit UpdateItem's price-changed integration event,
        // while UpdateItem (which genuinely raises it) must — proving the divergence is the real wiring.
        Assert.DoesNotContain("ProductPriceChangedIntegrationEvent", create, StringComparison.Ordinal);
        Assert.Contains("ProductPriceChangedIntegrationEvent", update, StringComparison.Ordinal);
    }

    /// <summary>Phase 1 negative guard on the eShop CQRS spine: focusing <c>POST /api/orders/</c> must show
    /// ONLY CreateOrderAsync's wiring. The genuine send→handler→raises(OrderStarted)→outbox spine survives
    /// (the controlled member bridge expands the handler's <c>Handle</c> method), while the sibling
    /// fabrications disappear: the trace must NOT contain <c>CancelOrderCommand</c>/<c>ShipOrderCommand</c>
    /// (siblings CancelOrderAsync/ShipOrderAsync send those) nor the <c>Order</c> aggregate's
    /// <c>OrderShipped</c>/<c>OrderCancelled</c> domain events (raised in unrelated Order methods, which
    /// the type-level <c>data Order</c> edge used to dump wholesale).</summary>
    [Fact]
    public async Task Orders_trace_keeps_the_real_spine_and_drops_sibling_edges()
    {
        var repoPath = RepoPath("eval-repos/eShop/src/Ordering.API");
        if (!Directory.Exists(repoPath))
            return; // eval repo not cloned in this environment — skip silently

        var trace = await RunTraceAsync(repoPath, "POST /api/orders/");

        // The genuine CreateOrder spine survives (send → handler → raises OrderStarted → outbox write).
        Assert.Contains("CreateOrderCommand", trace, StringComparison.Ordinal);
        Assert.Contains("CreateOrderCommandHandler", trace, StringComparison.Ordinal);
        Assert.Contains("OrderStartedIntegrationEvent", trace, StringComparison.Ordinal);
        Assert.Contains("AddAndSaveEventAsync", trace, StringComparison.Ordinal);

        // Sibling sends fabricated by the old Type-level edge inheritance are gone.
        Assert.DoesNotContain("CancelOrderCommand", trace, StringComparison.Ordinal);
        Assert.DoesNotContain("ShipOrderCommand", trace, StringComparison.Ordinal);

        // The Order ctor/data path no longer raises every Order method's domain event.
        Assert.DoesNotContain("OrderShippedDomainEvent", trace, StringComparison.Ordinal);
        Assert.DoesNotContain("OrderCancelledDomainEvent", trace, StringComparison.Ordinal);
    }

    /// <summary>Phase 2 (controllers-first) divergence guard: two sibling actions of one controller must
    /// produce DIFFERENT traces, each descending into the service IT calls (member-origin makes the
    /// action's own Calls edges precise). The fabrication this catches is a controller action inheriting
    /// every sibling action's wiring (the same class-collapse bug, now on the most common .NET shape).</summary>
    [Fact]
    public async Task Controller_sibling_actions_produce_divergent_traces()
    {
        var repoPath = RepoPath("tests/fixtures/ControllerApp");
        if (!Directory.Exists(repoPath))
            return; // fixture missing — skip silently

        var get = await RunTraceAsync(repoPath, "GET /api/Products");
        var del = await RunTraceAsync(repoPath, "DELETE /api/Products");

        // Each descends into its own service method...
        Assert.Contains("GetByIdAsync", get, StringComparison.Ordinal);
        Assert.Contains("DeleteAsync", del, StringComparison.Ordinal);

        // ...and not the other's, and the two traces differ.
        Assert.DoesNotContain("DeleteAsync", get, StringComparison.Ordinal);
        Assert.DoesNotContain("GetByIdAsync", del, StringComparison.Ordinal);
        Assert.NotEqual(get, del);

        // Iteration 4 noise polish: syntactic-resolver pseudo-calls must not pollute controller traces —
        // ControllerBase result-helpers (Ok/NotFound/...) and `nameof` resolve to `this` self-calls.
        Assert.DoesNotContain("NotFound", get, StringComparison.Ordinal);
        Assert.DoesNotContain("nameof", get, StringComparison.Ordinal);
    }

    /// <summary>Phase 3 (complete & honest traces): focusing <c>POST /api/orders/</c> must now render the
    /// whole relevant path and be honest about cuts — closing the probe's "missed the real domain-event
    /// path" finding. Asserts: the domain-event chain <c>raises OrderStartedDomainEvent → consumes
    /// ValidateOrAddBuyer…Handler</c> (Step 2); an entity reached via <c>Calls</c> in TOUCHES — <c>Buyer</c>
    /// (Step 1, the High-5 gap); the pipeline annotation rendered once (Step 3); and an explicit truncation
    /// marker (Step 4). Sibling fabrications stay gone (covered by the Iteration-1 guard above).</summary>
    [Fact]
    public async Task Orders_trace_is_complete_and_honest()
    {
        var repoPath = RepoPath("eval-repos/eShop/src/Ordering.API");
        if (!Directory.Exists(repoPath))
            return; // eval repo not cloned in this environment — skip silently

        var trace = await RunTraceAsync(repoPath, "POST /api/orders/");

        // Step 2 — the domain-event → handler path the trace used to drop.
        Assert.Contains("OrderStartedDomainEvent", trace, StringComparison.Ordinal);
        Assert.Contains("ValidateOrAddBuyer", trace, StringComparison.Ordinal);

        // Step 1 — TOUCHES includes an entity reached via Calls (EF access), not only ReadsWrites.
        Assert.Contains("Buyer", trace, StringComparison.Ordinal);

        // Step 3 — the pipeline (Logging/Validation/Transaction) rendered once under the send.
        Assert.Contains("pipeline", trace, StringComparison.Ordinal);

        // Step 4 — truncation is explicit, not silent.
        Assert.Contains("omitted", trace, StringComparison.Ordinal);
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
