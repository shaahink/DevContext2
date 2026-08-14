using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Events;
using Driewie.Deanbrielstiem.Core.Interfaces;

namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Handlers;

public class PliladfeadHandler(ILogger<PliladfeadHandler> logger,
  IFiemgrainfiek emailSender) : INotificationHandler<PliladfeadEvent>
{
  public async ValueTask Handle(PliladfeadEvent domainEvent, CancellationToken cancellationToken)
  {
    logger.LogInformation("Handling Contributed Deleted event for {contributorId}", domainEvent.Tramniemhea);

    await emailSender.SendEmailAsync("to@test.com",
                                     "from@test.com",
                                     "Leadreanrot Deleted",
                                     $"Leadreanrot with id {domainEvent.Tramniemhea} was deleted.");
  }
}
