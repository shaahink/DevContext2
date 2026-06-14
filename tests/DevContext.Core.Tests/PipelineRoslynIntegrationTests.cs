using DevContext.Core.Configuration;
using DevContext.Core.Contracts;
using DevContext.Core.Extractors.Generic;
using DevContext.Core.Extractors.Specific;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Tests;

/// <summary>Integration tests exercising Roslyn-dependent features with a real workspace
/// (based on fixture project) or verifying graceful degradation without one.
/// Tagged [Trait("Category","Roslyn")] — exclude from CI with --filter Category!=Roslyn.</summary>
[Trait("Category", "Roslyn")]
public sealed class PipelineRoslynIntegrationTests
{
    // PRI-1
    [Fact]
    public async Task Pipeline_with_real_roslyn_on_fixture_completes()
    {
        var fs = new FakeFileSystem();
        var fixturePath = GoldenTestHelper.GetFixturePath("MinimalApiProject");

        foreach (var file in Directory.EnumerateFiles(fixturePath, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(fixturePath, file);
            fs.AddFile(relative, await File.ReadAllTextAsync(file));
        }

        var slnFile = Directory.EnumerateFiles(fixturePath, "*.sln", SearchOption.TopDirectoryOnly).First();
        var slnPath = new DirectoryInfo(fixturePath).Name + "/" + Path.GetFileName(slnFile);

        var loggerFactory = LoggerFactory.Create(b => { });
        var provider = new DevContext.Roslyn.Services.RoslynWorkspaceProvider(
            slnPath, fs, loggerFactory.CreateLogger<DevContext.Roslyn.Services.RoslynWorkspaceProvider>());

        var ctx = new DiscoveryContext
        {
            RootPath = "",
            Options = new ExtractionOptions { MaxOutputTokens = 8000, Profile = ExtractionProfile.Debug },
            ActiveScenario = ScenarioRegistry.BuiltIn["deep-dive"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = new FakeAnalysisCache(fs),
            Analysis = new SharedAnalysisContext(),
            Logger = loggerFactory.CreateLogger<DiscoveryContext>(),
            RoslynWorkspace = provider,
        };

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
            new CallGraphExtractor(),
            new SourceBodyExtractor(),
            new DiRegistrationExtractor(),
        };

        var pruners = new List<IPruner>
        {
            new PathProximityPruner(),
            new CallReachabilityPruner(),
            new PatternRelevancePruner(),
        };

        var pipeline = new DiscoveryPipeline(
            extractors, pruners, [],
            new Dictionary<string, IContextRenderer>(),
            loggerFactory.CreateLogger<DiscoveryPipeline>());

        var snapshot = await pipeline.AnalyzeAsync(ctx, default);
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.Model);
        Assert.False(snapshot.Model.Types.IsEmpty, "Should discover types from fixture with Roslyn");
    }

    // PRI-3
    [Fact]
    public async Task Pipeline_completes_with_null_roslyn_degradation()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program { static void Main() {} }");
        fs.AddFile(@"src\MyApp.csproj", "<Project />");

        var ctx = new DiscoveryContext
        {
            RootPath = "src",
            Options = new ExtractionOptions { MaxOutputTokens = 8000, Profile = ExtractionProfile.Debug },
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = new FakeAnalysisCache(fs),
            Analysis = new SharedAnalysisContext(),
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider(),
        };

        var pipeline = new DiscoveryPipeline(
            [new FileTreeExtractor(), new CallGraphExtractor(), new SourceBodyExtractor()],
            [], [],
            new Dictionary<string, IContextRenderer>(),
            new NullLogger<DiscoveryPipeline>());

        var snapshot = await pipeline.AnalyzeAsync(ctx, default);
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.Model);
    }
}
