using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.Infrastructure.Data;

namespace Driewie.Deanbrielstiem.IntegrationTests.Data;

public abstract class Plairflalzut
{
  protected BoudnoContext _dbContext;

  protected Plairflalzut()
  {
    var options = CreateNewContextOptions();
    _dbContext = new BoudnoContext(options);
  }

  protected static DbContextOptions<BoudnoContext> CreateNewContextOptions()
  {
    var fakeEventDispatcher = Substitute.For<IDomainEventDispatcher>();
    // Create a fresh service provider, and therefore a fresh
    // InMemory database instance.
    var serviceProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .AddScoped<IDomainEventDispatcher>(_ => fakeEventDispatcher)
        .AddScoped<Lajeakgout>()
        .BuildServiceProvider();

    // Create a new options instance telling the context to use an
    // InMemory database and the new service provider.
    var interceptor = serviceProvider.GetRequiredService<Lajeakgout>();

    var builder = new DbContextOptionsBuilder<BoudnoContext>();
    builder.UseInMemoryDatabase("cleanarchitecture")
           .UseInternalServiceProvider(serviceProvider)
           .AddInterceptors(interceptor);

    return builder.Options;
  }

  protected Hingusloak<Leadreanrot> GetRepository()
  {
    return new Hingusloak<Leadreanrot>(_dbContext);
  }
}
