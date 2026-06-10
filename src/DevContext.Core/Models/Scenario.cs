namespace DevContext.Core.Models;

/// <summary>Configuration for pruning behavior within a scenario.</summary>
public sealed record PruningConfig
{
    /// <summary>Maximum directory distance from focus points before pruning.</summary>
    public int MaxPathDistance { get; init; } = 3;
    /// <summary>Maximum call graph depth to traverse.</summary>
    public int MaxCallDepth { get; init; } = 3;
    /// <summary>Whether to boost relevance scores for types that appear in detections.</summary>
    public bool EnablePatternBoost { get; init; } = true;
    /// <summary>Maximum number of types to retain after pruning.</summary>
    public int MaxSurvivingTypes { get; init; } = 50;
}

/// <summary>Configuration for compression behavior within a scenario.</summary>
public sealed record CompressionConfig
{
    /// <summary>Whether to remove trivial members (parameterless ctors, ToString, etc.).</summary>
    public bool RemoveTrivialMembers { get; init; } = true;
    /// <summary>Whether to remove boilerplate and designer-generated code.</summary>
    public bool RemoveBoilerplate { get; init; } = true;
    /// <summary>Whether to group structurally similar types.</summary>
    public bool GroupSimilar { get; init; } = true;
    /// <summary>Whether to group types by namespace.</summary>
    public bool GroupByNamespace { get; init; } = true;
    /// <summary>Whether to format source bodies for LLM consumption.</summary>
    public bool FormatForLlm { get; init; } = true;
    /// <summary>Whether to aggressively truncate source bodies.</summary>
    public bool AggressiveTruncation { get; init; }
    /// <summary>Character cap per type source body.</summary>
    public int PerTypeCharCap { get; init; } = 3000;
}

/// <summary>Defines a named scenario that controls which extractors, pruning, and compression are applied.</summary>
public sealed record Scenario
{
    /// <summary>Unique name of the scenario.</summary>
    public required string Name { get; init; }
    /// <summary>Human-readable display name.</summary>
    public required string DisplayName { get; init; }
    /// <summary>Optional description of the scenario's purpose.</summary>
    public string? Description { get; init; }
    /// <summary>Extractors to explicitly enable for this scenario.</summary>
    public ImmutableArray<string> EnableExtractors { get; init; } = [];
    /// <summary>Extractors to explicitly disable for this scenario.</summary>
    public ImmutableArray<string> DisableExtractors { get; init; } = [];
    /// <summary>Pruning configuration for this scenario.</summary>
    public required PruningConfig Pruning { get; init; }
    /// <summary>Compression configuration for this scenario.</summary>
    public required CompressionConfig Compression { get; init; }
    /// <summary>Sections that must appear in the rendered output for this scenario.</summary>
    public ImmutableArray<string> RequiredSections { get; init; } = [];
}
