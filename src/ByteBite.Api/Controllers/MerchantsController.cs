using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantsController : ControllerBase
{
    private readonly MerchantService _merchantService;

    public MerchantsController(MerchantService merchantService) { _merchantService = merchantService; }

    [HttpPost("login")]
    public async Task<Merchant> Login([FromBody] LoginRequest request) => await _merchantService.LoginAsync(request.Phone, request.Password);

    [HttpPost("register")]
    public async Task<Merchant> Register([FromBody] RegisterMerchantRequest request) => await _merchantService.RegisterAsync(request.Phone, request.Password, request.Nickname, request.StoreName);

    [HttpGet("{id:guid}")]
    public async Task<Merchant> GetById(Guid id) => await _merchantService.GetByIdAsync(id);

    [HttpGet("{merchantId:guid}/stores")]
    public async Task<List<Store>> GetStores(Guid merchantId) => await _merchantService.GetStoresAsync(merchantId);

    [HttpPost("{id:guid}/logout")]
    public async Task<object> Logout(Guid id) { await _merchantService.LogoutAsync(id); return new { }; }
}

public class LoginRequest { public string Phone { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class RegisterMerchantRequest { public string Phone { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string? Nickname { get; set; } public string? StoreName { get; set; } }
