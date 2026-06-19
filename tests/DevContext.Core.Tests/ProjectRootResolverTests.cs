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
        var result = await ProjectRootResolver.ResolveAsync(@"C:\project\MyApp.sln", fs);

        Assert.Equal(ResolutionMethod.ExplicitSln, result.Method);
        Assert.Equal(@"C:\project\MyApp.sln", result.SolutionFilePath);
    }

    [Fact]
    public async Task ResolveAsync_WithCsprojFile_ReturnsExplicitCsproj()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\src\MyApp.csproj", "");

        var resolver = new ProjectRootResolver();
        var result = await ProjectRootResolver.ResolveAsync(@"C:\project\src\MyApp.csproj", fs);

        Assert.Equal(ResolutionMethod.ExplicitCsproj, result.Method);
    }

    [Fact]
    public async Task ResolveAsync_DirectoryWithSln_ReturnsDirectoryContainsSln()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\MyApp.sln", "");
        fs.AddFile(@"C:\project\src\Program.cs", "");

        var resolver = new ProjectRootResolver();
        var result = await ProjectRootResolver.ResolveAsync(@"C:\project", fs);

        Assert.Equal(ResolutionMethod.DirectoryContainsSln, result.Method);
    }

    [Fact]
    public async Task ResolveAsync_DirectoryWithSlnx_ResolvesSolution()
    {
        // .slnx (XML solution) repos — eShop, AutoMapper, VerticalSlice — must resolve a solution
        // path, not fall through to folder mode (which also leaves Roslyn disabled).
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\MyApp.slnx", "<Solution />");
        fs.AddFile(@"C:\project\src\Program.cs", "");

        var result = await ProjectRootResolver.ResolveAsync(@"C:\project", fs);

        Assert.Equal(ResolutionMethod.DirectoryContainsSln, result.Method);
        Assert.Equal(@"C:\project\MyApp.slnx", result.SolutionFilePath);
    }

    [Fact]
    public async Task ResolveAsync_SubfolderWalksUpToSlnx()
    {
        // Pointing at a project subfolder finds the parent .slnx (as Roslyn already does).
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\MyApp.slnx", "<Solution />");
        fs.AddFile(@"C:\project\src\Api\Api.csproj", "");

        var result = await ProjectRootResolver.ResolveAsync(@"C:\project\src\Api", fs);

        Assert.Equal(ResolutionMethod.WalkedUp, result.Method);
        Assert.Equal(@"C:\project\MyApp.slnx", result.SolutionFilePath);
    }

    [Fact]
    public async Task ResolveAsync_NoSlnFound_ReturnsFolderMode()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:\project\src\Program.cs", "");

        var resolver = new ProjectRootResolver();
        var result = await ProjectRootResolver.ResolveAsync(@"C:\project", fs);

        Assert.Equal(ResolutionMethod.FolderMode, result.Method);
    }
}
