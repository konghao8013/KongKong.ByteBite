using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly StoreService _storeService;

    public StoresController(StoreService storeService) { _storeService = storeService; }

    [HttpGet("{id:guid}")]
    public async Task<Store> GetById(Guid id) => await _storeService.GetByIdAsync(id);

    [HttpGet("merchant/{merchantId:guid}")]
    public async Task<List<Store>> GetByMerchantId(Guid merchantId) => await _storeService.GetByMerchantIdAsync(merchantId);

    [HttpPut("{id:guid}")]
    public async Task<Store> Update(Guid id, [FromBody] UpdateStoreRequest request) => await _storeService.UpdateAsync(id, request.Name, request.Description, request.CoverImageUrl, request.BusinessStatus);
}

public class UpdateStoreRequest { public string? Name { get; set; } public string? Description { get; set; } public string? CoverImageUrl { get; set; } public string? BusinessStatus { get; set; } }