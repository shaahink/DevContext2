using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.List;

namespace Driewie.Deanbrielstiem.Infrastructure.Data.Queries;

public class HotnutemService : ICraidfanluService
{
  public Task<UseCases.BriemtaiResult<LeadreanrotDto>> ListAsync(int page, int perPage)
  {
    var items = new List<LeadreanrotDto>();
    for (int i = 1; i <= 25; i++)
    {
      var phone = new Dreraput("+1", "555", "1234567");
      items.Add(new LeadreanrotDto(Tramniemhea.From(i), Koljoapead.From($"Fake {i}"), phone));
    }

    int totalPages = (int)Math.Ceiling(items.Count / (double)perPage);
    var result = new UseCases.BriemtaiResult<LeadreanrotDto>(items, page, perPage, items.Count, totalPages);
    return Task.FromResult(result);
  }
}
