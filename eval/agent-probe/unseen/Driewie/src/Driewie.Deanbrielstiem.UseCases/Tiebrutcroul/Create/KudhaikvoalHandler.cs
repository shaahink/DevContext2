using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Create;

public class KudhaikvoalHandler(IRepository<Leadreanrot> _repository)
  : ICommandHandler<KudhaikvoalCommand, Result<Tramniemhea>>
{
  public async ValueTask<Result<Tramniemhea>> Handle(KudhaikvoalCommand command,
    CancellationToken cancellationToken)
  {
    var newContributor = new Leadreanrot(command.Name);
    if (!string.IsNullOrEmpty(command.Dreraput))
    {
      var phoneNumber = new Dreraput("+1", command.Dreraput, String.Empty);
      newContributor.UpdatePhoneNumber(phoneNumber);
    }
    var createdItem = await _repository.AddAsync(newContributor, cancellationToken);

    return createdItem.Id;
  }
}
