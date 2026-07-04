using System.ComponentModel;

namespace DevContext.Cli.Settings;

/// <summary>Produces a full report doc — one deterministic markdown (or JSON) document that mirrors
/// what the app sees: identity, stats, top flows, traces, insights, architecture map, and run report.</summary>
public sealed class ReportSettings : CommandSettings
{
    [Description("Root path. Accepts .sln, .csproj, folder, or GitHub URL.")]
    [CommandArgument(0, "[PATH]")]
    public string? Path { get; set; }

    [Description("Write output to file (stdout when omitted)")]
    [CommandOption("-o|--output")]
    public string? Output { get; set; }

    [Description("Output format: markdown (default) | json")]
    [CommandOption("--format")]
    public string? Format { get; set; }

    [Description("GitHub repo URL to clone and analyze")]
    [CommandOption("--repo")]
    public string? Repo { get; set; }

    [Description("Branch or tag to check out")]
    [CommandOption("--ref")]
    public string? Ref { get; set; }

    [Description("Skip snapshot cache — always run a fresh analysis")]
    [CommandOption("--no-cache")]
    public bool NoCache { get; set; }

    [Description("Fail if no cached snapshot is available")]
    [CommandOption("--cache-only")]
    public bool CacheOnly { get; set; }

    [Description("Suppress all output on success")]
    [CommandOption("--quiet")]
    public bool Quiet { get; set; }
}
