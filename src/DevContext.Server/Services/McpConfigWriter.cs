using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DevContext.Server.Services;

/// <summary>One agent host's project-scoped MCP config file, and the shape it expects.</summary>
/// <param name="Id">What the client sends — "claude" | "cursor" | "vscode".</param>
/// <param name="Label">The display name the MCP page shows on the card.</param>
/// <param name="RelativePath">Repo-relative path of the config file, forward-slashed.</param>
/// <param name="ServersKey">Top-level object the host reads servers from.</param>
/// <param name="StdioType">True when the host wants an explicit <c>"type": "stdio"</c> on the entry.</param>
public sealed record McpConfigTarget(
    string Id,
    string Label,
    string RelativePath,
    string ServersKey,
    bool StdioType);

/// <summary>What was written, described by the side that wrote it.</summary>
/// <param name="Action">"created" | "updated" | "unchanged".</param>
public sealed record McpConfigWriteResult(string Path, string RelativePath, string Action, string Command);

/// <summary>
/// N4.2 (STUDIO-MCP audit §4, Room 2 "setup that works") — the write-config-for-me half.
///
/// The MCP page could only ever hand out a snippet with a placeholder in it, and a placeholder
/// is where setup fails: the user has to know where <c>devcontext-mcp</c> ended up, which key
/// their host reads, and which file it lives in. All three are facts THIS side holds, so this
/// writes the file.
///
/// Two deliberate constraints:
///
/// 1. <b>Project-scoped, never user-global.</b> A host's global config (<c>~/.claude.json</c>,
///    <c>~/.cursor/mcp.json</c>) is a file this server cannot verify it edited correctly and
///    that carries every other project's servers. The repo the user just analysed is the one
///    they want the agent pointed at, and a repo-local config is reviewable and reversible with
///    <c>git status</c>. Decision 3 (N3.2's pack hand-off) set the same precedent.
/// 2. <b>Merge, never clobber.</b> An existing config keeps every other server, every unrelated
///    key, and its own formatting when nothing needs to change. A file that is not valid JSON
///    is an error, not something to overwrite.
/// </summary>
public static class McpConfigWriter
{
    /// <summary>The key DevContext registers itself under in every host's config.</summary>
    public const string ServerName = "devcontext";

    /// <summary>The hosts the MCP page offers. Order is the order the page renders.</summary>
    public static readonly IReadOnlyList<McpConfigTarget> Targets =
    [
        // Claude Code reads a project-scoped .mcp.json at the repo root (claude mcp add --scope project
        // writes the same file), keyed mcpServers.
        new("claude", "Claude Code", ".mcp.json", "mcpServers", StdioType: false),
        // Cursor: .cursor/mcp.json, same key as Claude's.
        new("cursor", "Cursor", ".cursor/mcp.json", "mcpServers", StdioType: false),
        // VS Code: .vscode/mcp.json, keyed `servers`, and it wants the transport named.
        new("vscode", "VS Code", ".vscode/mcp.json", "servers", StdioType: true),
    ];

