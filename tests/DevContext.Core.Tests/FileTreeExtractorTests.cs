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

    [Fact]
    public async Task ExtractAsync_DoesNotExcludeRootWhenRootPathContainsExcludedSegment()
    {
        // Regression: analyzing a repo that itself lives under a folder literally named "eval-repos"
        // must not be treated as fully excluded — only nested subfolders named "eval-repos" should be
        // pruned. A naive Contains(pattern) check over the full path previously excluded every file.
        var fs = new FakeFileSystem();
        fs.AddFile(@"code\eval-repos\TodoApi\Program.cs", "class Program {}");
        fs.AddFile(@"code\eval-repos\TodoApi\Todo.Api\TodoApi.csproj", "<Project />");
        fs.AddFile(@"code\eval-repos\TodoApi\nested\eval-repos\Ignored.cs", "class Ignored {}");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var options = new ExtractionOptions
        {
            ExcludePatterns = [".git", "bin", "obj", ".vs", "node_modules", "eval-repos", "analysis-repos"],
        };
        var ctx = CreateContext(fs, cache, analysis, options, rootPath: @"code\eval-repos\TodoApi");

        var extractor = new FileTreeExtractor();
        await extractor.ExtractAsync(ctx, new DiscoveryModel(), CancellationToken.None);

        Assert.Contains(analysis.AllSourceFiles, f => f.Contains("Program.cs"));
        Assert.Single(analysis.AllProjectFiles);
        Assert.DoesNotContain(analysis.AllSourceFiles, f => f.Contains("Ignored.cs"));
    }

    [Fact]
    public async Task ExtractAsync_ExcludesNestedAgentWorktreesByDefault()
    {
        // T6.0 shamshir catch: `.claude/worktrees/<branch>/` holds a full git-worktree COPY of the
        // repo. Walking it doubles every path-keyed surface (topology "28 projects" on a 14-project
        // repo, per-service cards x2) while id-keyed graph nodes merge silently. `.claude` is agent
        // tooling, never production source — the DEFAULT options must prune it.
        var fs = new FakeFileSystem();
        fs.AddFile(@"repo\src\App\Program.cs", "class Program {}");
        fs.AddFile(@"repo\src\App\App.csproj", "<Project />");
        fs.AddFile(@"repo\.claude\worktrees\refactor\src\App\Program.cs", "class Program {}");
        fs.AddFile(@"repo\.claude\worktrees\refactor\src\App\App.csproj", "<Project />");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext();
        var ctx = CreateContext(fs, cache, analysis, rootPath: "repo");

        var extractor = new FileTreeExtractor();
        await extractor.ExtractAsync(ctx, new DiscoveryModel(), CancellationToken.None);

        Assert.Single(analysis.AllProjectFiles);
        Assert.Single(analysis.AllSourceFiles);
        Assert.DoesNotContain(analysis.AllProjectFiles, f => f.Contains(".claude"));
    }

    private static DiscoveryContext CreateContext(
        IFileSystem fs, IAnalysisCache cache, SharedAnalysisContext analysis,
        ExtractionOptions? options = null, string rootPath = "src")
    {
        return new DiscoveryContext
        {
            RootPath = rootPath,
            Options = options ?? new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = new NullLogger<DiscoveryContext>(),
        };
    }


}
