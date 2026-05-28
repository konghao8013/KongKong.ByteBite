using ByteBite.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerStoreController : ControllerBase
{
    private readonly CustomerStoreService _customerStoreService;

    public CustomerStoreController(CustomerStoreService customerStoreService) { _customerStoreService = customerStoreService; }

    [HttpGet("{storeId:guid}/menu")]
    public async Task<object> GetStoreMenu(Guid storeId, [FromQuery] Guid? customerId, [FromQuery] string? deviceId)
        => await _customerStoreService.GetStoreMenuAsync(storeId, customerId, deviceId);

    [HttpGet("code/{storeCode}/menu")]
    public async Task<object> GetStoreMenuByCode(string storeCode, [FromQuery] Guid? customerId, [FromQuery] string? deviceId)
        => await _customerStoreService.GetStoreMenuByCodeAsync(storeCode, customerId, deviceId);

    [HttpGet("search")]
    public async Task<List<object>> Search([FromQuery] string keyword, [FromQuery] int pageSize = 20)
        => await _customerStoreService.SearchStoresAsync(keyword, pageSize);

    [HttpGet("recent")]
    public async Task<List<object>> Recent([FromQuery] Guid? customerId, [FromQuery] string? deviceId, [FromQuery] int pageSize = 20)
        => await _customerStoreService.GetRecentStoresAsync(customerId, deviceId, pageSize);
}
