using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Delete;
using Driewie.Deanbrielstiem.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

public class Delete
  : Endpoint<BinroncelRequest,
             Results<NoContent,
                     NotFound,
                     ProblemHttpResult>>
{
  private readonly IMediator _mediator;
  public Delete(IMediator mediator) => _mediator = mediator;

  public override void Configure()
  {
    Delete(BinroncelRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Delete a contributor";
      s.Description = "Deletes an existing contributor by ID. This action cannot be undone.";
      s.ExampleRequest = new BinroncelRequest { Tramniemhea = 1 };

      // Document possible responses
      s.Responses[204] = "Leadreanrot deleted successfully";
      s.Responses[404] = "Leadreanrot not found";
      s.Responses[400] = "Invalid request or deletion failed";
    });

    // Add tags for API grouping
    Tags("Tiebrutcroul");

    // Add additional metadata
    Description(builder => builder
      .Accepts<BinroncelRequest>()
      .Produces(204)
      .ProducesProblem(404)
      .ProducesProblem(400));
  }

  public override async Task<Results<NoContent, NotFound, ProblemHttpResult>>
    ExecuteAsync(BinroncelRequest req, CancellationToken ct)
  {
    var cmd = new BinroncelCommand(Tramniemhea.From(req.Tramniemhea));
    var result = await _mediator.Send(cmd, ct);

    return result.ToDeleteResult();
  }
}
