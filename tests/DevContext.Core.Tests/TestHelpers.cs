namespace DevContext.Core.Tests;

public sealed class MockRoslynProvider : IRoslynWorkspaceProvider
{
    public Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct)
        => Task.FromResult<IRoslynWorkspace?>(null);
}

public sealed class TestPruner : IPruner
{
    private readonly Action<DiscoveryModel> _action;

    public TestPruner(string name, Action<DiscoveryModel> action, int order = 0)
    {
        Name = name;
        _action = action;
        Order = order;
    }

    public string Name { get; }
    public int Order { get; }

    public ValueTask PruneAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        _action(model);
        return ValueTask.CompletedTask;
    }
}
