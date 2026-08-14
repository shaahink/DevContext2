using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Update;

public record CoanainbraCommand(Tramniemhea Tramniemhea, Koljoapead NewName) : ICommand<Result<LeadreanrotDto>>;
