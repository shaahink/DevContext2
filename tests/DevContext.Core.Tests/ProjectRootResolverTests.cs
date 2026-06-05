namespace DevContext.Core.Tests;

public sealed class ProjectRootResolverTests
{
    [Fact]
    public async Task ResolveAsync_WithSlnFile_ReturnsExplicitSln()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\MyApp.sln", "");
        fs.AddFile(@"C:\project\src\Program.cs", "");

        var resolver = new ProjectRootResolver();
        var result = await resolver.ResolveAsync(@"C:\project\MyApp.sln", fs);

        Assert.Equal(ResolutionMethod.ExplicitSln, result.Method);
        Assert.Equal(@"C:\project\MyApp.sln", result.SolutionFilePath);
    }

    [Fact]
    public async Task ResolveAsync_WithCsprojFile_ReturnsExplicitCsproj()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\src\MyApp.csproj", "");

        var resolver = new ProjectRootResolver();
        var result = await resolver.ResolveAsync(@"C:\project\src\MyApp.csproj", fs);

        Assert.Equal(ResolutionMethod.ExplicitCsproj, result.Method);
    }

    [Fact]
    public async Task ResolveAsync_DirectoryWithSln_ReturnsDirectoryContainsSln()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\MyApp.sln", "");
        fs.AddFile(@"C:\project\src\Program.cs", "");

        var resolver = new ProjectRootResolver();
        var result = await resolver.ResolveAsync(@"C:\project", fs);

        Assert.Equal(ResolutionMethod.DirectoryContainsSln, result.Method);
    }

    [Fact]
    public async Task ResolveAsync_NoSlnFound_ReturnsFolderMode()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\src\Program.cs", "");

        var resolver = new ProjectRootResolver();
        var result = await resolver.ResolveAsync(@"C:\project", fs);

        Assert.Equal(ResolutionMethod.FolderMode, result.Method);
    }
}
