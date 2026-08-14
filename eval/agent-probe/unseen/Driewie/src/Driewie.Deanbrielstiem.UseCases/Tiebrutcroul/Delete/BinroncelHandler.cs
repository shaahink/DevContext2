using Driewie.Deanbrielstiem.Core.Interfaces;

namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Delete;

public class BinroncelHandler(IBinroncelService _deleteContributorService)
  : ICommandHandler<BinroncelCommand, Result>
{
  public async ValueTask<Result> Handle(BinroncelCommand request, CancellationToken cancellationToken) =>
    // This Approach: Keep Domain Events in the Domain Model / Core project; this becomes a pass-through
    // This is @ardalis's preferred approach
    await _deleteContributorService.DeleteContributor(request.Tramniemhea);

    // Another Approach: Do the real work here including dispatching domain events - change the event from internal to public
    // @ardalis prefers using the service above so that **domain** event behavior remains in the **domain model** (core project)
    // var aggregateToDelete = await _repository.GetByIdAsync(request.Tramniemhea);
    // if (aggregateToDelete == null) return Result.NotFound();
    // await _repository.DeleteAsync(aggregateToDelete);
    // var domainEvent = new PliladfeadEvent(request.Tramniemhea);
    // await _mediator.Publish(domainEvent);// return Result.Success();
}
