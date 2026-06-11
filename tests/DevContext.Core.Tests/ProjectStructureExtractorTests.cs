namespace DevContext.Core.Tests;

public sealed class ProjectStructureExtractorTests
{
    [Fact]
    public async Task ExtractAsync_ParsesCsproj()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Web\Web.csproj", @"
<Project Sdk=""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MediatR"" Version=""12.0.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""9.0.0"" />
  </ItemGroup>
</Project>");
        fs.AddFile(@"src\Web\Program.cs", "");

        var cache = new FakeAnalysisCache(fs);
        var analysis = new SharedAnalysisContext
        {
            AllProjectFiles = [@"src\Web\Web.csproj"]
        };

        var ctx = new DiscoveryContext
        {
            RootPath = "src",
            Options = new ExtractionOptions(),
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = analysis,
            Logger = new NullLogger<DiscoveryContext>(),
            RoslynWorkspace = new MockRoslynProvider()
        };

        var model = new DiscoveryModel();
        var extractor = new ProjectStructureExtractor();
        await extractor.ExtractAsync(ctx, model, CancellationToken.None);

        Assert.NotEmpty(model.Projects);
        var project = model.Projects[0];
        Assert.Equal("Web", project.Name);
        Assert.Equal(2, project.PackageReferences.Length);
    }
}
