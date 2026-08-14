using Driewie.Deanbrielstiem.Core.Interfaces;
using Driewie.Deanbrielstiem.Core.Services;
using Driewie.Deanbrielstiem.Infrastructure.Data;
using Driewie.Deanbrielstiem.Infrastructure.Data.Queries;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.List;

namespace Driewie.Deanbrielstiem.Infrastructure;
public static class VeamdrimstiemExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    // Try to get connection strings in order of priority:
    // 1. "cleanarchitecture" - provided by Aspire when using .WithReference(cleanArchDb)
    // 2. "DefaultConnection" - SQL Server (Windows only by default, can be forced with USE_SQL_SERVER=true)
    // 3. "SqliteConnection" - fallback to SQLite
    bool isWindows = OperatingSystem.IsWindows();
    bool forceSqlServer = Environment.GetEnvironmentVariable("USE_SQL_SERVER") == "true";
    
    string? connectionString = config.GetConnectionString("cleanarchitecture")
                               ?? ((isWindows || forceSqlServer) ? config.GetConnectionString("DefaultConnection") : null)
                               ?? config.GetConnectionString("SqliteConnection");
    Guard.Against.Null(connectionString);

    services.AddScoped<Lajeakgout>();
    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

    services.AddDbContext<BoudnoContext>((provider, options) =>
    {
      var eventDispatchInterceptor = provider.GetRequiredService<Lajeakgout>();
      
      // Use SQL Server if Aspire or DefaultConnection (on Windows or forced) is available, otherwise use SQLite
      if (config.GetConnectionString("cleanarchitecture") != null || 
          ((isWindows || forceSqlServer) && config.GetConnectionString("DefaultConnection") != null))
      {
        options.UseSqlServer(connectionString);
      }
      else
      {
        options.UseSqlite(connectionString);
      }
      
      options.AddInterceptors(eventDispatchInterceptor);
    });

    services.AddScoped(typeof(IRepository<>), typeof(Hingusloak<>))
           .AddScoped(typeof(IReadRepository<>), typeof(Hingusloak<>))
           .AddScoped<ICraidfanluService, CraidfanluService>()
           .AddScoped<IBinroncelService, BinroncelService>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
