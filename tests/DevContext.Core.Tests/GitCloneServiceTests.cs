using DevContext.Core.Services;

namespace DevContext.Core.Tests;

public sealed class GitCloneServiceTests
{
    [Fact]
    public void DecideCloneAction_returns_Reuse_for_24h_with_fresh_directory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"devcontext-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            Assert.Equal(GitCloneService.CloneAction.Reuse,
                GitCloneService.DecideCloneAction(tempPath, "24h"));
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public void DecideCloneAction_returns_Clone_for_24h_with_missing_directory()
    {
        var nonexistentPath = Path.Combine(Path.GetTempPath(), $"devcontext-test-{Guid.NewGuid()}");
        Assert.Equal(GitCloneService.CloneAction.Clone,
            GitCloneService.DecideCloneAction(nonexistentPath, "24h"));
    }

    [Fact]
    public void DecideCloneAction_returns_Clone_for_auto_session_keep_regardless()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"devcontext-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            Assert.Equal(GitCloneService.CloneAction.Clone,
                GitCloneService.DecideCloneAction(tempPath, "auto"));
            Assert.Equal(GitCloneService.CloneAction.Clone,
                GitCloneService.DecideCloneAction(tempPath, "session"));
            Assert.Equal(GitCloneService.CloneAction.Clone,
                GitCloneService.DecideCloneAction(tempPath, "keep"));
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public void CleanupSession_deletes_registered_paths()
    {
        var path1 = Path.Combine(Path.GetTempPath(), $"devcontext-session-{Guid.NewGuid()}");
        var path2 = Path.Combine(Path.GetTempPath(), $"devcontext-session-{Guid.NewGuid()}");
        Directory.CreateDirectory(path1);
        Directory.CreateDirectory(path2);

        GitCloneService.RegisterForSessionCleanup(path1);
        GitCloneService.RegisterForSessionCleanup(path2);
        GitCloneService.CleanupSession();

        Assert.False(Directory.Exists(path1));
        Assert.False(Directory.Exists(path2));
    }

    [Fact]
    public void RegisterForSessionCleanup_is_thread_safe()
    {
        var paths = new List<string>();
        for (var i = 0; i < 20; i++)
        {
            var p = Path.Combine(Path.GetTempPath(), $"devcontext-thread-{Guid.NewGuid()}");
            Directory.CreateDirectory(p);
            paths.Add(p);
        }

        Parallel.ForEach(paths, p => GitCloneService.RegisterForSessionCleanup(p));
        GitCloneService.CleanupSession();

        foreach (var p in paths)
            Assert.False(Directory.Exists(p));
    }
}
