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
        Assert.Contains("Related types grouped by layer", result.Content);
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
}
