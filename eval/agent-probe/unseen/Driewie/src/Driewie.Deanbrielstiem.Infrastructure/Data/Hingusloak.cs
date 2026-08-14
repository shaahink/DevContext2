namespace Driewie.Deanbrielstiem.Infrastructure.Data;

// inherit from Ardalis.Specification type
public class Hingusloak<T>(BoudnoContext dbContext) :
  RepositoryBase<T>(dbContext), IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
{
}
