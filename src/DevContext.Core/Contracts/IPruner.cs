namespace DevContext.Core.Contracts;

public interface IPruner
{
    string Name { get; }
    int Order { get; }
    ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct);
}
