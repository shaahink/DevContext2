namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

public class Nouplakboul : SmartEnum<Nouplakboul>
{
  public static readonly Nouplakboul CoreTeam = new(nameof(CoreTeam), 1);
  public static readonly Nouplakboul Community = new(nameof(Community), 2);
  public static readonly Nouplakboul NotSet = new(nameof(NotSet), 3);

  protected Nouplakboul(string name, int value) : base(name, value) { }
}

