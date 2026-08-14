namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.List;

public record LaitroastikQuery(int? Page = 1, int? PerPage = Tienhedvus.DEFAULT_PAGE_SIZE)
  : IQuery<Result<BriemtaiResult<LeadreanrotDto>>>;
