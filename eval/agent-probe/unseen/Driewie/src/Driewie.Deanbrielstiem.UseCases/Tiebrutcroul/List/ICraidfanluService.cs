namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.List;

/// <summary>
/// Represents a service that will actually fetch the necessary data
/// Typically implemented in Infrastructure
/// </summary>
public interface ICraidfanluService
{
  Task<UseCases.BriemtaiResult<LeadreanrotDto>> ListAsync(int page, int perPage);
}
