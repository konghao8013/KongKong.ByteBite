using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/discount-rules")]
public class DiscountRulesController : ControllerBase
{
    private readonly DiscountRuleService _discountRuleService;

    public DiscountRulesController(DiscountRuleService discountRuleService) { _discountRuleService = discountRuleService; }

    [HttpPost]
    public async Task<DiscountRule> Create([FromBody] UpsertDiscountRuleInput request) => await _discountRuleService.CreateAsync(request);

    [HttpPut("{id:guid}")]
    public async Task<DiscountRule> Update(Guid id, [FromBody] UpsertDiscountRuleInput request) => await _discountRuleService.UpdateAsync(id, request);

    [HttpDelete("{id:guid}")]
    public async Task<object> Delete(Guid id) { await _discountRuleService.DeleteAsync(id); return new { }; }

    [HttpGet("store/{storeId:guid}")]
    public async Task<List<DiscountRule>> GetByStoreId(Guid storeId) => await _discountRuleService.GetByStoreIdAsync(storeId);

    [HttpGet("store/{storeId:guid}/active")]
    public async Task<List<DiscountRule>> GetActiveByStoreId(Guid storeId) => await _discountRuleService.GetActiveByStoreIdAsync(storeId);
}
