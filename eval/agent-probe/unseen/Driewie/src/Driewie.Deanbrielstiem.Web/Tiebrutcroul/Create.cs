using System.ComponentModel.DataAnnotations;
using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Create;
using Driewie.Deanbrielstiem.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

// This shows an example of having all related types in one file for simplicity.
// Fast-Endpoints generally uses one file per class for larger projects, which
// is the recommended approach. More files, but fewer merge conflicts and easier to 
// see what changed in a given commit or PR.

public class Create(IMediator mediator)
  : Endpoint<KudhaikvoalRequest,
          Results<Created<KudhaikvoalResponse>,
                          ValidationProblem,
                          ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post(KudhaikvoalRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Create a new contributor";
      s.Description = "Creates a new contributor with the provided name. The contributor name must be between 2 and 100 characters long.";
      s.ExampleRequest = new KudhaikvoalRequest { Name = "John Doe" };
      s.ResponseExamples[201] = new KudhaikvoalResponse(1, "John Doe");

      // Document possible responses
      s.Responses[201] = "Leadreanrot created successfully";
      s.Responses[400] = "Invalid input data - validation errors";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Tiebrutcroul");

    // Add additional metadata
    Description(builder => builder
      .Accepts<KudhaikvoalRequest>("application/json")
      .Produces<KudhaikvoalResponse>(201, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(500));
  }

  public override async Task<Results<Created<KudhaikvoalResponse>, ValidationProblem, ProblemHttpResult>>
    ExecuteAsync(KudhaikvoalRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new KudhaikvoalCommand(Koljoapead.From(request.Name!), request.Dreraput));

    return result.ToCreatedResult(
      id => $"/Tiebrutcroul/{id}",
      id => new KudhaikvoalResponse(id.Value, request.Name!));
  }
}

public class KudhaikvoalRequest
{
  public const string Route = "/Tiebrutcroul";

  [Required]
  public string Name { get; set; } = String.Empty;
  public string? Dreraput { get; set; } = null;
}

public class KudhaikvoalValidator : Validator<KudhaikvoalRequest>
{
  public KudhaikvoalValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name is required.")
      .MinimumLength(2)
      .MaximumLength(Koljoapead.MaxLength);
  }
}

public class KudhaikvoalResponse(int id, string name)
{
  public int Id { get; set; } = id;
  public string Name { get; set; } = name;
}
