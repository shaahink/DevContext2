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
        Assert.Contains(consumers, c => c.ConsumerType == "ShipConsumer" && c.MessageType == "OrderShipped");
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

    // ── B5 (Prism D1.2d): raw queue transports as [approx] channel seams ──

    private static ProjectInfo QueueProject(string package) => new(
        "App", @"src\App\App.csproj", "C#", [], [], [new PackageReferenceInfo(package, "1.0.0")]);

    [Fact]
    public async Task StorageQueue_SenderAndHostedConsumer_BecomeChannelSeam()
    {
        // The podcasts shape: DI registers the named QueueClient, the API sends, a worker receives.
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Api\Program.cs",
            """
            using Azure.Storage.Queues;
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton(new QueueClient(connectionString, "feed-queue"));
            """);
        fs.AddFile(@"src\Api\FeedsApi.cs",
            """
            using Azure.Storage.Queues;
            public class FeedsApi
            {
                public static async ValueTask CreateFeed(QueueClient queueClient, object feed)
                    => await queueClient.SendMessageAsync(new BinaryData(feed));
            }
            """);
        fs.AddFile(@"src\Worker\Worker.cs",
            """
            using Azure.Storage.Queues;
            public class Worker : BackgroundService
            {
                private readonly QueueClient _queueClient;
                protected override async Task ExecuteAsync(CancellationToken ct)
                {
                    var messages = await _queueClient.ReceiveMessagesAsync(10, cancellationToken: ct);
                }
            }
            """);

        var model = new DiscoveryModel();
        model.Projects = [QueueProject("Azure.Storage.Queues")];
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Api\Program.cs", @"src\Api\FeedsApi.cs", @"src\Worker\Worker.cs"];

        var extractor = new EventBusExtractor();
        Assert.True(extractor.ShouldRun(ctx, model));
        await extractor.ExtractAsync(ctx, model, default);

        var publish = Assert.Single(model.Detections.OfType<EventFlowDetection>(), d => d.Kind == "Publish");
        Assert.Equal("queue:AzureStorageQueue:feed-queue", publish.EventType);
        Assert.Equal("FeedsApi", publish.Target);

        var consumer = Assert.Single(model.Detections.OfType<MessageConsumerDetection>());
        Assert.Equal("queue:AzureStorageQueue:feed-queue", consumer.MessageType);
        Assert.Equal("Worker", consumer.ConsumerType);
        Assert.Equal("AzureStorageQueue", consumer.BusKind);
    }

    [Fact]
    public async Task BusImplementationType_PublishAndConsumeInOneClass_IsNotASeam()
    {
        // The eShop EventBusRabbitMQ shape: one class does BasicPublish AND BasicConsume — that is
        // the bus library itself, not an application seam. Nothing may be emitted (pole safety).
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\EventBus\EventBusRabbitMQ.cs",
            """
            using RabbitMQ.Client;
            public class EventBusRabbitMQ
            {
                public async Task Publish(object evt) => await _channel.BasicPublishAsync("x", "rk", body);
                public async Task Start() => await _channel.BasicConsumeAsync("q", true, consumer);
            }
            """);

        var model = new DiscoveryModel();
        model.Projects = [QueueProject("RabbitMQ.Client")];
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\EventBus\EventBusRabbitMQ.cs"];

        var extractor = new EventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        Assert.Empty(model.Detections.OfType<EventFlowDetection>());
        Assert.Empty(model.Detections.OfType<MessageConsumerDetection>());
    }

    [Fact]
    public async Task RabbitMq_SplitPublisherAndListener_JoinOnLiteralChannel()
    {
        // The bitwarden shape: publisher service and listener service are separate classes.
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Core\RabbitMqService.cs",
            """
            using RabbitMQ.Client;
            public class RabbitMqService
            {
                public async Task Send(object msg) => await _channel.BasicPublishAsync("exchange", "events", body);
            }
            """);
        fs.AddFile(@"src\Core\RabbitMqEventListenerService.cs",
            """
            using RabbitMQ.Client;
            public class RabbitMqEventListenerService : BackgroundService
            {
                protected override async Task ExecuteAsync(CancellationToken ct)
                    => await _channel.BasicConsumeAsync("events", true, _consumer);
            }
            """);

        var model = new DiscoveryModel();
        model.Projects = [QueueProject("RabbitMQ.Client")];
        model.Architecture.Seal();

        var (ctx, _) = new DiscoveryContextBuilder()
            .WithFileSystem(fs).WithRootPath(@"C:\repo").BuildWithRecording();
        ctx.Analysis.AllSourceFiles = [@"src\Core\RabbitMqService.cs", @"src\Core\RabbitMqEventListenerService.cs"];

        var extractor = new EventBusExtractor();
        await extractor.ExtractAsync(ctx, model, default);

        var publish = Assert.Single(model.Detections.OfType<EventFlowDetection>());
        var consumer = Assert.Single(model.Detections.OfType<MessageConsumerDetection>());
        Assert.Equal("queue:RabbitMQ:events", publish.EventType);
        Assert.Equal("queue:RabbitMQ:events", consumer.MessageType);
        Assert.Equal("RabbitMqEventListenerService", consumer.ConsumerType);
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
