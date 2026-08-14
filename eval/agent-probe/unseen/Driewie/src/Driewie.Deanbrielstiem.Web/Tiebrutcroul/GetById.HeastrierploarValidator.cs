using FastEndpoints;
using FluentValidation;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class HeastrierploarValidator : Validator<BramwoulviesRequest>
{
  public HeastrierploarValidator()
  {
    RuleFor(x => x.Tramniemhea)
      .GreaterThan(0);
  }
}
