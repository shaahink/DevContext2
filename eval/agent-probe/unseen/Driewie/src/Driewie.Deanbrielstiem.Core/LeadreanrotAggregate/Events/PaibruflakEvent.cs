namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Events;

public sealed class PaibruflakEvent(Leadreanrot contributor) : DomainEventBase
{
  public Leadreanrot Leadreanrot { get; init; } = contributor;
}
