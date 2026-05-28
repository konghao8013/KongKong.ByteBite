using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class CustomerStoreService
{
    private readonly ByteBiteDbContext _db;

    public CustomerStoreService(ByteBiteDbContext db) { _db = db; }

    public async Task<object> GetStoreMenuAsync(Guid storeId, Guid? customerId = null, string? deviceId = null, CancellationToken ct = default)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == storeId && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店铺不存在");
        await TouchVisitAsync(store.Id, customerId, deviceId, ct);
        return await BuildStoreMenuAsync(store, ct);
    }

    public async Task<object> GetStoreMenuByCodeAsync(string storeCode, Guid? customerId = null, string? deviceId = null, CancellationToken ct = default)
    {
        var normalizedCode = storeCode.Trim().ToUpperInvariant();
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreCode == normalizedCode && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店铺不存在");
        await TouchVisitAsync(store.Id, customerId, deviceId, ct);
        return await BuildStoreMenuAsync(store, ct);
    }

    public async Task<List<object>> SearchStoresAsync(string keyword, int pageSize = 20, CancellationToken ct = default)
    {
        var normalized = (keyword ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized.Length == 0) return [];

        pageSize = Math.Clamp(pageSize, 1, 50);
        var now = DateTime.UtcNow;
        return await _db.Stores
            .Include(s => s.IndustryCategory)
            .Where(s => s.DeletedAt == null && (
                s.Name.ToUpper().Contains(normalized) ||
                s.StoreCode.ToUpper().Contains(normalized) ||
                (s.IndustryCategory != null && s.IndustryCategory.Name.ToUpper().Contains(normalized))))
            .OrderBy(s => s.BusinessStatus == "open" ? 0 : 1)
            .ThenBy(s => s.Name)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.StoreCode,
                s.Name,
                s.Description,
                s.CoverImageUrl,
                s.BusinessStatus,
                s.MonthlySales,
                IndustryName = s.IndustryCategory != null ? s.IndustryCategory.Name : null,
                ActiveDiscounts = s.DiscountRules
                    .Where(d => d.Status == "active" && d.DeletedAt == null && d.StartTime <= now && d.EndTime >= now)
                    .OrderBy(d => d.ThresholdAmount)
                    .Take(2)
                    .Select(d => new { d.Id, d.Name, d.DiscountType, d.ThresholdAmount, d.DiscountAmount, d.DiscountRate })
                    .ToList()
            })
            .Cast<object>()
            .ToListAsync(ct);
    }

    public async Task<List<object>> GetRecentStoresAsync(Guid? customerId, string? deviceId, int pageSize = 20, CancellationToken ct = default)
    {
        if (customerId == null && string.IsNullOrWhiteSpace(deviceId)) return [];
        pageSize = Math.Clamp(pageSize, 1, 50);
        var now = DateTime.UtcNow;

        var query = _db.CustomerStoreVisits
            .Include(v => v.Store)
            .ThenInclude(s => s.IndustryCategory)
            .Where(v => v.Store.DeletedAt == null);

        query = customerId != null
            ? query.Where(v => v.CustomerId == customerId)
            : query.Where(v => v.DeviceId == deviceId);

        return await query
            .OrderByDescending(v => v.LastOrderedAt ?? v.LastVisitedAt)
            .Take(pageSize)
            .Select(v => new
            {
                v.Store.Id,
                v.Store.StoreCode,
                v.Store.Name,
                v.Store.Description,
                v.Store.CoverImageUrl,
                v.Store.BusinessStatus,
                v.LastVisitedAt,
                v.LastOrderedAt,
                IndustryName = v.Store.IndustryCategory != null ? v.Store.IndustryCategory.Name : null,
                ActiveDiscounts = v.Store.DiscountRules
                    .Where(d => d.Status == "active" && d.DeletedAt == null && d.StartTime <= now && d.EndTime >= now)
                    .OrderBy(d => d.ThresholdAmount)
                    .Take(2)
                    .Select(d => new { d.Id, d.Name, d.DiscountType, d.ThresholdAmount, d.DiscountAmount, d.DiscountRate })
                    .ToList()
            })
            .Cast<object>()
            .ToListAsync(ct);
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
            ActiveDiscounts = _db.DiscountRules
                .Where(d => d.StoreId == store.Id && d.Status == "active" && d.DeletedAt == null && d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow)
                .OrderBy(d => d.ThresholdAmount)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.DiscountType,
                    d.ThresholdAmount,
                    d.DiscountAmount,
                    d.DiscountRate,
                    d.ApplyScope
                })
                .ToList(),
            Categories = categories
                .Select(category =>
                {
                    var items = productsByCategory.TryGetValue(category.Id, out var categoryProducts)
                        ? categoryProducts.Select(ToMenuItem).ToList()
                        : new List<object>();
                    return new
                    {
                        category.Id,
                        category.Name,
                        category.Icon,
                        category.CategoryType,
                        category.SortOrder,
                        Items = items
                    };
                })
                .Where(category => category.Items.Count > 0)
                .ToList()
        };
    }

    private async Task TouchVisitAsync(Guid storeId, Guid? customerId, string? deviceId, CancellationToken ct)
    {
        if (customerId == null && string.IsNullOrWhiteSpace(deviceId)) return;

        var now = DateTime.UtcNow;
        var visit = customerId != null
            ? await _db.CustomerStoreVisits.FirstOrDefaultAsync(v => v.CustomerId == customerId && v.StoreId == storeId, ct)
            : await _db.CustomerStoreVisits.FirstOrDefaultAsync(v => v.DeviceId == deviceId && v.StoreId == storeId, ct);

        if (visit == null)
        {
            visit = new CustomerStoreVisit
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                CustomerId = customerId,
                DeviceId = customerId == null ? deviceId : null,
                CreatedAt = now
            };
            _db.CustomerStoreVisits.Add(visit);
        }

        visit.LastVisitedAt = now;
        visit.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
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
