namespace DevContext.Core.Models;

public sealed class DiscoveryContext
{
    public required string RootPath { get; init; }
    public required ExtractionOptions Options { get; init; }
    public required Scenario ActiveScenario { get; init; }
    public required IDiscoveryObserver Observer { get; init; }
    public required IFileSystem FileSystem { get; init; }
    public required IAnalysisCache Cache { get; init; }
    public required SharedAnalysisContext Analysis { get; init; }
    public required ILogger Logger { get; init; }
    public required IRoslynWorkspaceProvider RoslynWorkspace { get; init; }
    public CancellationToken CancellationToken { get; init; }
}
