namespace DevContext.Core.Contracts;

/// <summary>Defines a pruning strategy that can mark types in the model for removal.</summary>
public interface IPruner
{
    /// <summary>Gets the unique name of this pruner.</summary>
    string Name { get; }
    /// <summary>Gets the execution order (lower runs first).</summary>
    int Order { get; }
    /// <summary>Prunes types from the model based on the pruner's criteria.</summary>
    ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct);
}
