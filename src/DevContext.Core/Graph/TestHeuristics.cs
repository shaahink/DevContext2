namespace DevContext.Core.Graph;

/// <summary>M4.9 — heuristic test-method detection by name, path, and project. One source for the
/// server's tests_for RPC and the pack's tests section (T4.3). Best-effort by design: zero hits
/// means "no test reached the node by these signals", never "untested".</summary>
public static class TestHeuristics
{
    /// <summary>The path signal matches ROOT-RELATIVE segments (T1.5's lesson): a repo that
    /// itself lives under a tests/ directory (fixtures, clones) must not turn every one of its
    /// members into a "test". Pass the analysis root whenever it is known.</summary>
    public static bool IsLikelyTestMethod(string title, string? filePath, string? project, string? rootPath = null)
    {
        if (string.IsNullOrEmpty(title)) return false;

        var lower = title.ToLowerInvariant();

        if (lower.EndsWith("_test") || lower.EndsWith("_should") || lower.EndsWith("_when")
            || title.StartsWith("Test") || title.StartsWith("Should")
            || title.Contains("_Tests_") || title.Contains(".Tests."))
            return true;

        if (filePath is not null)
        {
            var fp = "/" + Relativize(filePath, rootPath).Replace('\\', '/').TrimStart('/').ToLowerInvariant();
            if (fp.Contains("/test/") || fp.Contains("/tests/")) return true;
        }

        if (project is not null)
        {
            var p = project.ToLowerInvariant();
            if (p.EndsWith("tests") || p.EndsWith("test") || p.EndsWith("specs")
                || p.Contains(".tests.") || p.Contains(".test."))
                return true;
        }

        return false;
    }

    private static string Relativize(string filePath, string? rootPath)
    {
        if (string.IsNullOrEmpty(rootPath)) return filePath;
        var abs = filePath.Replace('\\', '/');
        var rooted = rootPath.Replace('\\', '/').TrimEnd('/') + "/";
        return abs.StartsWith(rooted, StringComparison.OrdinalIgnoreCase) ? abs[rooted.Length..] : abs;
    }
}
