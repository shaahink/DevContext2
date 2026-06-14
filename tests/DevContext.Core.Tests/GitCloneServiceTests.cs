using DevContext.Core.Services;

namespace DevContext.Core.Tests;

public sealed class GitCloneServiceTests
{
    [Fact]
    public void IsCloneStale_returns_true_when_directory_missing()
    {
        var nonexistentPath = Path.Combine(Path.GetTempPath(), $"devcontext-test-{Guid.NewGuid()}");
        Assert.True(GitCloneService.IsCloneStale(nonexistentPath));
    }

    [Fact]
    public void IsCloneStale_returns_false_for_fresh_directory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"devcontext-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            Assert.False(GitCloneService.IsCloneStale(tempPath));
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
