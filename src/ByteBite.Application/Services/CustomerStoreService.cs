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
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == storeId && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店铺不存在");
        return await BuildStoreMenuAsync(store, ct);
    }

    public async Task<object> GetStoreMenuByCodeAsync(string storeCode, CancellationToken ct = default)
    {
        var normalizedCode = storeCode.Trim().ToUpperInvariant();
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreCode == normalizedCode && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店铺不存在");
        return await BuildStoreMenuAsync(store, ct);
    }

    private async Task<object> BuildStoreMenuAsync(Store store, CancellationToken ct)
    {
        var categories = await _db.Categories
            .Where(c => c.StoreId == store.Id && c.IsVisible && c.DeletedAt == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var products = await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .Where(p => p.StoreId == store.Id && p.DeletedAt == null && p.Status != "off")
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        var productsByCategory = products
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new
        {
            StoreId = store.Id,
            StoreCode = store.StoreCode,
            StoreName = store.Name,
            store.Description,
            store.CoverImageUrl,
            store.BusinessStatus,
            CanOrder = store.BusinessStatus == "open",
            store.DiningMode,
            PackingFee = store.PackingFee ?? 0m,
            Categories = categories.Select(category => new
            {
                category.Id,
                category.Name,
                category.Icon,
                category.CategoryType,
                category.SortOrder,
                Items = productsByCategory.TryGetValue(category.Id, out var categoryProducts)
                    ? categoryProducts.Select(ToMenuItem).ToList()
                    : new List<object>()
            }).ToList()
        };
    }

    private static object ToMenuItem(Product product)
    {
        var specs = product.SpecGroups
            .OrderBy(group => group.SortOrder)
            .Select(group => new
            {
                group.Id,
                group.Name,
                group.IsRequired,
                Options = group.SpecOptions
                    .OrderBy(option => option.SortOrder)
                    .Select(option => new
                    {
                        option.Id,
                        option.Name,
                        option.ExtraPrice,
                        option.IsDefault
                    })
                    .ToList()
            })
            .ToList();

        return new
        {
            product.Id,
            product.Name,
            product.BasePrice,
            FromPrice = product.BasePrice,
            product.ImageUrl,
            product.Description,
            product.MonthlySales,
            product.Status,
            product.MinOrderQty,
            product.IsCombo,
            Specs = specs
        };
    }
}
