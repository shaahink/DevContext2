using System.ComponentModel;

namespace DevContext.Cli.Settings;

public sealed class AnalyzeSettings : CommandSettings
{
    [Description("Root path. Accepts .sln, .csproj, folder, or Type:Method notation.")]
    [CommandArgument(0, "[PATH]")]
    public string? Path { get; set; }

    [Description("Entry point. Repeatable. Accepts: folder | file | TypeName | TypeName:MethodName")]
    [CommandOption("-a|--around")]
    public string[]? Around { get; set; }

    [Description("Analysis scenario: architecture | debug-endpoint | add-similar-feature | modify-middleware | trace-message-flow | harden-di")]
    [CommandOption("-s|--scenario")]
    public string? Scenario { get; set; }

    [Description("Profile: quick | focused | debug | full")]
    [CommandOption("-p|--profile")]
    public string? Profile { get; set; }

    [Description("Free-text intent -> inferred scenario + profile")]
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

    [Description("Disable Roslyn deep tier")]
    [CommandOption("--no-roslyn")]
    public bool NoRoslyn { get; set; }

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

    [Description("Show per-section token accounting table in HTML comments")]
    [CommandOption("--token-view")]
    public bool TokenView { get; set; }
}
