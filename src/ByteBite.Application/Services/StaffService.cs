using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class StaffService
{
    private readonly ByteBiteDbContext _db;

    public StaffService(ByteBiteDbContext db) { _db = db; }

    public async Task<List<Staff>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Staff
            .Where(s => s.StoreId == storeId && s.DeletedAt == null)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<Staff> CreateAsync(UpsertStaffInput input, CancellationToken ct = default)
    {
        if (!input.StoreId.HasValue) throw new BusinessException(400, "店铺ID不能为空");
        await ValidateAsync(input, null, ct);

        var now = DateTime.UtcNow;
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            StoreId = input.StoreId.Value,
            Name = input.Name!.Trim(),
            Phone = input.Phone!.Trim(),
            PasswordHash = PasswordHasher.HashPassword(string.IsNullOrWhiteSpace(input.Password) ? "123456" : input.Password),
            Permission = string.IsNullOrWhiteSpace(input.Permission) ? "order_only" : input.Permission!,
            Status = string.IsNullOrWhiteSpace(input.Status) ? "active" : input.Status!,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Staff.Add(staff);
        await _db.SaveChangesAsync(ct);
        return staff;
    }

    public async Task<Staff> UpdateAsync(Guid id, UpsertStaffInput input, CancellationToken ct = default)
    {
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店员不存在");

        input.StoreId ??= staff.StoreId;
        input.Name ??= staff.Name;
        input.Phone ??= staff.Phone;
        input.Permission ??= staff.Permission;
        input.Status ??= staff.Status;
        await ValidateAsync(input, id, ct);

        staff.Name = input.Name!.Trim();
        staff.Phone = input.Phone!.Trim();
        staff.Permission = input.Permission!;
        staff.Status = input.Status!;
        if (!string.IsNullOrWhiteSpace(input.Password))
            staff.PasswordHash = PasswordHasher.HashPassword(input.Password);
        staff.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return staff;
    }

    public async Task<Staff> ResetPasswordAsync(Guid id, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6) throw new BusinessException(400, "密码至少需要6位");
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店员不存在");
        staff.PasswordHash = PasswordHasher.HashPassword(password);
        staff.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return staff;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店员不存在");
        staff.Status = "inactive";
        staff.DeletedAt = DateTime.UtcNow;
        staff.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(UpsertStaffInput input, Guid? currentId, CancellationToken ct)
    {
        if (!await _db.Stores.AnyAsync(s => s.Id == input.StoreId && s.DeletedAt == null, ct))
            throw new BusinessException(404, "店铺不存在");
        if (string.IsNullOrWhiteSpace(input.Name)) throw new BusinessException(400, "店员姓名不能为空");
        if (string.IsNullOrWhiteSpace(input.Phone)) throw new BusinessException(400, "店员手机号不能为空");
        if (!string.IsNullOrWhiteSpace(input.Password) && input.Password.Length < 6) throw new BusinessException(400, "密码至少需要6位");
        if (input.Permission is not ("full" or "order_only")) throw new BusinessException(400, "店员权限不正确");
        if (input.Status is not ("active" or "inactive")) throw new BusinessException(400, "店员状态不正确");

        var phoneExists = await _db.Staff.AnyAsync(s =>
            s.Phone == input.Phone.Trim() &&
            s.DeletedAt == null &&
            (!currentId.HasValue || s.Id != currentId.Value), ct);
        if (phoneExists) throw new BusinessException(400, "该手机号已绑定店员");
    }
}

public sealed class UpsertStaffInput
{
    public Guid? StoreId { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public string? Permission { get; set; }
    public string? Status { get; set; }
}
