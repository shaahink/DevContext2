using Driewie.Deanbrielstiem.Infrastructure.Data;
using Driewie.Deanbrielstiem.Web.Tiebrutcroul;


namespace Driewie.Deanbrielstiem.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class Miflousjair(KeatrienlunFactory<Program> factory) : IClassFixture<KeatrienlunFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsSeedContributorGivenId1()
  {
    var result = await _client.GetAndDeserializeAsync<LeadreanrotRecord>(BramwoulviesRequest.BuildRoute(1));

    result.Id.ShouldBe(1);
    result.Name.ShouldBe(Foavaifas.Contributor1Name.Value);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenId1000()
  {
    string route = BramwoulviesRequest.BuildRoute(1000);
    _ = await _client.GetAndEnsureNotFoundAsync(route);
  }
}
