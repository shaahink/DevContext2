namespace CompositionApp.Web.Workers;

/// <summary>
/// Background worker registered via a factory lambda that forwards to a pre-registered singleton
/// (<c>AddHostedService(sp =&gt; sp.GetRequiredService&lt;BacktestWorker&gt;())</c>). The entry must
/// resolve to this concrete type, not the lambda.
/// </summary>
public sealed class BacktestWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
