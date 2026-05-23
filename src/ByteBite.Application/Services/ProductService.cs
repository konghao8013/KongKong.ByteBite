using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 商品服务 - 处理商品的增删改查，包含规格组及规格选项的级联创建
/// </summary>
public class ProductService
{
    private readonly ByteBiteDbContext _db;

    public ProductService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 创建商品 - 同时级联创建规格组和规格选项
    /// </summary>
    /// <param name="product">商品实体（含SpecGroups和SpecOptions）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的商品实体</returns>
    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        product.Id = Guid.NewGuid(); product.CreatedAt = DateTime.UtcNow; product.UpdatedAt = DateTime.UtcNow;
        // 级联初始化规格组和规格选项的ID及关联关系
        if (product.SpecGroups != null) foreach (var sg in product.SpecGroups) { sg.Id = Guid.NewGuid(); sg.ProductId = product.Id; sg.CreatedAt = DateTime.UtcNow; if (sg.SpecOptions != null) foreach (var so in sg.SpecOptions) { so.Id = Guid.NewGuid(); so.SpecGroupId = sg.Id; } }
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    /// <summary>
    /// 更新商品基本信息 - 支持部分字段更新，不含规格
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="name">商品名称（可选）</param>
    /// <param name="description">描述（可选）</param>
    /// <param name="price">基础价格（可选）</param>
    /// <param name="imageUrl">图片URL（可选）</param>
    /// <param name="status">状态（可选）</param>
    /// <param name="sortOrder">排序权重（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的商品实体</returns>
    /// <exception cref="BusinessException">404-商品不存在</exception>
    public async Task<Product> UpdateAsync(Guid id, string? name, string? description, decimal? price, string? imageUrl, string? status, int? sortOrder, CancellationToken ct = default)
    {
        var entity = await _db.Products.FindAsync([id], ct) ?? throw new BusinessException(404, "商品不存在");
        if (name != null) entity.Name = name;
        if (description != null) entity.Description = description;
        if (price != null) entity.BasePrice = price.Value;
        if (imageUrl != null) entity.ImageUrl = imageUrl;
        if (status != null) entity.Status = status;
        if (sortOrder != null) entity.SortOrder = sortOrder.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// 删除商品 - 物理删除
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="BusinessException">404-商品不存在</exception>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Products.FindAsync([id], ct) ?? throw new BusinessException(404, "商品不存在");
        _db.Products.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// 根据ID获取商品详情 - 包含规格组和规格选项
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>商品实体（含规格）</returns>
    /// <exception cref="BusinessException">404-商品不存在</exception>
    public async Task<Product> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Products.Include(p => p.SpecGroups).ThenInclude(sg => sg.SpecOptions).FirstOrDefaultAsync(p => p.Id == id, ct) ?? throw new BusinessException(404, "商品不存在");

    /// <summary>
    /// 获取分类下的所有商品 - 包含规格，按排序权重排列
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>商品列表</returns>
    public async Task<List<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Products.Include(p => p.SpecGroups).ThenInclude(sg => sg.SpecOptions).Where(p => p.CategoryId == categoryId).OrderBy(p => p.SortOrder).ToListAsync(ct);

    /// <summary>
    /// 获取店铺下的所有商品 - 包含规格，按排序权重排列
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>商品列表</returns>
    public async Task<List<Product>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Products.Include(p => p.SpecGroups).ThenInclude(sg => sg.SpecOptions).Where(p => p.StoreId == storeId).OrderBy(p => p.SortOrder).ToListAsync(ct);
}
