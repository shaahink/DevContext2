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
    private readonly CloneRegistry _registry;
    private bool? _gitAvailable;

    public GitCloneService(CloneRegistry registry)
    {
        _registry = registry;
    }

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
            // Check persistent registry first
            var registryEntry = _registry.Get(repo.Owner, repo.Repo, branch);
            if (registryEntry is not null && Directory.Exists(registryEntry.Path))
            {
                var head = ResolveHead(registryEntry.Path);
                if (head is not null && registryEntry.Head is not null
                    && head == registryEntry.Head)
                {
                    progress?.Report(new CloneProgress("Cached", 100, "Using cached clone"));
                    return registryEntry.Path;
                }
            }

            // If the target path is stale (different from registry), clean it
            if (Directory.Exists(targetPath) && registryEntry is not null
                && !string.Equals(targetPath, registryEntry.Path, StringComparison.OrdinalIgnoreCase))
            {
                DeleteDirectoryRobust(targetPath);
            }

            if (Directory.Exists(targetPath))
                DeleteDirectoryRobust(targetPath);

            Directory.CreateDirectory(targetPath);
            var url = $"https://github.com/{repo.Owner}/{repo.Repo}.git";

            // Try shallow git CLI first (L1 — shallow, fast, skip full history)
            string? clonedPath = null;
            if (IsGitAvailable)
            {
                var cloned = await TryCloneGitCli(url, targetPath, branch ?? repo.Ref ?? "", progress, ct)
                    .ConfigureAwait(false);
                if (cloned)
                    clonedPath = targetPath;
            }

            // Fallback to LibGit2Sharp
            if (clonedPath is null)
            {
                var cloned = await TryCloneLibGit2Sharp(url, targetPath, branch ?? repo.Ref, progress, ct)
                    .ConfigureAwait(false);
                if (cloned)
                    clonedPath = targetPath;
            }

            if (clonedPath is null) return null;

            var clonedHead = ResolveHead(clonedPath);

            _registry.Set(new CloneEntry(
                repo.Owner,
                repo.Repo,
                branch ?? "default",
                clonedPath,
                clonedHead,
                DateTime.UtcNow));

            progress?.Report(new CloneProgress("Complete", 100, "Done"));
            return clonedPath;
        }
        finally
        {
            _cloneLock.Release();
        }
    }

    private static string? ResolveHead(string repoPath)
    {
        try
        {
            var headFile = Path.Combine(repoPath, ".git", "HEAD");
            if (!File.Exists(headFile)) return null;
            return File.ReadAllText(headFile).Trim();
        }
        catch
        {
            return null;
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
                progress?.Report(new CloneProgress("Starting", 0, "Connecting to GitHub…"));
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
                    progress?.Report(new CloneProgress("Receiving", 20 + (int)(pct * 0.70),
                        $"Receiving: {transfer.ReceivedObjects}/{transfer.TotalObjects} objects"));
                    return true;
                };

                Repository.Clone(url, targetPath, co);
                progress?.Report(new CloneProgress("Checkout", 95, "Checking out files…"));
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
            var args = $"clone --depth 1 --single-branch --progress";
            if (!string.IsNullOrEmpty(branch))
                args += $" --branch {branch}";
            args += $" \"{url}\" \"{targetPath}\"";

            progress?.Report(new CloneProgress("Starting", 0, "Cloning from GitHub…"));

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
            p.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                stderr.AppendLine(e.Data);
                ParseCloneProgress(e.Data, progress);
            };
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

    private static void ParseCloneProgress(string line, IProgress<CloneProgress>? progress)
    {
        if (progress is null) return;

        // Git outputs progress to stderr with a format like:
        //   "remote: Enumerating objects: 1234, done."
        //   "remote: Counting objects: 100% (1234/1234), done."
        //   "remote: Compressing objects: 100% (890/890), done."
        //   "Receiving objects: 100% (1234/1234), 2.5 MiB | 5.0 MiB/s, done."
        //   "Receiving objects:  25% (312/1234)"
        //   "Resolving deltas: 100% (456/456), done."

        var trimmed = line.Trim();
        if (trimmed.StartsWith("remote: Enumerating"))
        {
            if (TryParsePercent(trimmed, out var pct))
                progress.Report(new CloneProgress("Enumerating", (int)pct, trimmed));
            else
                progress.Report(new CloneProgress("Enumerating", 5, "Enumerating objects…"));
        }
        else if (trimmed.StartsWith("remote: Counting"))
        {
            if (TryParsePercent(trimmed, out var pct))
                progress.Report(new CloneProgress("Counting", 10 + (int)(pct / 10), trimmed));
            else
                progress.Report(new CloneProgress("Counting", 10, "Counting objects…"));
        }
        else if (trimmed.StartsWith("remote: Compressing"))
        {
            if (TryParsePercent(trimmed, out var pct))
                progress.Report(new CloneProgress("Compressing", 20 + (int)(pct / 10), trimmed));
            else
                progress.Report(new CloneProgress("Compressing", 20, "Compressing objects…"));
        }
        else if (trimmed.StartsWith("Receiving objects:"))
        {
            if (TryParsePercent(trimmed, out var pct))
                progress.Report(new CloneProgress("Receiving", 30 + (int)(pct * 0.55), trimmed));
            else
                progress.Report(new CloneProgress("Receiving", 30, "Receiving objects…"));
        }
        else if (trimmed.StartsWith("Resolving deltas:"))
        {
            if (TryParsePercent(trimmed, out var pct))
                progress.Report(new CloneProgress("Resolving", 85 + (int)(pct / 10), trimmed));
            else
                progress.Report(new CloneProgress("Resolving", 85, "Resolving deltas…"));
        }
    }

    private static bool TryParsePercent(string line, out double pct)
    {
        pct = 0;
        var pctIdx = line.IndexOf('%');
        if (pctIdx < 0) return false;

        var start = pctIdx - 1;
        while (start >= 0 && char.IsDigit(line[start]) || line[start] == '.')
            start--;
        start++;

        if (double.TryParse(line[start..pctIdx],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out pct))
        {
            return true;
        }
        return false;
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
        GC.SuppressFinalize(this);
    }
}
