namespace DevContext.Core.Tests;

public sealed class PrunerTests
{
    [Fact]
    public async Task PathProximityPruner_PrunesByDirectoryDistance()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Api.ProductsController", new TypeDiscovery
        {
            Id = "MyApp.Api.ProductsController",
            Name = "ProductsController",
            Namespace = "MyApp.Api",
            FilePath = @"C:\repo\src\MyApp\Api\ProductsController.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Api,
        });
        model.Types.TryAdd("MyApp.Domain.Product", new TypeDiscovery
        {
            Id = "MyApp.Domain.Product",
            Name = "Product",
            Namespace = "MyApp.Domain",
            FilePath = @"C:\repo\src\MyApp\Domain\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });

        var focus = new FocusPoint(FocusKind.File,
            @"C:\repo\src\MyApp\Api\ProductsController.cs",
            "ProductsController", null);

        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.FocusPoints = [focus];

        var pruner = new PathProximityPruner();
        await pruner.PruneAsync(ctx, model, default);

        var controller = model.Types["MyApp.Api.ProductsController"];
        var product = model.Types["MyApp.Domain.Product"];

        Assert.Equal(1.0f, controller.PathProximityScore);

        Assert.True(product.PathProximityScore < 1.0f);
    }

    [Fact]
    public async Task PathProximityPruner_NoFocus_LeavesScoreAtZero()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Models.Product", new TypeDiscovery
        {
            Id = "MyApp.Models.Product",
            Name = "Product",
            Namespace = "MyApp.Models",
            FilePath = @"C:\repo\src\MyApp\Models\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });

        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.FocusPoints = [];

        var pruner = new PathProximityPruner();
        await pruner.PruneAsync(ctx, model, default);

        var type = model.Types["MyApp.Models.Product"];
        Assert.Equal(0f, type.PathProximityScore); // no focus → stays 0
    }

    [Fact]
    public async Task PatternRelevancePruner_BoostsDetectionTypes()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("MyApp.Handlers.CreateOrderHandler", new TypeDiscovery
        {
            Id = "MyApp.Handlers.CreateOrderHandler",
            Name = "CreateOrderHandler",
            Namespace = "MyApp.Handlers",
            FilePath = @"C:\repo\src\MyApp\Handlers\CreateOrderHandler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("MyApp.Models.Product", new TypeDiscovery
        {
            Id = "MyApp.Models.Product",
            Name = "Product",
            Namespace = "MyApp.Models",
            FilePath = @"C:\repo\src\MyApp\Models\Product.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });

        model.Detections.Add(new MediatRHandlerDetection(
            RequestType: "CreateOrderCommand",
            ResponseType: "OrderId",
            HandlerType: "CreateOrderHandler",
            Kind: MediatRKind.Command)
        {
            ExtractorName = "MediatRExtractor",
            SourceFile = @"C:\repo\src\MyApp\Handlers\CreateOrderHandler.cs",
            LineNumber = 5,
        });

        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        var pruner = new PatternRelevancePruner();
        await pruner.PruneAsync(ctx, model, default);

        var handler = model.Types["MyApp.Handlers.CreateOrderHandler"];
        Assert.True(handler.RoleScore > 0);
    }

    [Fact]
    public async Task PatternRelevancePruner_LibraryMode_BoostsPublicTypes()
    {
        // No web signals present — library mode should activate
        var model = new DiscoveryModel
        {
            Projects = [
                new ProjectInfo("MyApp.Tests", @"C:\repo\src\MyApp.Tests\MyApp.Tests.csproj", "C#", ["net10.0"], [], [])
            ],
        };
        model.Types.TryAdd("MyApp.Services.PublicService", new TypeDiscovery
        {
            Id = "MyApp.Services.PublicService",
            Name = "PublicService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\PublicService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("MyApp.Tests.MyServiceTests", new TypeDiscovery
        {
            Id = "MyApp.Tests.MyServiceTests",
            Name = "MyServiceTests",
            Namespace = "MyApp.Tests",
            FilePath = @"C:\repo\src\MyApp.Tests\MyServiceTests.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Internal,
            Layer = ArchitectureLayer.Testing,
        });

        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        var pruner = new PatternRelevancePruner();
        await pruner.PruneAsync(ctx, model, default);

        var publicService = model.Types["MyApp.Services.PublicService"];
        var testType = model.Types["MyApp.Tests.MyServiceTests"];

        // Public type should get a boost (library mode)
        Assert.True(publicService.RoleScore > 0, "Public type should be boosted in library mode");
        // Test type in a test project should be penalized
        Assert.True(testType.RoleScore < publicService.RoleScore, "Test type should have lower score than public type");
    }

    [Fact]
    public async Task PatternRelevancePruner_LibraryMode_SkipsWhenWebSignalsPresent()
    {
        // Web signals present — library mode should NOT activate
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 1.0f, "test"));
        model.Architecture.Seal();

        model.Types.TryAdd("MyApp.Services.PublicService", new TypeDiscovery
        {
            Id = "MyApp.Services.PublicService",
            Name = "PublicService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\PublicService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Types.TryAdd("MyApp.Tests.MyServiceTests", new TypeDiscovery
        {
            Id = "MyApp.Tests.MyServiceTests",
            Name = "MyServiceTests",
            Namespace = "MyApp.Tests",
            FilePath = @"C:\repo\src\MyApp\Tests\MyServiceTests.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Testing,
        });

        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        var pruner = new PatternRelevancePruner();
        await pruner.PruneAsync(ctx, model, default);

        // With web signals, scores should remain 0 (no library mode boost)
        var publicService = model.Types["MyApp.Services.PublicService"];

        Assert.Equal(0, publicService.RoleScore);
    }

    [Fact]
    public async Task CallReachabilityPruner_BoostsReachableTypes()
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
            PathProximityScore = 0.0f,
        });
        model.Types.TryAdd("MyApp.Domain.Order", new TypeDiscovery
        {
            Id = "MyApp.Domain.Order",
            Name = "Order",
            Namespace = "MyApp.Domain",
            FilePath = @"C:\repo\src\MyApp\Domain\Order.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
            PathProximityScore = 0.5f,
        });

        var focus = new FocusPoint(FocusKind.Method,
            @"C:\repo\src\MyApp\Services\OrderService.cs",
            "OrderService", "ProcessOrder");

        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        ctx.Analysis.FocusPoints = [focus];
        ctx.Analysis.CallGraph = new CallGraph(new Dictionary<string, ImmutableArray<CallEdge>>());

        var pruner = new CallReachabilityPruner();
        await pruner.PruneAsync(ctx, model, default);

        var orderService = model.Types["MyApp.Services.OrderService"];
        Assert.True(orderService.GraphProximity > 0);
    }

}
