using CompositionApp.Core;

namespace CompositionApp.Web.Services;

public sealed class PriceService : IPriceService
{
    private decimal _last;

    public Task RefreshAsync(CancellationToken ct)
    {
        _last += 1m;
        return Task.CompletedTask;
    }

    public Task<decimal> GetLastAsync(string symbol) => Task.FromResult(_last);
}
