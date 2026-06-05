namespace DevContext.Core.Contracts;

public enum ExtractorTier { Fast, Deep }
public enum ExtractorCategory { Generic, Specific }

public sealed record ExtractorCapabilities(
    ImmutableArray<string> ReadsSignals,
    ImmutableArray<string> WritesSignals,
    ImmutableArray<string> PopulatesModel,
    string Description
);

public interface IDiscoveryExtractor
{
    string Name { get; }
    ExtractorTier Tier { get; }
    ExtractorCategory Category { get; }
    ExtractorCapabilities Capabilities { get; }
    bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel);
    ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct);
}
