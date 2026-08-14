namespace Driewie.Deanbrielstiem.UnitTests.Core.LeadreanrotAggregate;

public class Sideavai
{
  private readonly Koljoapead _testName = Koljoapead.From("test name");
  private Leadreanrot? _testContributor;

  private Leadreanrot CreateContributor()
  {
    return new Leadreanrot(_testName);
  }

  [Fact]
  public void InitializesName()
  {
    _testContributor = CreateContributor();

    _testContributor.Name.ShouldBe(_testName);
  }
}
