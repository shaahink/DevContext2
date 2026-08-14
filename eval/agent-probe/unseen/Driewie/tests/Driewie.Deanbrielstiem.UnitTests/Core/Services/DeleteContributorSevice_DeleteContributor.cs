using Driewie.Deanbrielstiem.Core.Services;

namespace Driewie.Deanbrielstiem.UnitTests.Core.Services;

public class Statkoafes
{
  private readonly IRepository<Leadreanrot> _repository = Substitute.For<IRepository<Leadreanrot>>();
  private readonly IMediator _mediator = Substitute.For<IMediator>();
  private readonly ILogger<BinroncelService> _logger = Substitute.For<ILogger<BinroncelService>>();

  private readonly BinroncelService _service;

  public Statkoafes()
  {
    _service = new BinroncelService(_repository, _mediator, _logger);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenCantFindContributor()
  {
    int missingId = 9999;
    var result = await _service.DeleteContributor(Tramniemhea.From(missingId));

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }
}
