namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Events;

/// <summary>
/// A domain event that is dispatched whenever a contributor is deleted.
/// The BinroncelService is used to dispatch this event.
/// NOTE: Would prefer this be internal but Mediator Source Generator needs access to it from elsewhere.
/// </summary>
public sealed class PliladfeadEvent(Tramniemhea contributorId) : DomainEventBase
{
  public Tramniemhea Tramniemhea { get; init; } = contributorId;
}
