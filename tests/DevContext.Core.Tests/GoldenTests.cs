namespace DevContext.Core.Tests;

[Collection("Golden tests")]
public sealed class GoldenTests
{
    private readonly GoldenTestFixture _fixture;

    public GoldenTests(GoldenTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task BasicProject_Analysis()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", "class Program { static void Main() {} }");
        fs.AddFile("MyApp.csproj", "<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var observer = new RecordingDiscoveryObserver();
        var loggerFactory = LoggerFactory.Create(b => { });

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = observer,
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("Golden"),
        };

        var extractors = new List<IDiscoveryExtractor>
        {
            new FileTreeExtractor(),
            new SolutionDiscoveryExtractor(),
            new ProjectStructureExtractor()
        };

        var pipeline = new DiscoveryPipeline(
            extractors, [], [], new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new TestMarkdownRenderer(),
                ["json"] = new TestJsonRenderer()
            },
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        var result = await pipeline.RunAsync(ctx);
        var goldenPath = Path.Combine(_fixture.GoldensPath, "basic-project.md");

        GoldenTestHelper.AssertMatchesGolden(result.Content, goldenPath);
    }

    [Fact]
    public async Task PipelineObserverEvents_RecordedCorrectly()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", "class Program { static void Main() {} }");

        var cache = new FakeAnalysisCache(fs);
        var observer = new RecordingDiscoveryObserver();
        var loggerFactory = LoggerFactory.Create(b => { });

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = observer,
            FileSystem = fs,
            Cache = cache,
            Analysis = new SharedAnalysisContext(),
            Logger = loggerFactory.CreateLogger("Test"),
        };

        var pipeline = new DiscoveryPipeline(
            new List<IDiscoveryExtractor>
            {
                new FileTreeExtractor(),
                new SolutionDiscoveryExtractor()
            },
            [], [], new Dictionary<string, IContextRenderer>
            {
                ["markdown"] = new TestMarkdownRenderer(),
                ["json"] = new TestJsonRenderer()
            },
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        await pipeline.RunAsync(ctx);

        Assert.Contains(observer.Events, e => e.StartsWith("PipelineStarted"));
        Assert.Contains(observer.Events, e => e.StartsWith("StageStarted"));
        Assert.Contains(observer.Events, e => e.StartsWith("ExtractorStarted"));
        Assert.Contains(observer.Events, e => e.StartsWith("ExtractorCompleted"));
        Assert.Contains(observer.Events, e => e.StartsWith("SignalsSealed"));
        Assert.Contains(observer.Events, e => e.StartsWith("StageCompleted"));
        Assert.Contains(observer.Events, e => e.StartsWith("PipelineCompleted"));
    }
}
