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
        Assert.False(controller.IsPruned);

        Assert.True(product.PathProximityScore < 1.0f);
    }

    [Fact]
    public async Task PathProximityPruner_AssignsDefaultScoreWhenNoFocus()
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
        Assert.Equal(0.5f, type.PathProximityScore);
    }

    [Fact]
    public async Task TokenBudgetEnforcer_PrunesExcessTypes()
    {
        var model = new DiscoveryModel
        {
            Budget = new TokenBudget { MaxTokens = 50, SafetyMargin = 10 },
        };

        for (var i = 0; i < 20; i++)
        {
            var id = $"MyApp.Type{i}";
            model.Types.TryAdd(id, new TypeDiscovery
            {
                Id = id,
                Name = $"Type{i}",
                Namespace = "MyApp.Services.LongNamespace",
                FilePath = $"C:\\repo\\src\\MyApp\\Type{i}.cs",
                Kind = TypeKind.Class,
                Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Unknown,
                Methods = [
                    new MethodSignature("Method1", "void", ["string", "int", "bool"], ["a", "b", "c"],
                        Microsoft.CodeAnalysis.Accessibility.Public, false, false),
                ],
            });
        }

        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo");
        var (ctx, _) = builder.BuildWithRecording();

        var pruner = new TokenBudgetEnforcer();
        await pruner.PruneAsync(ctx, model, default);

        var surviving = model.Types.Values.Count(t => !t.IsPruned);
        Assert.True(surviving < 20);

        Assert.NotEmpty(model.PruningNotes);
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
        Assert.True(handler.RelevanceScore > 0);
        Assert.False(handler.IsPruned);
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
        var order = model.Types["MyApp.Domain.Order"];

        Assert.True(orderService.RelevanceScore > 0);
        Assert.False(order.IsPruned);
    }
}
