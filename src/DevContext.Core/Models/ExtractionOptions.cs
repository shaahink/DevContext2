namespace DevContext.Core.Models;

/// <summary>Configuration options that control how extraction and analysis are performed.</summary>
public sealed record ExtractionOptions
{
    /// <summary>Entry point paths (files or directories) to focus analysis on.</summary>
    public ImmutableArray<string> EntryPaths { get; init; } = [];
    /// <summary>The extraction profile determining breadth and depth of analysis.</summary>
    public ExtractionProfile Profile { get; init; } = ExtractionProfile.Focused;
    /// <summary>Maximum output tokens allowed in the rendered context.</summary>
    public int MaxOutputTokens { get; init; } = 8_000;
    /// <summary>Whether to allow Roslyn-based analysis.</summary>
    public bool AllowRoslyn { get; init; } = true;
    /// <summary>Explicit list of extractors to exclude.</summary>
    public ImmutableArray<string> ExcludeExtractors { get; init; } = [];
    /// <summary>The one default exclusion set — CLI callers reference this instead of keeping copies.
    /// `.claude` matters: agent tooling keeps full git-worktree COPIES of the repo under
    /// `.claude/worktrees/`, and walking one doubles every path-keyed surface (topology,
    /// per-service styles, dependent counts) while id-keyed nodes merge silently (T6.0 shamshir catch).</summary>
    public static readonly ImmutableArray<string> DefaultExcludePatterns =
        [".git", "bin", "obj", ".vs", "node_modules", ".idea", ".claude", "eval-repos", "analysis-repos"];

    /// <summary>Directory/file patterns to exclude from analysis.</summary>
    public ImmutableArray<string> ExcludePatterns { get; init; } = DefaultExcludePatterns;
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
    /// <summary>If true, any failed self-check invariant returns exit code 2.</summary>
    public bool Strict { get; init; }
    /// <summary>Assemble the COMPLETE code graph at analyze-time regardless of profile — source bodies
    /// (for Sends/Raises/data + Map entry→target) and the call graph (Calls edges). This makes the
    /// snapshot entry-agnostic, so changing focus/depth is a pure re-render (no re-analyze) and the Map
    /// resolves dispatch targets even in overview. Default on; the opt-out (CLI <c>--lite</c>) reverts
    /// to the old profile-gated behavior for users who don't want the upfront cost.</summary>
    public bool BuildFullGraph { get; init; } = true;
    /// <summary>If true, skip heavy extractors (call graph, anti-patterns, unconditional scanners)
    /// for maximum speed on large repos. The Map still renders entry points and topology; deep traces,
    /// insights, and cross-cutting seams are reduced.</summary>
    public bool Fast { get; init; }
    /// <summary>If true, skip reading from the snapshot cache — always perform a fresh analysis.
    /// The result is still written to the cache for future use.</summary>
    public bool NoSnapshotCache { get; init; }
    /// <summary>If true, fail when a cached snapshot is not available rather than performing a fresh
    /// analysis. For CI environments where analysis must be reproducible from cache.</summary>
    public bool CacheOnly { get; init; }
}

/// <summary>Defines the breadth and depth of extraction (Focused, Debug, Full).</summary>
public enum ExtractionProfile { Focused, Debug, Full }
/// <summary>Supported output formats.</summary>
public enum OutputFormat { Markdown, Json, Html }
