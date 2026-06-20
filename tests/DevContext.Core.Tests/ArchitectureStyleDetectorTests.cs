namespace DevContext.Core.Tests;

public sealed class ArchitectureStyleDetectorTests
{
    private static ProjectInfo Project(string name) =>
        new(name, $"{name}.csproj", "C#", [], [], []);

    [Fact]
    public void ControllerBased_when_controllers_stronger_than_minimal_apis()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Controllers, 0.9f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Projects = [Project("WebApp")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.ControllerBased, style);
    }

    [Fact]
    public void MinimalApi_when_no_controllers_signal()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Projects = [Project("WebApp")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.MinimalApi, style);
    }

    [Fact]
    public void NLayer_when_efcore_and_multi_project()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects = [Project("Web"), Project("Core"), Project("Infrastructure")];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.NLayer, style);
    }

    [Fact]
    public void CleanArchitecture_when_mediatr_and_named_layers()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Projects = [
            Project("MyApp.Domain"),
            Project("MyApp.Application"),
            Project("MyApp.Infrastructure")
        ];

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.CleanArchitecture, style);
    }

    [Fact]
    public void TodoApi_shape_is_MinimalApi_not_CleanArchitecture()
    {
        // Single project, Minimal API signal, no MediatR → MinimalApi
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.9f));
        model.Projects = [Project("TodoApi")];

        var (style, _, _) = ArchitectureStyleDetector.Detect(model);
        Assert.Equal(ArchitectureStyle.MinimalApi, style);
    }

    [Fact]
    public void VerticalSlice_shape_is_CleanArchitecture_not_MinimalApi()
    {
        // MediatR + FastEndpoints + DDD folder roles + aggregates → CleanArchitecture or VerticalSlices
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.FastEndpoints, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects = [
            Project("CleanArchitecture.Api"),
            Project("CleanArchitecture.Core"),
            Project("CleanArchitecture.Infrastructure")
        ];
        // Add types in DDD folder conventions
        model.Types.TryAdd("CleanArchitecture.Core.ContributorAggregate.Contributor", new TypeDiscovery
        {
            Id = "CleanArchitecture.Core.ContributorAggregate.Contributor",
            Name = "Contributor",
            Namespace = "CleanArchitecture.Core.ContributorAggregate",
            FilePath = @"C:\repo\src\CleanArchitecture.Core\ContributorAggregate\Contributor.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Domain,
        });
        model.Types.TryAdd("CleanArchitecture.UseCases.Contributors.List.Handler", new TypeDiscovery
        {
            Id = "CleanArchitecture.UseCases.Contributors.List.Handler",
            Name = "Handler",
            Namespace = "CleanArchitecture.UseCases.Contributors.List",
            FilePath = @"C:\repo\src\CleanArchitecture.UseCases\Contributors\List\Handler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
        });
        model.Detections.Add(new MediatRHandlerDetection("ListContributorsQuery", "Result", "Handler", MediatRKind.Query)
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\CleanArchitecture.UseCases\Contributors\List\Handler.cs", LineNumber = 10,
        });
        model.Detections.Add(new EfEntityDetection("Contributor", "AppDbContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"C:\repo\src\CleanArchitecture.Core\ContributorAggregate\Contributor.cs", LineNumber = 5,
        });

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        // Should be CleanArchitecture or VerticalSlices — NOT MinimalApi
        Assert.NotEqual(ArchitectureStyle.MinimalApi, style);
        Assert.True(style is ArchitectureStyle.CleanArchitecture or ArchitectureStyle.VerticalSlices,
            $"Expected CleanArchitecture or VerticalSlices, got {style} ({via})");
        Assert.True(confidence > 0.5f, $"Confidence {confidence} should be > 0.5");
    }

    [Fact]
    public void MediatR_handlers_in_code_outrank_MinimalApi_without_a_package_signal()
    {
        // eShop's Ordering.API scoped alone: the MediatR package signal isn't lit (package lives in a
        // sibling project), but the handler types are right here. The style must read CleanArchitecture
        // off those handlers, not fall through to MinimalApi (assessment G7). Detections are NOT used —
        // the style detector runs before the Stage-3 MediatR extractor — so this proves the type path.
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.9f));
        model.Projects = [Project("Ordering.API")];
        model.Types.TryAdd("Ordering.API.Application.CreateOrderCommandHandler", new TypeDiscovery
        {
            Id = "Ordering.API.Application.CreateOrderCommandHandler",
            Name = "CreateOrderCommandHandler",
            Namespace = "Ordering.API.Application.Commands",
            FilePath = @"C:\repo\src\Ordering.API\Application\Commands\CreateOrderCommandHandler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            ImplementedInterfaces = ["IRequestHandler<CreateOrderCommand, bool>"],
        });
        model.Types.TryAdd("Ordering.API.Application.OrderPaidDomainEventHandler", new TypeDiscovery
        {
            Id = "Ordering.API.Application.OrderPaidDomainEventHandler",
            Name = "OrderPaidDomainEventHandler",
            Namespace = "Ordering.API.Application.DomainEventHandlers",
            FilePath = @"C:\repo\src\Ordering.API\Application\DomainEventHandlers\OrderPaidDomainEventHandler.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            ImplementedInterfaces = ["INotificationHandler<OrderPaidDomainEvent>"],
        });

        var (style, _, via) = ArchitectureStyleDetector.Detect(model);

        Assert.Equal(ArchitectureStyle.CleanArchitecture, style);
        Assert.Contains("domain-event handlers", via);
    }

    [Fact]
    public void EShop_shape_is_microservices_not_MinimalApi()
    {
        // Aspire + many projects → Microservices
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.Aspire, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MediatR, 1.0f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis, 0.8f));
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.EfCore, 1.0f));
        model.Projects = [
            Project("Ordering.API"),
            Project("Ordering.Domain"),
            Project("Ordering.Application"),
            Project("Ordering.Infrastructure"),
            Project("Basket.API"),
            Project("Payment.API"),
            Project("Catalog.API"),
            Project("EventBus"),
            Project("ServiceDefaults"),
            Project("AppHost"),
        ];
        model.Detections.Add(new MediatRHandlerDetection("CreateOrderCommand", "bool", "CreateOrderCommandHandler", MediatRKind.Command)
        {
            ExtractorName = "test", SourceFile = "Ordering.Application/Commands/CreateOrderCommandHandler.cs", LineNumber = 20,
        });
        model.Detections.Add(new EfEntityDetection("Order", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "test", SourceFile = @"Ordering.Domain/AggregatesModel/Order.cs", LineNumber = 5,
        });

        var (style, confidence, via) = ArchitectureStyleDetector.Detect(model);
        Assert.NotEqual(ArchitectureStyle.MinimalApi, style);
        Assert.True(style is ArchitectureStyle.Microservices or ArchitectureStyle.CleanArchitecture,
            $"Expected Microservices or CleanArchitecture, got {style} ({via})");
    }
}
