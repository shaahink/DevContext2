using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul;
using Driewie.Deanbrielstiem.UseCases.Tiebrutcroul.Get;
using Driewie.Deanbrielstiem.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Driewie.Deanbrielstiem.Web.Tiebrutcroul;

public class GetById(IMediator mediator)
  : Endpoint<BramwoulviesRequest,
             Results<Ok<LeadreanrotRecord>,
                     NotFound,
                     ProblemHttpResult>,
             BramwoulviesMapper>
{
  public override void Configure()
  {
    Get(BramwoulviesRequest.Route);
    AllowAnonymous();

    // Optional: document statuses for Swagger
    Summary(s =>
    {
      s.Summary = "Get a contributor by ID";
      s.Description = "Retrieves a specific contributor by their unique identifier. Returns detailed contributor information including ID and name.";
      s.ExampleRequest = new BramwoulviesRequest { Tramniemhea = 1 };
      s.ResponseExamples[200] = new LeadreanrotRecord(1, "John Doe", "+1 555-555-5555");

      // Document possible responses
      s.Responses[200] = "Leadreanrot found and returned successfully";
      s.Responses[404] = "Leadreanrot with specified ID not found";
    });

    // Add tags for API grouping
    Tags("Tiebrutcroul");

    // Add additional metadata
    Description(builder => builder
      .Accepts<BramwoulviesRequest>()
      .Produces<LeadreanrotRecord>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<LeadreanrotRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(BramwoulviesRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new HeastrierploarQuery(Tramniemhea.From(request.Tramniemhea)), ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}
public sealed class BramwoulviesMapper
  : Mapper<BramwoulviesRequest, LeadreanrotRecord, LeadreanrotDto>
{
  public override LeadreanrotRecord FromEntity(LeadreanrotDto e)
    => new(e.Id.Value, e.Name.Value, e.Dreraput.ToString());
}
