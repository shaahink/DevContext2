namespace DevContext.Core.Tests;

/// <summary>Tests endpoint detection against patterns found in real-world repos (TodoApi, eShop, VerticalSlice).</summary>
public sealed class RealPatternEndpointTests
{
    [Fact]
    public async Task MapGroup_WithChainedMaps_DetectsAllRoutes()
    {
        var result = await RunOnSourceAsync("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            var orders = app.MapGroup("/orders");
            orders.MapGet("/", () => Results.Ok(new[] { "order1" }));
            orders.MapGet("/{id}", (int id) => Results.Ok(id));
            orders.MapPost("/", (CreateOrder cmd) => Results.Created());
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/orders/" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/orders/{id}" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/orders/" && e.HttpMethod == "POST");
        Assert.Equal(3, endpoints.Count);
        Assert.All(endpoints, e => Assert.Equal("/orders", e.GroupPrefix));
    }

    [Fact]
    public async Task ExtensionMethodOnIEndpointRouteBuilder_DetectsEndpoints()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapTodoEndpoints();
            app.Run();
            """);
        fs.AddFile("TodoEndpoints.cs", """
            public static class TodoEndpoints
            {
                public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder routes)
                {
                    routes.MapGet("/todos", () => Results.Ok(new[] { "todo1" }));
                    routes.MapPost("/todos", (CreateTodo cmd) => Results.Created());
                    return routes;
                }
            }
            """);

        var result = await RunOnFilesAsync(fs);
        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/todos" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/todos" && e.HttpMethod == "POST");
    }

    [Fact]
    public async Task FastEndpoints_AddFastEndpointsAndMap_DetectsRoutes()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            app.AddFastEndpoints();
            app.MapFastEndpoints();
            app.Run();
            """);
        fs.AddFile("GetProductsEndpoint.cs", """
            [HttpGet("/products")]
            public sealed class GetProductsEndpoint : Endpoint<EmptyRequest, Product[]>
            {
                public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
                {
                    await SendAsync(new[] { new Product() });
                }
            }
            public sealed class Product { }
            public sealed class EmptyRequest { }
            """);

        var result = await RunOnFilesAsync(fs);
        // The [HttpGet("/products")] attribute on the Endpoint class should be detected
        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/products" && e.HttpMethod == "GET");
    }

    [Fact]
    public async Task TodoApiStyle_ExtensionMethodWithMapX_DetectsEndpoint()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            app.MapTodoApi();
            app.Run();
            """);
        fs.AddFile("TodoApiExtensions.cs", """
            public static class TodoApiExtensions
            {
                public static WebApplication MapTodoApi(this WebApplication app)
                {
                    app.MapGet("/api/todos", () => Results.Ok(new[] { "todo" }));
                    app.MapPost("/api/todos", (CreateTodo cmd) => Results.Created());
                    return app;
                }
            }
            """);

        var result = await RunOnFilesAsync(fs);
        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/api/todos" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/api/todos" && e.HttpMethod == "POST");
    }

    [Fact]
    public async Task HandlerMethodReference_DetectsEndpoint()
    {
        var result = await RunOnSourceAsync("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/hello", HelloHandler);
            app.Run();
            static string HelloHandler() => "hello";
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/hello" && e.HttpMethod == "GET");
        // Handler should reference the method name
        var endpoint = endpoints.First(e => e.RouteTemplate == "/hello");
        Assert.Equal("HelloHandler", endpoint.HandlerMethod);
    }

    [Fact]
    public async Task RouteInterpolation_DetectsEndpoint()
    {
        var result = await RunOnSourceAsync("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            var prefix = "/api";
            app.MapGet($"{prefix}/items", () => Results.Ok(new[] { "item" }));
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.NotEmpty(endpoints);
    }

    [Fact]
    public async Task VersionedApi_WithMapGroup_DetectsRoutesWithPrefix()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("CatalogApi.cs", """
            using Asp.Versioning;

            public static class CatalogApi
            {
                public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app)
                {
                    var vApi = app.NewVersionedApi("Catalog");
                    var api = vApi.MapGroup("api/catalog").HasApiVersion(1, 0);
                    api.MapGet("/items", () => Results.Ok(new[] { "item" }));
                    api.MapGet("/items/{id:int}", (int id) => Results.Ok(id));
                    api.MapPost("/items", (CreateItem cmd) => Results.Created());
                    return app;
                }
            }
            """);

        var result = await RunOnFilesAsync(fs);
        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();

        Assert.Contains(endpoints, e => e.RouteTemplate == "/api/catalog/items" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/api/catalog/items/{id:int}" && e.HttpMethod == "GET");
        Assert.Contains(endpoints, e => e.RouteTemplate == "/api/catalog/items" && e.HttpMethod == "POST");
        Assert.All(endpoints, e => Assert.Equal("api/catalog", e.GroupPrefix));
    }

    [Fact]
    public async Task MiddlewareAndEndpoints_InSameFile_DetectedSeparately()
    {
        var result = await RunOnSourceAsync("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            app.UseHttpsRedirection();
            app.MapGet("/health", () => "ok");
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => e.RouteTemplate == "/health");
    }

    [Fact]
    public void WithoutMinimalApisSignal_ShouldRunReturnsFalse()
    {
        var model = new DiscoveryModel();
        model.Architecture.Seal();

        var extractor = new EndpointExtractor();
        var shouldRun = extractor.ShouldRun(new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = new FakeFileSystem(),
            Cache = new FakeAnalysisCache(new FakeFileSystem()),
            Analysis = new SharedAnalysisContext(),
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        }, model);

        Assert.False(shouldRun);
    }

    [Fact]
    public void WithMinimalApisSignal_ShouldRunReturnsTrue()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));
        model.Architecture.Seal();

        var extractor = new EndpointExtractor();
        var shouldRun = extractor.ShouldRun(new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = new FakeFileSystem(),
            Cache = new FakeAnalysisCache(new FakeFileSystem()),
            Analysis = new SharedAnalysisContext(),
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        }, model);

        Assert.True(shouldRun);
    }

    private static async Task<DiscoveryModel> RunOnSourceAsync(string fileName, string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(fileName, source);
        return await RunOnFilesAsync(fs);
    }

    private static async Task<DiscoveryModel> RunOnFilesAsync(FakeFileSystem fs)
    {
        var cache = new FakeAnalysisCache(fs);
        var allFiles = new List<string>();
        await foreach (var f in fs.EnumerateFilesAsync("", "*", SearchOption.AllDirectories))
            allFiles.Add(f);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));
        model.Architecture.Seal();

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions { MaxOutputTokens = 8000 },
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = new SharedAnalysisContext { AllSourceFiles = allFiles },
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        };

        await new EndpointExtractor().ExtractAsync(ctx, model, CancellationToken.None);
        return model;
    }
}
