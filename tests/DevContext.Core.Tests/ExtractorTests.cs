using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Tests;

public sealed class ExtractorTests
{
    [Fact]
    public async Task DependencyExtractor_DetectsSignalFromPackageRefs()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="MediatR" Version="12.0.0" />
                <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];

        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo(
                    "MyApp",
                    @"C:\repo\src\MyApp\MyApp.csproj",
                    "C#",
                    ["net10.0"],
                    [],
                    [
                        new PackageReferenceInfo("MediatR", "12.0.0"),
                        new PackageReferenceInfo("Microsoft.EntityFrameworkCore", "8.0.0"),
                    ])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.MediatR));
        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.EfCore));

        var mediatR = model.Architecture.Get(ArchitectureSignals.Keys.MediatR);
        Assert.NotNull(mediatR);
        Assert.True(mediatR.Detected);

        var efCore = model.Architecture.Get(ArchitectureSignals.Keys.EfCore);
        Assert.NotNull(efCore);
        Assert.True(efCore.Detected);
    }

    [Fact]
    public async Task SyntaxStructureExtractor_DiscoversTypesFromCsFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Models\Product.cs", """
            namespace MyApp.Models;

            public sealed class Product
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public string GetDisplayName() => Name;
            }
            """);
        fs.AddFile(@"C:\repo\src\MyApp\Services\IProductRepository.cs", """
            namespace MyApp.Services;

            public interface IProductRepository
            {
                Product? GetById(int id);
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [
            @"C:\repo\src\MyApp\Models\Product.cs",
            @"C:\repo\src\MyApp\Services\IProductRepository.cs",
        ];

        var model = new DiscoveryModel();

        var extractor = new SyntaxStructureExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.Equal(2, model.Types.Count);

        Assert.True(model.Types.ContainsKey("MyApp.Models.Product"));
        var product = model.Types["MyApp.Models.Product"];
        Assert.Equal("Product", product.Name);
        Assert.Equal(TypeKind.Class, product.Kind);
        Assert.Equal(2, product.Properties.Length);
        Assert.Single(product.Methods);

        Assert.True(model.Types.ContainsKey("MyApp.Services.IProductRepository"));
        var repo = model.Types["MyApp.Services.IProductRepository"];
        Assert.Equal("IProductRepository", repo.Name);
        Assert.Equal(TypeKind.Interface, repo.Kind);
        Assert.Single(repo.Methods);
    }

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
    public async Task MediatRExtractor_DetectsIRequestHandler()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Handlers\GetProductHandler.cs", """
            namespace MyApp.Handlers;

            public sealed class GetProductHandler : IRequestHandler<GetProductQuery, Product>
            {
                public Task<Product> Handle(GetProductQuery request, CancellationToken ct)
                    => Task.FromResult(new Product());
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo")
            .WithSignal(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR));
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Handlers\GetProductHandler.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR));

        var extractor = new MediatRExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var handlers = model.Detections.OfType<MediatRHandlerDetection>().ToArray();
        Assert.Single(handlers);
        Assert.Equal("GetProductQuery", handlers[0].RequestType);
        Assert.Equal("Product", handlers[0].ResponseType);
        Assert.Equal("GetProductHandler", handlers[0].HandlerType);
        Assert.Equal(MediatRKind.Command, handlers[0].Kind);
    }

    [Fact]
    public async Task EfCoreExtractor_DetectsDbContextAndDbSet()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Data\AppDbContext.cs", """
            namespace MyApp.Data;

            public sealed class AppDbContext : DbContext
            {
                public DbSet<Product> Products { get; set; }
                public DbSet<Order> Orders { get; set; }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo")
            .WithSignal(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore));
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Data\AppDbContext.cs"];

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore));

        var extractor = new EfCoreExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var entities = model.Detections.OfType<EfEntityDetection>().ToArray();
        Assert.Equal(2, entities.Length);

        Assert.Contains(entities, e => e.EntityType == "Product" && e.DbContextType == "AppDbContext");
        Assert.Contains(entities, e => e.EntityType == "Order" && e.DbContextType == "AppDbContext");
    }

    [Fact]
    public async Task ProgramCsFlowExtractor_DetectsMiddlewareAndBackgroundWorkers()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHostedService<OrderProcessingService>();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();
            app.MapGet("/health", () => "OK");
            app.MapPost("/api/orders", (OrderRequest req) => Results.Ok());

            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new ProgramCsFlowExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var middleware = model.Detections.OfType<MiddlewareDetection>().ToArray();
        Assert.Equal(5, middleware.Length);

        var useAuth = middleware.FirstOrDefault(m => m.MiddlewareType == "UseAuthentication");
        Assert.NotNull(useAuth);
        Assert.Equal(MiddlewareKind.UseX, useAuth.Kind);
        Assert.Equal(1, useAuth.PipelineOrder);

        var useCors = middleware.FirstOrDefault(m => m.MiddlewareType == "UseCors");
        Assert.NotNull(useCors);
        Assert.Equal(3, useCors.PipelineOrder);

        var mapGet = middleware.FirstOrDefault(m => m.MiddlewareType == "MapGet");
        Assert.NotNull(mapGet);
        Assert.Equal(MiddlewareKind.MapX, mapGet.Kind);

        var workers = model.Detections.OfType<BackgroundWorkerDetection>().ToArray();
        Assert.Single(workers);
        Assert.Equal("OrderProcessingService", workers[0].ImplementationType);
        Assert.Equal(BackgroundWorkerKind.HostedService, workers[0].Kind);
    }

    [Fact]
    public async Task DiRegistrationExtractor_DetectsServiceRegistrations()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddTransient<INotificationService, EmailNotificationService>();
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            var app = builder.Build();
            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var registrations = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.Equal(4, registrations.Length);

        var singleton = registrations.FirstOrDefault(r => r.Lifetime == "Singleton");
        Assert.NotNull(singleton);
        Assert.Equal("IProductRepository", singleton.ServiceType);
        Assert.Equal("InMemoryProductRepository", singleton.ImplementationType);

        var scoped = registrations.FirstOrDefault(r => r.Lifetime == "Scoped");
        Assert.NotNull(scoped);
        Assert.Equal("IOrderService", scoped.ServiceType);

        var transient = registrations.FirstOrDefault(r => r.Lifetime == "Transient");
        Assert.NotNull(transient);

        var addMediatR = registrations.FirstOrDefault(r => r.Lifetime == "Extension");
        Assert.NotNull(addMediatR);
        Assert.Equal("AddMediatR", addMediatR.ServiceType);
    }

    [Fact]
    public async Task CallGraphExtractor_DiscoversBasicInvocations()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Services\ProductService.cs", """
            namespace MyApp.Services;

            public sealed class ProductService
            {
                private readonly IProductRepository _repo;

                public ProductService(IProductRepository repo) => _repo = repo;

                public Product? GetProduct(int id)
                {
                    return _repo.GetById(id);
                }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Services\ProductService.cs"];
        ctx.Analysis.FocusPoints = [];

        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Services.ProductService", new TypeDiscovery
        {
            Id = "MyApp.Services.ProductService",
            Name = "ProductService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\ProductService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            Methods = [
                new MethodSignature(".ctor", "void", ["IProductRepository"], ["repo"],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
                new MethodSignature("GetProduct", "Product?", ["int"], ["id"],
                    Microsoft.CodeAnalysis.Accessibility.Public, false, false),
            ],
        });

        var extractor = new CallGraphExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.NotEmpty(model.CallEdges);
    }

    [Fact]
    public async Task SourceBodyExtractor_PopulatesSourceBodyForTypes()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Models\Product.cs", """
            namespace MyApp.Models;

            public sealed class Product
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Models\Product.cs"];

        var model = new DiscoveryModel();
        var type = new TypeDiscovery
        {
            Id = "MyApp.Models.Product",
            Name = "Product",
            Namespace = "MyApp.Models",
            FilePath = @"C:\repo\src\MyApp\Models\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        };
        model.Types.TryAdd("MyApp.Models.Product", type);

        var extractor = new SourceBodyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.NotNull(type.SourceBody);
        Assert.Contains("class Product", type.SourceBody);
        Assert.Contains("Id", type.SourceBody);
    }

    [Fact]
    public async Task ProgramCsFlowExtractor_DetectsOrphanPatterns()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors();
            builder.Services.AddAuthentication();

            var app = builder.Build();
            app.UseAuthentication();
            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new ProgramCsFlowExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var diags = model.Diagnostics.ToArray();
        var corsDiag = diags.FirstOrDefault(d =>
            d.Message.Contains("AddCors") && d.Message.Contains("UseCors"));

        Assert.NotNull(corsDiag);
        Assert.Equal(DiagnosticLevel.Info, corsDiag.Level);
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
    public async Task DiRegistrationExtractor_DetectsChainedExtensions()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Program.cs", """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services
                .AddCors()
                .AddMemoryCache()
                .AddSingleton<ICache, MemoryCache>()
                .AddScoped<IUserService, UserService>();
            var app = builder.Build();
            app.Run();
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Program.cs"];

        var model = new DiscoveryModel();

        var extractor = new DiRegistrationExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var registrations = model.Detections.OfType<DiRegistrationDetection>().ToArray();
        Assert.NotEmpty(registrations);

        var singleton = registrations.FirstOrDefault(r => r.Lifetime == "Singleton");
        Assert.NotNull(singleton);
        Assert.Equal("ICache", singleton.ServiceType);
    }

    [Fact]
    public async Task DependencyExtractor_DetectsFastEndpointsPackage()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="FastEndpoints" Version="5.0.0" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [],
                    [new PackageReferenceInfo("FastEndpoints", "5.0.0")])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.FastEndpoints));
    }

    [Fact]
    public async Task DependencyExtractor_DetectsCpmPackageReference()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="MediatR" />
              </ItemGroup>
            </Project>
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [];
        ctx.Analysis.AllProjectFiles = [@"C:\repo\src\MyApp\MyApp.csproj"];
        ctx.Cache.RegisterPath(@"C:\repo\src\MyApp\MyApp.csproj");

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [], [])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);
    }

    [Fact]
    public async Task DependencyExtractor_DetectsSignalFromWebSdk()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
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

        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp", @"C:\repo\src\MyApp\MyApp.csproj", "C#", ["net10.0"], [], [])
            ],
        };

        var extractor = new DependencyExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.MinimalApis));
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
    public async Task SyntaxStructureExtractor_DetectsControllerBase_SetsControllerSignal()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\repo\src\MyApp\Controllers\ProductsController.cs", """
            namespace MyApp.Controllers;
            public sealed class ProductsController : ControllerBase
            {
                public IActionResult Get() => Ok();
            }
            """);

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.AllSourceFiles = [@"C:\repo\src\MyApp\Controllers\ProductsController.cs"];

        var model = new DiscoveryModel();

        var extractor = new SyntaxStructureExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.True(model.Architecture.Has(ArchitectureSignals.Keys.Controllers));
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
