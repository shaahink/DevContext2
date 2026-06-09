using DevContext.Core.Rendering;

namespace DevContext.Core.Tests;

public sealed class RendererTests
{
    [Fact]
    public async Task MarkdownRenderer_GeneratesSectionStructure()
    {
        var model = new DiscoveryModel();
        var options = new RenderOptions(false, false, 8000);

        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.NotNull(result.Content);
        Assert.Contains("Architecture overview", result.Content);
        // Library mode (no detections): uses namespace-grouped output
        Assert.Contains("Types by namespace", result.Content);
        Assert.Contains("Schema v2.0", result.Content);
        Assert.Equal("2.0", result.SchemaVersion);
    }

    [Fact]
    public async Task MarkdownRenderer_HandlesEmptyModel()
    {
        var model = new DiscoveryModel();
        var options = new RenderOptions(false, false, 8000);

        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("No projects discovered", result.Content);
        Assert.Contains("No endpoints detected", result.Content);
        Assert.Contains("No types discovered", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_IncludesDiagnosticsWhenRequested()
    {
        var model = new DiscoveryModel();
        model.AddDiagnostic(DiagnosticLevel.Info, "TestExtractor", "Test diagnostic message");

        var options = new RenderOptions(false, true, 8000);

        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Diagnostics", result.Content);
        Assert.Contains("TestExtractor", result.Content);
        Assert.Contains("Test diagnostic message", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_ShowsArchitectureStyle()
    {
        var model = new DiscoveryModel
        {
            DetectedStyle = ArchitectureStyle.CleanArchitecture,
            StyleConfidence = 0.85f,
            StyleDetectedVia = "ArchitectureStyleDetector",
        };

        var options = new RenderOptions(false, false, 8000);

        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("CleanArchitecture", result.Content);
        Assert.Contains("85%", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_ListsSignals()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(
            ArchitectureSignals.Keys.MediatR, 1.0f, "PackageReference", "MediatR"));
        model.Architecture.Register(FeatureSignal.CreateDetected(
            ArchitectureSignals.Keys.EfCore, 1.0f, "PackageReference", "Microsoft.EntityFrameworkCore"));

        var options = new RenderOptions(false, false, 8000);

        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains(ArchitectureSignals.Keys.MediatR, result.Content);
        Assert.Contains(ArchitectureSignals.Keys.EfCore, result.Content);
    }

    [Fact]
    public async Task JsonContextRenderer_IncludesSchemaVersion()
    {
        var model = new DiscoveryModel();
        var options = new RenderOptions(false, false, 8000);

        var renderer = new JsonContextRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.NotNull(result.Content);
        Assert.Contains("schemaVersion", result.Content);
        Assert.Contains("2.0", result.Content);
        Assert.Equal("2.0", result.SchemaVersion);
    }

    [Fact]
    public async Task JsonContextRenderer_IncludesTypeCounts()
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
        model.Types.TryAdd("MyApp.Services.ProductService", new TypeDiscovery
        {
            Id = "MyApp.Services.ProductService",
            Name = "ProductService",
            Namespace = "MyApp.Services",
            FilePath = @"C:\repo\src\MyApp\Services\ProductService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Application,
            IsPruned = true,
        });

        var options = new RenderOptions(false, false, 8000);

        var renderer = new JsonContextRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("typesSummary", result.Content);
        Assert.Contains("\"found\": 2", result.Content);
        Assert.Contains("\"inOutput\": 1", result.Content);
        Assert.Contains("\"prunedPercent\": 50", result.Content);
    }

    [Fact]
    public async Task JsonContextRenderer_IncludesDetections()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection(
            "GET", "/api/products", "<Program>", "<lambda>", [], [])
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = @"C:\repo\src\MyApp\Program.cs",
            LineNumber = 5,
        });

        var options = new RenderOptions(false, false, 8000);

        var renderer = new JsonContextRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("detections", result.Content);
        Assert.Contains("EndpointDetection", result.Content);
    }

