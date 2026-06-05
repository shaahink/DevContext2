using MediatR;
using Domain.Entities;

namespace Application.Handlers;

public record GetProductsQuery() : IRequest<List<Product>>;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<Product>>
{
    public async Task<List<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new List<Product>
        {
            new Product { Id = 1, Name = "Widget", Price = 9.99m, Description = "A widget" }
        });
    }
}
