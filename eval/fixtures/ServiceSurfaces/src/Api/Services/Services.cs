namespace Api.Services;

// The domain services each entry surface delegates to. Distinct method names per surface so the
// resolved entry target / call edge is unambiguous in the T1.1 pin.

public interface IGreetingService
{
    string BuildGreeting(string name);
}

public sealed class GreetingService : IGreetingService
{
    public string BuildGreeting(string name) => $"Hello, {name}";
}

public interface IOrderIngestService
{
    Task IngestOrder(string payload);
}

public sealed class OrderIngestService : IOrderIngestService
{
    public Task IngestOrder(string payload) => Task.CompletedTask;
}

public interface ICatalogLookupService
{
    string FindProduct(int id);
}

public sealed class CatalogLookupService : ICatalogLookupService
{
    public string FindProduct(int id) => $"product-{id}";
}
