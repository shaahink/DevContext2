using DevContext.Core.Graph;

namespace DevContext.Core.Tests;

public sealed class SolutionScopeTests
{
    [Fact]
    public void FromModel_resolves_relative_solution_paths_to_scoped_projects()
    {
        var model = new DiscoveryModel
        {
            Solution = new SolutionInfo(
                @"C:\repo\App.slnx", "App",
                ["src/A/A.csproj", "src/B/B.csproj"]),
            Projects =
            [
                new ProjectInfo("A", @"C:\repo\src\A\A.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("B", @"C:\repo\src\B\B.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("C", @"C:\repo\src\C\C.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        var scope = SolutionScope.FromModel(model);

        Assert.Equal("App", scope.SolutionName);
        Assert.Equal(new[] { "A", "B" }, scope.Projects.Select(p => p.Name).OrderBy(n => n));
        Assert.True(scope.Contains(@"C:\repo\src\A\Order.cs"));
        Assert.False(scope.Contains(@"C:\repo\src\C\OutOfScope.cs"));
    }

    [Fact]
    public void FromModel_handles_backslash_separators_in_solution_paths()
    {
        var model = new DiscoveryModel
        {
            Solution = new SolutionInfo(@"C:\repo\App.sln", "App", [@"src\A\A.csproj"]),
            Projects =
            [
                new ProjectInfo("A", @"C:\repo\src\A\A.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("B", @"C:\repo\src\B\B.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        var scope = SolutionScope.FromModel(model);

        Assert.Equal(new[] { "A" }, scope.Projects.Select(p => p.Name));
    }

    [Fact]
    public void FromModel_falls_back_to_all_projects_when_no_solution()
    {
        var model = new DiscoveryModel
        {
            Projects =
            [
                new ProjectInfo("A", @"C:\repo\src\A\A.csproj", "C#", ["net10.0"], [], []),
                new ProjectInfo("B", @"C:\repo\src\B\B.csproj", "C#", ["net10.0"], [], []),
            ],
        };

        var scope = SolutionScope.FromModel(model);

        Assert.Equal(2, scope.Projects.Length);
        Assert.Null(scope.SolutionName);
    }
}
