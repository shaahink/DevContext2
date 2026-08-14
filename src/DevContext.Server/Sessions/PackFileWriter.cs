using System.Text;

namespace DevContext.Server.Sessions;

/// <summary>What was written, described by the side that wrote it (N3.2, STUDIO-MCP §8.3).</summary>
public sealed record PackFileResult(string Path, string RelativePath, bool Gitignored, string AgentLine);

/// <summary>
/// N3.2 — the repo-file hand-off. Save writes the composed pack into the analyzed repo as
/// <c>.devcontext/packs/&lt;slug&gt;.md</c>, so the artifact a human composed has an address an
/// agent can be pointed at. Everything here is a fact only the SERVER holds: the repo root, the
/// sanitized slug, whether the gitignore already existed. The client is told, never asked.
/// </summary>
public static class PackFileWriter
{
    /// <summary>The convention directory — one per repo, next to the analyzed root.</summary>
    public const string DevContextDir = ".devcontext";

    /// <summary>Packs live under their own subdirectory so the gitignore can cover exactly them.</summary>
    public const string PacksDir = "packs";

    /// <summary>Decision 3 says gitignored BY DEFAULT: a pack is a working artifact, not a commit.</summary>
    public const string GitignoreBody = "# DevContext writes composed context packs here (Studio -> Save).\n"
        + "# They are working artifacts, not source. Delete this line to commit them.\n"
        + "packs/\n";

    /// <summary>Longest slug we will write — long enough for a descriptive name, short of MAX_PATH trouble.</summary>
    private const int MaxSlugLength = 60;

    /// <summary>
    /// A caller-proposed name becomes a file name here and nowhere else. Anything outside
    /// [a-z0-9-] collapses to a single dash, so <c>../../etc/passwd</c> cannot address a path:
    /// the separators are not merely rejected, they cannot survive the transform.
    /// </summary>
    public static string Slugify(string? proposed)
    {
        if (string.IsNullOrWhiteSpace(proposed)) return "context-pack";

        var sb = new StringBuilder(proposed.Length);
        foreach (var ch in proposed)
        {
            if (char.IsAsciiLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > MaxSlugLength) slug = slug[..MaxSlugLength].TrimEnd('-');
        return slug.Length == 0 ? "context-pack" : slug;
    }

    /// <summary>The Studio's three output formats, as file extensions.</summary>
    public static string ExtensionFor(string? format) => format switch
    {
        "plain" => "txt",
        "json" => "json",
        _ => "md",
    };

    /// <summary>
    /// The copyable "point your agent here" line for CLAUDE.md / AGENTS.md. Composed here rather
    /// than in the app because every other pack string is (the app is a thin client — one build
    /// path, no client-side assembly), and because it must name the path the server actually
    /// wrote. Deliberately free of backticks and of a generation date: it goes into a repo file
    /// verbatim, and a date in it would be a claim that rots the moment the pack is rebuilt.
    /// </summary>
    public static string AgentLine(string relativePath) =>
        $"Read {relativePath} before answering questions about this repo — "
        + "it is a DevContext context pack (map, flow trace and signatures, each with file:line).";

    /// <summary>
    /// Writes the pack and returns where it went. Creates <c>.devcontext/packs/</c> and, when the
    /// repo has no <c>.devcontext/.gitignore</c> yet, writes one covering <c>packs/</c>. An
    /// existing gitignore is never edited — we report whether it covers packs instead, so the UI
    /// stops promising a default it did not set.
    /// </summary>
    public static PackFileResult Write(string repoRoot, string? slug, string content, string? format)
    {
        if (string.IsNullOrWhiteSpace(repoRoot))
            throw new InvalidOperationException("This session has no repository root on disk, so there is nowhere to write the pack.");

        var root = System.IO.Path.GetFullPath(repoRoot);
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"Repository root no longer exists: {root}");

        var dir = System.IO.Path.Combine(root, DevContextDir);
        var packsDir = System.IO.Path.Combine(dir, PacksDir);
        Directory.CreateDirectory(packsDir);

        var fileName = $"{Slugify(slug)}.{ExtensionFor(format)}";
        var fullPath = System.IO.Path.Combine(packsDir, fileName);
        // UTF-8 without a BOM: the pack is pasted into prompts and diffed by humans.
        File.WriteAllText(fullPath, content ?? "", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var gitignorePath = System.IO.Path.Combine(dir, ".gitignore");
        bool gitignored;
        if (!File.Exists(gitignorePath))
        {
            File.WriteAllText(gitignorePath, GitignoreBody, new UTF8Encoding(false));
            gitignored = true;
        }
        else
        {
            gitignored = CoversPacks(File.ReadAllLines(gitignorePath));
        }

        var relative = $"{DevContextDir}/{PacksDir}/{fileName}";
        return new PackFileResult(fullPath, relative, gitignored, AgentLine(relative));
    }

    /// <summary>
    /// Does an existing .devcontext/.gitignore actually cover the packs directory? Only the
    /// patterns that unambiguously do count — this answers "is the default still in force",
    /// and a maybe has to read as a no or the flag is worth nothing.
    /// </summary>
    private static bool CoversPacks(IEnumerable<string> lines)
    {
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line is "packs/" or "packs" or "/packs/" or "/packs" or "*" or "**") return true;
        }
        return false;
    }
}
