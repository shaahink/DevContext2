namespace DevContext.Core.Analysis;

/// <summary>Reads the analyzed repo's git HEAD sha — snapshot cache keying and pack identity (T4.1)
/// share this one reader. Shells out to `git rev-parse HEAD` (worktree- and packed-refs-safe);
/// returns null when git is unavailable or the path is not inside a repository.</summary>
public static class GitHeadReader
{
    public static string? Read(string rootPath)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", "rev-parse HEAD")
            {
                WorkingDirectory = rootPath, RedirectStandardOutput = true,
                RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc is null) return null;
            var output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit(5000);
            return proc.ExitCode == 0 && output.Length > 0 ? output : null;
        }
        catch { return null; }
    }
}
