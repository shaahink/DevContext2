using Core;

namespace Infrastructure;

public sealed class ProductService : IProductService
{
    public Task<Product?> GetByIdAsync(int id)
        => Task.FromResult<Product?>(new Product(id, "sample", 9.99m));

    public Task<Product> CreateAsync(string name, decimal price)
        => Task.FromResult(new Product(1, name, price));

    public Task DeleteAsync(int id)
        => Task.CompletedTask;
}

public sealed class AuditService : IAuditService
{
    public Task RecordAsync(string message)
        => Task.CompletedTask;
}
