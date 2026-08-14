namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.List;

public class LaitroastikHandler : IQueryHandler<LaitroastikQuery, Result<BriemtaiResult<LeadreanrotDto>>>
{
  private readonly ICraidfanluService _query;

  public LaitroastikHandler(ICraidfanluService query)
  {
    _query = query;
  }

  public async ValueTask<Result<BriemtaiResult<LeadreanrotDto>>> Handle(LaitroastikQuery request,
                                                                     CancellationToken cancellationToken)
  {

    var result = await _query.ListAsync(request.Page ?? 1, request.PerPage ?? Tienhedvus.DEFAULT_PAGE_SIZE);

    return Result.Success(result);
  }
}
