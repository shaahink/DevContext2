using System.Diagnostics;
using LibGit2Sharp;

namespace DevContext.Core.Services;

public enum RepoStatus
{
    None,
    Checking,
    Valid,
    NotFound,
    Private,
    NetworkError,
    RateLimited,
    InvalidUrl,
    NoGit,
}

public sealed record CloneProgress(string Phase, int PercentComplete, string Message);

public sealed class GitCloneService : IDisposable
{
    private readonly SemaphoreSlim _cloneLock = new(1, 1);
    private readonly Dictionary<string, (string Path, DateTime ClonedAt)> _cache = new();
    private bool? _gitAvailable;

    public bool IsGitAvailable
    {
        get
        {
            _gitAvailable ??= CheckGitAvailable();
            return _gitAvailable.Value;
        }
    }

    private static bool CheckGitAvailable()
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            });
            p?.WaitForExit(5000);
            return p?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<RepoStatus> ValidateAsync(RepoUrl repo, CancellationToken ct)
    {
        var url = $"https://github.com/{repo.Owner}/{repo.Repo}.git";
        var branch = repo.Ref ?? "HEAD";

        try
        {
            // Try LibGit2Sharp first
            ct.ThrowIfCancellationRequested();
            var refs = await Task.Run(() =>
            {
                try
                {
                    return Repository.ListRemoteReferences(url);
                }
                catch
                {
                    return null;
                }
            }, ct).ConfigureAwait(false);

            if (refs is not null)
                return refs.Any() ? RepoStatus.Valid : RepoStatus.NotFound;
        }
        catch (OperationCanceledException) { throw; }
        catch
        {
            // Fall through to git CLI
        }

        // Fallback to git CLI
        if (!IsGitAvailable) return RepoStatus.NoGit;

        try
        {
            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"ls-remote --exit-code \"{url}\" {branch}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            });

            if (p is null) return RepoStatus.NetworkError;

            var stderr = new System.Text.StringBuilder();
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
            p.BeginErrorReadLine();

            await p.WaitForExitAsync(ct).ConfigureAwait(false);

            if (p.ExitCode == 0) return RepoStatus.Valid;

            var err = stderr.ToString();
            if (err.Contains("403") || err.Contains("401")) return RepoStatus.Private;
            if (err.Contains("not found") || err.Contains("Could not read")) return RepoStatus.NotFound;
            if (err.Contains("429") || err.Contains("rate limit")) return RepoStatus.RateLimited;
            return RepoStatus.NetworkError;
        }
        catch (OperationCanceledException) { throw; }
        catch { return RepoStatus.NetworkError; }
    }

    public async Task<string?> CloneAsync(RepoUrl repo, string targetPath, string? branch,
        IProgress<CloneProgress>? progress, CancellationToken ct)
    {
        await _cloneLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var cacheKey = $"{repo.Owner}/{repo.Repo}-{branch ?? "default"}";
            if (_cache.TryGetValue(cacheKey, out var cached)
                && Directory.Exists(cached.Path)
                && (DateTime.UtcNow - cached.ClonedAt).TotalHours < 24)
            {
                progress?.Report(new CloneProgress("Cached", 100, "Using cached clone"));
                return cached.Path;
            }

            if (Directory.Exists(targetPath))
                DeleteDirectoryRobust(targetPath);

            Directory.CreateDirectory(targetPath);
            var url = $"https://github.com/{repo.Owner}/{repo.Repo}.git";

            // Try LibGit2Sharp first
            var cloned = await TryCloneLibGit2Sharp(url, targetPath, branch ?? repo.Ref, progress, ct).ConfigureAwait(false);
            if (cloned)
            {
                _cache[cacheKey] = (targetPath, DateTime.UtcNow);
                progress?.Report(new CloneProgress("Complete", 100, "Done"));
                return targetPath;
            }

            // Fallback to git CLI
            if (!IsGitAvailable) return null;
            cloned = await TryCloneGitCli(url, targetPath, branch ?? repo.Ref ?? "", progress, ct).ConfigureAwait(false);
            if (cloned)
            {
                _cache[cacheKey] = (targetPath, DateTime.UtcNow);
                progress?.Report(new CloneProgress("Complete", 100, "Done"));
                return targetPath;
            }

            return null;
        }
        finally
        {
            _cloneLock.Release();
        }
    }

    private static async Task<bool> TryCloneLibGit2Sharp(string url, string targetPath, string? branch,
        IProgress<CloneProgress>? progress, CancellationToken ct)
    {
        try
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                var co = new CloneOptions
                {
                    BranchName = branch,
                    Checkout = true,
                };
                co.FetchOptions.OnTransferProgress = transfer =>
                {
                    var pct = transfer.ReceivedObjects > 0 && transfer.TotalObjects > 0
                        ? (int)(transfer.ReceivedObjects * 100 / transfer.TotalObjects)
                        : 0;
                    progress?.Report(new CloneProgress("Transferring", pct,
                        $"Receiving: {transfer.ReceivedObjects}/{transfer.TotalObjects} objects"));
                    return true;
                };

                Repository.Clone(url, targetPath, co);
                return true;
            }, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch { return false; }
    }

    private static async Task<bool> TryCloneGitCli(string url, string targetPath, string branch,
        IProgress<CloneProgress>? progress, CancellationToken ct)
    {
        try
        {
            var args = $"clone --depth 1 --single-branch";
            if (!string.IsNullOrEmpty(branch))
                args += $" --branch {branch}";
            args += $" \"{url}\" \"{targetPath}\"";

            progress?.Report(new CloneProgress("Cloning", 0, "Cloning from GitHub (git CLI)..."));

            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            });

            if (p is null) return false;

            var stderr = new System.Text.StringBuilder();
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            await p.WaitForExitAsync(ct).ConfigureAwait(false);

            if (p.ExitCode != 0)
            {
                progress?.Report(new CloneProgress("Error", 0, $"Clone failed: {stderr}"));
                return false;
            }

            return true;
        }
        catch (OperationCanceledException) { throw; }
        catch { return false; }
    }

    public static void Cleanup(string localPath)
    {
        try { DeleteDirectoryRobust(localPath); }
        catch { /* best effort */ }
    }

    private static void DeleteDirectoryRobust(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            var attrs = File.GetAttributes(file);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
        }
        Directory.Delete(path, true);
    }

    public void Dispose()
    {
        _cloneLock.Dispose();
        _cache.Clear();
        GC.SuppressFinalize(this);
    }
}
