using System.ComponentModel;

namespace DevContext.Cli.Settings;

public sealed class AnalyzeSettings : CommandSettings
{
    [Description("Root path. Accepts .sln, .csproj, folder, or Type:Method notation.")]
    [CommandArgument(0, "[PATH]")]
    public string? Path { get; set; }

    [Description("Focus point. Repeatable. Accepts: TypeName | TypeName:MethodName | GET /route")]
    [CommandOption("-f|--focus")]
    public string[]? Focus { get; set; }

    [Description("Graph depth from focus point (1-10)")]
    [CommandOption("--depth")]
    public int? Depth { get; set; }

    [Description("Trace detail level: signature | salient | full (default: salient)")]
    [CommandOption("--detail")]
    public string? Detail { get; set; }

    [Description("Entry point (alias for --focus)")]
    [CommandOption("-a|--around")]
    public string[]? Around { get; set; }

    [Description("Analysis scenario: overview | deep-dive (advanced — derived from --focus)")]
    [CommandOption("-s|--scenario")]
    public string? Scenario { get; set; }

    [Description("Profile: focused | debug | full (advanced — derived from --focus)")]
    [CommandOption("-p|--profile")]
    public string? Profile { get; set; }

    [Description("Free-text intent (deprecated — use --focus instead)")]
    [CommandOption("-t|--task")]
    public string? Task { get; set; }

    [Description("Token cap (default 8000)")]
    [CommandOption("--max-tokens")]
    public int? MaxTokens { get; set; }

    [Description("Write output to file")]
    [CommandOption("-o|--output")]
    public string? Output { get; set; }

    [Description("Output format: markdown | json")]
    [CommandOption("--format")]
    public string? Format { get; set; }

    [Description("Include provenance info in output")]
    [CommandOption("--include-provenance")]
    public bool IncludeProvenance { get; set; }

    [Description("Include diagnostics in output")]
    [CommandOption("--include-diagnostics")]
    public bool IncludeDiagnostics { get; set; }

    [Description("When tracing (--focus), also render the Map/architecture sections alongside the trace")]
    [CommandOption("--include-map")]
    public bool IncludeMapWithTrace { get; set; }

    [Description("Disable Roslyn deep tier")]
    [CommandOption("--no-roslyn")]
    public bool NoRoslyn { get; set; }

    [Description("Lite mode: skip the full graph build (source bodies + call graph) for speed; "
        + "the Map still renders but loses dispatch targets/deep traces, and focus re-analyzes")]
    [CommandOption("--lite")]
    public bool Lite { get; set; }

    [Description("Info-level logging")]
    [CommandOption("--verbose")]
    public bool Verbose { get; set; }

    [Description("Debug-level logging (includes Roslyn events)")]
    [CommandOption("--trace")]
    public bool Trace { get; set; }

    [Description("Plan only -- no extraction")]
    [CommandOption("--dry-run")]
    public bool DryRun { get; set; }

    [Description("Emit structured per-extractor timing report")]
    [CommandOption("--metrics")]
    public bool Metrics { get; set; }

    [Description("Show full RunReport statistics (--metrics is an alias)")]
    [CommandOption("--stats")]
    public bool Stats { get; set; }

    [Description("Show per-section token accounting table in HTML comments")]
    [CommandOption("--token-view")]
    public bool TokenView { get; set; }

    [Description("Include anti-pattern detection in output")]
    [CommandOption("--include-anti-patterns")]
    public bool IncludeAntiPatterns { get; set; }

    [Description("GitHub repo URL to clone and analyze")]
    [CommandOption("--repo")]
    public string? Repo { get; set; }

    [Description("Branch or tag to check out (default: repo default)")]
    [CommandOption("--ref")]
    public string? Ref { get; set; }

    [Description("Clone cleanup: auto (default — delete after analysis) | keep (retain clone)")]
    [CommandOption("--cleanup")]
    public string? Cleanup { get; set; }

    [Description("Keep cloned repo after analysis")]
    [CommandOption("--keep")]
    public bool Keep { get; set; }

    [Description("Fail with exit code 2 on any self-check invariant violation")]
    [CommandOption("--strict")]
    public bool Strict { get; set; }
}
