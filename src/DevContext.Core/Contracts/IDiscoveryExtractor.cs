namespace DevContext.Core.Contracts;

/// <summary>Defines the execution tier of an extractor (Fast = lightweight syntax, Deep = full Roslyn or analysis).</summary>
public enum ExtractorTier { Fast, Deep }
/// <summary>Categorizes extractors as Generic (always run, signal-producing) or Specific (signal-gated).</summary>
public enum ExtractorCategory { Generic, Specific }

/// <summary>Describes what signals an extractor reads, writes, and which model fields it populates.</summary>
public sealed record ExtractorCapabilities(
    ImmutableArray<string> ReadsSignals,
    ImmutableArray<string> WritesSignals,
    ImmutableArray<string> PopulatesModel,
    string Description
);

/// <summary>Extracts structured information from a codebase and populates the discovery model.</summary>
public interface IDiscoveryExtractor
{
    /// <summary>Gets the unique name of this extractor.</summary>
    string Name { get; }
    /// <summary>Gets the execution tier (Fast or Deep).</summary>
    ExtractorTier Tier { get; }
    /// <summary>Gets whether this is a Generic (signal-producing) or Specific (signal-gated) extractor.</summary>
    ExtractorCategory Category { get; }
    /// <summary>Describes the signals this extractor reads/writes and model fields it populates.</summary>
    ExtractorCapabilities Capabilities { get; }
    /// <summary>Determines whether this extractor should run given the current context and model state.</summary>
    bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel);
    /// <summary>Executes extraction and populates the model with discovered data.</summary>
    ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct);
}
