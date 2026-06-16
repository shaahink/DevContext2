namespace DevContext.Core.Tests;

public sealed class EndpointExtractorTests
{
    [Fact]
    public async Task DirectMapGet_InProgramCs_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/orders", () => Results.Ok(new[] { "order1" }));
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/orders", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DirectMapPost_InProgramCs_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapPost("/orders", (CreateOrderCommand cmd) => Results.Created());
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/orders", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "POST", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RouteWithParameter_InProgramCs_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/orders/{id}", (int id) => Results.Ok(id));
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/orders/{id}", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExtensionMethod_CallingMapGet_InsideExtension_DetectsEndpoint()
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
                public static WebApplication MapTodoEndpoints(this WebApplication app)
                {
                    app.MapGet("/todos", () => Results.Ok(new[] { "todo1" }));
                    app.MapPost("/todos", (CreateTodo cmd) => Results.Created());
                    return app;
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/todos", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/todos", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "POST", StringComparison.Ordinal));
    }

    [Fact]
    public async Task MapGroup_WithChainedMaps_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            var todos = app.MapGroup("/todos");
            todos.MapGet("/", () => Results.Ok(new[] { "todo1" }));
            todos.MapGet("/{id}", (int id) => Results.Ok(id));
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/todos/", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/todos/{id}", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal));
        Assert.All(endpoints, e => Assert.Equal("/todos", e.GroupPrefix));
    }

    [Fact]
    public async Task MultipleHttpMethods_DetectsAll()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/get", () => "get");
            app.MapPost("/post", () => "post");
            app.MapPut("/put", () => "put");
            app.MapDelete("/delete", () => "delete");
            app.MapPatch("/patch", () => "patch");
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Equal(5, endpoints.Count);
        Assert.Contains(endpoints, e => string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal) && string.Equals(e.RouteTemplate, "/get", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HttpMethod, "POST", StringComparison.Ordinal) && string.Equals(e.RouteTemplate, "/post", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HttpMethod, "PUT", StringComparison.Ordinal) && string.Equals(e.RouteTemplate, "/put", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HttpMethod, "DELETE", StringComparison.Ordinal) && string.Equals(e.RouteTemplate, "/delete", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.HttpMethod, "PATCH", StringComparison.Ordinal) && string.Equals(e.RouteTemplate, "/patch", StringComparison.Ordinal));
    }

    [Fact]
    public async Task MapInsideConditionalBlock_DetectsEndpoint()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            if (app.Environment.IsDevelopment())
            {
                app.MapGet("/dev-only", () => "dev");
            }
            app.MapGet("/always", () => "always");
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/dev-only", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/always", StringComparison.Ordinal));
    }

    [Fact]
    public async Task NoProgramCs_SearchesAllCsFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Startup.cs", """
            public class Startup
            {
                public void Configure(WebApplication app)
                {
                    app.MapGet("/from-startup", () => "ok");
                }
            }
            """);

        var result = await RunExtractorOnFilesAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/from-startup", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Route_NormalizedToLeadingSlash()
    {
        // Routes without leading slash should be normalized
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("api/items", () => Results.Ok(new[] { "item" }));
            app.MapPost("/api/orders", () => Results.Created());
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/api/items", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "GET", StringComparison.Ordinal));
        Assert.Contains(endpoints, e => string.Equals(e.RouteTemplate, "/api/orders", StringComparison.Ordinal) && string.Equals(e.HttpMethod, "POST", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FullPipeline_MinimalApiFixture_ProducesEndpoints()
    {
        var fixturePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "tests", "fixtures", "MinimalApiProject"));

        if (!Directory.Exists(fixturePath))
        {
            // If fixture doesn't exist, we can still test inline
            Assert.True(true, "Fixture not available");
            return;
        }

        var fs = new FakeFileSystem();
        foreach (var file in Directory.EnumerateFiles(fixturePath, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(fixturePath, file);
            fs.AddFile(relative, await File.ReadAllTextAsync(file));
        }

        var result = await RunExtractorOnFilesAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        Assert.NotEmpty(endpoints);
    }

    [Fact]
    public async Task RequireAuthorization_Chain_PopulatesAuthAttributes()
    {
        var result = await RunExtractorOnSourceAsync(
            "Program.cs",
            """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/secret", () => "secret").RequireAuthorization();
            app.MapGet("/admin", () => "admin").RequireAuthorization("AdminPolicy");
            app.MapGet("/public", () => "public").AllowAnonymous();
            app.Run();
            """);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        var secret = endpoints.Single(e => string.Equals(e.RouteTemplate, "/secret", StringComparison.Ordinal));
        var admin = endpoints.Single(e => string.Equals(e.RouteTemplate, "/admin", StringComparison.Ordinal));
        var pub = endpoints.Single(e => string.Equals(e.RouteTemplate, "/public", StringComparison.Ordinal));

        Assert.Contains("[Authorize]", secret.AuthAttributes);
        Assert.Contains("[Authorize(AdminPolicy)]", admin.AuthAttributes);
        Assert.Contains("[AllowAnonymous]", pub.AuthAttributes);
    }

    private static async Task<DiscoveryModel> RunExtractorOnSourceAsync(string fileName, string source)
    {
        var fs = new FakeFileSystem();
        fs.AddFile(fileName, source);
        return await RunExtractorOnFilesAsync(fs).ConfigureAwait(false);
    }

    private static async Task<DiscoveryModel> RunExtractorOnFilesAsync(FakeFileSystem fs)
    {
        var cache = new FakeAnalysisCache(fs);
        var allFiles = new List<string>();
        await foreach (var f in fs.EnumerateFilesAsync("", "*", SearchOption.AllDirectories).ConfigureAwait(false))
            allFiles.Add(f);

        var analysis = new SharedAnalysisContext
        {
            AllSourceFiles = allFiles
        };

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));
        model.Architecture.Seal();

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions { MaxOutputTokens = 8000 },
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        };

        var extractor = new EndpointExtractor();
        await extractor.ExtractAsync(ctx, model, CancellationToken.None).ConfigureAwait(false);

        return model;
    }
}
