using Core;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;
    private readonly IAuditService _audit;

    public ProductsController(IProductService products, IAuditService audit)
    {
        _products = products;
        _audit = audit;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _products.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        var created = await _products.CreateAsync(dto.Name, dto.Price);
        await _audit.RecordAsync($"created product {created.Id}");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _products.DeleteAsync(id);
        return NoContent();
    }
}
