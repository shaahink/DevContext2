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
            "MinimalApiProject", "architecture", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-architecture.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task MinimalApiProject_ArchitectureScenario_ProducesJson()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "MinimalApiProject", "architecture", "json");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-architecture.json");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task MinimalApiProject_DebugEndpointScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "MinimalApiProject", "debug-endpoint", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-debug-endpoint.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task MinimalApiProject_AddSimilarFeatureScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "MinimalApiProject", "add-similar-feature", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "minimal-api-add-similar-feature.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task CleanArchProject_ArchitectureScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "architecture", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-architecture.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task CleanArchProject_ArchitectureScenario_ProducesJson()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "architecture", "json");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-architecture.json");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task CleanArchProject_DebugEndpointScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "debug-endpoint", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-debug-endpoint.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task CleanArchProject_AddSimilarFeatureScenario_ProducesMarkdown()
    {
        var result = await GoldenTestHelper.RunPipelineOnFixtureWithAllExtractors(
            "CleanArchProject", "add-similar-feature", "markdown");
        var goldenPath = Path.Combine(_fixture.GoldensPath, "clean-arch-add-similar-feature.md");
        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }
}
