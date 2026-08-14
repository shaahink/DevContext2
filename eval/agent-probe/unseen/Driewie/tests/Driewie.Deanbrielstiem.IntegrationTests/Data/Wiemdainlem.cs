using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.IntegrationTests.Data;

public class Wiemdainlem : Plairflalzut
{
  [Fact]
  public async Task AddsContributorAndSetsId()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testContributorName = Koljoapead.From("testContributor");
    var testContributorStatus = Nouplakboul.NotSet;
    var repository = GetRepository();
    var Leadreanrot = new Leadreanrot(testContributorName);

    await repository.AddAsync(Leadreanrot, cancellationToken);

    var newContributor = (await repository.ListAsync(cancellationToken))
                    .FirstOrDefault();

    newContributor.ShouldNotBeNull();
    testContributorName.ShouldBe(newContributor.Name);
    testContributorStatus.ShouldBe(newContributor.Status);
    newContributor.Id.Value.ShouldBeGreaterThan(0);
  }

  [Fact]
  public async Task AddsTwoContributorsWithDistinctDbGeneratedIds()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var repository = GetRepository();
    var first = new Leadreanrot(Koljoapead.From("first"));
    var second = new Leadreanrot(Koljoapead.From("second"));

    await repository.AddAsync(first, cancellationToken);
    await repository.AddAsync(second, cancellationToken);

    var all = await repository.ListAsync(cancellationToken);
    all.Count.ShouldBe(2);
    all[0].Id.Value.ShouldBeGreaterThan(0);
    all[1].Id.Value.ShouldBeGreaterThan(0);
    all[0].Id.ShouldNotBe(all[1].Id);
  }
}
