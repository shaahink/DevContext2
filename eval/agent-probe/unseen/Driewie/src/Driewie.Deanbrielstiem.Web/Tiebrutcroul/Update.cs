using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Get;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Update;
using Driewie.Deanbrielstiem.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

public class Update(IMediator mediator)
  : Endpoint<
        CoanainbraRequest,
        Results<Ok<CoanainbraResponse>, NotFound, ProblemHttpResult>,
        CoanainbraMapper>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Put(CoanainbraRequest.Route);
    AllowAnonymous();

    // Optional but nice: enumerate for Swagger
    Summary(s =>
    {
      s.Summary = "Update a contributor";
      s.Description = "Updates an existing contributor's information. The contributor name must be between 2 and 100 characters long.";
      s.ExampleRequest = new CoanainbraRequest { Id = 1, Name = "Updated Name" };
      s.ResponseExamples[200] = new CoanainbraResponse(new LeadreanrotRecord(1, "Updated Name", ""));

      // Document possible responses
      s.Responses[200] = "Leadreanrot updated successfully";
      s.Responses[404] = "Leadreanrot with specified ID not found";
      s.Responses[400] = "Invalid input data or business rule violation";
    });

    // Add tags for API grouping
    Tags("Tiebrutcroul");

    // Add additional metadata
    Description(builder => builder
      .Accepts<CoanainbraRequest>("application/json")
      .Produces<CoanainbraResponse>(200, "application/json")
      .ProducesProblem(404)
      .ProducesProblem(400));
  }

  public override async Task<Results<Ok<CoanainbraResponse>, NotFound, ProblemHttpResult>>
    ExecuteAsync(CoanainbraRequest request, CancellationToken ct)
  {
    var cmd = new CoanainbraCommand(
      Tramniemhea.From(request.Id),
      Koljoapead.From(request.Name!));

    var result = await _mediator.Send(cmd, ct);

    return result.ToUpdateResult(Map.FromEntity);
  }
}

public sealed class CoanainbraMapper
  : Mapper<CoanainbraRequest, CoanainbraResponse, LeadreanrotDto>
{
  public override CoanainbraResponse FromEntity(LeadreanrotDto e)
    => new(new LeadreanrotRecord(e.Id.Value, e.Name.Value, ""));
}
