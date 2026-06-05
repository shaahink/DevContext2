namespace DevContext.Core.Tests;

public sealed class FakeFileSystemTests
{
    [Fact]
    public async Task AddFile_AndRead_ReturnsContent()
    {
        var fs = new FakeFileSystem();
        fs.AddFile("test.txt", "hello world");

        Assert.True(fs.FileExists("test.txt"));
        var content = await fs.ReadAllTextAsync("test.txt");
        Assert.Equal("hello world", content);
    }

    [Fact]
    public async Task EnumerateFilesAsync_ReturnsMatchingFiles()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Orders\Program.cs", "content");
        fs.AddFile(@"src\Orders\Order.cs", "content");
        fs.AddFile(@"src\Shipping\Ship.cs", "content");

        var files = new List<string>();
        await foreach (var f in fs.EnumerateFilesAsync("src", "*.cs", SearchOption.AllDirectories))
            files.Add(f);

        Assert.Equal(3, files.Count);
    }

    [Fact]
    public async Task EnumerateFilesAsync_WithPattern_Filters()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Program.cs", "content");
        fs.AddFile(@"src\app.config", "content");

        var files = new List<string>();
        await foreach (var f in fs.EnumerateFilesAsync("src", "*.cs", SearchOption.TopDirectoryOnly))
            files.Add(f);

        Assert.Single(files);
    }

    [Fact]
    public void DirectoryExists_AfterAddFile_CreatesParents()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"a\b\c\file.txt", "");

        Assert.True(fs.DirectoryExists(@"a\b\c"));
        Assert.True(fs.DirectoryExists(@"a\b"));
        Assert.True(fs.DirectoryExists("a"));
    }

    [Fact]
    public void GetRelativePath_ComputesCorrectly()
    {
        var fs = new FakeFileSystem();
        var rel = fs.GetRelativePath(@"src\Api", @"src\Api\Controllers\Test.cs");
        Assert.Equal(@"Controllers\Test.cs", rel);
    }
}
