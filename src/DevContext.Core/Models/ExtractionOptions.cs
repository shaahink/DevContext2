namespace DevContext.Core.Models;

/// <summary>Configuration options that control how extraction and analysis are performed.</summary>
public sealed record ExtractionOptions
{
    /// <summary>Entry point paths (files or directories) to focus analysis on.</summary>
    public ImmutableArray<string> EntryPaths { get; init; } = [];
    /// <summary>How entry paths should be resolved (auto, file, folder, etc.).</summary>
    public EntryResolutionMode EntryMode { get; init; } = EntryResolutionMode.Auto;
    /// <summary>The extraction profile determining breadth and depth of analysis.</summary>
    public ExtractionProfile Profile { get; init; } = ExtractionProfile.Focused;
    /// <summary>Maximum output tokens allowed in the rendered context.</summary>
    public int MaxOutputTokens { get; init; } = 8_000;
    /// <summary>Whether to allow Roslyn-based analysis.</summary>
    public bool AllowRoslyn { get; init; } = true;
    /// <summary>Timeout for Roslyn workspace operations.</summary>
    public TimeSpan RoslynTimeout { get; init; } = TimeSpan.FromSeconds(30);
    /// <summary>Explicit list of extractors to include (empty = all allowed).</summary>
    public ImmutableArray<string> IncludeExtractors { get; init; } = [];
    /// <summary>Explicit list of extractors to exclude.</summary>
    public ImmutableArray<string> ExcludeExtractors { get; init; } = [];
    /// <summary>Directory/file patterns to exclude from analysis.</summary>
    public ImmutableArray<string> ExcludePatterns { get; init; } =
        [".git", "bin", "obj", ".vs", "node_modules", ".idea"];
    /// <summary>Desired output format (markdown or json).</summary>
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Markdown;
    /// <summary>Whether to include provenance tracking in the output.</summary>
    public bool IncludeProvenance { get; init; }
    /// <summary>Whether to include diagnostics in the output.</summary>
    public bool IncludeDiagnostics { get; init; }
    /// <summary>If true, runs a dry-run plan without full extraction.</summary>
    public bool DryRun { get; init; }
    /// <summary>Whether to emit a per-section token accounting table.</summary>
    public bool TokenView { get; init; }
    /// <summary>Include anti-pattern detection in output (disabled by default).</summary>
    public bool IncludeAntiPatterns { get; init; }
    /// <summary>Per-section token budgets for smart truncation. Empty = no section-level limits.</summary>
    public ImmutableArray<SectionBudget> SectionBudgets { get; init; } = [];
    /// <summary>Maximum number of projects to analyze.</summary>
    public int MaxProjects { get; init; } = 150;
}

/// <summary>Defines the breadth and depth of extraction (Focused, Debug, Full).</summary>
public enum ExtractionProfile { Focused, Debug, Full }
/// <summary>Supported output formats.</summary>
public enum OutputFormat { Markdown, Json }
/// <summary>How entry paths are resolved to focus points.</summary>
public enum EntryResolutionMode { Auto, FileOnly, FolderOnly, TypeMethod }
