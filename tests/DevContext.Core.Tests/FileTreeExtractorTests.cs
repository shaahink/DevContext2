namespace DevContext.Core.Tests;

public sealed class FileTreeExtractorTests
{
    [Fact]
    public async Task ExtractAsync_DiscoversSourceFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "class Program {}");
        fs.AddFile(@"src\Orders\Order.cs", "class Order {}");
        fs.AddFile(@"src\Orders\OrderRepository.cs", "class OrderRepository {}");
        fs.AddFile(@"src\Orders\Order.csproj", "<Project />");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var ctx = CreateContext(fs, cache, analysis);

        var extractor = new FileTreeExtractor();
        await extractor.ExtractAsync(ctx, new DiscoveryModel(), CancellationToken.None);

        Assert.Equal(3, analysis.AllSourceFiles.Count);
        Assert.Single(analysis.AllProjectFiles);
    }

    [Fact]
    public async Task ExtractAsync_RespectsExcludePatterns()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "");
        fs.AddFile(@"src\bin\output.dll", "");
        fs.AddFile(@"src\obj\debug\temp.obj", "");
        fs.AddFile(@"node_modules\package\index.js", "");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var options = new ExtractionOptions
        {
            ExcludePatterns = [".git", "bin", "obj", ".vs", "node_modules"]
        };
        var ctx = CreateContext(fs, cache, analysis, options);

        var extractor = new FileTreeExtractor();
        await extractor.ExtractAsync(ctx, new DiscoveryModel(), CancellationToken.None);

        Assert.Single(analysis.AllSourceFiles);
        Assert.Contains(analysis.AllSourceFiles, f => f.Contains("Program.cs"));
    }

    private static DiscoveryContext CreateContext(
        IFileSystem fs, IAnalysisCache cache, SharedAnalysisContext analysis,
        ExtractionOptions? options = null)
    {
        return new DiscoveryContext
        {
            RootPath = "src",
            Options = options ?? new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["architecture"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        };
    }


}
