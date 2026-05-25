using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class CustomerStoreService
{
    private readonly ByteBiteDbContext _db;

    public CustomerStoreService(ByteBiteDbContext db) { _db = db; }

    public async Task<object> GetStoreMenuAsync(Guid storeId, CancellationToken ct = default)
    {
        var store = await _db.Stores.FindAsync([storeId], ct) ?? throw new BusinessException(404, "店铺不存在");
        var categories = await _db.Categories.Where(c => c.StoreId == storeId && c.IsVisible).OrderBy(c => c.SortOrder).ToListAsync(ct);
        var products = await _db.Products.Include(p => p.SpecGroups).ThenInclude(sg => sg.SpecOptions).Where(p => p.StoreId == storeId && p.Status == "on").OrderBy(p => p.SortOrder).ToListAsync(ct);
        return new { Store = store, Categories = categories, Products = products };
    }

    public async Task<object> GetStoreMenuByCodeAsync(string storeCode, CancellationToken ct = default)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreCode == storeCode.ToUpperInvariant(), ct)
            ?? throw new BusinessException(404, "店铺不存在");
        var storeId = store.Id;
        var categories = await _db.Categories.Where(c => c.StoreId == storeId && c.IsVisible).OrderBy(c => c.SortOrder).ToListAsync(ct);
        var products = await _db.Products.Include(p => p.SpecGroups).ThenInclude(sg => sg.SpecOptions).Where(p => p.StoreId == storeId && p.Status == "on").OrderBy(p => p.SortOrder).ToListAsync(ct);
        return new { Store = store, Categories = categories, Products = products };
    }
}