using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;

    public CustomersController(CustomerService customerService) { _customerService = customerService; }

    [HttpPost("register")]
    public async Task<Customer> Register([FromBody] RegisterCustomerRequest request)
        => await _customerService.RegisterAsync(request.Phone, request.Username, request.Password, request.Nickname, request.DeviceId);

    [HttpPost("login")]
    public async Task<Customer> Login([FromBody] CustomerLoginRequest request) => await _customerService.LoginAsync(request.Account, request.Password, request.DeviceId);

    [HttpPost("anonymous")]
    public async Task<Customer> EnsureAnonymous([FromQuery] string deviceId) => await _customerService.EnsureAnonymousAsync(deviceId);

    [HttpGet("{id:guid}")]
    public async Task<Customer> GetById(Guid id) => await _customerService.GetByIdAsync(id);

    [HttpPost("{id:guid}/merge")]
    public async Task<DataMergeResultDto> MergeData(Guid id, [FromBody] MergeDataRequest? request, [FromQuery] string? deviceId)
    {
        var sourceDeviceId = request?.SourceDeviceId ?? deviceId ?? string.Empty;
        return await _customerService.MergeDataAsync(id, sourceDeviceId);
    }

    [HttpGet("merge-preview")]
    public async Task<DataMergeResultDto> GetMergePreview([FromQuery] string deviceId)
        => await _customerService.GetMergePreviewAsync(deviceId);

    [HttpGet("cart")]
    public async Task<List<CustomerCartStoreDto>> GetCart([FromQuery] Guid? customerId, [FromQuery] string? deviceId, [FromQuery] Guid? storeId)
        => await _customerService.GetCartAsync(customerId, deviceId, storeId);

    [HttpPut("cart")]
    public async Task<CustomerCartStoreDto> SaveCart([FromBody] SaveCustomerCartInput request)
        => await _customerService.SaveCartAsync(request);

    [HttpPost("cart/merge")]
    public async Task<List<CustomerCartStoreDto>> MergeCart([FromBody] MergeCustomerCartInput request)
        => await _customerService.MergeLocalCartAsync(request);

    [HttpPost("{id:guid}/logout")]
    public async Task<object> Logout(Guid id) { await _customerService.LogoutAsync(id); return new { }; }
}

public class RegisterCustomerRequest { public string? Phone { get; set; } public string? Username { get; set; } public string Password { get; set; } = string.Empty; public string? Nickname { get; set; } public string? DeviceId { get; set; } }
public class CustomerLoginRequest { public string Account { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string? DeviceId { get; set; } }
public class MergeDataRequest { public string SourceDeviceId { get; set; } = string.Empty; }
