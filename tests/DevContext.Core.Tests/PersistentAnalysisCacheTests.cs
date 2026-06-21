namespace DevContext.Core.Tests;

public sealed class PersistentAnalysisCacheTests
{
    [Fact]
    public async Task UnchangedFile_ReturnsSameCachedTree()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("A.cs", "class A {}");
        var cache = new PersistentAnalysisCache(fs);

        var tree1 = await cache.GetSyntaxTreeAsync("A.cs");
        var tree2 = await cache.GetSyntaxTreeAsync("A.cs");

        // No edit → same parsed instance reused (the cross-run win: no re-parse).
        Assert.Same(tree1, tree2);
    }

    [Fact]
    public async Task ChangedFile_IsReParsedAndReRead()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("A.cs", "class Before {}");
        var cache = new PersistentAnalysisCache(fs);

        var first = await cache.GetSyntaxTreeAsync("A.cs");
        var firstText = await cache.GetTextAsync("A.cs");
        Assert.Contains("Before", (await first.GetRootAsync()).ToFullString());
        Assert.Contains("Before", firstText);

        // Simulate an edit on disk: AddFile bumps the fake mtime monotonically.
        fs.AddFile("A.cs", "class After {}");

        var second = await cache.GetSyntaxTreeAsync("A.cs");
        var secondText = await cache.GetTextAsync("A.cs");

        // mtime changed → the entry is invalidated and re-parsed/re-read (correctness preserved).
        Assert.NotSame(first, second);
        Assert.Contains("After", (await second.GetRootAsync()).ToFullString());
        Assert.Contains("After", secondText);
    }
}
