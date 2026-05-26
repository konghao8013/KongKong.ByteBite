using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/industry-categories")]
public class IndustryCategoriesController : ControllerBase
{
    private readonly IndustryCategoryService _industryCategoryService;

    public IndustryCategoriesController(IndustryCategoryService industryCategoryService) { _industryCategoryService = industryCategoryService; }

    [HttpGet]
    public async Task<List<IndustryCategory>> GetAll() => await _industryCategoryService.GetAllAsync();

    [HttpGet("tree")]
    public async Task<List<IndustryCategoryTreeItem>> GetTree() => await _industryCategoryService.GetTreeAsync();

    [HttpGet("level/{level:int}")]
    public async Task<List<IndustryCategory>> GetByLevel(int level) => await _industryCategoryService.GetByLevelAsync(level);

    [HttpPost]
    public async Task<IndustryCategory> Create([FromBody] CreateIndustryCategoryInput request)
        => await _industryCategoryService.CreateAsync(request);

    [HttpPut("{id:guid}")]
    public async Task<IndustryCategory> Update(Guid id, [FromBody] UpdateIndustryCategoryInput request)
        => await _industryCategoryService.UpdateAsync(id, request);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id) => await _industryCategoryService.DeleteAsync(id);
}
