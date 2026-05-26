using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService) { _categoryService = categoryService; }

    [HttpPost]
    public async Task<Category> Create([FromBody] CreateCategoryRequest request) => await _categoryService.CreateAsync(request.StoreId, request.Name, request.SortOrder, request.CategoryType, request.Icon);

    [HttpPut("{id:guid}")]
    public async Task<Category> Update(Guid id, [FromBody] UpdateCategoryRequest request) => await _categoryService.UpdateAsync(id, request.Name, request.SortOrder, request.CategoryType, request.Icon, request.IsVisible);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await _categoryService.DeleteAsync(id);

    [HttpGet("store/{storeId:guid}")]
    public async Task<List<Category>> GetByStoreId(Guid storeId) => await _categoryService.GetByStoreIdAsync(storeId);

    [HttpGet("{id:guid}")]
    public async Task<Category> GetById(Guid id) => await _categoryService.GetByIdAsync(id);
}

public class CreateCategoryRequest { public Guid StoreId { get; set; } public string Name { get; set; } = string.Empty; public int SortOrder { get; set; } public string? CategoryType { get; set; } public string? Icon { get; set; } }
public class UpdateCategoryRequest { public string Name { get; set; } = string.Empty; public int SortOrder { get; set; } public string? CategoryType { get; set; } public string? Icon { get; set; } public bool? IsVisible { get; set; } }
