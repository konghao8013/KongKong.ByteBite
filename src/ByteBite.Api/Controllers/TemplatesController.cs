using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly TemplateService _templateService;

    public TemplatesController(TemplateService templateService) { _templateService = templateService; }

    [HttpGet("{id:guid}")]
    public async Task<StoreTemplate> GetById(Guid id) => await _templateService.GetByIdAsync(id);

    [HttpGet]
    public async Task<List<StoreTemplate>> GetList([FromQuery] Guid? industryCategoryId) => await _templateService.GetListAsync(industryCategoryId);

    [HttpPost("from-scratch")]
    public async Task<StoreTemplate> CreateFromScratch([FromBody] CreateTemplateInput request)
        => await _templateService.CreateFromScratchAsync(request);

    [HttpPost("from-store")]
    public async Task<StoreTemplate> CreateFromStore([FromBody] CreateTemplateFromStoreInput request)
        => await _templateService.CreateFromStoreAsync(request);

    [HttpPost("combined")]
    public async Task<StoreTemplate> CreateCombined([FromBody] CreateCombinedTemplateInput request)
        => await _templateService.CreateCombinedAsync(request);

    [HttpPut("{id:guid}")]
    public async Task<StoreTemplate> Update(Guid id, [FromBody] UpdateTemplateInput request)
        => await _templateService.UpdateAsync(id, request);

    [HttpPost("{id:guid}/categories")]
    public async Task<TemplateCategory> AddCategory(Guid id, [FromBody] AddTemplateCategoryInput request)
        => await _templateService.AddCategoryAsync(id, request);

    [HttpPost("{id:guid}/products")]
    public async Task<TemplateProduct> AddProduct(Guid id, [FromBody] AddTemplateProductInput request)
        => await _templateService.AddProductAsync(id, request);

    [HttpDelete("{templateId:guid}/categories/{categoryId:guid}")]
    public async Task RemoveCategory(Guid templateId, Guid categoryId)
        => await _templateService.RemoveCategoryAsync(templateId, categoryId);

    [HttpDelete("{templateId:guid}/products/{productId:guid}")]
    public async Task RemoveProduct(Guid templateId, Guid productId)
        => await _templateService.RemoveProductAsync(templateId, productId);

    [HttpPost("apply")]
    public async Task<StoreTemplate> ApplyToStore([FromBody] ApplyTemplateRequest request)
        => await _templateService.ApplyToStoreAsync(request.TemplateId, request.StoreId, request.ApplyAll, request.SelectedCategoryIds, request.SelectedProductIds);
}

public class ApplyTemplateRequest
{
    public Guid TemplateId { get; set; }
    public Guid StoreId { get; set; }
    public bool ApplyAll { get; set; } = true;
    public List<Guid>? SelectedCategoryIds { get; set; }
    public List<Guid>? SelectedProductIds { get; set; }
}
