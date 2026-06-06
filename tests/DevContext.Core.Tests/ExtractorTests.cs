using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Tests;

public sealed class ExtractorTests
{
    [Fact]
    public async Task EndpointExtractor_DetectsMinimalApiEndpoints()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/api/products", () => "Hello");
            app.MapPost("/api/products", (CreateProductRequest req) => Results.Created());
            app.MapDelete("/api/products/{id}", (int id) => Results.NoContent());

            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo")
            .WithSignal(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));

        var extractor = new EndpointExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var endpoints = model.Detections.OfType<EndpointDetection>().ToArray();
        Assert.Equal(3, endpoints.Length);

        Assert.Contains(endpoints, e => e.HttpMethod == "GET" && e.RouteTemplate == "/api/products");
        Assert.Contains(endpoints, e => e.HttpMethod == "POST" && e.RouteTemplate == "/api/products");
        Assert.Contains(endpoints, e => e.HttpMethod == "DELETE" && e.RouteTemplate == "/api/products/{id}");
    }

    [Fact]
    public async Task EndpointExtractor_DetectsFastEndpointsEndpoint_WhenSignalPresent()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Endpoints\CreateOrderEndpoint.cs", """
            using FastEndpoints;

            public class CreateOrderEndpoint : Endpoint<CreateOrderRequest, CreateOrderResponse>
            {
                public override void Configure()
                {
                    Post("/api/orders");
                    AllowAnonymous();
                }
                public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
                {
                    await SendAsync(new CreateOrderResponse());
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Endpoints\CreateOrderEndpoint.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.FastEndpoints));

        var extractor = new EndpointExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var detection = Assert.Single(model.Detections.OfType<EndpointDetection>());
        Assert.Equal("POST", detection.HttpMethod);
        Assert.Equal("/api/orders", detection.RouteTemplate);
    }

    [Fact]
    public async Task EndpointExtractor_DetectsFastEndpointsEndpoint_WithHttpAttribute()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\Endpoints\GetOrdersEndpoint.cs", """
            using FastEndpoints;

            [HttpGet("/api/orders")]
            public class GetOrdersEndpoint : EndpointWithoutRequest
            {
                public override async Task HandleAsync(CancellationToken ct)
                {
                    await SendAsync(new List<Order>());
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\Endpoints\GetOrdersEndpoint.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.FastEndpoints));

        var extractor = new EndpointExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var detection = Assert.Single(model.Detections.OfType<EndpointDetection>());
        Assert.Equal("GET", detection.HttpMethod);
        Assert.Equal("/api/orders", detection.RouteTemplate);
    }

    [Fact]
    public async Task EndpointExtractor_DoesNotRunWhenSignalAbsent()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", "app.MapGet(\"/test\", () => \"ok\");");

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new EndpointExtractor();
        var shouldRun = extractor.ShouldRun(ctx, model);

        Assert.False(shouldRun);
    }

    [Fact]
    public async Task ProjectStructureExtractor_HandlesMultiTfm()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
              </PropertyGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel();

        var extractor = new ProjectStructureExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var project = Assert.Single(model.Projects);
        Assert.Contains("net8.0;net9.0", project.TargetFrameworks);
    }

    [Fact]
    public async Task EndpointExtractor_DetectsStaticMethodHandler()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", """
            var app = WebApplication.CreateBuilder(args).Build();
            app.MapGet("/hello", HelloHandler);
            app.Run();
            static string HelloHandler() => "hello";
            """);

        var result = await RunEndpointExtractorAsync(fs);

        var endpoints = result.Detections.OfType<EndpointDetection>().ToList();
        var endpoint = Assert.Single(endpoints);
        Assert.Equal("HelloHandler", endpoint.HandlerMethod);
        Assert.Equal("/hello", endpoint.RouteTemplate);
    }

    [Fact]
    public void FocusPointResolver_ResolvesTypeName()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Services.OrderService", new TypeDiscovery
        {
            Id = "MyApp.Services.OrderService",
            Name = "OrderService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\OrderService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        var unresolved = new List<FocusPoint>
        {
            new(FocusKind.Type, "", "OrderService", null),
        };

        var resolved = DevContext.Core.Resolvers.FocusPointResolver.Resolve(unresolved, model);

        var rp = Assert.Single(resolved);
        Assert.Equal(FocusKind.Type, rp.Kind);
        Assert.Equal(@"C:\repo\src\MyApp\Services\OrderService.cs", rp.FilePath);
    }

    [Fact]
    public void FocusPointResolver_UnknownType_FallsBackWithoutCrash()
    {
        var model = new DiscoveryModel();

        var unresolved = new List<FocusPoint>
        {
            new(FocusKind.Type, "", "NonExistentType", null),
        };

        var resolved = DevContext.Core.Resolvers.FocusPointResolver.Resolve(unresolved, model);

        var rp = Assert.Single(resolved);
        Assert.Equal(FocusKind.Type, rp.Kind);
        Assert.Equal("", rp.FilePath);
    }

    private static async Task<DiscoveryModel> RunEndpointExtractorAsync(FakeFileSystem fs)
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
