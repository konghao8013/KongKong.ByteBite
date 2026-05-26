using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService) { _productService = productService; }

    [HttpPost]
    public async Task<Product> Create([FromBody] UpsertProductInput request) => await _productService.CreateAsync(request);

    [HttpPut("{id:guid}")]
    public async Task<Product> Update(Guid id, [FromBody] UpsertProductInput request) => await _productService.UpdateAsync(id, request);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await _productService.DeleteAsync(id);

    [HttpGet("{id:guid}")]
    public async Task<Product> GetById(Guid id) => await _productService.GetByIdAsync(id);

    [HttpGet("category/{categoryId:guid}")]
    public async Task<List<Product>> GetByCategoryId(Guid categoryId) => await _productService.GetByCategoryIdAsync(categoryId);

    [HttpGet("store/{storeId:guid}")]
    public async Task<List<Product>> GetByStoreId(Guid storeId) => await _productService.GetByStoreIdAsync(storeId);
}
