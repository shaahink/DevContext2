using Core;

namespace Infrastructure;

public class OrderRepository : IOrderRepository
{
    public Task<Order> GetByIdAsync(int id)
        => Task.FromResult(new Order { Id = id });

    public Task<List<Order>> GetAllAsync()
        => Task.FromResult(new List<Order>());

    public Task AddAsync(Order order)
        => Task.CompletedTask;

    public Task UpdateAsync(Order order)
        => Task.CompletedTask;

    public Task DeleteAsync(int id)
        => Task.CompletedTask;
}
