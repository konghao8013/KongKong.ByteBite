using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class StoreService
{
    private readonly ByteBiteDbContext _db;

    public StoreService(ByteBiteDbContext db) { _db = db; }

    public async Task<Store> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Stores.FindAsync([id], ct) ?? throw new BusinessException(404, "店铺不存在");

    public async Task<Store> GetByStoreCodeAsync(string storeCode, CancellationToken ct = default)
        => await _db.Stores.FirstOrDefaultAsync(s => s.StoreCode == storeCode.ToUpperInvariant(), ct)
            ?? throw new BusinessException(404, "店铺不存在");

    public async Task<List<Store>> GetByMerchantIdAsync(Guid merchantId, CancellationToken ct = default)
        => await _db.Stores.Where(s => s.MerchantId == merchantId && s.DeletedAt == null).ToListAsync(ct);

    public async Task<Store> UpdateAsync(Guid id, UpdateStoreInput input, CancellationToken ct = default)
    {
        var store = await _db.Stores.FindAsync([id], ct) ?? throw new BusinessException(404, "店铺不存在");
        if (input.Name != null) store.Name = input.Name;
        if (input.Description != null) store.Description = input.Description;
        if (input.CoverImageUrl != null) store.CoverImageUrl = input.CoverImageUrl;
        if (input.BusinessStatus != null) store.BusinessStatus = input.BusinessStatus;
        if (input.BusinessHoursStart != null) store.BusinessHoursStart = input.BusinessHoursStart;
        if (input.BusinessHoursEnd != null) store.BusinessHoursEnd = input.BusinessHoursEnd;
        if (input.IndustryCategoryId.HasValue) store.IndustryCategoryId = input.IndustryCategoryId;
        if (input.DiningMode != null) store.DiningMode = input.DiningMode;
        if (input.DeliveryMinAmount.HasValue) store.DeliveryMinAmount = input.DeliveryMinAmount;
        if (input.PackingFee.HasValue) store.PackingFee = input.PackingFee;
        store.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return store;
    }

    public async Task<string> GenerateStoreCodeAsync(CancellationToken ct = default)
    {
        var count = await _db.Stores.CountAsync(ct);
        return Base36Encoder.Encode(count + 1);
    }
}

public sealed class UpdateStoreInput
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? BusinessStatus { get; set; }
    public TimeOnly? BusinessHoursStart { get; set; }
    public TimeOnly? BusinessHoursEnd { get; set; }
    public Guid? IndustryCategoryId { get; set; }
    public string? DiningMode { get; set; }
    public decimal? DeliveryMinAmount { get; set; }
    public decimal? PackingFee { get; set; }
}
