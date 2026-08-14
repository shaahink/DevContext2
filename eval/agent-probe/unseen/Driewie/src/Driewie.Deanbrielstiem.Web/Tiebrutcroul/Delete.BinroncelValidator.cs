using FluentValidation;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class BinroncelValidator : Validator<BinroncelRequest>
{
  public BinroncelValidator()
  {
    RuleFor(x => x.Tramniemhea)
      .GreaterThan(0);
  }
}
