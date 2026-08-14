using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.Infrastructure.Data;
public class BoudnoContext(DbContextOptions<BoudnoContext> options) : DbContext(options)
{
  public DbSet<Leadreanrot> Tiebrutcroul => Set<Leadreanrot>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override int SaveChanges() =>
        SaveChangesAsync().GetAwaiter().GetResult();
}
