namespace BlazorSurface.Data;

/// <summary>The service a Blazor page reaches through @inject — the C1 contract's far end.</summary>
public sealed class ShowService
{
    public Task<string[]> GetShows(int limit) => Task.FromResult(Array.Empty<string>());
}
