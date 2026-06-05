namespace DevContext.Core.Tests;

public sealed class PipelineTests
{
    [Fact]
    public async Task RunAsync_WithDryRun_ReturnsPlan()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "");
        fs.AddFile(@"src\MyApp.csproj", "<Project />");

        var cache = new FakeAnalysisCache(fs);
        var observer = new RecordingDiscoveryObserver();

        var ctx = new DiscoveryContext
        {
            RootPath = "src",
            Options = new ExtractionOptions { DryRun = true },
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = observer,
            FileSystem = fs,
            Cache = cache,
            Analysis = new SharedAnalysisContext(),
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
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
            new NullLogger<DiscoveryPipeline>());

        var result = await pipeline.RunAsync(ctx);

        Assert.Contains("Dry Run Plan", result.Content);
    }

    [Fact]
    public async Task RunAsync_WithRealExtraction_Completes()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", @"
<Project>
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MediatR"" Version=""12.0.0"" />
  </ItemGroup>
</Project>");
        fs.AddFile(@"src\MyApp.sln", @"
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""MyApp"", ""MyApp.csproj"", ""{GUID}""
");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var observer = new RecordingDiscoveryObserver();
        var loggerFactory = LoggerFactory.Create(b => { });

        var ctx = new DiscoveryContext
        {
            RootPath = "src",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = observer,
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = loggerFactory.CreateLogger("Test"),
            RoslynWorkspace = new MockRoslynProvider()
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

        Assert.NotNull(result.Content);
        Assert.True(result.EstimatedTokens > 0);
        Assert.Contains("PipelineStarted", observer.Events[0]);
        Assert.Contains("PipelineCompleted", observer.Events[^1]);
    }

    [Fact]
    public void GetOrder_UsesAttribute()
    {
        var early = new FileTreeExtractor();
        var normal = new ProjectStructureExtractor();

        var earlyOrder = DiscoveryPipeline.GetOrder(early);
        var normalOrder = DiscoveryPipeline.GetOrder(normal);

        Assert.True(earlyOrder < normalOrder);
    }
}


