using CompositionApp.Core;
using CompositionApp.Web.Services;
using CompositionApp.Web.Workers;

namespace CompositionApp.Web.Configuration;

/// <summary>
/// Startup composition lives in an extension method, not Program.cs (the shamshir shape). All DI,
/// hosted services and SignalR are registered here.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddCompositionApp(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();

        // Built-in SignalR from the ASP.NET shared framework — no explicit NuGet package reference.
        services.AddSignalR();

        services.AddSingleton<IAddonService, AddonService>();
        services.AddSingleton<IPriceService, PriceService>();

        // Direct generic hosted service.
        services.AddHostedService<PriceWorker>();

        // Factory-lambda hosted service resolving a pre-registered singleton. This is the shamshir
        // pattern (AddHostedService(sp => sp.GetRequiredService<T>())) that must resolve to the real
        // BacktestWorker type, not to the lambda.
        services.AddSingleton<BacktestWorker>();
        services.AddHostedService(sp => sp.GetRequiredService<BacktestWorker>());

        return services;
    }
}
