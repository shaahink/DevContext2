using DevContext.Core.Graph;
using Xunit;
using Xunit.Abstractions;

namespace DevContext.Core.Tests;

/// <summary>
/// Loom L0 truth gate: named-fact assertions against DevContext output,
/// driven from the TARGET REPO SOURCE, not from today's broken output.
/// Red tests carry [TruthPending("L<n>")] — they ratchet into place when
/// the named stage fixes the underlying defect. Never weaken a truth assertion
/// to match broken output; fix the pipeline, then flip the attribute.
/// </summary>
[Trait("Category", "Eval")]
[Trait("Category", "Truth")]
public sealed class TruthExpectationTests
{
    private readonly ITestOutputHelper _output;

    public TruthExpectationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ═══════════════════════════════════════════════════════════════════
    // Dogfood (eshop-microservices)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// L0 truth #1: the checkout flow must trace across three services.
    /// Steps: POST /basket/checkout → CheckoutBasketCommand (send) →
    /// CheckoutBasketCommandHandler (handler) → BasketCheckoutEvent (bus-publish) →
    /// BasketCheckoutEventHandler (Ordering consumer) → CreateOrder… depth ≥ 5.
    /// Today: depth-1, Sends edge mis-anchored on a DTO lambda. Fix: L2.4.
    /// </summary>
    [Fact]
    public async Task Dogfood_checkout_flow_traces_cross_service_depth_ge_5()
    {
        var repoPath = DogfoodPath();
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var trace = await RunTraceAsync(repoPath, "POST /basket/checkout");

        Assert.Contains("TRACE", trace);
        Assert.Contains("ENTRY", trace);

        // Minimum seam hops (send + handler + bus + consumer + create ≥ 5).
        // The named source chain has 5 hops past the entry:
        //   send CheckoutBasketCommand → handler CheckoutBasketCommandHandler →
        //   bus BasketCheckoutEvent → consumer BasketCheckoutEventHandler →
        //   send CreateOrderCommand. depth_ge_5 in the test name is the real bar.
        var seamHops = trace.Split('\n').Count(l =>
            l.Contains("call ") || l.Contains("send ") || l.Contains("handler ")
            || l.Contains("raises ") || l.Contains("data ") || l.Contains("bus "));
        Assert.True(seamHops >= 5, $"Checkout trace seam hops: {seamHops}, expected ≥5:\n{trace}");

        // Named steps (from reading the source, verbatim:
        //   CheckoutBasketEndpoints.cs → CheckoutBasketHandler.cs →
        //   BuildingBlocks.Messaging/Events/BasketCheckoutEvent →
        //   Ordering.Application/Orders/EventHandlers/Integration/BasketCheckoutEventHandler.cs
        //   → sender.Send(CreateOrderCommand)). The last two are the CROSS-SERVICE
        //   hop into Ordering — the whole point of the flagship flow.
        Assert.Contains("CheckoutBasketCommand", trace, StringComparison.Ordinal);
        Assert.Contains("BasketCheckoutEvent", trace, StringComparison.Ordinal);
        Assert.Contains("BasketCheckoutEventHandler", trace, StringComparison.Ordinal);
        Assert.Contains("CreateOrderCommand", trace, StringComparison.Ordinal);
        _output.WriteLine($"Checkout trace depth: {seamHops} hops");
    }

    /// <summary>
    /// L0 truth #2: the service set must be exactly 6 runnables with full names,
    /// NO class libraries. Today: 3 cards labeled "API", class libraries shown as
    /// peer "services". Fix: L1.2 (Service node kind + boundary inference).
    /// </summary>
    [SkippableFact]
    public async Task Dogfood_service_names_are_full_and_runnables_only()
    {
        var repoPath = DogfoodPath();
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);

        // Per-service block must show full names
        Assert.Contains("Basket.API", result.Content, StringComparison.Ordinal);
        Assert.Contains("Catalog.API", result.Content, StringComparison.Ordinal);
        Assert.Contains("Ordering.API", result.Content, StringComparison.Ordinal);
        Assert.Contains("Discount.Grpc", result.Content, StringComparison.Ordinal);
        Assert.Contains("Shopping.Web", result.Content, StringComparison.Ordinal);
        Assert.Contains("YarpApiGateway", result.Content, StringComparison.Ordinal);

