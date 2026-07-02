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

    // ── Deprecated / removed flags (kept as invisible stubs for one-release grace) ──

    [Description("(removed) use --focus")]
    [CommandOption("-a|--around", IsHidden = true)]
    public string[]? Around { get; set; }

    [Description("(removed) use --focus")]
    [CommandOption("-s|--scenario", IsHidden = true)]
    public string? Scenario { get; set; }

    [Description("(removed) use --focus")]
    [CommandOption("-p|--profile", IsHidden = true)]
    public string? Profile { get; set; }

    [Description("(removed) use --focus")]
    [CommandOption("-t|--task", IsHidden = true)]
    public string? Task { get; set; }

    [Description("(removed) no longer needed — the token budget is retired")]
    [CommandOption("--max-tokens", IsHidden = true)]
    public int? MaxTokens { get; set; }

    [Description("(removed) token view is retired with the catalog")]
    [CommandOption("--token-view", IsHidden = true)]
    public bool TokenView { get; set; }

    [Description("(removed) provenance is always on in narrative traces")]
    [CommandOption("--include-provenance", IsHidden = true)]
    public bool IncludeProvenance { get; set; }

    [Description("(removed) anti-patterns are an insight source now")]
    [CommandOption("--include-anti-patterns", IsHidden = true)]
    public bool IncludeAntiPatterns { get; set; }

    [Description("(removed) --stats already shows the RunReport")]
    [CommandOption("--metrics", IsHidden = true)]
    public bool Metrics { get; set; }

    [Description("(removed) --keep is the inverse")]
    [CommandOption("--cleanup", IsHidden = true)]
    public string? Cleanup { get; set; }

    // ── Active flags ──

    [Description("Write output to file")]
    [CommandOption("-o|--output")]
    public string? Output { get; set; }

    [Description("Output format: markdown | json")]
    [CommandOption("--format")]
    public string? Format { get; set; }

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

    [Description("Show full RunReport statistics")]
    [CommandOption("--stats")]
    public bool Stats { get; set; }

    [Description("GitHub repo URL to clone and analyze")]
    [CommandOption("--repo")]
    public string? Repo { get; set; }

    [Description("Branch or tag to check out (default: repo default)")]
    [CommandOption("--ref")]
    public string? Ref { get; set; }

    [Description("Keep cloned repo after analysis")]
    [CommandOption("--keep")]
    public bool Keep { get; set; }

    [Description("Fail with exit code 2 on any self-check invariant violation")]
    [CommandOption("--strict")]
    public bool Strict { get; set; }
}
