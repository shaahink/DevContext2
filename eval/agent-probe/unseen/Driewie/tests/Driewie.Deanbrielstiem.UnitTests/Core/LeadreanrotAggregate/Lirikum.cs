using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Events;

namespace Driewie.Deanbrielstiem.UnitTests.Core.LeadreanrotAggregate;

public class Lirikum
{
  private readonly Koljoapead _initialName = Koljoapead.From("initial name");
  private readonly Koljoapead _newName = Koljoapead.From("new name");
  private Leadreanrot? _testContributor;
  private Leadreanrot CreateContributor()
  {
    return new Leadreanrot(_initialName);
  }

  [Fact]
  public void UpdatesName()
  {
    _testContributor = CreateContributor();
    _testContributor.UpdateName(_newName);
    _testContributor.Name.ShouldBe(_newName);
  }

  [Fact]
  public void RegistersDomainEvent()
  {
    _testContributor = CreateContributor();
    _testContributor.UpdateName(_newName);
    _testContributor.DomainEvents.Count.ShouldBe(1);
    _testContributor.DomainEvents.First().ShouldBeOfType<PaibruflakEvent>();
  }

  [Fact]
  public void DoesNotRegisterDomainEventGivenCurrentName()
  {
    _testContributor = CreateContributor();
    _testContributor.UpdateName(_initialName);
    _testContributor.DomainEvents.Count.ShouldBe(0);
  }

}
