namespace DevContext.Core.Tests;

public sealed class EventBusExtractorTests
{
    [Fact]
    public async Task Detects_MassTransit_ConsumerImplementation()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Consumers\OrderConsumer.cs",
            """
            using MassTransit;
            public class OrderConsumer : IConsumer<OrderPlaced>
            {
                public Task Consume(ConsumeContext<OrderPlaced> context) => Task.CompletedTask;
            }
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MassTransit));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Consumers\OrderConsumer.cs"];

        var extractor = new EventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var consumers = model.Detections.OfType<MessageConsumerDetection>().ToList();
        Assert.Single(consumers);
        Assert.Equal("OrderConsumer", consumers[0].ConsumerType);
        Assert.Equal("OrderPlaced", consumers[0].MessageType);
        Assert.Equal("MassTransit", consumers[0].BusKind);
    }

    [Fact]
    public async Task Detects_MassTransit_ConsumerWithIConsumer()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Consumers\ShipConsumer.cs",
            """
            using MassTransit;
            public class ShipConsumer : IConsumer<OrderShipped>
            {
                public Task Consume(ConsumeContext<OrderShipped> context) => Task.CompletedTask;
            }
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MassTransit));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Consumers\ShipConsumer.cs"];

        var extractor = new EventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var consumers = model.Detections.OfType<MessageConsumerDetection>().ToList();
        Assert.Contains(consumers, c => string.Equals(c.ConsumerType, "ShipConsumer", StringComparison.Ordinal) && string.Equals(c.MessageType, "OrderShipped", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Detects_NServiceBus_ConsumerPattern()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Handlers\OrderHandler.cs",
            """
            using MassTransit;
            public class OrderHandler : IConsumer<OrderCreated>
            {
                public Task Consume(ConsumeContext<OrderCreated> context) => Task.CompletedTask;
            }
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.NServiceBus));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Handlers\OrderHandler.cs"];

        var extractor = new EventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var consumers = model.Detections.OfType<MessageConsumerDetection>().ToList();
        Assert.Single(consumers);
        Assert.Equal("NServiceBus", consumers[0].BusKind);
        Assert.Equal("OrderCreated", consumers[0].MessageType);
    }

    [Fact]
    public async Task Ignores_NonConsumer_Classes()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\OrderService.cs",
            """
            public class OrderService
            {
                public void ShipOrder() { }
            }
            """);

        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MassTransit));
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Services\OrderService.cs"];

        var extractor = new EventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var consumers = model.Detections.OfType<MessageConsumerDetection>().ToList();
        Assert.Empty(consumers);
    }

    [Fact]
    public async Task SignalGate_NoMassTransitOrNServiceBus_ReturnsNoDetections()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Services\SomeClass.cs",
            """
            using MassTransit;
            public class SomeClass : IConsumer<Event> { }
            """);

        var model = new DiscoveryModel();
        // No signal registered — extractor should not run

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();

        Assert.False(new EventBusExtractor().ShouldRun(ctx, model));
    }
}
