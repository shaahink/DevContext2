using DevContext.Core.Configuration;
using DevContext.Core.Extractors.Specific;
using DevContext.Core.Pipeline;
using DevContext.Core.Services;

namespace DevContext.Core.Tests;

/// <summary>Integration tests that run the real pipeline (all extractors, pruners, compressors,
/// real renderers) against the MinimalApiProject fixture loaded from disk into FakeFileSystem.
/// MockRoslynProvider — Roslyn-dependent features (CallGraph, SourceBody) gracefully degrade.</summary>
public sealed class PipelineIntegrationTests
{
    private static async Task<RenderedContext> RunFixtureAsync(
        string scenario = "overview",
        string[]? requiredSections = null,
        string[]? focusTypes = null,
        bool includeAntiPatterns = false)
    {
        var fs = new FakeFileSystem();
        var fixturePath = GoldenTestHelper.GetFixturePath("MinimalApiProject");

        foreach (var file in Directory.EnumerateFiles(fixturePath, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(fixturePath, file);
            fs.AddFile(relative, await File.ReadAllTextAsync(file));
        }

        var scenarioConfig = ScenarioRegistry.BuiltIn[scenario];

        var options = new ExtractionOptions
        {
            MaxOutputTokens = 8000,
            OutputFormat = OutputFormat.Markdown,
            IncludeAntiPatterns = includeAntiPatterns,
        };

        if (requiredSections is { Length: > 0 })
        {
            scenarioConfig = scenarioConfig with
            {
                RequiredSections = requiredSections.ToImmutableArray()
            };
        }

        var focusPoints = (focusTypes ?? [])
            .Select(f => new FocusPoint(FocusKind.Type, "", f, null))
            .ToImmutableArray();

        var builder = new DiscoveryContextBuilder()
            .WithFileSystem(fs)
            .WithRootPath("")
            .WithOptions(options)
            .WithScenario(scenarioConfig);
        var ctx = builder.Build();

        if (focusPoints.Length > 0)
            ctx.Analysis.FocusPoints = focusPoints;

        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor(),
            new DependencyExtractor(),
            new SyntaxStructureExtractor(),
            new LayerClassifier(),
            new EndpointExtractor(),
            new MediatRExtractor(),
            new ControllerActionExtractor(),
            new EfCoreExtractor(),
            new EventBusExtractor(),
            new CallGraphExtractor(),
            new SourceBodyExtractor(),
            new IndirectWiringDetector(),
            new AspireExtractor(),
            new ProgramCsFlowExtractor(),
            new DiRegistrationExtractor(),
            new AntiPatternDetector(),
        };

        var pruners = new List<IPruner>
        {
            new PathProximityPruner(),
            new CallReachabilityPruner(),
            new PatternRelevancePruner(),
        };

        var compressors = new List<ICompressionStrategy>
        {
            new TrivialMemberCompressor(),
            new BoilerplateCompressor(),
            new StructuralDeduplicator(),
            new NamespaceGrouper(),
            new LlmFriendlyFormatter(),
            new AggressiveTruncator(),
        };

        var loggerFactory = LoggerFactory.Create(b => { });
        var pipeline = new DiscoveryPipeline(
            extractors, pruners, compressors,
            new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new MarkdownRenderer(),
                ["json"] = new JsonContextRenderer(),
            },
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        return await pipeline.RunAsync(ctx);
    }

    // PI-1
    [Fact]
    public async Task Section_filtering_filters_rendered_output()
    {
        var rendered = await RunFixtureAsync(
            scenario: "overview",
            requiredSections: ["Endpoints", "MediatR Handlers"]);

        var content = rendered.Content!;

        Assert.Contains("## Endpoints", content);
        Assert.Contains("## MediatR Handlers", content);
        Assert.DoesNotContain("## Architecture overview", content);
        Assert.DoesNotContain("## DI registrations", content);
    }

    // PI-2
    [Fact]
    public async Task Focus_changes_output_content()
    {
        var noFocus = await RunFixtureAsync(scenario: "overview");
        var withFocus = await RunFixtureAsync(scenario: "deep-dive", focusTypes: ["CreateOrderHandler"]);

        Assert.NotEmpty(noFocus.Content!);
        Assert.NotEmpty(withFocus.Content!);
        Assert.NotEqual(noFocus.Content, withFocus.Content);
        Assert.True(withFocus.EstimatedTokens > 0, "Deep-dive with focus should produce output");
    }

    // PI-3
    [Fact]
    public async Task IncludeAntiPatterns_flag_does_not_crash_pipeline()
    {
        var rendered = await RunFixtureAsync(
            scenario: "overview",
            includeAntiPatterns: true);

        Assert.NotNull(rendered.Content);
        Assert.True(rendered.Content!.Length > 0);
        Assert.True(rendered.EstimatedTokens > 0);
    }
}
