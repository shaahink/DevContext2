namespace DevContext.Core.Rendering;

/// <summary>Formats source locations for display. Traces render repo-relative paths (relative to the
/// analysis root) instead of absolute machine paths — far easier to read and to navigate to.</summary>
public static class PathDisplay
{
    /// <summary>Converts an absolute path to one relative to <paramref name="basePath"/>, using forward
    /// slashes. Containment is decided by TEXT prefix first (H1: off-Windows, IsPathRooted reads a
    /// drive-style path as relative and GetRelativePath would CWD-prefix it); GetRelativePath remains
    /// the fallback for rooted-but-not-contained paths ("../sibling" display). Returns the input
    /// (slash-normalized) when there's no base or relativization fails.</summary>
    public static string Relative(string? basePath, string path)
    {
        if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(path))
            return (path ?? "").Replace('\\', '/');
        var p = path.Replace('\\', '/');
        var b = basePath.Replace('\\', '/').TrimEnd('/');
        if (p.StartsWith(b + "/", StringComparison.OrdinalIgnoreCase))
            return p[(b.Length + 1)..];
        if (Path.IsPathRooted(path))
        {
            try
            {
                return Path.GetRelativePath(basePath, path).Replace('\\', '/');
            }
            catch (ArgumentException ex)
            {
                // Invalid path chars — display the normalized input instead.
                Pipeline.PipelineDiagnostics.Swallowed("PathDisplay", "relativize", ex);
            }
        }
        return p;
    }

    /// <summary>Relativizes a "file:line" provenance string, preserving the trailing ":line". Tolerates
    /// drive-letter colons (e.g. <c>C:\…\X.cs:42</c>).</summary>
    public static string RelativeProvenance(string? basePath, string provenance)
    {
        if (string.IsNullOrEmpty(provenance)) return provenance;

        if (SplitProvenance(provenance) is var (path, line) && line is not null)
            return Relative(basePath, path) + ":" + line;
        return Relative(basePath, provenance);
    }

    /// <summary>M1.1 — the same "file:line" rule as <see cref="RelativeProvenance"/>, but returning the
    /// two halves as DATA. Every surface that wants the site structured (the wire's
    /// <c>TraceNode.file_path</c>/<c>line_number</c>, and so every client that would otherwise
    /// re-invent the split) goes through here, so the drive-letter-colon rule lives in one place.
    /// Line is null when the string is a bare path or the suffix is not a number.</summary>
    public static (string Path, int? Line) SplitProvenance(string? provenance)
    {
        if (string.IsNullOrEmpty(provenance)) return ("", null);

        var colon = provenance.LastIndexOf(':');
        if (colon > 1 && int.TryParse(provenance[(colon + 1)..], out var line))
            return (provenance[..colon], line);
        return (provenance, null);
    }
}
