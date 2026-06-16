namespace DevContext.Core.Tests;

public sealed class AspireExtractorTests
{
    [Fact]
    public async Task Detects_AppHost_ProjectResources()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\AppHost\Program.cs",
            """
            var builder = DistributedApplication.CreateBuilder(args);
            builder.AddProject<Projects>("api", "api");
            builder.AddRedis("cache");
            builder.AddPostgres("db");
            builder.Build().Run();
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\AppHost\Program.cs"];

        var extractor = new AspireExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var resources = model.Detections.OfType<AspireResourceDetection>().ToList();
        Assert.True(resources.Count >= 2);
        Assert.Contains(resources, r => string.Equals(r.ResourceType, "Redis", StringComparison.Ordinal));
        Assert.Contains(resources, r => string.Equals(r.ResourceType, "Postgres", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Detects_Resource_Relationships()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\AppHost\Program.cs",
            """
            var builder = DistributedApplication.CreateBuilder(args);
            var api = builder.AddProject<Projects>("api", "api");
            var cache = builder.AddRedis("cache");
            api.WithReference(cache);
            api.WithEnvironment("REDIS_URL", cache);
            builder.Build().Run();
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\AppHost\Program.cs"];

        var extractor = new AspireExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var relationships = model.Detections.OfType<AspireRelationshipDetection>().ToList();
        Assert.True(relationships.Count >= 2, $"Expected >= 2 relationships, got {relationships.Count}");
    }

    [Fact]
    public async Task NonAspire_Project_ReturnsNoDetections()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\Program.cs",
            """
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            app.MapGet("/", () => "Hello");
            app.Run();
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\Program.cs"];

        var extractor = new AspireExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var resources = model.Detections.OfType<AspireResourceDetection>().ToList();
        Assert.Empty(resources);
    }

    [Fact]
    public async Task SignalGate_NoAspireSignal_ReturnsNoDetections()
    {
        var model = new DiscoveryModel();
        // No Aspire signal registered

        var mockContext = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = new FakeFileSystem(),
            Cache = new FakeAnalysisCache(new FakeFileSystem()),
            Analysis = new SharedAnalysisContext(),
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider(),
        };

        Assert.False(new AspireExtractor().ShouldRun(mockContext, model));
    }
}
