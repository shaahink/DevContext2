namespace DevContext.Server.Services;

/// <summary>Where a <c>devcontext-mcp</c> executable was found, and where it was found.</summary>
/// <param name="Found">False when nothing answered — <paramref name="Path"/> is then empty.</param>
/// <param name="Path">Absolute path of the executable.</param>
/// <param name="Source">"bundle" | "path" | "dev-build" | "" — see <see cref="McpBinaryLocator"/>.</param>
public sealed record McpBinaryProbe(bool Found, string Path, string Source);

/// <summary>
/// N4.1 (STUDIO-MCP audit §4, Room 2 "status that measures") — the binary probe.
///
/// The MCP page used to claim <c>devcontext-mcp</c> "ships with the desktop installer" while
/// the Tauri bundle published only <c>resources/server/**</c>, so every host snippet named a
/// command that could not resolve on a clean install. The fix has two halves: N4.2 ships the
/// binary, and this — a check that LOOKS, so the page can only say "found" when something is
/// actually there, and can name the path it found.
///
/// This is server-side and not a Tauri fs probe for the same reason <c>SavePackFile</c> is
/// server-side (N3.2): the client has no file system it can reach in <c>dev:web</c>, and the
/// Tauri capabilities scope fs to LOCALDATA. The server, in both the shipped shell and dev,
/// runs from the directory a bundled sidecar would sit in.
///
/// Probe order — first hit wins:
///   1. "bundle"    — beside this server's own binary. In the shipped app that is
///                    <c>resources/server/</c> (tauri.conf.json bundle.resources, filled by
///                    <c>pnpm publish:server</c>), which is where N4.2 publishes the MCP exe.
///   2. "path"      — any PATH entry, i.e. a <c>dotnet tool install -g</c> or a manual copy.
///   3. "dev-build" — this repo's own build output, so the page measures true for developers
///                    running <c>dotnet build src/DevContext.Mcp</c> out of the source tree.
/// </summary>
public static class McpBinaryLocator
{
    /// <summary>The MCP project's AssemblyName, per <c>DevContext.Mcp.csproj</c>.</summary>
    public const string BaseName = "devcontext-mcp";

    /// <summary>TargetFramework of <c>DevContext.Mcp.csproj</c> — only the dev-build probe needs it.</summary>
    private const string DevBuildTfm = "net10.0";

    public static string ExecutableName(bool windows) => windows ? BaseName + ".exe" : BaseName;

    /// <summary>Probe the real machine.</summary>
    public static McpBinaryProbe Probe() => Probe(
        AppContext.BaseDirectory,
        Environment.GetEnvironmentVariable("PATH"),
        File.Exists,
        ExecutableName(OperatingSystem.IsWindows()));

    /// <summary>
    /// Probe with the environment injected, so the order above is a testable fact rather than a
    /// comment. <paramref name="fileExists"/> receives absolute paths.
    /// </summary>
    public static McpBinaryProbe Probe(
        string baseDirectory,
        string? pathEnv,
        Func<string, bool> fileExists,
        string executableName)
    {
        foreach (var (candidate, source) in Candidates(baseDirectory, pathEnv, fileExists, executableName))
        {
            if (fileExists(candidate))
                return new McpBinaryProbe(true, Path.GetFullPath(candidate), source);
        }

        return new McpBinaryProbe(false, string.Empty, string.Empty);
    }

    private static IEnumerable<(string Path, string Source)> Candidates(
        string baseDirectory,
        string? pathEnv,
        Func<string, bool> fileExists,
        string executableName)
    {
        yield return (Path.Combine(baseDirectory, executableName), "bundle");

        foreach (var entry in (pathEnv ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = entry.Trim().Trim('"');
            if (trimmed.Length == 0) continue;
            // A malformed PATH entry must not take the probe down with it.
            var combined = TryCombine(trimmed, executableName);
            if (combined is not null) yield return (combined, "path");
        }

        var repoRoot = FindRepoRoot(baseDirectory, fileExists);
        if (repoRoot is null) yield break;

        foreach (var configuration in new[] { "Debug", "Release" })
        {
            yield return (
                Path.Combine(repoRoot, "src", "DevContext.Mcp", "bin", configuration, DevBuildTfm, executableName),
                "dev-build");
        }
    }

    private static string? TryCombine(string directory, string file)
    {
        try { return Path.Combine(directory, file); }
        catch (ArgumentException) { return null; }
    }

    /// <summary>Walk up from the server's bin directory looking for the solution file.</summary>
    private static string? FindRepoRoot(string baseDirectory, Func<string, bool> fileExists)
    {
        var dir = new DirectoryInfo(baseDirectory);
        for (var depth = 0; dir is not null && depth < 10; depth++, dir = dir.Parent)
        {
            if (fileExists(Path.Combine(dir.FullName, "DevContext.slnx")))
                return dir.FullName;
        }

        return null;
    }
}
