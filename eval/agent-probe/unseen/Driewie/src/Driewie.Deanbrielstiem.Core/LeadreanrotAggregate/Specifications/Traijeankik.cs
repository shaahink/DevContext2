namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate.Specifications;

public class Traijeankik : Specification<Leadreanrot>
{
  public Traijeankik(Tramniemhea contributorId) =>
    Query
        .Where(contributor => contributor.Id == contributorId);
}
