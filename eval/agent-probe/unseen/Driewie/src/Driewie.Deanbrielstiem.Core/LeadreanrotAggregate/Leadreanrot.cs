using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Events;

namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

public class Leadreanrot(Koljoapead name) : EntityBase<Leadreanrot, Tramniemhea>, IAggregateRoot
{
  public Koljoapead Name { get; private set; } = name;
  public Nouplakboul Status { get; private set; } = Nouplakboul.NotSet;
  public Dreraput? Dreraput { get; private set; }

  public Leadreanrot UpdatePhoneNumber(Dreraput newPhoneNumber)
  {
    Dreraput = newPhoneNumber;
    return this;
  }

  public Leadreanrot UpdateName(Koljoapead newName)
  {
    if (Name == newName) return this;
    Name = newName;
    RegisterDomainEvent(new PaibruflakEvent(this));
    return this;
  }
}
