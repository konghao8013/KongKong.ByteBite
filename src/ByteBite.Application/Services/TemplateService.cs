using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class TemplateService
{
    private readonly ByteBiteDbContext _db;

    public TemplateService(ByteBiteDbContext db) { _db = db; }

    public async Task<StoreTemplate> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await TemplateDetails()
            .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "模板不存在");

    public async Task<List<StoreTemplate>> GetListAsync(Guid? industryCategoryId, CancellationToken ct = default)
    {
        var query = TemplateDetails().Where(t => t.DeletedAt == null);
        if (industryCategoryId != null) query = query.Where(t => t.IndustryCategoryId == industryCategoryId);

        return await query
            .OrderByDescending(t => t.UseCount)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<StoreTemplate> CreateFromScratchAsync(CreateTemplateInput input, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var template = new StoreTemplate
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            IndustryCategoryId = input.IndustryCategoryId,
            CoverImageUrl = input.CoverImageUrl,
            Description = input.Description,
            SourceType = "manual",
            Status = "active",
            UseCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.StoreTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(template.Id, ct);
    }

    public async Task<StoreTemplate> CreateFromStoreAsync(CreateTemplateFromStoreInput input, CancellationToken ct = default)
    {
        var store = await LoadStoreMenuAsync(input.StoreId, ct);
        var now = DateTime.UtcNow;
        var template = new StoreTemplate
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            IndustryCategoryId = input.IndustryCategoryId,
            Description = input.Description,
            SourceType = "from_store",
            SourceStoreIds = input.StoreId.ToString(),
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.StoreTemplates.Add(template);
        CopyStoreSelectionToTemplate(template, store, input.IncludeAll, input.SelectedCategoryIds, input.SelectedProductIds, now);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(template.Id, ct);
    }

    public async Task<StoreTemplate> CreateCombinedAsync(CreateCombinedTemplateInput input, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var template = new StoreTemplate
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            IndustryCategoryId = input.IndustryCategoryId,
            Description = input.Description,
            SourceType = "combined",
            SourceStoreIds = string.Join(",", input.StoreSelections.Select(s => s.StoreId).Distinct()),
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.StoreTemplates.Add(template);
        foreach (var selection in input.StoreSelections)
        {
            var store = await LoadStoreMenuAsync(selection.StoreId, ct);
            CopyStoreSelectionToTemplate(template, store, selection.IncludeAll, selection.SelectedCategoryIds, selection.SelectedProductIds, now);
        }

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(template.Id, ct);
    }

    public async Task<StoreTemplate> UpdateAsync(Guid id, UpdateTemplateInput input, CancellationToken ct = default)
    {
        var template = await _db.StoreTemplates.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "模板不存在");

        if (!string.IsNullOrWhiteSpace(input.Name)) template.Name = input.Name;
        if (input.IndustryCategoryId.HasValue) template.IndustryCategoryId = input.IndustryCategoryId;
        if (input.CoverImageUrl != null) template.CoverImageUrl = input.CoverImageUrl;
        if (input.Description != null) template.Description = input.Description;
        if (input.Status != null) template.Status = input.Status;
        template.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<TemplateCategory> AddCategoryAsync(Guid templateId, AddTemplateCategoryInput input, CancellationToken ct = default)
    {
        var template = await _db.StoreTemplates.FirstOrDefaultAsync(t => t.Id == templateId && t.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "模板不存在");
        var now = DateTime.UtcNow;
        var category = new TemplateCategory
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Name = input.Name,
            CategoryType = string.IsNullOrWhiteSpace(input.CategoryType) ? "normal" : input.CategoryType,
            Icon = input.Icon,
            SortOrder = input.SortOrder ?? await _db.TemplateCategories.CountAsync(c => c.TemplateId == templateId, ct) + 1,
            HotTopCount = input.HotTopCount,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.TemplateCategories.Add(category);
        template.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<TemplateProduct> AddProductAsync(Guid templateId, AddTemplateProductInput input, CancellationToken ct = default)
    {
        var category = await _db.TemplateCategories.FirstOrDefaultAsync(c => c.Id == input.CategoryId && c.TemplateId == templateId, ct)
            ?? throw new BusinessException(404, "模板分类不存在");
        var now = DateTime.UtcNow;
        var product = new TemplateProduct
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            TemplateCategoryId = category.Id,
            Name = input.Name,
            Description = input.Description,
            ReferencePrice = input.ReferencePrice,
            ImageUrl = input.ImageUrl,
            SortOrder = input.SortOrder ?? await _db.TemplateProducts.CountAsync(p => p.TemplateCategoryId == category.Id, ct) + 1,
            MinOrderQty = Math.Max(1, input.MinOrderQty ?? 1),
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var groupInput in input.SpecGroups)
        {
            var group = new TemplateSpecGroup
            {
                Id = Guid.NewGuid(),
                TemplateProductId = product.Id,
                Name = groupInput.Name,
                IsRequired = groupInput.IsRequired,
                SortOrder = groupInput.SortOrder,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var optionInput in groupInput.Options)
            {
                group.TemplateSpecOptions.Add(new TemplateSpecOption
                {
                    Id = Guid.NewGuid(),
                    TemplateSpecGroupId = group.Id,
                    Name = optionInput.Name,
                    ExtraPrice = optionInput.ExtraPrice,
                    SortOrder = optionInput.SortOrder,
                    IsDefault = optionInput.IsDefault,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            product.TemplateSpecGroups.Add(group);
        }

        _db.TemplateProducts.Add(product);
        await _db.SaveChangesAsync(ct);
        return await _db.TemplateProducts
            .Include(p => p.TemplateSpecGroups)
            .ThenInclude(g => g.TemplateSpecOptions)
            .FirstAsync(p => p.Id == product.Id, ct);
    }

    public async Task RemoveCategoryAsync(Guid templateId, Guid categoryId, CancellationToken ct = default)
    {
        var category = await _db.TemplateCategories
            .Include(c => c.TemplateProducts)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.TemplateId == templateId, ct)
            ?? throw new BusinessException(404, "模板分类不存在");

        foreach (var product in category.TemplateProducts.ToList())
        {
            await RemoveTemplateProductGraphAsync(product.Id, ct);
        }

        _db.TemplateCategories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveProductAsync(Guid templateId, Guid productId, CancellationToken ct = default)
    {
        var exists = await _db.TemplateProducts.AnyAsync(p => p.Id == productId && p.TemplateId == templateId, ct);
        if (!exists) throw new BusinessException(404, "模板商品不存在");

        await RemoveTemplateProductGraphAsync(productId, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<StoreTemplate> ApplyToStoreAsync(Guid templateId, Guid storeId, bool applyAll = true, List<Guid>? selectedCategoryIds = null, List<Guid>? selectedProductIds = null, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(templateId, ct);
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == storeId && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店铺不存在");

        var selectedCategories = SelectTemplateCategories(template, applyAll, selectedCategoryIds, selectedProductIds);
        var selectedProducts = SelectTemplateProducts(template, selectedCategories, applyAll, selectedProductIds);
        if (selectedCategories.Count == 0 || selectedProducts.Count == 0)
            throw new BusinessException(400, "模板中没有可应用的分类或商品");

        var existingCategories = await _db.Categories
            .Where(c => c.StoreId == store.Id && c.DeletedAt == null)
            .ToListAsync(ct);
        var existingCategoryByName = existingCategories
            .GroupBy(c => NormalizeCategoryName(c.Name), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.SortOrder).First(), StringComparer.OrdinalIgnoreCase);
        var productSortOrderByCategory = await _db.Products
            .Where(p => p.StoreId == store.Id && p.DeletedAt == null)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, MaxSortOrder = g.Max(p => p.SortOrder) })
            .ToDictionaryAsync(g => g.CategoryId, g => g.MaxSortOrder, ct);
        var nextCategorySortOrder = existingCategories.Count == 0 ? 1 : existingCategories.Max(c => c.SortOrder) + 1;
        var now = DateTime.UtcNow;
        var categoryMap = new Dictionary<Guid, Guid>();
        foreach (var templateCategory in selectedCategories.OrderBy(c => c.SortOrder))
        {
            var normalizedCategoryName = NormalizeCategoryName(templateCategory.Name);
            if (existingCategoryByName.TryGetValue(normalizedCategoryName, out var existingCategory))
            {
                categoryMap[templateCategory.Id] = existingCategory.Id;
                productSortOrderByCategory.TryAdd(existingCategory.Id, 0);
                continue;
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Name = templateCategory.Name,
                Icon = templateCategory.Icon,
                CategoryType = templateCategory.CategoryType,
                SortOrder = nextCategorySortOrder++,
                IsVisible = true,
                HotTopCount = templateCategory.HotTopCount,
                CreatedAt = now,
                UpdatedAt = now
            };
            categoryMap[templateCategory.Id] = category.Id;
            existingCategoryByName[normalizedCategoryName] = category;
            productSortOrderByCategory[category.Id] = 0;
            _db.Categories.Add(category);
        }

        var productMap = new Dictionary<Guid, Guid>();
        foreach (var templateProduct in selectedProducts.OrderBy(p => p.SortOrder))
        {
            if (!categoryMap.TryGetValue(templateProduct.TemplateCategoryId, out var categoryId)) continue;

            var product = new Product
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                CategoryId = categoryId,
                Name = templateProduct.Name,
                Description = templateProduct.Description,
                BasePrice = templateProduct.ReferencePrice,
                ImageUrl = templateProduct.ImageUrl,
                Status = "off",
                SortOrder = productSortOrderByCategory.TryGetValue(categoryId, out var currentSortOrder) ? currentSortOrder + 1 : 1,
                MinOrderQty = Math.Max(1, templateProduct.MinOrderQty),
                IsCombo = templateProduct.TemplateComboItemComboTemplateProducts.Count > 0,
                CreatedAt = now,
                UpdatedAt = now
            };
            productSortOrderByCategory[categoryId] = product.SortOrder;
            productMap[templateProduct.Id] = product.Id;

            foreach (var templateGroup in templateProduct.TemplateSpecGroups.OrderBy(g => g.SortOrder))
            {
                var group = new SpecGroup
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = templateGroup.Name,
                    IsRequired = templateGroup.IsRequired,
                    SortOrder = templateGroup.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                foreach (var templateOption in templateGroup.TemplateSpecOptions.OrderBy(o => o.SortOrder))
                {
                    group.SpecOptions.Add(new SpecOption
                    {
                        Id = Guid.NewGuid(),
                        SpecGroupId = group.Id,
                        Name = templateOption.Name,
                        ExtraPrice = templateOption.ExtraPrice,
                        SortOrder = templateOption.SortOrder,
                        IsDefault = templateOption.IsDefault,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                product.SpecGroups.Add(group);
            }

            _db.Products.Add(product);
        }

        foreach (var templateProduct in selectedProducts)
        {
            if (!productMap.TryGetValue(templateProduct.Id, out var comboProductId)) continue;

            foreach (var comboItem in templateProduct.TemplateComboItemComboTemplateProducts.OrderBy(i => i.SortOrder))
            {
                if (!productMap.TryGetValue(comboItem.TemplateProductId, out var childProductId)) continue;
                _db.ComboItems.Add(new ComboItem
                {
                    Id = Guid.NewGuid(),
                    ComboProductId = comboProductId,
                    ProductId = childProductId,
                    Quantity = comboItem.Quantity,
                    Remark = comboItem.Remark,
                    SortOrder = comboItem.SortOrder,
                    AllowChangeSpec = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }

        template.UseCount++;
        template.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(template.Id, ct);
    }

    private IQueryable<StoreTemplate> TemplateDetails()
        => _db.StoreTemplates
            .Include(t => t.TemplateCategories)
            .ThenInclude(c => c.TemplateProducts)
            .ThenInclude(p => p.TemplateSpecGroups)
            .ThenInclude(g => g.TemplateSpecOptions)
            .Include(t => t.TemplateProducts)
            .ThenInclude(p => p.TemplateSpecGroups)
            .ThenInclude(g => g.TemplateSpecOptions)
            .Include(t => t.TemplateProducts)
            .ThenInclude(p => p.TemplateComboItemComboTemplateProducts)
            .Include(t => t.IndustryCategory);

    private static string NormalizeCategoryName(string name) => name.Trim();

    private async Task<Store> LoadStoreMenuAsync(Guid storeId, CancellationToken ct)
        => await _db.Stores
            .Include(s => s.Categories)
            .Include(s => s.Products)
            .ThenInclude(p => p.SpecGroups)
            .ThenInclude(g => g.SpecOptions)
            .Include(s => s.Products)
            .ThenInclude(p => p.ComboItemComboProducts)
            .FirstOrDefaultAsync(s => s.Id == storeId && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "来源店铺不存在");

    private static void CopyStoreSelectionToTemplate(StoreTemplate template, Store store, bool includeAll, List<Guid>? selectedCategoryIds, List<Guid>? selectedProductIds, DateTime now)
    {
        var categoryFilter = selectedCategoryIds?.ToHashSet() ?? [];
        var productFilter = selectedProductIds?.ToHashSet() ?? [];
        var sourceProducts = store.Products
            .Where(p => p.DeletedAt == null)
            .Where(p => includeAll || categoryFilter.Contains(p.CategoryId) || productFilter.Contains(p.Id))
            .OrderBy(p => p.SortOrder)
            .ToList();

        var neededCategoryIds = sourceProducts.Select(p => p.CategoryId).ToHashSet();
        var sourceCategories = store.Categories
            .Where(c => c.DeletedAt == null)
            .Where(c => includeAll || categoryFilter.Contains(c.Id) || neededCategoryIds.Contains(c.Id))
            .OrderBy(c => c.SortOrder)
            .ToList();

        var categoryMap = new Dictionary<Guid, Guid>();
        foreach (var sourceCategory in sourceCategories)
        {
            var templateCategory = new TemplateCategory
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                Name = sourceCategory.Name,
                Icon = sourceCategory.Icon,
                CategoryType = sourceCategory.CategoryType,
                SortOrder = sourceCategory.SortOrder,
                HotTopCount = sourceCategory.HotTopCount,
                CreatedAt = now,
                UpdatedAt = now
            };
            categoryMap[sourceCategory.Id] = templateCategory.Id;
            template.TemplateCategories.Add(templateCategory);
        }

        var productMap = new Dictionary<Guid, Guid>();
        foreach (var sourceProduct in sourceProducts)
        {
            if (!categoryMap.TryGetValue(sourceProduct.CategoryId, out var templateCategoryId)) continue;

            var templateProduct = new TemplateProduct
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                TemplateCategoryId = templateCategoryId,
                Name = sourceProduct.Name,
                Description = sourceProduct.Description,
                ReferencePrice = sourceProduct.BasePrice,
                ImageUrl = sourceProduct.ImageUrl,
                SortOrder = sourceProduct.SortOrder,
                MinOrderQty = Math.Max(1, sourceProduct.MinOrderQty),
                CreatedAt = now,
                UpdatedAt = now
            };
            productMap[sourceProduct.Id] = templateProduct.Id;

            foreach (var sourceGroup in sourceProduct.SpecGroups.OrderBy(g => g.SortOrder))
            {
                var templateGroup = new TemplateSpecGroup
                {
                    Id = Guid.NewGuid(),
                    TemplateProductId = templateProduct.Id,
                    Name = sourceGroup.Name,
                    IsRequired = sourceGroup.IsRequired,
                    SortOrder = sourceGroup.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                foreach (var sourceOption in sourceGroup.SpecOptions.OrderBy(o => o.SortOrder))
                {
                    templateGroup.TemplateSpecOptions.Add(new TemplateSpecOption
                    {
                        Id = Guid.NewGuid(),
                        TemplateSpecGroupId = templateGroup.Id,
                        Name = sourceOption.Name,
                        ExtraPrice = sourceOption.ExtraPrice,
                        SortOrder = sourceOption.SortOrder,
                        IsDefault = sourceOption.IsDefault,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                templateProduct.TemplateSpecGroups.Add(templateGroup);
            }

            template.TemplateProducts.Add(templateProduct);
        }

        foreach (var sourceProduct in sourceProducts.Where(p => p.IsCombo))
        {
            if (!productMap.TryGetValue(sourceProduct.Id, out var comboTemplateProductId)) continue;

            var templateComboProduct = template.TemplateProducts.First(p => p.Id == comboTemplateProductId);
            foreach (var sourceComboItem in sourceProduct.ComboItemComboProducts.OrderBy(i => i.SortOrder))
            {
                if (!productMap.TryGetValue(sourceComboItem.ProductId, out var childTemplateProductId)) continue;
                templateComboProduct.TemplateComboItemComboTemplateProducts.Add(new TemplateComboItem
                {
                    Id = Guid.NewGuid(),
                    ComboTemplateProductId = comboTemplateProductId,
                    TemplateProductId = childTemplateProductId,
                    Quantity = sourceComboItem.Quantity,
                    Remark = sourceComboItem.Remark,
                    SortOrder = sourceComboItem.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }
    }

    private static List<TemplateCategory> SelectTemplateCategories(StoreTemplate template, bool applyAll, List<Guid>? selectedCategoryIds, List<Guid>? selectedProductIds)
    {
        var categoryFilter = selectedCategoryIds?.ToHashSet() ?? [];
        var productFilter = selectedProductIds?.ToHashSet() ?? [];
        var productCategoryIds = template.TemplateProducts.Where(p => productFilter.Contains(p.Id)).Select(p => p.TemplateCategoryId).ToHashSet();

        return template.TemplateCategories
            .Where(c => applyAll || categoryFilter.Contains(c.Id) || productCategoryIds.Contains(c.Id))
            .OrderBy(c => c.SortOrder)
            .ToList();
    }

    private static List<TemplateProduct> SelectTemplateProducts(StoreTemplate template, List<TemplateCategory> selectedCategories, bool applyAll, List<Guid>? selectedProductIds)
    {
        var categoryIds = selectedCategories.Select(c => c.Id).ToHashSet();
        var productFilter = selectedProductIds?.ToHashSet() ?? [];
        var hasProductFilter = productFilter.Count > 0;

        return template.TemplateProducts
            .Where(p => categoryIds.Contains(p.TemplateCategoryId))
            .Where(p => applyAll || !hasProductFilter || productFilter.Contains(p.Id))
            .OrderBy(p => p.SortOrder)
            .ToList();
    }

    private async Task RemoveTemplateProductGraphAsync(Guid productId, CancellationToken ct)
    {
        var product = await _db.TemplateProducts
            .Include(p => p.TemplateSpecGroups)
            .ThenInclude(g => g.TemplateSpecOptions)
            .Include(p => p.TemplateComboItemComboTemplateProducts)
            .Include(p => p.TemplateComboItemTemplateProducts)
            .FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new BusinessException(404, "模板商品不存在");

        _db.TemplateComboItems.RemoveRange(product.TemplateComboItemComboTemplateProducts);
        _db.TemplateComboItems.RemoveRange(product.TemplateComboItemTemplateProducts);
        _db.TemplateSpecOptions.RemoveRange(product.TemplateSpecGroups.SelectMany(g => g.TemplateSpecOptions));
        _db.TemplateSpecGroups.RemoveRange(product.TemplateSpecGroups);
        _db.TemplateProducts.Remove(product);
    }
}

public class CreateTemplateInput
{
    public string Name { get; set; } = string.Empty;
    public Guid? IndustryCategoryId { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
}

public sealed class CreateTemplateFromStoreInput : CreateTemplateInput
{
    public Guid StoreId { get; set; }
    public bool IncludeAll { get; set; } = true;
    public List<Guid>? SelectedCategoryIds { get; set; }
    public List<Guid>? SelectedProductIds { get; set; }
}

public sealed class CreateCombinedTemplateInput : CreateTemplateInput
{
    public List<StoreTemplateSelectionInput> StoreSelections { get; set; } = [];
}

public sealed class StoreTemplateSelectionInput
{
    public Guid StoreId { get; set; }
    public bool IncludeAll { get; set; } = true;
    public List<Guid>? SelectedCategoryIds { get; set; }
    public List<Guid>? SelectedProductIds { get; set; }
}

public sealed class UpdateTemplateInput
{
    public string? Name { get; set; }
    public Guid? IndustryCategoryId { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}

public sealed class AddTemplateCategoryInput
{
    public string Name { get; set; } = string.Empty;
    public string? CategoryType { get; set; }
    public string? Icon { get; set; }
    public int? SortOrder { get; set; }
    public int? HotTopCount { get; set; }
}

public sealed class AddTemplateProductInput
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ReferencePrice { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? SortOrder { get; set; }
    public int? MinOrderQty { get; set; }
    public List<TemplateSpecGroupInput> SpecGroups { get; set; } = [];
}

public sealed class TemplateSpecGroupInput
{
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public int SortOrder { get; set; }
    public List<TemplateSpecOptionInput> Options { get; set; } = [];
}

public sealed class TemplateSpecOptionInput
{
    public string Name { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
}
