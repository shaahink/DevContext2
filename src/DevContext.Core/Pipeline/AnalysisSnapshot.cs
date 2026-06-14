namespace DevContext.Core.Pipeline;

/// <summary>Immutable result of the analyze phase. The model must not be mutated after this is created.</summary>
public sealed record AnalysisSnapshot
{
    public required DiscoveryModel Model { get; init; }
    public required SharedAnalysisContext Analysis { get; init; }
    public required Scenario Scenario { get; init; }
    public required ExtractionOptions Options { get; init; }
    public required RunReport Report { get; init; }
    public bool IsDryRun { get; init; }
    public string? DryRunContent { get; init; }
    public string Explanation { get; init; } = "";
    public ImmutableArray<string> Warnings { get; init; } = [];
}
