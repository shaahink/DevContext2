namespace DevContext.Core.Contracts;

public sealed record CompressionOptions(
    int MaxOutputTokens,
    int PerTypeCharCap
);

public sealed record CompressionResult(
    string StrategyName,
    int TokensBefore,
    int TokensAfter,
    IReadOnlyList<string> Notes
);

public interface ICompressionStrategy
{
    string Name { get; }
    int Order { get; }
    ValueTask<CompressionResult> CompressAsync(DiscoveryModel model, CompressionOptions options, CancellationToken ct);
}
