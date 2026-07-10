namespace DevContext.Core.Tests;

public sealed class RepoUrlTests
{
    [Theory]
    [InlineData("dotnet/aspnetcore")]
    [InlineData("github.com/dotnet/aspnetcore")]
    [InlineData("https://github.com/dotnet/aspnetcore")]
    public void GenuineGitHubShorthandOrUrl_ParsesToRepoUrl(string input)
    {
        var repo = RepoUrl.Parse(input);
        Assert.NotNull(repo);
        Assert.True(repo!.IsValid);
        Assert.Equal("dotnet", repo.Owner);
        Assert.Equal("aspnetcore", repo.Repo);
    }

    [Theory]
    [InlineData("C:/repos/eShop")]        // Windows absolute path (one slash)
    [InlineData("./eShop")]               // explicit relative path
    [InlineData("/eShop")]                // Unix absolute path, single segment
    public void StructurallyPathLikeInput_DoesNotParseAsGitHubShorthand(string input)
    {
        // E9: anything that structurally looks like a path rather than "owner/repo" — a drive letter or
        // a leading "."/"/" — must be rejected here so it isn't sent to GitHub validation and fails with
        // a misleading "Repository not found". A plain "word/word" like "eval-repos/eShop" is genuinely
        // ambiguous at this layer (it's valid GitHub shorthand shape too) — that case is resolved by an
        // existence check one layer up, in AnalyzeCommand, not here.
        var repo = RepoUrl.Parse(input);
        Assert.True(repo is null || !repo.IsValid);
    }

    [Fact]
    public void MixedSeparatorPath_DoesNotParseAsGitHubShorthand()
    {
        // A copy-pasted or programmatically joined path can mix separators; the drive-letter colon
        // alone is enough to reject it, but the backslash guard covers the case without one too
        // (e.g. a UNC-ish fragment like "repos\eShop/src" would otherwise still have exactly one '/').
        var repo = RepoUrl.Parse(@"repos\eShop/src");
        Assert.True(repo is null || !repo.IsValid);
    }
}
