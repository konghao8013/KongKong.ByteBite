using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly StaffService _staffService;

    public StaffController(StaffService staffService) { _staffService = staffService; }

    [HttpGet("store/{storeId:guid}")]
    public async Task<List<Staff>> GetByStoreId(Guid storeId) => await _staffService.GetByStoreIdAsync(storeId);

    [HttpPost]
    public async Task<Staff> Create([FromBody] UpsertStaffInput request) => await _staffService.CreateAsync(request);

    [HttpPut("{id:guid}")]
    public async Task<Staff> Update(Guid id, [FromBody] UpsertStaffInput request) => await _staffService.UpdateAsync(id, request);

    [HttpPatch("{id:guid}/password")]
    public async Task<Staff> ResetPassword(Guid id, [FromBody] ResetStaffPasswordRequest request)
        => await _staffService.ResetPasswordAsync(id, request.Password);

    [HttpDelete("{id:guid}")]
    public async Task<object> Delete(Guid id) { await _staffService.DeleteAsync(id); return new { }; }
}

public sealed class ResetStaffPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}
