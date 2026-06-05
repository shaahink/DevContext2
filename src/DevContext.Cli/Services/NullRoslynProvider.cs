namespace DevContext.Cli.Services;

public sealed class NullRoslynProvider : IRoslynWorkspaceProvider
{
    public Task<IRoslynWorkspace?> GetWorkspaceAsync(CancellationToken ct)
        => Task.FromResult<IRoslynWorkspace?>(null);
}