    [Fact]
    public async Task JsonContextRenderer_IncludesSignals()
    {
        var model = new DiscoveryModel();
        model.Architecture.Register(FeatureSignal.CreateDetected(ArchitectureSignals.Keys.MinimalApis));

        var options = new RenderOptions(false, false, 8000);

        var renderer = new JsonContextRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("signals", result.Content);
        Assert.Contains(ArchitectureSignals.Keys.MinimalApis, result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_LibraryMode_ShowsNamespaceSummary()
    {
        var model = new DiscoveryModel();
        model.Types.TryAdd("Lib.Services.MyService", new TypeDiscovery
        {
            Id = "Lib.Services.MyService",
            Name = "MyService",
            Namespace = "Lib.Services",
            FilePath = @"C:\repo\src\Lib\Services\MyService.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Public,
            Layer = ArchitectureLayer.Unknown,
        });
        model.Types.TryAdd("Lib.Services.InternalHelper", new TypeDiscovery
        {
            Id = "Lib.Services.InternalHelper",
            Name = "InternalHelper",
            Namespace = "Lib.Services",
            FilePath = @"C:\repo\src\Lib\Services\InternalHelper.cs",
            Kind = TypeKind.Class,
            Accessibility = Microsoft.CodeAnalysis.Accessibility.Internal,
            Layer = ArchitectureLayer.Unknown,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        // In library mode (no detections), output should show namespace group
        Assert.Contains("Types by namespace", result.Content);
        Assert.Contains("Lib.Services", result.Content);
        Assert.Contains("MyService", result.Content);
        // Should mention 2 types, 1 public
        Assert.Contains("2 types (1 public)", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_ShowsEfEntitiesSection()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EfEntityDetection("Product", "CatalogContext", true, ["Id"])
        {
            ExtractorName = "EfCoreExtractor",
            SourceFile = "CatalogContext.cs",
            LineNumber = 5,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Data model", result.Content);
        Assert.Contains("Product", result.Content);
        Assert.Contains("CatalogContext", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_ShowsMessageConsumersSection()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new MessageConsumerDetection("OrderCreated", "OrderCreatedConsumer", "MassTransit")
        {
            ExtractorName = "EventBusExtractor",
            SourceFile = "OrderCreatedConsumer.cs",
            LineNumber = 10,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Message consumers", result.Content);
        Assert.Contains("OrderCreated", result.Content);
        Assert.Contains("OrderCreatedConsumer", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_EndpointTable_ShowsSourceColumn()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api/products", "<Program>", "<lambda>", [], [])
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = @"C:\repo\src\MyApp\Program.cs",
            LineNumber = 5,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Source", result.Content);
        Assert.Contains("Program.cs:5", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_EndpointTable_ShowsGroupPrefix()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api/products", "ProductsHandler", "HandleAsync", [], [], "/api")
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = @"C:\repo\src\MyApp\Endpoints.cs",
            LineNumber = 10,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Group", result.Content);
        Assert.Contains("/api", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_EndpointTable_TruncatesLongLambda()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection(
            "GET", "/api/products",
            "async (IMediator mediator, ILogger logger, IConfiguration config) => { var products = await mediator.Send(new GetProductsQuery()); return Results.Ok(products); }",
            "<lambda>", [], [])
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = @"C:\repo\src\MyApp\Program.cs",
            LineNumber = 5,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        // Should truncate the long lambda with file:line reference
        Assert.Contains("Program.cs:5", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_RendersEfEntitiesSection()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EfEntityDetection("Order", "OrderingContext", true, ["Id"])
        {
            ExtractorName = "EfCoreExtractor",
            SourceFile = @"C:\repo\src\Ordering\OrderingContext.cs",
            LineNumber = 10,
        });
        model.Detections.Add(new EfEntityDetection("OrderItem", "OrderingContext", false, ["Id", "OrderId"])
        {
            ExtractorName = "EfCoreExtractor",
            SourceFile = @"C:\repo\src\Ordering\OrderingContext.cs",
            LineNumber = 15,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Data model (EF Core)", result.Content);
        Assert.Contains("OrderingContext", result.Content);
        Assert.Contains("`Order`", result.Content);
        Assert.Contains("`OrderItem`", result.Content);
        Assert.Contains("✓", result.Content); // Order is aggregate root
    }

    [Fact]
    public async Task MarkdownRenderer_RendersMessageConsumersSection()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new MessageConsumerDetection("OrderPlacedEvent", "OrderPlacedConsumer", "MassTransit")
        {
            ExtractorName = "EventBusExtractor",
            SourceFile = @"C:\repo\src\Consumers\OrderPlacedConsumer.cs",
            LineNumber = 5,
        });

        var options = new RenderOptions(false, false, 8000);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Message consumers", result.Content);
        Assert.Contains("MassTransit", result.Content);
        Assert.Contains("`OrderPlacedEvent`", result.Content);
        Assert.Contains("`OrderPlacedConsumer`", result.Content);
    }

    [Fact]
    public async Task MarkdownRenderer_RequiredSections_FiltersOutput()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new EndpointDetection("GET", "/api/test", "TestHandler", "Handle", [], [])
        {
            ExtractorName = "EndpointExtractor",
            SourceFile = @"C:\repo\src\Program.cs",
            LineNumber = 1,
        });
        model.Detections.Add(new EfEntityDetection("Product", "AppDbContext", false, [])
        {
            ExtractorName = "EfCoreExtractor",
            SourceFile = @"C:\repo\src\AppDbContext.cs",
            LineNumber = 1,
        });

        // Only show endpoints — suppress data model
        var options = new RenderOptions(false, false, 8000, RequiredSections: ["Endpoints"]);
        var renderer = new MarkdownRenderer();
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("Endpoints", result.Content);
        Assert.DoesNotContain("Data model", result.Content);
    }

    [Fact]
    public async Task AntiPatterns_GroupedByFile_WhenMultipleFiles()
    {
        var model = new DiscoveryModel();
        model.Detections.Add(new AntiPatternDetection("FireAndForget", "desc1", "high", "t1")
            { ExtractorName = "a", SourceFile = @"\src\Worker.cs", LineNumber = 1 });
        model.Detections.Add(new AntiPatternDetection("FireAndForget", "desc2", "high", "t2")
            { ExtractorName = "a", SourceFile = @"\src\Worker.cs", LineNumber = 2 });
        model.Detections.Add(new AntiPatternDetection("ServiceLocator", "desc3", "high", "t3")
            { ExtractorName = "a", SourceFile = @"\tests\TestFile.cs", LineNumber = 1 });

        var renderer = new MarkdownRenderer();
        var options = new RenderOptions(false, false, 8000);
        var result = await renderer.RenderAsync(model, options, default);

        Assert.Contains("### Worker.cs (2)", result.Content);
        Assert.Contains("### TestFile.cs (1)", result.Content);
    }
}
