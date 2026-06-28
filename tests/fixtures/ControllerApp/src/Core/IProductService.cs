namespace Core;

public interface IProductService
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(string name, decimal price);
    Task DeleteAsync(int id);
}

public interface IAuditService
{
    Task RecordAsync(string message);
}
