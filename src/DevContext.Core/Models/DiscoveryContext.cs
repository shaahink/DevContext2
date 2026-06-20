namespace DevContext.Core.Models;

/// <summary>Aggregates all context and services needed to run the discovery pipeline.</summary>
public sealed class DiscoveryContext
{
    /// <summary>The root directory to analyze.</summary>
    public required string RootPath { get; init; }
    /// <summary>
    /// When non-empty, source-file discovery walks the union of these project directories (the Hybrid
    /// closure scan set from <see cref="Resolvers.ScopeResolver"/>) instead of <see cref="RootPath"/>.
    /// Empty ⇒ walk <see cref="RootPath"/> (whole-solution / folder mode).
    /// </summary>
    public ImmutableArray<string> ScopedProjectDirs { get; init; } = [];
    /// <summary>Options controlling extraction behavior.</summary>
    public required ExtractionOptions Options { get; init; }
    /// <summary>The active scenario defining what to extract, prune, and compress.</summary>
    public required Scenario ActiveScenario { get; init; }
    /// <summary>Observer for pipeline lifecycle events.</summary>
    public required IDiscoveryObserver Observer { get; init; }
    /// <summary>Abstraction over the file system.</summary>
    public required IFileSystem FileSystem { get; init; }
    /// <summary>Analysis cache for parsed files, syntax trees, and XML.</summary>
    public required IAnalysisCache Cache { get; init; }
    /// <summary>Shared analysis state accumulated across pipeline stages.</summary>
    public required SharedAnalysisContext Analysis { get; init; }
    /// <summary>Logger for diagnostic output.</summary>
    public required ILogger Logger { get; init; }
    /// <summary>Provider for Roslyn workspace access.</summary>
    public required IRoslynWorkspaceProvider RoslynWorkspace { get; init; }
    /// <summary>Optional cancellation token for the pipeline run.</summary>
    public CancellationToken CancellationToken { get; init; }
}
