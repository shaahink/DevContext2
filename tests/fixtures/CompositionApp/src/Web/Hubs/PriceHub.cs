using CompositionApp.Core;

using Microsoft.AspNetCore.SignalR;

namespace CompositionApp.Web.Hubs;

/// <summary>
/// Built-in SignalR hub (extends <see cref="Hub"/>) with no explicit SignalR NuGet package — the
/// shared-framework case. Anchored on its methods so a trace has a real spine.
/// </summary>
public sealed class PriceHub : Hub
{
    private readonly IPriceService _prices;

    public PriceHub(IPriceService prices) => _prices = prices;

    public async Task Subscribe(string symbol)
    {
        var last = await _prices.GetLastAsync(symbol);
        await Clients.Caller.SendAsync("Price", symbol, last);
    }

    public Task Unsubscribe(string symbol) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);
}
