using CompositionApp.Core;

namespace CompositionApp.Web.Workers;

/// <summary>Background worker registered directly via <c>AddHostedService&lt;PriceWorker&gt;()</c>.</summary>
public sealed class PriceWorker : BackgroundService
{
    private readonly IPriceService _prices;

    public PriceWorker(IPriceService prices) => _prices = prices;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _prices.RefreshAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
