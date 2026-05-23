using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountRulesController : ControllerBase
{
    private readonly DiscountRuleService _discountRuleService;

    public DiscountRulesController(DiscountRuleService discountRuleService) { _discountRuleService = discountRuleService; }

    [HttpPost]
    public async Task<DiscountRule> Create([FromBody] DiscountRule entity) => await _discountRuleService.CreateAsync(entity);

    [HttpPut("{id:guid}")]
    public async Task<DiscountRule> Update(Guid id, [FromBody] UpdateDiscountRuleRequest request) => await _discountRuleService.UpdateAsync(id, request.Name, request.Status);

    [HttpGet("store/{storeId:guid}")]
    public async Task<List<DiscountRule>> GetByStoreId(Guid storeId) => await _discountRuleService.GetByStoreIdAsync(storeId);

    [HttpGet("store/{storeId:guid}/active")]
    public async Task<List<DiscountRule>> GetActiveByStoreId(Guid storeId) => await _discountRuleService.GetActiveByStoreIdAsync(storeId);
}

public class UpdateDiscountRuleRequest { public string? Name { get; set; } public string? Status { get; set; } }