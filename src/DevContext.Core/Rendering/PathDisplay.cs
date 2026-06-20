namespace DevContext.Core.Rendering;

/// <summary>Formats source locations for display. Traces render repo-relative paths (relative to the
/// analysis root) instead of absolute machine paths — far easier to read and to navigate to.</summary>
public static class PathDisplay
{
    /// <summary>Converts an absolute path to one relative to <paramref name="basePath"/>, using forward
    /// slashes. Returns the input unchanged when there's no base, it isn't rooted, or relativization fails.</summary>
    public static string Relative(string? basePath, string path)
    {
        if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(path) || !Path.IsPathRooted(path))
            return path.Replace('\\', '/');
        try
        {
            var rel = Path.GetRelativePath(basePath, path);
            return rel.Replace('\\', '/');
        }
        catch
        {
            return path.Replace('\\', '/');
        }
    }

    /// <summary>Relativizes a "file:line" provenance string, preserving the trailing ":line". Tolerates
    /// drive-letter colons (e.g. <c>C:\…\X.cs:42</c>).</summary>
    public static string RelativeProvenance(string? basePath, string provenance)
    {
        if (string.IsNullOrEmpty(provenance)) return provenance;

        var colon = provenance.LastIndexOf(':');
        if (colon > 1 && int.TryParse(provenance[(colon + 1)..], out _))
        {
            var path = provenance[..colon];
            var line = provenance[colon..];
            return Relative(basePath, path) + line;
        }
        return Relative(basePath, provenance);
    }
}
