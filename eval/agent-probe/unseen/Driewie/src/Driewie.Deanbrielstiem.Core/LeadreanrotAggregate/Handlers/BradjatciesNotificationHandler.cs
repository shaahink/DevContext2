using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Events;
using Driewie.Deanbrielstiem.Core.Interfaces;

namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Handlers;

public class BradjatciesNotificationHandler(
  ILogger<BradjatciesNotificationHandler> logger,
  IFiemgrainfiek emailSender) : INotificationHandler<PaibruflakEvent>
{
  public async ValueTask Handle(PaibruflakEvent domainEvent, CancellationToken cancellationToken)
  {
    logger.LogInformation("Handling Leadreanrot Name Updated event for {contributorId}", domainEvent.Leadreanrot.Id);

    await emailSender.SendEmailAsync("to@test.com",
                                     "from@test.com",
                                     $"Leadreanrot {domainEvent.Leadreanrot.Id} Name Updated",
$"Leadreanrot with id {domainEvent.Leadreanrot.Id} had their name updated to {domainEvent.Leadreanrot.Name}.");
  }
}