        // Class libraries must NOT appear as runnable services. In the STYLE
        // "per service:" block each runnable renders as "Name: <style>"; library
        // projects only appear in the dependency list (with "→"), never as a
        // per-service style line. Assert no library is presented as a service.
        Assert.DoesNotContain("BuildingBlocks: ", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildingBlocks.Messaging: ", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Ordering.Application: ", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Ordering.Domain: ", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Ordering.Infrastructure: ", result.Content, StringComparison.Ordinal);
    }

    /// <summary>
    /// L0 baseline: dogfood presence numbers must not drift.
    /// Checks markdown for MAP + service names, JSON for detections count.
    /// </summary>
    [SkippableFact]
    public async Task Dogfood_baseline_presence_ok()
    {
        var repoPath = DogfoodPath();
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);

        // Markdown must contain the MAP section and all 6 runnable service names
        Assert.Contains("MAP", result.Content, StringComparison.Ordinal);
        Assert.Contains("eshop-microservices", result.Content, StringComparison.Ordinal);

        // Service names must appear in output
        Assert.Contains("Basket.API", result.Content, StringComparison.Ordinal);
        Assert.Contains("Catalog.API", result.Content, StringComparison.Ordinal);
        Assert.Contains("Ordering.API", result.Content, StringComparison.Ordinal);
        Assert.Contains("Discount.Grpc", result.Content, StringComparison.Ordinal);

        // JSON output must have detections (baseline: 493 nodes produce many detections)
        var json = result.JsonContent;
        Assert.NotEmpty(json);

        var typesCount = ExtractJsonInt(json, "typesSummary.found");
        Assert.True(typesCount >= 80, $"typesSummary.found={typesCount} < 80");

        _output.WriteLine($"Dogfood baseline: {typesCount} types found, MAP+services confirmed");
    }

    // ═══════════════════════════════════════════════════════════════════
    // RazorPages
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// L0 truth #3: RazorPages (AspNetCore.Docs, multi-sample repo) must contain
    /// ZERO edges whose endpoints live in different sample roots. Today the
    /// NameResolver picks first-match on short-name collision, fabricating wiring
    /// between unrelated samples. Fix: L1 (ambiguity handling, Law R1).
    /// </summary>
    [TruthPending("L1")]
    public async Task RazorPages_no_fabricated_cross_sample_edges()
    {
        var repoPath = RepoPath("eval-repos/RazorPages");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var trace = await RunTraceAsync(repoPath, "POST /Students");

        // The fabrication: trace jumps from one sample dir to another.
        // Example: entry in entity-framework-6/3.xsample/MVCCore but
        // call edge resolves to ef-mvc/intro/samples/5cu.
        var lines = trace.Split('\n');
        var sampleDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            // Extract directory paths from the trace lines (e.g., aspnetcore/data/...)
            var idx = line.IndexOf("aspnetcore/", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var rest = line[idx..];
                // Take up to the sample directory (3 levels deep)
                var parts = rest.Split('/');
                if (parts.Length >= 4)
                {
                    var sampleRoot = string.Join("/", parts.Take(4));
                    sampleDirs.Add(sampleRoot);
                }
            }
        }

        _output.WriteLine($"Trace spans {sampleDirs.Count} sample roots: {string.Join(", ", sampleDirs)}");

        // A trace of one entry must not span multiple sample roots
        Assert.True(sampleDirs.Count <= 1,
            $"Fabricated cross-sample trace: entry in one sample, edges resolved to {sampleDirs.Count} roots ({string.Join(", ", sampleDirs)})");
    }

    // ═══════════════════════════════════════════════════════════════════
    // Blazor
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// L0 truth #4: Blazor WASM sample must NOT be labeled Microservices.
    /// Today it is (E4: 142 doc-sample projects counting as microservice signals).
    /// Fix: L7.3 (style detection guardrails) + L7.4 (multi-.sln directory detection).
    /// </summary>
    [SkippableFact]
    public async Task Blazor_archetype_is_not_microservices()
    {
        var repoPath = RepoPath("eval-repos/Blazor");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);
        var json = result.JsonContent;

        // Extract the style field from the report content
        var styleLine = result.Content.Split('\n')
            .FirstOrDefault(l => l.Contains("STYLE") || l.Contains("Style:"));
        _output.WriteLine($"Blazor style: {styleLine ?? "(not found)"}");

        // Must NOT be Microservices
        Assert.DoesNotContain("Microservices", styleLine ?? "", StringComparison.Ordinal);
        // Must be SampleCollection or another honest non-Microservices label
        Assert.Contains("SampleCollection", result.Content, StringComparison.Ordinal);
    }

    // ═══════════════════════════════════════════════════════════════════
    // CleanArchitecture
    // ═══════════════════════════════════════════════════════════════════

    [SkippableFact]
    public async Task CleanArchitecture_baseline_presence_ok()
    {
        var repoPath = RepoPath("eval-repos/CleanArchitecture");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);
        Assert.NotEmpty(result.Content);
        Assert.Contains("MAP", result.Content, StringComparison.Ordinal);

        var json = result.JsonContent;
        Assert.NotEmpty(json);

        var typesCount = ExtractJsonInt(json, "typesSummary.found");
        Assert.True(typesCount >= 200, $"typesSummary.found={typesCount} < 200 (baseline 647 types reported)");

        _output.WriteLine($"CleanArchitecture baseline: {typesCount} types found");
    }

    // ═══════════════════════════════════════════════════════════════════
    // TodoApi
    // ═══════════════════════════════════════════════════════════════════

    [SkippableFact]
    public async Task TodoApi_baseline_presence_ok()
    {
        var repoPath = RepoPath("eval-repos/TodoApi");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var trace = await RunTraceAsync(repoPath, "POST /todos/");
        Assert.Contains("TRACE", trace, StringComparison.Ordinal);
        Assert.Contains("ENTRY", trace, StringComparison.Ordinal);
        Assert.Contains("TodoDbContext", trace, StringComparison.Ordinal);

        _output.WriteLine("TodoApi baseline: trace confirms TodoDbContext");
    }

    // ═══════════════════════════════════════════════════════════════════
    // DntSite
    // ═══════════════════════════════════════════════════════════════════

    [SkippableFact]
    public async Task DntSite_baseline_presence_ok()
    {
        var repoPath = DntSitePath();
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);
        Assert.NotEmpty(result.Content);
        Assert.Contains("MAP", result.Content, StringComparison.Ordinal);

        var json = result.JsonContent;
        Assert.NotEmpty(json);

        var typesCount = ExtractJsonInt(json, "typesSummary.found");
        Assert.True(typesCount >= 1000, $"typesSummary.found={typesCount} < 1000 (baseline 4965 nodes)");

        _output.WriteLine($"DntSite baseline: {typesCount} types found");
    }

    // ═══════════════════════════════════════════════════════════════════
    // Library archetype (L7.4)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// L7.4 truth: a library repo (FluentValidation) must produce a meaningful
    /// public surface and NOT be detected as an App. Library archetype repos
    /// should show their public API, not pretend to have application entries.
    /// </summary>
    [SkippableFact]
    public async Task Library_archetype_has_public_surface()
    {
        var repoPath = RepoPath("eval-repos/FluentValidation");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);
        Assert.NotEmpty(result.Content);
        Assert.DoesNotContain("No projects discovered.", result.Content, StringComparison.Ordinal);
        Assert.Contains("LIBRARY", result.Content, StringComparison.Ordinal);

        var json = result.JsonContent;
        Assert.NotEmpty(json);

        var typesCount = ExtractJsonInt(json, "typesSummary.found");
        Assert.True(typesCount >= 50, $"typesSummary.found={typesCount} < 50 (baseline ~275 types)");

        // A library report must talk about its public API surface, not application entries
        Assert.Contains("PUBLIC SURFACE", result.Content, StringComparison.OrdinalIgnoreCase);

        _output.WriteLine($"Library (FluentValidation): {typesCount} types, public surface confirmed");
    }

    // ═══════════════════════════════════════════════════════════════════
    // Desktop archetype (L7.4)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// L7.4 truth: a desktop/WPF repo (PowerToys) must produce entries and
    /// a non-trivial graph. Desktop repos should not report empty or stub output.
    /// </summary>
    [SkippableFact]
    public async Task Desktop_archetype_produces_entries()
    {
        var repoPath = RepoPath("eval-repos/PowerToys");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);
        Assert.NotEmpty(result.Content);
        Assert.DoesNotContain("No projects discovered.", result.Content, StringComparison.Ordinal);
        Assert.Contains("DESKTOP APP", result.Content, StringComparison.Ordinal);

        var json = result.JsonContent;
        Assert.NotEmpty(json);

        var typesCount = ExtractJsonInt(json, "typesSummary.found");
        Assert.True(typesCount >= 500, $"typesSummary.found={typesCount} < 500 (PowerToys is a large repo)");

        _output.WriteLine($"Desktop (PowerToys): {typesCount} types found");
    }

    // ═══════════════════════════════════════════════════════════════════
    // Worker archetype (L7.4)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// L7.4 truth: a worker/function repo (Azure Functions .NET worker) must
    /// produce a non-trivial graph and NOT be conflated with a web App.
    /// Worker archetypes should not report web endpoints as their primary identity.
    /// </summary>
    [SkippableFact]
    public async Task Worker_archetype_produces_graph()
    {
        var repoPath = RepoPath("eval-repos/AzureFunctions");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);
        Assert.NotEmpty(result.Content);
        Assert.DoesNotContain("No projects discovered.", result.Content, StringComparison.Ordinal);

        var json = result.JsonContent;
        Assert.NotEmpty(json);

        var typesCount = ExtractJsonInt(json, "typesSummary.found");
        Assert.True(typesCount >= 30, $"typesSummary.found={typesCount} < 30 (baseline ~50 types)");

        _output.WriteLine($"Worker (AzureFunctions): {typesCount} types found");
    }

    // ═══════════════════════════════════════════════════════════════════
    // CompositionApp fixture (T7.1)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// T7.1 truth: entry kinds counted from the fixture SOURCE — 3 controller actions
    /// (AddonsController: GetPack packs/{id} · CreatePack packs · GetTheme themes/{slug}),
    /// 2 hosted workers (PriceWorker direct AddHostedService + BacktestWorker via the
    /// factory lambda), 1 SignalR hub (PriceHub). The per-kind headers must carry those
    /// exact counts — presence alone is already pinned by compositionapp.json (eval).
    /// </summary>
    [SkippableFact]
    public async Task CompositionApp_per_kind_entry_counts_match_source()
    {
        var repoPath = RepoPath("tests/fixtures/CompositionApp");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);

        Assert.Contains("HTTP (3)", result.Content, StringComparison.Ordinal);
        Assert.Contains("Background (2)", result.Content, StringComparison.Ordinal);
        Assert.Contains("SignalR (1)", result.Content, StringComparison.Ordinal);

        _output.WriteLine("CompositionApp per-kind: HTTP 3 · Background 2 · SignalR 1 (from source)");
    }

    // ═══════════════════════════════════════════════════════════════════
    // GrpcAggregator — real gRPC service app (T7.1)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// T7.1 truth: grpc-dotnet's Aggregator example is a REAL gRPC service app —
    /// Program.cs maps three services (Greeter, Counter, Aggregator) and the source
    /// carries exactly 6 `public override` RPCs. The map must render a gRPC (6) kind
    /// header and name all three service impls. AggregatorService additionally forwards
    /// to Greeter/Counter over injected gRPC clients (server-to-server), which is why
    /// this repo is the standing real-gRPC pole rather than a synthetic fixture.
    /// </summary>
    [SkippableFact]
    public async Task GrpcAggregator_rpc_entries_match_source()
    {
        var repoPath = RepoPath("eval-repos/gRPC/examples/Aggregator");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);

        Assert.Contains("gRPC (6)", result.Content, StringComparison.Ordinal);
        Assert.Contains("AggregatorService", result.Content, StringComparison.Ordinal);
        Assert.Contains("CounterService", result.Content, StringComparison.Ordinal);
        Assert.Contains("GreeterService", result.Content, StringComparison.Ordinal);

        _output.WriteLine("GrpcAggregator: 6 RPC entries across 3 services (from source)");
    }

    // ═══════════════════════════════════════════════════════════════════
    // aspire-samples (T7.1)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// T7.1 truth (green half): scaffolding solutions never define the repo. Before the
    /// T7.1 fixes, dotnet/aspire-samples was titled after `.github/for.dependabot.only.sln`
    /// (dot-directory decoy) and then `tests/SamplesTests.slnx` (shallow tooling solution
    /// beating 13 deeper per-sample .slnx files). The JSON style verdict is now honestly
    /// SampleCollection — pinned by eval/expectations/aspire-samples.json.
    /// </summary>
    [SkippableFact]
    public async Task AspireSamples_solution_pick_ignores_scaffolding()
    {
        var repoPath = RepoPath("eval-repos/aspire-samples");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);

        Assert.DoesNotContain("for.dependabot.only", result.Content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SamplesTests", result.Content, StringComparison.Ordinal);
    }

    /// <summary>
    /// T7.1 truth (pending half): dotnet/aspire-samples is 13 independent per-sample .slnx
    /// solutions with no unifying root solution — a samples collection. The MARKDOWN render
    /// must say so. Today the style verdict is honest in JSON, but every entry's provenance
    /// is under samples/ so NoiseFilter suppresses them all → 0 entries → the archetype
    /// ladder lands Library ("0 public types") → the Library render hides the STYLE line
    /// entirely. Fix = sample-collection render honesty: when style is SampleCollection the
    /// samples ARE the product (entries, archetype, style line). Found by T7.1; fixed by T8.2
    /// (model.SamplesAreTheProduct — samples-only repos waive sample-path suppression).
    /// </summary>
    [SkippableFact]
    public async Task AspireSamples_style_is_sample_collection_not_microservices()
    {
        var repoPath = RepoPath("eval-repos/aspire-samples");
        Skip.IfNot(Directory.Exists(repoPath), $"fixture absent (not a pass): {repoPath}");

        var result = await RunOverviewAsync(repoPath);

        var styleLine = result.Content.Split('\n')
            .FirstOrDefault(l => l.Contains("STYLE") || l.Contains("Style:"));
        _output.WriteLine($"aspire-samples style: {styleLine ?? "(not found)"}");

        Assert.DoesNotContain("Microservices", styleLine ?? "", StringComparison.Ordinal);
        Assert.Contains("SampleCollection", result.Content, StringComparison.Ordinal);
    }

    // ═══════════════════════════════════════════════════════════════════
    // Pipeline helpers (following TraceQualityTests pattern)
    // ═══════════════════════════════════════════════════════════════════

    private sealed record OverviewResult(string Content, string JsonContent);

    private static async Task<OverviewResult> RunOverviewAsync(string repoPath)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);

        var options = new ExtractionOptions
        {
            MaxOutputTokens = 8000,
            OutputFormat = OutputFormat.Markdown,
            AllowRoslyn = true,
        };

        using var loggerFactory = LoggerFactory.Create(_ => { });
        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = new SharedAnalysisContext(),
            Logger = loggerFactory.CreateLogger("Truth"),
        };

        var pipeline = TestPipeline.Build(loggerFactory);
        var snapshot = await pipeline.AnalyzeAsync(ctx);

        // Render markdown
        var mdRequest = new RenderRequest { Format = "markdown", MaxTokens = 8000 };
        var mdResult = await pipeline.RenderAsync(snapshot, mdRequest);

        // Render JSON
        var jsonRequest = new RenderRequest { Format = "json", MaxTokens = 8000 };
        var jsonResult = await pipeline.RenderAsync(snapshot, jsonRequest);

        return new OverviewResult(mdResult.Content, jsonResult.Content);
    }

    private static async Task<string> RunTraceAsync(string repoPath, string entry)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);

        var intent = AnalysisIntentResolver.Resolve(new IntentInput { Focus = entry });

        var options = new ExtractionOptions
        {
            MaxOutputTokens = 8000,
            OutputFormat = OutputFormat.Markdown,
            AllowRoslyn = true,
            BuildFullGraph = true,
            Profile = intent.Profile,
        };

        using var loggerFactory = LoggerFactory.Create(_ => { });
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
            Logger = loggerFactory.CreateLogger("TruthTrace"),
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
        };
        var rendered = await pipeline.RenderAsync(snapshot, request);
        return rendered.Content;
    }

    // ═══════════════════════════════════════════════════════════════════
    // Path resolution
    // ═══════════════════════════════════════════════════════════════════

    private static string? _repoRootCache;

    private static string FindRepoRoot()
    {
        if (_repoRootCache is not null) return _repoRootCache;

        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "DevContext.slnx")))
            {
                _repoRootCache = dir;
                return dir;
            }
            var parent = Path.GetDirectoryName(dir);
            if (parent == dir) break;
            dir = parent;
        }
        _repoRootCache = Environment.CurrentDirectory ?? ".";
        return _repoRootCache;
    }

    private static string RepoPath(string relativePath)
    {
        var root = FindRepoRoot();
        return Path.GetFullPath(Path.Combine(root, relativePath));
    }

    private static string DogfoodPath() =>
        @"C:\Users\shahi\source\repos\run-aspnetcore-microservices\src";

    private static string DntSitePath() =>
        @"C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default";

    /// <summary>Extracts an integer from a simple dot-notation JSON path (e.g. "typesSummary.found").</summary>
    private static int ExtractJsonInt(string json, string path)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (node is null) return 0;

            var segments = path.Split('.');
            System.Text.Json.Nodes.JsonNode? current = node;
            foreach (var seg in segments)
            {
                if (current is null) return 0;
                current = current[seg];
            }

            if (current is System.Text.Json.Nodes.JsonValue val)
            {
                if (val.TryGetValue<int>(out var i)) return i;
                if (val.TryGetValue<long>(out var l)) return (int)l;
            }
            return 0;
        }
        catch (Exception) { return -1; }
    }
}
