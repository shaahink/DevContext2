namespace DevContext.Core.Tests;

public sealed class InMemoryEventBusExtractorTests
{
    [Fact]
    public async Task Detects_EventHandler_Implementation()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("App.Handlers.OrderPlacedHandler", new TypeDiscovery
        {
            Id = "App.Handlers.OrderPlacedHandler", Name = "OrderPlacedHandler",
            Namespace = "App.Handlers", FilePath = @"src\Handlers\OrderPlacedHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            ImplementedInterfaces = ["IEventHandler<OrderPlaced>"],
        });

        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Handlers\OrderPlacedHandler.cs", "// handler impl");

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Handlers\OrderPlacedHandler.cs"];

        var extractor = new InMemoryEventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var flows = model.Detections.OfType<EventFlowDetection>().ToList();
        Assert.Contains(flows, f => f.EventType == "OrderPlaced" && f.Target == "OrderPlacedHandler");
    }

    [Fact]
    public async Task Detects_EventBusSubscribe_Call()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("App.Handlers.OrderHandler", new TypeDiscovery
        {
            Id = "App.Handlers.OrderHandler", Name = "OrderHandler",
            Namespace = "App.Handlers", FilePath = @"src\Handlers\OrderHandler.cs",
            Kind = TypeKind.Class, Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });

        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\EventBusSetup.cs",
            """
            public class EventBusSetup
            {
                public static void Configure(IEventBus bus)
                {
                    bus.Subscribe<OrderPlaced>(handler);
                }
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\EventBusSetup.cs"];

        var extractor = new InMemoryEventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var flows = model.Detections.OfType<EventFlowDetection>().Where(f => f.Kind == "Subscribe").ToList();
        Assert.Contains(flows, f => f.EventType == "OrderPlaced");
    }

    [Fact]
    public async Task Detects_PublishAsync_Call()
    {
        var model = new DiscoveryModel();
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\Publisher.cs",
            """
            public class Publisher
            {
                public async Task Publish(IEventBus bus)
                {
                    await bus.Publish<OrderCreated>(new OrderCreated());
                }
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\Publisher.cs"];

        var extractor = new InMemoryEventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var publishes = model.Detections.OfType<EventFlowDetection>().Where(f => f.Kind == "Publish").ToList();
        Assert.Contains(publishes, f => f.EventType == "OrderCreated");
    }

    [Fact]
    public async Task Ignores_UnrelatedPublish_MethodNames()
    {
        var model = new DiscoveryModel();
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\SomeService.cs",
            """
            public class SomeService
            {
                public void PublishSomething() { }
                public async Task PublishAsync(string message) { }
            }
            """);

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\SomeService.cs"];

        var extractor = new InMemoryEventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var publishes = model.Detections.OfType<EventFlowDetection>().Where(f => f.Kind == "Publish").ToList();
        Assert.Empty(publishes);
    }
}
