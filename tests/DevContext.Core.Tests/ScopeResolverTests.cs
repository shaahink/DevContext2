using DevContext.Core.IO;
using DevContext.Core.Resolvers;

namespace DevContext.Core.Tests;

public sealed class ScopeResolverTests
{
    private static FakeFileSystem BuildRepo()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"C:/repo/App.slnx", """
            <Solution>
              <Project Path="src/Ordering.API/Ordering.API.csproj" />
              <Project Path="src/Ordering.Domain/Ordering.Domain.csproj" />
              <Project Path="src/Ordering.Infrastructure/Ordering.Infrastructure.csproj" />
              <Project Path="src/EventBus/EventBus.csproj" />
              <Project Path="src/Catalog.API/Catalog.API.csproj" />
            </Solution>
            """);
        fs.AddFile(@"C:/repo/src/Ordering.API/Ordering.API.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <ItemGroup>
                <ProjectReference Include="..\Ordering.Domain\Ordering.Domain.csproj" />
                <ProjectReference Include="..\Ordering.Infrastructure\Ordering.Infrastructure.csproj" />
              </ItemGroup>
            </Project>
            """);
        fs.AddFile(@"C:/repo/src/Ordering.API/Program.cs", "// entry");
        fs.AddFile(@"C:/repo/src/Ordering.Domain/Ordering.Domain.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        fs.AddFile(@"C:/repo/src/Ordering.Infrastructure/Ordering.Infrastructure.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <ProjectReference Include="..\Ordering.Domain\Ordering.Domain.csproj" />
              </ItemGroup>
            </Project>
            """);
        fs.AddFile(@"C:/repo/src/EventBus/EventBus.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        fs.AddFile(@"C:/repo/src/Catalog.API/Catalog.API.csproj", "<Project Sdk=\"Microsoft.NET.Sdk.Web\" />");
        return fs;
    }

    private static IEnumerable<string> Names(ImmutableArray<string> dirs)
        => dirs.Select(d => Path.GetFileName(d.TrimEnd('\\', '/'))).OrderBy(n => n);

    [Fact]
    public async Task Subfolder_input_resolves_to_anchor_plus_projectreference_closure()
    {
        var fs = BuildRepo();

        var result = await ProjectRootResolver.ResolveAsync(@"C:/repo/src/Ordering.API", fs);

        Assert.Equal(ResolutionMethod.WalkedUp, result.Method);
        Assert.EndsWith(@"Ordering.API.csproj", result.AnchorProjectPath);
        Assert.Equal(
            new[] { "Ordering.API", "Ordering.Domain", "Ordering.Infrastructure" },
            Names(result.ScopeProjectDirs));
    }

    [Fact]
    public async Task Explicit_csproj_input_resolves_to_its_closure()
    {
        var fs = BuildRepo();

        var result = await ProjectRootResolver.ResolveAsync(
            @"C:/repo/src/Ordering.Infrastructure/Ordering.Infrastructure.csproj", fs);

        Assert.Equal(ResolutionMethod.ExplicitCsproj, result.Method);
        Assert.Equal(
            new[] { "Ordering.Domain", "Ordering.Infrastructure" },
            Names(result.ScopeProjectDirs));
    }

    [Fact]
    public async Task Repo_root_input_is_whole_solution_with_empty_scan_set()
    {
        var fs = BuildRepo();

        var result = await ProjectRootResolver.ResolveAsync(@"C:/repo", fs);

        Assert.Equal(ResolutionMethod.DirectoryContainsSln, result.Method);
        Assert.True(result.ScopeProjectDirs.IsDefaultOrEmpty);
        Assert.Null(result.AnchorProjectPath);
    }
}
