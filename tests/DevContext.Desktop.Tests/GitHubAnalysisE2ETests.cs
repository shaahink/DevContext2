using DevContext.Core.Models;
using DevContext.Core.Services;
using DevContext.Desktop.Services;
using DevContext.Desktop.ViewModels;

using NSubstitute;

namespace DevContext.Desktop.Tests;

public class GitHubAnalysisE2ETests
{
    [Fact]
    public void Parse_valid_github_url()
    {
        var url = RepoUrl.Parse("https://github.com/shaahink/DevContext2");
        Assert.NotNull(url);
        Assert.Equal("shaahink", url.Owner);
        Assert.Equal("DevContext2", url.Repo);
        Assert.Null(url.Ref);
    }

    [Fact]
    public void Parse_shorthand()
    {
        var url = RepoUrl.Parse("shaahink/DevContext2");
        Assert.NotNull(url);
        Assert.Equal("shaahink", url.Owner);
        Assert.Equal("DevContext2", url.Repo);
    }

    [Fact]
    public void Parse_url_with_tree_ref()
    {
        var url = RepoUrl.Parse("https://github.com/shaahink/DevContext2/tree/develop");
        Assert.NotNull(url);
        Assert.Equal("shaahink", url.Owner);
        Assert.Equal("DevContext2", url.Repo);
        Assert.Equal("develop", url.Ref);
    }

    [Fact]
    public void Parse_url_with_git_suffix()
    {
        var url = RepoUrl.Parse("https://github.com/shaahink/DevContext2.git");
        Assert.NotNull(url);
        Assert.Equal("DevContext2", url.Repo); // .git stripped
    }

    [Fact]
    public void Parse_invalid_url_returns_null()
    {
        Assert.Null(RepoUrl.Parse("C:\\Code\\MyProject"));
        Assert.Null(RepoUrl.Parse(""));
        Assert.Null(RepoUrl.Parse("https://gitlab.com/user/repo"));
    }

    [Fact]
    public void ClonePath_is_correct()
    {
        var url = RepoUrl.Parse("https://github.com/user/myrepo");
        Assert.NotNull(url);
        Assert.EndsWith("user-myrepo-default", url.ClonePath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Validate_repo_exists()
    {
        var git = new GitCloneService();
        if (!git.IsGitAvailable)
            return; // skip if git not installed

        var url = RepoUrl.Parse("https://github.com/shaahink/DevContext2");
        Assert.NotNull(url);

        var status = await git.ValidateAsync(url, CancellationToken.None);
        Assert.Equal(RepoStatus.Valid, status);
    }

    [Fact]
    public async Task Full_clone_and_analyze_flow()
    {
        var git = new GitCloneService();
        if (!git.IsGitAvailable)
            return; // skip

        var url = RepoUrl.Parse("https://github.com/shaahink/DevContext2");
        Assert.NotNull(url);

        // Clone
        var cloneDir = Path.Combine(Path.GetTempPath(), $"devcontext-e2e-{Guid.NewGuid():N}");
        try
        {
            var progress = new List<CloneProgress>();
            var clonePath = await git.CloneAsync(url, cloneDir, "main",
                new Progress<CloneProgress>(msg => progress.Add(msg)), CancellationToken.None);

            Assert.NotNull(clonePath);
            Assert.True(Directory.Exists(clonePath));
            Assert.NotEmpty(progress);

            // Verify the cloned repo has expected files
            Assert.True(File.Exists(Path.Combine(clonePath, "README.md")));
            Assert.True(Directory.Exists(Path.Combine(clonePath, "src")));
        }
        finally
        {
            GitCloneService.Cleanup(cloneDir);
        }
    }

    [Fact]
    public async Task Validation_returns_not_found_for_bad_repo()
    {
        var git = new GitCloneService();
        if (!git.IsGitAvailable)
            return;

        var url = new RepoUrl("shaahink", "does-not-exist-999999", null);
        var status = await git.ValidateAsync(url, CancellationToken.None);
        Assert.Equal(RepoStatus.NotFound, status);
    }

    [Fact]
    public void ViewModel_detects_github_url()
    {
        var svc = Substitute.For<IAnalysisService>();
        _ = svc.LoadSettings().Returns(new AppSettings());
        _ = svc.LoadRecent().Returns([]);

        var vm = new MainViewModel(svc);
        vm.ProjectPath = "https://github.com/shaahink/DevContext2";

        Assert.True(vm.IsGitHubUrl);
        Assert.Contains("github.com/shaahink/DevContext2", vm.GitRepoDisplay, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewModel_shows_clone_button_text()
    {
        var svc = Substitute.For<IAnalysisService>();
        _ = svc.LoadSettings().Returns(new AppSettings());
        _ = svc.LoadRecent().Returns([]);

        var vm = new MainViewModel(svc);
        vm.ProjectPath = "C:\\Code\\Project";
        Assert.Equal("Analyze", vm.AnalyzeButtonText);

        vm.ProjectPath = "https://github.com/shaahink/DevContext2";
        Assert.Contains("Clone", vm.AnalyzeButtonText, StringComparison.Ordinal);
    }
}
