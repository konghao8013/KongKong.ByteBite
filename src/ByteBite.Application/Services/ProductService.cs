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

        AddSpecGroups(product, NormalizeSpecGroups(input.SpecGroups), now);
        _db.Products.Add(product);
        await UpsertComboItemsAsync(product, input.ComboItems, now, ct);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<Product> UpdateAsync(Guid id, UpsertProductInput input, CancellationToken ct = default)
    {
        var product = await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .Include(p => p.ComboItemComboProducts)
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
            AddSpecGroups(product, NormalizeSpecGroups(input.SpecGroups), product.UpdatedAt);
        }

        if (input.ComboItems != null || input.IsCombo.HasValue)
        {
            _db.ComboItems.RemoveRange(product.ComboItemComboProducts);
            product.ComboItemComboProducts.Clear();
            await UpsertComboItemsAsync(product, input.ComboItems, product.UpdatedAt, ct);
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
            .Include(p => p.ComboItemComboProducts)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "商品不存在");

    public async Task<List<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .Include(p => p.ComboItemComboProducts)
            .ThenInclude(i => i.Product)
            .Where(p => p.CategoryId == categoryId && p.DeletedAt == null)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

    public async Task<List<Product>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .Include(p => p.ComboItemComboProducts)
            .ThenInclude(i => i.Product)
            .Where(p => p.StoreId == storeId && p.DeletedAt == null)
            .OrderBy(p => p.Category.SortOrder)
            .ThenBy(p => p.SortOrder)
            .ToListAsync(ct);

    private async Task UpsertComboItemsAsync(Product product, List<ProductComboItemInput>? comboItems, DateTime now, CancellationToken ct)
    {
        if (!product.IsCombo)
        {
            if (comboItems?.Count > 0) throw new BusinessException(400, "普通商品不能配置套餐明细");
            return;
        }

        var normalizedItems = (comboItems ?? [])
            .Where(item => item.ProductId.HasValue)
            .Select((item, index) => new ProductComboItemInput
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity <= 0 ? 1 : item.Quantity,
                DefaultSpecOptionIds = item.DefaultSpecOptionIds,
                AllowChangeSpec = item.AllowChangeSpec,
                Remark = item.Remark,
                SortOrder = item.SortOrder <= 0 ? index + 1 : item.SortOrder
            })
            .ToList();

        if (normalizedItems.Count == 0) throw new BusinessException(400, "套餐商品至少需要配置一个子商品");
        if (normalizedItems.Any(item => item.ProductId == product.Id)) throw new BusinessException(400, "套餐不能包含自身");

        var childIds = normalizedItems.Select(i => i.ProductId!.Value).Distinct().ToList();
        var childProducts = await _db.Products
            .Where(p => childIds.Contains(p.Id) && p.StoreId == product.StoreId && p.DeletedAt == null)
            .ToDictionaryAsync(p => p.Id, ct);

        if (childProducts.Count != childIds.Count) throw new BusinessException(400, "套餐子商品必须来自当前店铺且不能已删除");
        if (childProducts.Values.Any(p => p.IsCombo)) throw new BusinessException(400, "套餐子商品不能再选择套餐商品");

        foreach (var item in normalizedItems)
        {
            product.ComboItemComboProducts.Add(new ComboItem
            {
                Id = Guid.NewGuid(),
                ComboProductId = product.Id,
                ProductId = item.ProductId!.Value,
                Quantity = item.Quantity <= 0 ? 1 : item.Quantity,
                DefaultSpecOptionIds = item.DefaultSpecOptionIds == null ? null : string.Join(",", item.DefaultSpecOptionIds.Distinct()),
                AllowChangeSpec = item.AllowChangeSpec,
                Remark = item.Remark,
                SortOrder = item.SortOrder,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    private static List<ProductSpecGroupInput>? NormalizeSpecGroups(List<ProductSpecGroupInput>? specGroups)
    {
        if (specGroups == null) return null;

        var result = new List<ProductSpecGroupInput>();
        foreach (var groupInput in specGroups.Where(g => !string.IsNullOrWhiteSpace(g.Name)))
        {
            var options = groupInput.GetOptions()
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .Select((option, index) => new ProductSpecOptionInput
                {
                    Name = option.Name.Trim(),
                    ExtraPrice = option.ExtraPrice,
                    Stock = option.Stock,
                    SortOrder = option.SortOrder <= 0 ? index + 1 : option.SortOrder,
                    IsDefault = option.IsDefault
                })
                .ToList();

            if (options.Count == 0) throw new BusinessException(400, $"规格组「{groupInput.Name}」至少需要一个规格项");
            if (options.Any(o => o.Stock.HasValue && o.Stock.Value < 0)) throw new BusinessException(400, "规格库存不能为负数");

            var defaultOptions = options.Where(o => o.IsDefault).ToList();
            if (defaultOptions.Count == 0)
            {
                options[0].IsDefault = true;
            }
            else if (defaultOptions.Count > 1)
            {
                var keep = defaultOptions[0];
                foreach (var option in options) option.IsDefault = ReferenceEquals(option, keep);
            }

            result.Add(new ProductSpecGroupInput
            {
                Name = groupInput.Name.Trim(),
                IsRequired = groupInput.IsRequired,
                SortOrder = groupInput.SortOrder <= 0 ? result.Count + 1 : groupInput.SortOrder,
                Options = options
            });
        }

        return result;
    }

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
    public List<ProductComboItemInput>? ComboItems { get; set; }
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

public sealed class ProductComboItemInput
{
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public List<Guid>? DefaultSpecOptionIds { get; set; }
    public bool AllowChangeSpec { get; set; } = true;
    public string? Remark { get; set; }
    public int SortOrder { get; set; }
}
