using DevContext.Core.Graph;

using Xunit;

namespace DevContext.Core.Tests;

/// <summary>
/// T1.1 — catalog-driven entry seeds. Every <see cref="Models.IEntrySurfaceDetection"/> surface
/// (gRPC/Functions/GraphQL, previously unseeded — only endpoints/handlers/workers/hubs fed seeds) now
/// contributes its file to the Map-mode call graph, so its handler method gets Calls edges and its entry
/// resolves the real service it dispatches to — a non-shallow, targetful entry. Before the fix these
/// three surfaces resolved no target (or the owning type) because their files were never bound in Map
/// mode. The ServiceSurfaces fixture is a service app tripping all three package signals, each surface's
/// handler delegating to a distinctly-named domain service.
/// </summary>
[Trait("Category", "Eval")]
public sealed class EntrySurfaceSeedTests
{
    [Theory]
    [InlineData(EntryPointKind.GrpcService, "Greeter.SayHello", "GreetingService.BuildGreeting")]
    [InlineData(EntryPointKind.FunctionEntry, "OrderIngestFunction.Run", "OrderIngestService.IngestOrder")]
    [InlineData(EntryPointKind.GraphQlField, "CatalogQuery.GetProduct", "CatalogLookupService.FindProduct")]
    public async Task Entry_surface_resolves_its_service_target(EntryPointKind kind, string titlePart, string expectedTarget)
    {
        var repoPath = RepoPath("eval/fixtures/ServiceSurfaces");
        Assert.True(Directory.Exists(repoPath), repoPath);

        var snapshot = await AnalyzeMapAsync(repoPath);

        var entry = snapshot.Entries.SingleOrDefault(e =>
            e.Kind == kind && e.Title.Contains(titlePart, StringComparison.Ordinal));
        Assert.True(entry is not null,
            $"No {kind} entry titled '{titlePart}'. Entries: {string.Join(" · ", snapshot.Entries.Select(e => $"{e.Kind}:{e.Title}"))}");

        // The decisive T1.1 assertion: the target is the SERVICE METHOD the handler calls — which resolves
        // only if the surface's file was seeded into the Map-mode call graph AND the entry anchors on the
        // handler member. A shallow (unseeded) entry would carry the owning type name or null here.
        Assert.Equal(expectedTarget, entry!.Target);
    }

    /// <summary>
    /// T1.8 — kind single-sourcing. The EntryTable facet (what the deck chips and App facet counts read)
    /// must carry each entry's TRUE <see cref="EntryPointKind"/> — the same record the builder stamped —
    /// not a tag-re-derived kind that silently defaults to PublicApi (the "gRPC 75" facet lie). Proven
    /// end-to-end: the table's kind multiset equals the inventory's, and the non-HTTP surfaces keep their
    /// real kinds (GrpcService/FunctionEntry/GraphQlField), never PublicApi.
    /// </summary>
    [Fact]
    public async Task EntryTable_facet_kinds_single_source_from_the_inventory()
    {
        var repoPath = RepoPath("eval/fixtures/ServiceSurfaces");
        Assert.True(Directory.Exists(repoPath), repoPath);

        var snapshot = await AnalyzeMapAsync(repoPath);
        Assert.NotNull(snapshot.Graph);
        var table = new EntryTableProjection().Project(snapshot.Graph!, ProjectionOptions.Default);

        // Facet rows == inventory (deduped by node) — one entry, one row, no invented rows.
        var inventoryKinds = snapshot.Entries
            .DistinctBy(e => e.Node)
            .Select(e => e.Kind.ToString())
            .OrderBy(k => k).ToArray();
        var tableKinds = table.Rows.Select(r => r.Kind).OrderBy(k => k).ToArray();
        Assert.Equal(inventoryKinds, tableKinds);

        // The three service surfaces surface with their real kinds — no PublicApi default.
        foreach (var kind in new[] { "GrpcService", "FunctionEntry", "GraphQlField" })
            Assert.Contains(kind, tableKinds);

        // T1.7 — gRPC entries are proto RPCs only, and only for the nested `Service.ServiceBase` protoc
        // form. The private helper Normalize() is not an entry; WeatherViewModel : ViewModelBase (a plain
        // MVVM base, not nested) is not a gRPC service and its override RefreshAsync() is not an RPC.
        var grpc = snapshot.Entries.Where(e => e.Kind == EntryPointKind.GrpcService).ToArray();
        Assert.Single(grpc);
        Assert.Contains("SayHello", grpc[0].Title, StringComparison.Ordinal);
        Assert.DoesNotContain(snapshot.Entries, e => e.Title.Contains("Normalize", StringComparison.Ordinal));
        Assert.DoesNotContain(snapshot.Entries, e => e.Title.Contains("RefreshAsync", StringComparison.Ordinal));
        Assert.DoesNotContain(snapshot.Entries, e =>
            e.Kind == EntryPointKind.GrpcService && e.Title.Contains("Weather", StringComparison.Ordinal));
    }

    private static async Task<AnalysisSnapshot> AnalyzeMapAsync(string repoPath)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);
        var options = new ExtractionOptions { MaxOutputTokens = 8000, OutputFormat = OutputFormat.Markdown, AllowRoslyn = true };
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
            Logger = loggerFactory.CreateLogger("SeedTest"),
        };
        var pipeline = TestPipeline.Build(loggerFactory);
        return await pipeline.AnalyzeAsync(ctx);
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
