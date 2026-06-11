namespace DevContext.Core.Tests;

[Collection("Golden tests")]
public sealed class GoldenExtractionTests
{
    private readonly GoldenTestFixture _fixture;

    public GoldenExtractionTests(GoldenTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MinimalApiProject_ArchitectureScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "MinimalApiProject", "overview", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-architecture.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("GET", result.Content);
        Assert.Contains("/orders", result.Content);
        Assert.Contains("CreateOrderHandler", result.Content);
    }

    [Fact]
    public async Task MinimalApiProject_ArchitectureScenario_ProducesJson()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "MinimalApiProject", "overview", "json");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-architecture.json");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("\"schemaVersion\": \"2.0\"", result.Content);
        Assert.Contains("\"style\": \"MinimalApi\"", result.Content);
        Assert.Contains("EndpointDetection", result.Content);
    }

    [Fact]
    public async Task MinimalApiProject_DebugEndpointScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "MinimalApiProject", "deep-dive", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-debug-endpoint.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("GET", result.Content);
        Assert.Contains("CreateOrderHandler", result.Content);
    }

    [Fact]
    public async Task MinimalApiProject_AddSimilarFeatureScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "MinimalApiProject", "overview", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-add-similar-feature.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("CreateOrderHandler", result.Content);
        Assert.Contains("Order", result.Content);
    }

    [Fact]
    public async Task CleanArchProject_ArchitectureScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "overview", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-architecture.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("GetProductsHandler", result.Content);
        Assert.Contains("Product", result.Content);
    }

    [Fact]
    public async Task CleanArchProject_ArchitectureScenario_ProducesJson()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "overview", "json");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-architecture.json");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("\"schemaVersion\": \"2.0\"", result.Content);
        Assert.Contains("\"style\"", result.Content);
    }

    [Fact]
    public async Task CleanArchProject_DebugEndpointScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "deep-dive", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-debug-endpoint.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("GetProductsHandler", result.Content);
    }

    [Fact]
    public async Task CleanArchProject_AddSimilarFeatureScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "overview", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-add-similar-feature.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);

        Assert.Contains("GetProductsHandler", result.Content);
        Assert.Contains("Product", result.Content);
    }
}
