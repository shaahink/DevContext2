using DevContext.Core.Models;
using DevContext.Core.Pipeline;

namespace DevContext.Core.Contracts;

/// <summary>Per-section token accounting record produced during rendering.</summary>
public sealed record SectionTokenRecord(
    string SectionName,
    int RawTokens,
    int CompressedTokens,
    bool WasTruncated,
    string? TruncationReason = null
);

/// <summary>Lightweight section stat exposed to the desktop for token accounting.</summary>
public sealed record SectionStat(string Name, int Tokens);

/// <summary>Options that control how a renderer produces output.</summary>
public sealed record RenderOptions(
    bool IncludeProvenance,
    bool IncludeDiagnostics,
    int EstimatedTokens,
    string? ScenarioDisplayName = null,
    string? ProfileDisplayName = null,
    ImmutableArray<string> RequiredSections = default,
    ImmutableArray<FocusPoint> FocusPoints = default,
    CallGraph? CallGraph = null,
    ProjectDependencyGraph? ProjectGraph = null,
    bool TokenView = false,
    bool FullSections = false
)
{
    /// <summary>The RenderPlan lens applied during this render. May be null for legacy paths.</summary>
    public RenderPlan? Plan { get; init; }
    /// <summary>The RunReport from the analysis phase. Populated in JSON output.</summary>
    public RunReport? Report { get; init; }
}

/// <summary>The result of rendering a discovery model into a specific output format.</summary>
public sealed record RenderedContext(
    string Content,
    int EstimatedTokens,
    IReadOnlyList<CompressionResult> AppliedCompressions,
    TimeSpan ElapsedTotal,
    string SchemaVersion,
    IReadOnlyList<SectionTokenRecord>? SectionTokens = null
)
{
    /// <summary>Self-check invariant failures collected after rendering. Empty when no failures.</summary>
    public ImmutableArray<string> SelfCheckFailures { get; init; } = [];
    /// <summary>Per-section token counts for desktop section display.</summary>
    public ImmutableArray<SectionStat> Sections { get; init; } = [];
    /// <summary>Per-section rendered fragments (section key → fragment content) for interactive desktop toggling. Null when the renderer doesn't fragment.</summary>
    public IReadOnlyDictionary<string, string>? SectionFragments { get; init; }
    /// <summary>Token funnel computed during render from the plan and output. Null on legacy paths.</summary>
    public TokenFunnel? RenderFunnel { get; init; }
}

/// <summary>Renders a discovery model into a specific output format (e.g. markdown, JSON).</summary>
public interface IContextRenderer
{
    /// <summary>Gets the format identifier (e.g. "markdown", "json").</summary>
    string Format { get; }
    /// <summary>Renders the model and returns the formatted output.</summary>
    ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct);
}
