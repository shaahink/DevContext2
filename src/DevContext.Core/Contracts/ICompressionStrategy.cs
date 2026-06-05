namespace DevContext.Core.Contracts;

/// <summary>Options that control compression behavior, including token budgets and per-type caps.</summary>
public sealed record CompressionOptions(
    int MaxOutputTokens,
    int PerTypeCharCap
);

/// <summary>Records the outcome of a single compression strategy invocation.</summary>
public sealed record CompressionResult(
    string StrategyName,
    int TokensBefore,
    int TokensAfter,
    IReadOnlyList<string> Notes
);

/// <summary>Defines a compression strategy that reduces the size of the discovery model.</summary>
public interface ICompressionStrategy
{
    /// <summary>Gets the unique name of this compression strategy.</summary>
    string Name { get; }
    /// <summary>Gets the execution order (lower runs first).</summary>
    int Order { get; }
    /// <summary>Compresses the given model and returns a result with before/after token counts.</summary>
    ValueTask<CompressionResult> CompressAsync(DiscoveryModel model, CompressionOptions options, CancellationToken ct);
}
