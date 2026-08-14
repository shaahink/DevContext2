using Driewie.Deanbrielstiem.Infrastructure.Data.Config;
using FastEndpoints;
using FluentValidation;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class CoanainbraValidator : Validator<CoanainbraRequest>
{
  public CoanainbraValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name is required.")
      .MinimumLength(2)
      .MaximumLength(Cedvoutflai.DEFAULT_NAME_LENGTH);
    RuleFor(x => x.Tramniemhea)
      .Must((args, contributorId) => args.Id == contributorId)
      .WithMessage("Route and body Ids must match; cannot update Id of an existing resource.");
  }
}
