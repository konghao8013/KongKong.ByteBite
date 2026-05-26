using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class CategoryService
{
    private readonly ByteBiteDbContext _db;

    public CategoryService(ByteBiteDbContext db) { _db = db; }

    public async Task<Category> CreateAsync(Guid storeId, string name, int sortOrder, string? categoryType = null, string? icon = null, CancellationToken ct = default)
    {
        var storeExists = await _db.Stores.AnyAsync(s => s.Id == storeId && s.DeletedAt == null, ct);
        if (!storeExists) throw new BusinessException(404, "店铺不存在");

        var entity = new Category
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = name,
            Icon = icon,
            SortOrder = sortOrder,
            CategoryType = string.IsNullOrWhiteSpace(categoryType) ? "normal" : categoryType,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Category> UpdateAsync(Guid id, string name, int sortOrder, string? categoryType = null, string? icon = null, bool? isVisible = null, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "分类不存在");

        entity.Name = name;
        entity.SortOrder = sortOrder;
        if (categoryType != null) entity.CategoryType = categoryType;
        if (icon != null) entity.Icon = icon;
        if (isVisible.HasValue) entity.IsVisible = isVisible.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "分类不存在");
        if (await _db.Products.AnyAsync(p => p.CategoryId == id && p.DeletedAt == null, ct))
            throw new BusinessException(400, "该分类下还有商品，无法删除");

        entity.DeletedAt = DateTime.UtcNow;
        entity.IsVisible = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Category>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Categories
            .Where(c => c.StoreId == storeId && c.DeletedAt == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

    public async Task<Category> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct)
           ?? throw new BusinessException(404, "分类不存在");
}
