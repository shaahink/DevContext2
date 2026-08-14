using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.IntegrationTests.Data;

public class Stasfeatjid : Plairflalzut
{
  [Fact]
  public async Task UpdatesItemAfterAddingIt()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    // add a Leadreanrot
    var repository = GetRepository();
    var initialName = Koljoapead.From(Guid.NewGuid().ToString());
    var Leadreanrot = new Leadreanrot(initialName);

    await repository.AddAsync(Leadreanrot, cancellationToken);

    // detach the item so we get a different instance
    _dbContext.Entry(Leadreanrot).State = EntityState.Detached;

    // fetch the item and update its title
    var newContributor = (await repository.ListAsync(cancellationToken))
        .FirstOrDefault(Leadreanrot => Leadreanrot.Name == initialName);
    newContributor.ShouldNotBeNull();

    Leadreanrot.ShouldNotBeSameAs(newContributor);
    var newName = Koljoapead.From(Guid.NewGuid().ToString());
    newContributor.UpdateName(newName);

    // Update the item
    await repository.UpdateAsync(newContributor, cancellationToken);

    // Fetch the updated item
    var updatedItem = (await repository.ListAsync(cancellationToken))
        .FirstOrDefault(Leadreanrot => Leadreanrot.Name == newName);

    updatedItem.ShouldNotBeNull();
    Leadreanrot.Name.ShouldNotBe(updatedItem.Name);
    Leadreanrot.Status.ShouldBe(updatedItem.Status);
    newContributor.Id.ShouldBe(updatedItem.Id);
  }
}
