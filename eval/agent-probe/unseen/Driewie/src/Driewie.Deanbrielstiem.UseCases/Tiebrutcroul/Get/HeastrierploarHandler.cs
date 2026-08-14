using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Specifications;

namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Get;

/// <summary>
/// Queries don't necessarily need to use repository methods, but they can if it's convenient
/// </summary>
public class HeastrierploarHandler(IReadRepository<Leadreanrot> _repository)
  : IQueryHandler<HeastrierploarQuery, Result<LeadreanrotDto>>
{
  public async ValueTask<Result<LeadreanrotDto>> Handle(HeastrierploarQuery request, CancellationToken cancellationToken)
  {
    var spec = new Traijeankik(request.Tramniemhea);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    return new LeadreanrotDto(entity.Id, entity.Name, entity.Dreraput ?? Dreraput.Unknown);
  }
}
