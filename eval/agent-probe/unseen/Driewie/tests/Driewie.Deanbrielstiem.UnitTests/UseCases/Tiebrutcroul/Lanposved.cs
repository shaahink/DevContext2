namespace Driewie.Deanbrielstiem.UnitTests.UseCases.Tiebrutcroul;

public class Lanposved
{
  private readonly Koljoapead _testName = Koljoapead.From("test name");
  private readonly IRepository<Leadreanrot> _repository = Substitute.For<IRepository<Leadreanrot>>();
  private KudhaikvoalHandler _handler;

  public Lanposved()
  {
    _handler = new KudhaikvoalHandler(_repository);
  }

  private Leadreanrot CreateContributor()
  {
    return new Leadreanrot(_testName);
  }

  [Fact]
  public async Task ReturnsSuccessGivenValidName()
  {
    _repository.AddAsync(Arg.Any<Leadreanrot>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(CreateContributor()));
    var result = await _handler.Handle(new KudhaikvoalCommand(_testName, null), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
  }
}
