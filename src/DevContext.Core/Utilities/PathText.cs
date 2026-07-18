namespace DevContext.Core.Utilities;

/// <summary>
/// String-pure path algebra for code that compares paths by TEXT (H1). Solution files write '\',
/// Roslyn and the file system hand back native separators, and tests feed Windows-style literals —
/// any System.IO.Path call over the non-native flavor silently misbehaves (off-Windows, '\' is a
/// name character; GetFullPath prefixes the CWD onto drive-style relatives). Everything here
/// normalizes to '/' and never consults the OS, so the same inputs produce the same comparisons
/// on every platform.
/// </summary>
internal static class PathText
{
    /// <summary>Canonical comparison form: '/' separators, no trailing separator.</summary>
    public static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');

    /// <summary>Parent directory of the normalized path, or null at a root ("/", "C:", bare name).</summary>
    public static string? DirOf(string path)
    {
        var p = Normalize(path);
        var i = p.LastIndexOf('/');
        if (i <= 0) return null;
        var dir = p[..i];
        return dir.Length == 2 && dir[1] == ':' ? null : dir;
    }

    /// <summary>Last segment of the normalized path (file or directory name).</summary>
    public static string NameOf(string path)
    {
        var p = Normalize(path);
        var i = p.LastIndexOf('/');
        return i < 0 ? p : p[(i + 1)..];
    }

    /// <summary>Combines a base directory and a (possibly '\'-separated, possibly "..\"-relative)
    /// path into normalized form, collapsing "." and ".." textually.</summary>
    public static string Join(string baseDir, string path)
    {
        var p = Normalize(path);
        if (baseDir.Length == 0 || IsRootedText(p)) return Collapse(p);
        return Collapse(Normalize(baseDir) + "/" + p);
    }

    /// <summary>Rooted by TEXT — Unix-absolute or Windows drive-style — regardless of host OS.</summary>
    private static bool IsRootedText(string p)
        => p.StartsWith('/') || (p.Length >= 2 && p[1] == ':');

    private static string Collapse(string p)
    {
        if (!p.Contains("./", StringComparison.Ordinal)) return p;
        var lead = p.StartsWith('/') ? "/" : "";
        var parts = new List<string>();
        foreach (var seg in p.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (seg == ".") continue;
            if (seg == ".." && parts.Count > 0 && parts[^1] != ".." && !(parts[^1].Length == 2 && parts[^1][1] == ':'))
            {
                parts.RemoveAt(parts.Count - 1);
                continue;
            }
            parts.Add(seg);
        }
        return lead + string.Join('/', parts);
    }
}
