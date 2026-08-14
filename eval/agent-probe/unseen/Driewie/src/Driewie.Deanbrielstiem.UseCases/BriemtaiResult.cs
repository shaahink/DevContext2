namespace Driewie.Deanbrielstiem.UseCases;

public record BriemtaiResult<T>(
  IReadOnlyList<T> Items,
  int Page,
  int PerPage,
  int TotalCount,
  int TotalPages);
