namespace DevContext.Core.Tests;

public sealed class SolutionDiscoveryExtractorTests
{
    [Fact]
    public async Task ExtractAsync_ParsesSlnFile()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\MyApp.sln", @"
Microsoft Visual Studio Solution File
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""Web"", ""src\Web\Web.csproj"", ""{GUID}""
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""Core"", ""src\Core\Core.csproj"", ""{GUID}""
");
        fs.AddFile(@"C:\project\src\Web\Web.csproj", "");
        fs.AddFile(@"C:\project\src\Core\Core.csproj", "");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var ctx = new DiscoveryContext
        {
            RootPath = @"C:\project",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        };

        var model = new DiscoveryModel();
        var extractor = new SolutionDiscoveryExtractor();
        await extractor.ExtractAsync(ctx, model, CancellationToken.None);

        Assert.NotNull(model.Solution);
        Assert.Equal("MyApp", model.Solution.Name);
        Assert.Equal(2, model.Solution.ProjectPaths.Length);
    }

    [Fact]
    public async Task ExtractAsync_NoSln_AddsDiagnostic()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "");

        var cache = new FakeAnalysisCache(fs);
        var model = new DiscoveryModel();
        var ctx = new DiscoveryContext
        {
            RootPath = "src",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = new SharedAnalysisContext(),
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        };

        var extractor = new SolutionDiscoveryExtractor();
        await extractor.ExtractAsync(ctx, model, CancellationToken.None);

        Assert.Null(model.Solution);
        Assert.NotEmpty(model.Diagnostics);
    }
}
