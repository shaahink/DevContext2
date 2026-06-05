namespace DevContext.Core.Tests;

public sealed class MockRoslynProvider : IRoslynWorkspaceProvider
{
    public Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct)
        => Task.FromResult<IRoslynWorkspace?>(null);
}
