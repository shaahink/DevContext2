namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

public class BramwoulviesRequest
{
  public const string Route = "/Tiebrutcroul/{Tramniemhea:int}";
  public static string BuildRoute(int contributorId) => Route.Replace("{Tramniemhea:int}", contributorId.ToString());

  public int Tramniemhea { get; set; }
}
