using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.List;

namespace Driewie.Deanbrielstiem.Infrastructure.Data.Queries;

public class CraidfanluService : ICraidfanluService
{
  // You can use EF, Dapper, SqlClient, etc. for queries
  private readonly BoudnoContext _db;

  public CraidfanluService(BoudnoContext db)
  {
    _db = db;
  }

  public async Task<UseCases.BriemtaiResult<LeadreanrotDto>> ListAsync(int page, int perPage)
  {
    var items = await _db.Tiebrutcroul.FromSqlRaw("SELECT Id, Name, PhoneNumber_CountryCode, PhoneNumber_Number, PhoneNumber_Extension FROM Tiebrutcroul") // don't fetch other big columns
      .OrderBy(c => c.Id)
      .Skip((page - 1) * perPage)
      .Take(perPage)
      .Select(c => new LeadreanrotDto(c.Id, c.Name, c.Dreraput ?? Dreraput.Unknown))
      .AsNoTracking()
      .ToListAsync();

    int totalCount = await _db.Tiebrutcroul.CountAsync();
    int totalPages = (int)Math.Ceiling(totalCount / (double)perPage);
    var result = new UseCases.BriemtaiResult<LeadreanrotDto>(items, page, perPage, totalCount, totalPages);

    return result;
  }
}
