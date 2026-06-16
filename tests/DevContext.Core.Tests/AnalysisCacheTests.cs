namespace DevContext.Core.Tests;

public sealed class AnalysisCacheTests
{
    [Fact]
    public async Task GetTextAsync_ReturnsCachedContent()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("test.cs", "class Foo {}");
        var cache = new FakeAnalysisCache(fs);

        var text1 = await cache.GetTextAsync("test.cs");
        var text2 = await cache.GetTextAsync("test.cs");

        Assert.Equal("class Foo {}", text1);
        Assert.Equal(text1, text2);
    }

    [Fact]
    public async Task GetSyntaxTreeAsync_ParsesCSharp()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("Program.cs", "class Program { static void Main() {} }");
        var cache = new FakeAnalysisCache(fs);

        var tree = await cache.GetSyntaxTreeAsync("Program.cs");

        Assert.NotNull(tree);
        var root = await tree.GetRootAsync();
        Assert.Contains("Program", root.ToFullString(), StringComparison.Ordinal);
    }

    [Fact]
    public void RegisterPath_AddsToKnownPaths()
    {
        var fs = new FakeFileSystem();
        var cache = new FakeAnalysisCache(fs);

        cache.RegisterPath("file1.cs");
        cache.RegisterPath("file2.cs");

        Assert.Equal(2, cache.KnownFilePaths.Count);
    }
}
