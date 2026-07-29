using System.ComponentModel;

namespace DevContext.Cli.Commands;

/// <summary>Query the analysis graph — same operations available in the desktop palette and MCP.
/// All ops run over an in-process analyze session (or attach to a running server with --attach).</summary>
public sealed class QuerySettings : CommandSettings
{
    [Description("Operation: entrypoints | map | trace | stats | node | neighbors | usages | search | graphdump")]
    [CommandArgument(0, "<OP>")]
    public string Op { get; set; } = "";

    [Description("Solution to analyze when the repo declares several (name, file name, or relative path)")]
    [CommandOption("--sln")]
    public string? Solution { get; set; }

    [Description("Focus for trace/node/neighbors/usages ops")]
    [CommandOption("-f|--focus")]
    public string? Focus { get; set; }

    [Description("Path to repo or solution")]
    [CommandOption("--path")]
    public string? Path { get; set; }

    [Description("Output format: json (default) | md")]
    [CommandOption("--format")]
    public string? Format { get; set; }

    [Description("Depth for trace/neighbors exploration")]
    [CommandOption("--depth")]
    public int? Depth { get; set; }

    [Description("Direction for neighbors op: out (default) | in")]
    [CommandOption("--direction")]
    public string? Direction { get; set; }

    // G3.2 (R4 item 9) — the pointed question: who WRITES this table
    // (query neighbors --focus Orders --direction in --kind ReadsWrites — OP is POSITIONAL, there
    // is no --op), who SENDS this command, who CONSUMES this event. The answer always reports the
    // unfiltered count and every kind present, so a kind that matches nothing says what to ask
    // instead.
    // Deliberately not an exhaustive list here — a --help line that enumerates an enum is a second
    // copy of it. An unknown kind is refused at runtime WITH the list, read from the enum itself.
    [Description("Seam kind filter for neighbors op (e.g. ReadsWrites, Sends, Consumes)")]
    [CommandOption("--kind")]
    public string? Kind { get; set; }

    [Description("gRPC server to attach to (host:port)")]
    [CommandOption("--attach")]
    public string? Attach { get; set; }

    [Description("Skip the snapshot cache: always re-analyze, and don't write the result")]
    [CommandOption("--no-cache")]
    public bool NoCache { get; set; }
}
