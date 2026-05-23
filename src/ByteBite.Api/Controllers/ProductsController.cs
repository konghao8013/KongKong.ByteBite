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
    public async Task<Product> Create([FromBody] Product product) => await _productService.CreateAsync(product);

    [HttpPut("{id:guid}")]
    public async Task<Product> Update(Guid id, [FromBody] UpdateProductRequest request) => await _productService.UpdateAsync(id, request.Name, request.Description, request.Price, request.ImageUrl, request.Status, request.SortOrder);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await _productService.DeleteAsync(id);

    [HttpGet("{id:guid}")]
    public async Task<Product> GetById(Guid id) => await _productService.GetByIdAsync(id);

    [HttpGet("category/{categoryId:guid}")]
    public async Task<List<Product>> GetByCategoryId(Guid categoryId) => await _productService.GetByCategoryIdAsync(categoryId);

    [HttpGet("store/{storeId:guid}")]
    public async Task<List<Product>> GetByStoreId(Guid storeId) => await _productService.GetByStoreIdAsync(storeId);
}

public class UpdateProductRequest { public string? Name { get; set; } public string? Description { get; set; } public decimal? Price { get; set; } public string? ImageUrl { get; set; } public string? Status { get; set; } public int? SortOrder { get; set; } }