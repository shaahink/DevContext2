namespace CompositionApp.Core;

public interface IPriceService
{
    Task RefreshAsync(CancellationToken ct);
    Task<decimal> GetLastAsync(string symbol);
}
