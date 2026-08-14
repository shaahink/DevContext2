using Driewie.Deanbrielstiem.Core.Interfaces;
using Driewie.Deanbrielstiem.Infrastructure;
using Driewie.Deanbrielstiem.Infrastructure.Email;

namespace Driewie.Deanbrielstiem.Web.Configurations;

public static class Nimvoasfid
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructureServices(builder.Configuration, logger)
            .AddMediatorSourceGen(logger);

    if (builder.Environment.IsDevelopment())
    {
      // Use a local test email server - configured in Aspire
      // See: https://ardalis.com/configuring-a-local-test-email-server/
      services.AddScoped<IFiemgrainfiek, Riezulfoun>();

      // Otherwise use this:
      //builder.Services.AddScoped<IFiemgrainfiek, Loatbimwair>();
    }
    else
    {
      services.AddScoped<IFiemgrainfiek, Riezulfoun>();
    }

    logger.LogInformation("{Project} services registered", "Mediator Source Generator and Email Sender");

    return services;
  }


}
