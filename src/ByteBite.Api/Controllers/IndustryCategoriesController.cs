using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IndustryCategoriesController : ControllerBase
{
    private readonly IndustryCategoryService _industryCategoryService;

    public IndustryCategoriesController(IndustryCategoryService industryCategoryService) { _industryCategoryService = industryCategoryService; }

    [HttpGet]
    public async Task<List<IndustryCategory>> GetAll() => await _industryCategoryService.GetAllAsync();

    [HttpGet("level/{level:int}")]
    public async Task<List<IndustryCategory>> GetByLevel(int level) => await _industryCategoryService.GetByLevelAsync(level);
}