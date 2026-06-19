using DevContext.Core.Resolvers;

namespace DevContext.Core.Tests;

public sealed class SolutionFileParserTests
{
    [Fact]
    public void ParsesLegacySln()
    {
        const string content = """
            Microsoft Visual Studio Solution File
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Web", "src\Web\Web.csproj", "{GUID}"
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Core", "src\Core\Core.csproj", "{GUID}"
            """;

        var projects = SolutionFileParser.ParseProjectPaths(content, @"C:\app\App.sln");

        Assert.Equal([@"src\Web\Web.csproj", @"src\Core\Core.csproj"], projects.ToArray());
    }

    [Fact]
    public void ParsesSlnx_IncludingProjectsNestedInFolders()
    {
        // Mirrors the eShop layout: projects live inside <Folder> elements, and a non-.csproj
        // <File> entry must be ignored.
        const string content = """
            <Solution>
              <Folder Name="/src/">
                <Project Path="src/Ordering.API/Ordering.API.csproj" />
                <Project Path="src/Ordering.Domain/Ordering.Domain.csproj" />
              </Folder>
              <Folder Name="/Solution Items/">
                <File Path="README.md" />
              </Folder>
              <Project Path="src/EventBus/EventBus.csproj" />
            </Solution>
            """;

        var projects = SolutionFileParser.ParseProjectPaths(content, @"C:\eShop\eShop.slnx");

        Assert.Equal(
            ["src/Ordering.API/Ordering.API.csproj", "src/Ordering.Domain/Ordering.Domain.csproj", "src/EventBus/EventBus.csproj"],
            projects.ToArray());
    }

    [Fact]
    public void ParsesSlnx_WithLeadingByteOrderMark()
    {
        const string content = "﻿<Solution><Project Path=\"A/A.csproj\" /></Solution>";

        var projects = SolutionFileParser.ParseProjectPaths(content, "A.slnx");

        Assert.Equal(["A/A.csproj"], projects.ToArray());
    }

    [Fact]
    public void MalformedSlnx_ReturnsEmpty_DoesNotThrow()
    {
        var projects = SolutionFileParser.ParseProjectPaths("<Solution><Project Path=", "broken.slnx");

        Assert.Empty(projects);
    }
}
