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
        => await _db.Stores.Where(s => s.MerchantId == merchantId).ToListAsync(ct);

    public async Task<Store> UpdateAsync(Guid id, string? name, string? description, string? coverImageUrl, string? businessStatus, CancellationToken ct = default)
    {
        var store = await _db.Stores.FindAsync([id], ct) ?? throw new BusinessException(404, "店铺不存在");
        if (name != null) store.Name = name;
        if (description != null) store.Description = description;
        if (coverImageUrl != null) store.CoverImageUrl = coverImageUrl;
        if (businessStatus != null) store.BusinessStatus = businessStatus;
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
