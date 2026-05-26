using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class ProductService
{
    private readonly ByteBiteDbContext _db;

    public ProductService(ByteBiteDbContext db) { _db = db; }

    public async Task<Product> CreateAsync(UpsertProductInput input, CancellationToken ct = default)
    {
        if (!input.StoreId.HasValue) throw new BusinessException(400, "店铺ID不能为空");
        if (!input.CategoryId.HasValue) throw new BusinessException(400, "分类ID不能为空");
        if (string.IsNullOrWhiteSpace(input.Name)) throw new BusinessException(400, "商品名称不能为空");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == input.CategoryId && c.StoreId == input.StoreId && c.DeletedAt == null, ct);
        if (!categoryExists) throw new BusinessException(404, "分类不存在");

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            StoreId = input.StoreId.Value,
            CategoryId = input.CategoryId.Value,
            Name = input.Name,
            Description = input.Description,
            BasePrice = input.BasePrice ?? input.Price ?? 0m,
            ImageUrl = input.ImageUrl,
            Status = string.IsNullOrWhiteSpace(input.Status) ? "on" : input.Status,
            SortOrder = input.SortOrder ?? await _db.Products.CountAsync(p => p.CategoryId == input.CategoryId && p.DeletedAt == null, ct) + 1,
            MinOrderQty = Math.Max(1, input.MinOrderQty ?? 1),
            IsCombo = input.IsCombo.GetValueOrDefault(),
            CreatedAt = now,
            UpdatedAt = now
        };

        AddSpecGroups(product, input.SpecGroups, now);
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<Product> UpdateAsync(Guid id, UpsertProductInput input, CancellationToken ct = default)
    {
        var product = await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "商品不存在");

        if (input.CategoryId.HasValue)
        {
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == input.CategoryId && c.StoreId == product.StoreId && c.DeletedAt == null, ct);
            if (!categoryExists) throw new BusinessException(404, "分类不存在");
            product.CategoryId = input.CategoryId.Value;
        }

        if (!string.IsNullOrWhiteSpace(input.Name)) product.Name = input.Name;
        if (input.Description != null) product.Description = input.Description;
        if (input.BasePrice.HasValue || input.Price.HasValue) product.BasePrice = input.BasePrice ?? input.Price!.Value;
        if (input.ImageUrl != null) product.ImageUrl = input.ImageUrl;
        if (input.Status != null) product.Status = input.Status;
        if (input.SortOrder.HasValue) product.SortOrder = input.SortOrder.Value;
        if (input.MinOrderQty.HasValue) product.MinOrderQty = Math.Max(1, input.MinOrderQty.Value);
        if (input.IsCombo.HasValue) product.IsCombo = input.IsCombo.Value;
        product.UpdatedAt = DateTime.UtcNow;

        if (input.SpecGroups != null)
        {
            _db.SpecOptions.RemoveRange(product.SpecGroups.SelectMany(g => g.SpecOptions));
            _db.SpecGroups.RemoveRange(product.SpecGroups);
            product.SpecGroups.Clear();
            AddSpecGroups(product, input.SpecGroups, product.UpdatedAt);
        }

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(product.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Products.FindAsync([id], ct) ?? throw new BusinessException(404, "商品不存在");
        entity.DeletedAt = DateTime.UtcNow;
        entity.Status = "off";
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Product> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "商品不存在");

    public async Task<List<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .Where(p => p.CategoryId == categoryId && p.DeletedAt == null)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

    public async Task<List<Product>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .Where(p => p.StoreId == storeId && p.DeletedAt == null)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

    private static void AddSpecGroups(Product product, List<ProductSpecGroupInput>? specGroups, DateTime now)
    {
        if (specGroups == null) return;

        foreach (var groupInput in specGroups)
        {
            if (string.IsNullOrWhiteSpace(groupInput.Name)) continue;
            var group = new SpecGroup
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Name = groupInput.Name,
                IsRequired = groupInput.IsRequired,
                SortOrder = groupInput.SortOrder,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var optionInput in groupInput.GetOptions())
            {
                if (string.IsNullOrWhiteSpace(optionInput.Name)) continue;
                group.SpecOptions.Add(new SpecOption
                {
                    Id = Guid.NewGuid(),
                    SpecGroupId = group.Id,
                    Name = optionInput.Name,
                    ExtraPrice = optionInput.ExtraPrice,
                    Stock = optionInput.Stock,
                    SortOrder = optionInput.SortOrder,
                    IsDefault = optionInput.IsDefault,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            product.SpecGroups.Add(group);
        }
    }
}

public sealed class UpsertProductInput
{
    public Guid? StoreId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Status { get; set; }
    public int? SortOrder { get; set; }
    public int? MinOrderQty { get; set; }
    public bool? IsCombo { get; set; }
    public List<ProductSpecGroupInput>? SpecGroups { get; set; }
}

public sealed class ProductSpecGroupInput
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public List<ProductSpecOptionInput>? Options { get; set; }
    public List<ProductSpecOptionInput>? SpecOptions { get; set; }

    public IEnumerable<ProductSpecOptionInput> GetOptions() => Options ?? SpecOptions ?? [];
}

public sealed class ProductSpecOptionInput
{
    public string Name { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public int? Stock { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
}
