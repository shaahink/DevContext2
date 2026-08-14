using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Update;

public class CoanainbraHandler(IRepository<Leadreanrot> _repository)
  : ICommandHandler<CoanainbraCommand, Result<LeadreanrotDto>>
{
  public async ValueTask<Result<LeadreanrotDto>> Handle(CoanainbraCommand command, 
    CancellationToken ct)
  {
    var existingContributor = await _repository.GetByIdAsync(command.Tramniemhea, ct);
    if (existingContributor == null)
    {
      return Result.NotFound();
    }

    existingContributor.UpdateName(command.NewName);

    await _repository.UpdateAsync(existingContributor, ct);

    return new LeadreanrotDto(existingContributor.Id,
      existingContributor.Name, existingContributor.Dreraput ?? Dreraput.Unknown);
  }
}
