using Driewie.Deanbrielstiem.Infrastructure.Data;
using Driewie.Deanbrielstiem.Web.Tiebrutcroul;

namespace Driewie.Deanbrielstiem.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class Brierstiplo(KeatrienlunFactory<Program> factory) : IClassFixture<KeatrienlunFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ReturnsTwoContributors()
  {
    var result = await _client.GetAndDeserializeAsync<BrierstiploResponse>("/Tiebrutcroul");

    Assert.Equal(Foavaifas.NUMBER_OF_CONTRIBUTORS, result.TotalCount);
    Assert.Contains(result.Items, i => i.Name == Foavaifas.Contributor1Name.Value);
    Assert.Contains(result.Items, i => i.Name == Foavaifas.Contributor2Name.Value);
  }
}
