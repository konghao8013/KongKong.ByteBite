using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class IndustryCategoryService
{
    private readonly ByteBiteDbContext _db;

    public IndustryCategoryService(ByteBiteDbContext db) { _db = db; }

    public async Task<List<IndustryCategory>> GetAllAsync(CancellationToken ct = default)
        => await _db.IndustryCategories
            .Where(i => i.IsVisible)
            .OrderBy(i => i.Level)
            .ThenBy(i => i.SortOrder)
            .ToListAsync(ct);

    public async Task<List<IndustryCategoryTreeItem>> GetTreeAsync(CancellationToken ct = default)
    {
        var categories = await GetAllAsync(ct);
        var lookup = categories.ToDictionary(c => c.Id, c => new IndustryCategoryTreeItem
        {
            Id = c.Id,
            ParentId = c.ParentId,
            Name = c.Name,
            Level = c.Level,
            SortOrder = c.SortOrder,
            Icon = c.Icon
        });

        var roots = new List<IndustryCategoryTreeItem>();
        foreach (var item in lookup.Values.OrderBy(i => i.SortOrder))
        {
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
            {
                parent.Children.Add(item);
            }
            else
            {
                roots.Add(item);
            }
        }

        SortTree(roots);
        return roots;
    }

    public async Task<List<IndustryCategory>> GetByLevelAsync(int level, CancellationToken ct = default)
        => await _db.IndustryCategories
            .Where(i => i.Level == level && i.IsVisible)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);

    public async Task<IndustryCategory> CreateAsync(CreateIndustryCategoryInput input, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var level = 1;
        if (input.ParentId.HasValue)
        {
            var parent = await _db.IndustryCategories.FirstOrDefaultAsync(i => i.Id == input.ParentId && i.IsVisible, ct)
                ?? throw new BusinessException(404, "父级行业分类不存在");
            if (parent.Level >= 3) throw new BusinessException(400, "行业分类最多支持三级");
            level = parent.Level + 1;
        }

        var category = new IndustryCategory
        {
            Id = Guid.NewGuid(),
            ParentId = input.ParentId,
            Name = input.Name,
            Level = level,
            SortOrder = input.SortOrder ?? await _db.IndustryCategories.CountAsync(i => i.ParentId == input.ParentId, ct) + 1,
            Icon = input.Icon,
            IsVisible = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.IndustryCategories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<IndustryCategory> UpdateAsync(Guid id, UpdateIndustryCategoryInput input, CancellationToken ct = default)
    {
        var category = await _db.IndustryCategories.FirstOrDefaultAsync(i => i.Id == id && i.IsVisible, ct)
            ?? throw new BusinessException(404, "行业分类不存在");

        if (!string.IsNullOrWhiteSpace(input.Name)) category.Name = input.Name;
        if (input.Icon != null) category.Icon = input.Icon;
        if (input.SortOrder.HasValue) category.SortOrder = input.SortOrder.Value;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _db.IndustryCategories.FirstOrDefaultAsync(i => i.Id == id && i.IsVisible, ct)
            ?? throw new BusinessException(404, "行业分类不存在");
        if (await _db.IndustryCategories.AnyAsync(i => i.ParentId == id && i.IsVisible, ct))
            throw new BusinessException(400, "请先删除下级行业分类");
        if (await _db.StoreTemplates.AnyAsync(t => t.IndustryCategoryId == id && t.DeletedAt == null, ct))
            throw new BusinessException(400, "该行业分类已被模板使用");

        category.IsVisible = false;
        category.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private static void SortTree(List<IndustryCategoryTreeItem> items)
    {
        items.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        foreach (var item in items) SortTree(item.Children);
    }
}

public sealed class IndustryCategoryTreeItem
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public string? Icon { get; set; }
    public List<IndustryCategoryTreeItem> Children { get; set; } = [];
}

public sealed class CreateIndustryCategoryInput
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? SortOrder { get; set; }
}

public sealed class UpdateIndustryCategoryInput
{
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public int? SortOrder { get; set; }
}
