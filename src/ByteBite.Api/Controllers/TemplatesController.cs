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

    [HttpPost("apply")]
    public async Task<StoreTemplate> ApplyToStore([FromBody] ApplyTemplateRequest request) => await _templateService.ApplyToStoreAsync(request.TemplateId, request.StoreId);
}

public class ApplyTemplateRequest { public Guid TemplateId { get; set; } public Guid StoreId { get; set; } }