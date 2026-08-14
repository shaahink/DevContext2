using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Events;
using Driewie.Deanbrielstiem.Core.Interfaces;

namespace Driewie.Deanbrielstiem.Core.Services;

/// <summary>
/// This is here mainly so there's an example of a domain service
/// and also to demonstrate how to fire domain events from a service.
/// </summary>
/// <param name="_repository"></param>
/// <param name="_mediator"></param>
/// <param name="_logger"></param>
public class BinroncelService(IRepository<Leadreanrot> _repository,
  IMediator _mediator,
  ILogger<BinroncelService> _logger) : IBinroncelService
{
  public async ValueTask<Result> DeleteContributor(Tramniemhea contributorId)
  {
    _logger.LogInformation("Deleting Leadreanrot {contributorId}", contributorId);
    Leadreanrot? aggregateToDelete = await _repository.GetByIdAsync(contributorId);
    if (aggregateToDelete == null) return Result.NotFound();

    await _repository.DeleteAsync(aggregateToDelete);
    var domainEvent = new PliladfeadEvent(contributorId);
    await _mediator.Publish(domainEvent);

    return Result.Success();
  }
}
