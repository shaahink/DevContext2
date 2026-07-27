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

    /// <summary>J2 — fingerprint of the working tree's uncommitted changes: SHA-256 over every
    /// git-status-reported path joined with its mtime+length ("deleted" when gone), first 16 hex
    /// chars. Null when the tree is CLEAN or when git status can't run — then the cache keys on
    /// HEAD alone, the same trust it had before.</summary>
    public static string? ReadDirtyFingerprint(string rootPath)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", "status --porcelain -uall")
            {
                WorkingDirectory = rootPath, RedirectStandardOutput = true,
                RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc is null) return null;
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(15000);
            if (proc.ExitCode != 0) return null;

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return null;

            var sb = new System.Text.StringBuilder();
            foreach (var line in lines.OrderBy(l => l, StringComparer.Ordinal))
            {
                if (line.Length < 4) continue;
                var rel = line[3..].Trim();
                // Rename lines read "R  old -> new" — the NEW path is the one on disk.
                var arrow = rel.IndexOf(" -> ", StringComparison.Ordinal);
                if (arrow >= 0) rel = rel[(arrow + 4)..];
                rel = rel.Trim('"', '\r');
                sb.Append(rel).Append('|');
                var full = Path.Combine(rootPath, rel);
                if (File.Exists(full))
                {
                    var fi = new FileInfo(full);
                    sb.Append(fi.LastWriteTimeUtc.Ticks).Append(':').Append(fi.Length);
                }
                else
                {
                    sb.Append("deleted");
                }
                sb.Append('\n');
            }
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(sb.ToString())));
            return hash[..16];
        }
        catch { return null; }
    }
}