    public static McpConfigTarget? TargetFor(string? hostId) =>
        Targets.FirstOrDefault(t => string.Equals(t.Id, hostId, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 2-space indented, which is what every one of these files is written as by hand.
    ///
    /// MEASURED: the default encoder escapes the HTML-sensitive set, so it rendered the "not
    /// found" placeholder's angle brackets as escape sequences — and would do the same to a
    /// non-ASCII repo path. This output is a config file and a snippet a human reads, never HTML,
    /// so the relaxed encoder is the correct one here.
    /// </summary>
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>The snippet text the page shows for a host — the same JSON this writer produces
    /// for a fresh file, so what is on screen and what lands on disk cannot drift.</summary>
    public static string Snippet(McpConfigTarget target, string command)
    {
        var root = new JsonObject { [target.ServersKey] = new JsonObject { [ServerName] = Entry(target, command) } };
        return root.ToJsonString(WriteOptions);
    }

    /// <summary>
    /// What the page renders per host. With a binary found this carries its RESOLVED absolute
    /// path — the whole point of N4.2. With none found it carries an angle-bracketed placeholder
    /// rather than a plausible-looking path: the page's old hard-coded
    /// <c>C:/path/to/DevContext2/...</c> read like a real machine's, and a user who pasted it got
    /// a host that failed to spawn with no clue why.
    /// </summary>
    public static string SnippetFor(McpConfigTarget target, McpBinaryProbe binary) =>
        Snippet(target, binary.Found && binary.Path.Length > 0 ? binary.Path : PlaceholderCommand);

    /// <summary>Visibly not a path. See <see cref="SnippetFor"/>.</summary>
    public static string PlaceholderCommand =>
        $"<absolute path to {McpBinaryLocator.ExecutableName(OperatingSystem.IsWindows())}>";

    /// <summary>
    /// Writes (or merges into) the host's project config and reports what happened.
    /// </summary>
    /// <param name="repoRoot">The analysed repository root — the config goes inside it.</param>
    /// <param name="target">Which host.</param>
    /// <param name="binary">The resolved devcontext-mcp probe; a config naming a path that does
    /// not exist is the exact lie this checkpoint removes, so a miss is refused.</param>
    public static McpConfigWriteResult Write(string repoRoot, McpConfigTarget target, McpBinaryProbe binary)
    {
        if (string.IsNullOrWhiteSpace(repoRoot))
            throw new InvalidOperationException("This session has no repository root on disk, so there is nowhere to write a host config.");
        if (!binary.Found || binary.Path.Length == 0)
            throw new InvalidOperationException(
                $"{McpBinaryLocator.BaseName} was not found on this machine, so any config written here would name a path that does not exist. "
                + "Build it (dotnet build src/DevContext.Mcp) or install the desktop bundle, then re-check.");

        var root = Path.GetFullPath(repoRoot);
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"Repository root no longer exists: {root}");

        var fullPath = Path.GetFullPath(Path.Combine(root, target.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var existed = File.Exists(fullPath);

        var document = existed ? Parse(fullPath) : new JsonObject();
        var servers = Servers(document, target, fullPath);
        var desired = Entry(target, binary.Path);

        if (servers.TryGetPropertyValue(ServerName, out var current) && JsonNode.DeepEquals(current, desired))
        {
            // Nothing to say and nothing to touch — rewriting here would reformat a file the user
            // owns for no gain, and would report a change that did not happen.
            return new McpConfigWriteResult(fullPath, target.RelativePath, "unchanged", binary.Path);
        }

        servers[ServerName] = desired;

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        // UTF-8 without a BOM and a trailing newline: these files get committed and diffed.
        File.WriteAllText(fullPath, document.ToJsonString(WriteOptions) + "\n", new UTF8Encoding(false));

        return new McpConfigWriteResult(fullPath, target.RelativePath, existed ? "updated" : "created", binary.Path);
    }

    private static JsonObject Entry(McpConfigTarget target, string command)
    {
        var entry = new JsonObject();
        if (target.StdioType) entry["type"] = "stdio";
        entry["command"] = command;
        entry["args"] = new JsonArray();
        return entry;
    }

    private static JsonObject Parse(string path)
    {
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(
                File.ReadAllText(path),
                documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        }
        catch (JsonException ex)
        {
            // Refuse rather than overwrite: this is the user's file and it may hold other servers.
            throw new InvalidOperationException($"{path} is not valid JSON ({ex.Message}) — fix or remove it, then try again.");
        }

        return parsed as JsonObject
            ?? throw new InvalidOperationException($"{path} does not contain a JSON object, so there is nothing to merge into.");
    }

    private static JsonObject Servers(JsonObject document, McpConfigTarget target, string path)
    {
        if (!document.TryGetPropertyValue(target.ServersKey, out var existing) || existing is null)
        {
            var created = new JsonObject();
            document[target.ServersKey] = created;
            return created;
        }

        return existing as JsonObject
            ?? throw new InvalidOperationException(
                $"{path} has a \"{target.ServersKey}\" that is not an object, so {target.Label} cannot be reading servers from it. Fix it and try again.");
    }
}
