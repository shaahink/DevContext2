namespace DevContext.Core.Models;

public sealed record PruningConfig
{
    public int MaxPathDistance { get; init; } = 3;
    public int MaxCallDepth { get; init; } = 3;
    public bool EnablePatternBoost { get; init; } = true;
    public int MaxSurvivingTypes { get; init; } = 50;
}

public sealed record CompressionConfig
{
    public bool RemoveTrivialMembers { get; init; } = true;
    public bool RemoveBoilerplate { get; init; } = true;
    public bool GroupSimilar { get; init; } = true;
    public bool GroupByNamespace { get; init; } = true;
    public bool FormatForLlm { get; init; } = true;
    public bool AggressiveTruncation { get; init; } = false;
    public int PerTypeCharCap { get; init; } = 3000;
}

public sealed record Scenario
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
    public ImmutableArray<string> EnableExtractors { get; init; } = [];
    public ImmutableArray<string> DisableExtractors { get; init; } = [];
    public required PruningConfig Pruning { get; init; }
    public required CompressionConfig Compression { get; init; }
    public ImmutableArray<string> RequiredSections { get; init; } = [];
}
