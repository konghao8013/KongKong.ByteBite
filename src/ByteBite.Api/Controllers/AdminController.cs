using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService) { _adminService = adminService; }

    [HttpPost("login")]
    public async Task<Admin> Login([FromBody] AdminLoginRequest request) => await _adminService.LoginAsync(request.Username, request.Password);

    [HttpGet("merchants")]
    public async Task<List<Merchant>> GetMerchants([FromQuery] string? status, [FromQuery] string? keyword) => await _adminService.GetMerchantsAsync(status, keyword);

    [HttpPatch("merchants/{merchantId:guid}/status")]
    public async Task<Merchant> UpdateMerchantStatus(Guid merchantId, [FromBody] UpdateMerchantStatusRequest request)
        => await _adminService.UpdateMerchantStatusAsync(merchantId, request.Status, request.OperatorId, request.Reason);

    [HttpPatch("{adminId:guid}")]
    public async Task<Admin> UpdateAdmin(Guid adminId, [FromBody] UpdateAdminRequest request) => await _adminService.UpdateAdminAsync(adminId, request.DisplayName, request.Role);

    [HttpGet("audit-logs")]
    public async Task<List<MerchantAuditLog>> GetMerchantAuditLogs([FromQuery] Guid? merchantId) => await _adminService.GetMerchantAuditLogsAsync(merchantId);

    [HttpGet("operation-logs")]
    public async Task<List<AdminOperationLog>> GetAdminOperationLogs([FromQuery] Guid? adminId) => await _adminService.GetAdminOperationLogsAsync(adminId);

    [HttpGet("configs")]
    public async Task<List<SystemConfig>> GetSystemConfigs([FromQuery] bool publicOnly = false)
        => await _adminService.GetSystemConfigsAsync(publicOnly);

    [HttpGet("configs/public")]
    public async Task<List<SystemConfig>> GetPublicSystemConfigs()
        => await _adminService.GetSystemConfigsAsync(publicOnly: true);

    [HttpPut("configs")]
    public async Task<SystemConfig> UpsertSystemConfig([FromBody] UpsertSystemConfigInput request)
        => await _adminService.UpsertSystemConfigAsync(request);

    [HttpDelete("configs/{id:guid}")]
    public async Task<object> DeleteSystemConfig(Guid id, [FromQuery] Guid? operatorId)
    {
        await _adminService.DeleteSystemConfigAsync(id, operatorId);
        return new { };
    }

    [HttpPost("{id:guid}/logout")]
    public async Task<object> Logout(Guid id) { await _adminService.LogoutAsync(id); return new { }; }

    [HttpGet("platform-stats")]
    public async Task<object> GetPlatformStats() => await _adminService.GetPlatformStatsAsync();

    [HttpGet("stats")]
    public async Task<object> GetStats() => await _adminService.GetPlatformStatsAsync();
}

public class AdminLoginRequest { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class UpdateMerchantStatusRequest { public string Status { get; set; } = string.Empty; public Guid OperatorId { get; set; } public string Reason { get; set; } = string.Empty; }
public class UpdateAdminRequest { public string? DisplayName { get; set; } public string? Role { get; set; } }
