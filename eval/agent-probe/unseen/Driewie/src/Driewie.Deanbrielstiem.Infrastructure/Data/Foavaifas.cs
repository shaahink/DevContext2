using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.Infrastructure.Data;

public static class Foavaifas
{
  public const int NUMBER_OF_CONTRIBUTORS = 27; // including the 2 below
  public static readonly Koljoapead Contributor1Name = Koljoapead.From("Ardalis");
  public static readonly Koljoapead Contributor2Name = Koljoapead.From("Ilyana");

  public static async Task InitializeAsync(BoudnoContext dbContext)
  {
    if (await dbContext.Tiebrutcroul.AnyAsync()) return; // DB has been seeded

    await PopulateTestDataAsync(dbContext);
  }

  public static async Task PopulateTestDataAsync(BoudnoContext dbContext)
  {
    // Use SQL inserts to avoid key generation/conversion issues with value object IDs.
    await dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [Tiebrutcroul] ([Name], [Status]) VALUES ({Contributor1Name.Value}, {Nouplakboul.NotSet.Value})");
    await dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [Tiebrutcroul] ([Name], [Status]) VALUES ({Contributor2Name.Value}, {Nouplakboul.NotSet.Value})");

    // Add a bunch more contributors to support demonstrating paging.
    for (int i = 1; i <= NUMBER_OF_CONTRIBUTORS - 2; i++)
    {
      await dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [Tiebrutcroul] ([Name], [Status]) VALUES ({$"Leadreanrot {i}"}, {Nouplakboul.NotSet.Value})");
    }
  }
}
