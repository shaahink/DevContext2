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
        Assert.True(publicService.RelevanceScore > 0, "Public type should be boosted in library mode");
        // Test type in a test project should be penalized
        Assert.True(testType.RelevanceScore < 0, "Test type should be penalized in library mode");
        // Public type should have higher score than test type
        Assert.True(publicService.RelevanceScore > testType.RelevanceScore);
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
        var testType = model.Types["MyApp.Tests.MyServiceTests"];

        Assert.Equal(0, publicService.RelevanceScore);
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

    [Fact]
    public async Task TokenBudgetEnforcer_MaxSurvivingTypes_CapsAtScenarioLimit()
    {
        var model = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = 100_000 } };
        for (int i = 0; i < 10; i++)
        {
            model.Types.TryAdd($"MyApp.Type{i}", new TypeDiscovery
            {
                Id = $"MyApp.Type{i}",
                Name = $"Type{i}",
                Namespace = "MyApp",
                FilePath = $@"C:\repo\src\Type{i}.cs",
                Kind = TypeKind.Class,
                Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Domain,
            });
        }

        // debug-endpoint scenario has MaxSurvivingTypes = 20, but here we use a small budget scenario
        var scenario = ScenarioRegistry.BuiltIn["deep-dive"]; // MaxSurvivingTypes = 25
        var builder = new DiscoveryContextBuilder()
            .WithRootPath(@"C:\repo")
            .WithScenario(scenario);
        var (ctx, _) = builder.BuildWithRecording();

        var pruner = new TokenBudgetEnforcer();
        await pruner.PruneAsync(ctx, model, default);

        // deep-dive has MaxSurvivingTypes=25 and we have 10 types → all survive
        var surviving = model.Types.Values.Count(t => !t.IsPruned);
        Assert.Equal(10, surviving);

        // Now create a model with 50 types and use debug-endpoint (MaxSurvivingTypes=20)
        var model2 = new DiscoveryModel { Budget = new TokenBudget { MaxTokens = 100_000 } };
        for (int i = 0; i < 50; i++)
        {
            model2.Types.TryAdd($"MyApp.Big{i}", new TypeDiscovery
            {
                Id = $"MyApp.Big{i}",
                Name = $"Big{i}",
                Namespace = "MyApp",
                FilePath = $@"C:\repo\src\Big{i}.cs",
                Kind = TypeKind.Class,
                Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
                Layer = ArchitectureLayer.Domain,
            });
        }

        var pruner2 = new TokenBudgetEnforcer();
        await pruner2.PruneAsync(ctx, model2, default);

        var surviving2 = model2.Types.Values.Count(t => !t.IsPruned);
        Assert.Equal(25, surviving2); // capped at deep-dive MaxSurvivingTypes=25
        Assert.Contains(model2.PruningNotes, n => n.Contains("capped at 25 types"));
    }
}
