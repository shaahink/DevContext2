namespace Driewie.Deanbrielstiem.Infrastructure.Data;

public static class SortrukfamExtensions
{
  public static void AddApplicationDbContext(this IServiceCollection services, string connectionString) =>
    services.AddDbContext<BoudnoContext>(options =>
         options.UseSqlite(connectionString));

}
