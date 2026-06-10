using System.Diagnostics;

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

public sealed class GitCloneService
{
    private readonly bool _gitAvailable;
    private readonly SemaphoreSlim _cloneLock = new(1, 1);
    private readonly Dictionary<string, (string Path, DateTime ClonedAt)> _cache = new();

    public bool IsGitAvailable => _gitAvailable;

    public GitCloneService()
    {
        _gitAvailable = CheckGitInstalled();
    }

    private static bool CheckGitInstalled()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(5000);
            return p?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Validates a repo URL by running git ls-remote without downloading.</summary>
    public async Task<RepoStatus> ValidateAsync(RepoUrl repo, CancellationToken ct)
    {
        if (!_gitAvailable) return RepoStatus.NoGit;

        var url = $"https://github.com/{repo.Owner}/{repo.Repo}.git";
        var branch = repo.Ref ?? "HEAD";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"ls-remote --exit-code \"{url}\" {branch}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var p = Process.Start(psi);
            if (p is null) return RepoStatus.NetworkError;

            var stderr = new System.Text.StringBuilder();
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
            p.BeginErrorReadLine();

            await p.WaitForExitAsync(ct);

            if (p.ExitCode == 0)
            {
                // Read output to capture branch info if HEAD
                var output = await p.StandardOutput.ReadToEndAsync(ct);
                if (string.IsNullOrEmpty(repo.Ref) && !string.IsNullOrEmpty(output))
                {
                    // Parse HEAD ref → detect default branch name
                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Contains("refs/heads/"))
                        {
                            var refName = line.Split("refs/heads/").Last().Trim();
                            if (!string.IsNullOrEmpty(refName))
                                break; // Found branch
                        }
                    }
                }
                return RepoStatus.Valid;
            }

            var err = stderr.ToString();
            if (err.Contains("403") || err.Contains("401"))
                return RepoStatus.Private;
            if (err.Contains("not found") || err.Contains("Could not read"))
                return RepoStatus.NotFound;
            if (err.Contains("429") || err.Contains("rate limit"))
                return RepoStatus.RateLimited;

            return RepoStatus.NetworkError;
        }
        catch (TaskCanceledException)
        {
            return RepoStatus.Checking;
        }
        catch (Exception) when (ct.IsCancellationRequested)
        {
            return RepoStatus.Checking;
        }
        catch
        {
            return RepoStatus.NetworkError;
        }
    }

    /// <summary>Clones a repo to the given target path. Returns the local path on success.</summary>
    public async Task<string?> CloneAsync(RepoUrl repo, string targetPath, string? branch, IProgress<string>? progress, CancellationToken ct)
    {
        if (!_gitAvailable) return null;

        await _cloneLock.WaitAsync(ct);
        try
        {
            // Check cache
            var cacheKey = $"{repo.Owner}/{repo.Repo}-{branch ?? "default"}";
            if (_cache.TryGetValue(cacheKey, out var cached)
                && Directory.Exists(cached.Path)
                && (DateTime.UtcNow - cached.ClonedAt).TotalHours < 24)
            {
                progress?.Report("Using cached clone...");
                return cached.Path;
            }

            var url = $"https://github.com/{repo.Owner}/{repo.Repo}.git";
            var branchArg = branch ?? repo.Ref ?? "";

            if (Directory.Exists(targetPath))
                DeleteDirectoryRobust(targetPath);

            Directory.CreateDirectory(targetPath);
            progress?.Report("Cloning from GitHub...");

            var args = $"clone --depth 1 --single-branch";
            if (!string.IsNullOrEmpty(branchArg))
                args += $" --branch {branchArg}";
            args += $" \"{url}\" \"{targetPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var p = Process.Start(psi);
            if (p is null) return null;

            var stderr = new System.Text.StringBuilder();
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

            await p.WaitForExitAsync(ct);

            if (p.ExitCode != 0)
            {
                progress?.Report($"Clone failed: {stderr}");
                return null;
            }

            _cache[cacheKey] = (targetPath, DateTime.UtcNow);
            return targetPath;
        }
        finally
        {
            _cloneLock.Release();
        }
    }

    public static void Cleanup(string localPath)
    {
        try
        {
            DeleteDirectoryRobust(localPath);
        }
        catch { /* best effort */ }
    }

    private static void DeleteDirectoryRobust(string path)
    {
        if (!Directory.Exists(path)) return;

        // Remove read-only attribute from all files (git marks packs as read-only on Windows)
        foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            var attrs = File.GetAttributes(file);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
        }

        Directory.Delete(path, true);
    }
}
