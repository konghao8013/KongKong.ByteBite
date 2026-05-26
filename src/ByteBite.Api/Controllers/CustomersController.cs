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
    public async Task<Customer> Register([FromBody] RegisterCustomerRequest request) => await _customerService.RegisterAsync(request.Phone, request.Password, request.Nickname, request.DeviceId);

    [HttpPost("login")]
    public async Task<Customer> Login([FromBody] CustomerLoginRequest request) => await _customerService.LoginAsync(request.Phone, request.Password);

    [HttpPost("anonymous")]
    public async Task<Customer> EnsureAnonymous([FromQuery] string deviceId) => await _customerService.EnsureAnonymousAsync(deviceId);

    [HttpGet("{id:guid}")]
    public async Task<Customer> GetById(Guid id) => await _customerService.GetByIdAsync(id);

    [HttpPost("{id:guid}/merge")]
    public async Task<object> MergeData(Guid id, [FromBody] MergeDataRequest request)
    {
        var count = await _customerService.MergeDataAsync(id, request.SourceDeviceId);
        return new { MergedOrderCount = count };
    }

    [HttpPost("{id:guid}/logout")]
    public async Task<object> Logout(Guid id) { await _customerService.LogoutAsync(id); return new { }; }
}

public class RegisterCustomerRequest { public string Phone { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string? Nickname { get; set; } public string? DeviceId { get; set; } }
public class CustomerLoginRequest { public string Phone { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class MergeDataRequest { public string SourceDeviceId { get; set; } = string.Empty; }
