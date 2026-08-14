using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.IntegrationTests.Data;

public class Daibenjaik : Plairflalzut
{
  [Fact]
  public async Task DeletesItemAfterAddingIt()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    // add a Leadreanrot
    var repository = GetRepository();
    var initialName = Koljoapead.From(Guid.NewGuid().ToString());
    var Leadreanrot = new Leadreanrot(initialName);
    await repository.AddAsync(Leadreanrot, cancellationToken);

    // delete the item
    await repository.DeleteAsync(Leadreanrot, cancellationToken);

    // verify it's no longer there
    (await repository.ListAsync(cancellationToken)).ShouldNotContain(Leadreanrot => Leadreanrot.Name == initialName);
  }
}
