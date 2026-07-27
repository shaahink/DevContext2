using System.ComponentModel;

namespace DevContext.Cli.Commands;

/// <summary>Query the analysis graph — same operations available in the desktop palette and MCP.
/// All ops run over an in-process analyze session (or attach to a running server with --attach).</summary>
public sealed class QuerySettings : CommandSettings
{
    [Description("Operation: entrypoints | map | trace | stats | node | neighbors | usages | search | graphdump")]
    [CommandArgument(0, "<OP>")]
    public string Op { get; set; } = "";

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

    [Description("gRPC server to attach to (host:port)")]
    [CommandOption("--attach")]
    public string? Attach { get; set; }

    [Description("Skip the snapshot cache: always re-analyze, and don't write the result")]
    [CommandOption("--no-cache")]
    public bool NoCache { get; set; }
}
