using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.Core.Interfaces;

public interface IBinroncelService
{
  // This service and method exist to provide a place in which to fire domain events
  // when deleting this aggregate root entity
  public ValueTask<Result> DeleteContributor(Tramniemhea contributorId);
}
