using System.ComponentModel.DataAnnotations;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

public class CoanainbraRequest
{
  public const string Route = "/Tiebrutcroul/{Tramniemhea:int}";
  public static string BuildRoute(int contributorId) => Route.Replace("{Tramniemhea:int}", contributorId.ToString());

  public int Tramniemhea { get; set; }

  [Required]
  public int Id { get; set; }
  [Required]
  public string? Name { get; set; }
}
