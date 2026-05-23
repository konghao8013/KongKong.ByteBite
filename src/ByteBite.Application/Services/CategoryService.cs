using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 分类服务 - 处理商品分类的增删改查
/// </summary>
public class CategoryService
{
    private readonly ByteBiteDbContext _db;

    public CategoryService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 创建分类 - 在指定店铺下新建商品分类
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="name">分类名称</param>
    /// <param name="sortOrder">排序权重（越大越靠前）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的分类实体</returns>
    public async Task<Category> CreateAsync(Guid storeId, string name, int sortOrder, CancellationToken ct = default)
    {
        var entity = new Category { Id = Guid.NewGuid(), StoreId = storeId, Name = name, SortOrder = sortOrder, CategoryType = "normal", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// 更新分类 - 修改分类名称和排序
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <param name="name">分类名称</param>
    /// <param name="sortOrder">排序权重</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的分类实体</returns>
    /// <exception cref="BusinessException">404-分类不存在</exception>
    public async Task<Category> UpdateAsync(Guid id, string name, int sortOrder, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FindAsync([id], ct) ?? throw new BusinessException(404, "分类不存在");
        entity.Name = name; entity.SortOrder = sortOrder; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// 删除分类 - 分类下有商品时不允许删除
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="BusinessException">404-分类不存在, 400-该分类下还有商品</exception>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FindAsync([id], ct) ?? throw new BusinessException(404, "分类不存在");
        // 分类下有商品时不允许删除，防止数据孤立
        if (await _db.Products.AnyAsync(p => p.CategoryId == id, ct)) throw new BusinessException(400, "该分类下还有商品，无法删除");
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// 获取店铺下的所有分类，按排序权重排列
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>分类列表</returns>
    public async Task<List<Category>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Categories.Where(c => c.StoreId == storeId).OrderBy(c => c.SortOrder).ToListAsync(ct);

    /// <summary>
    /// 根据ID获取分类信息
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>分类实体</returns>
    /// <exception cref="BusinessException">404-分类不存在</exception>
    public async Task<Category> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Categories.FindAsync([id], ct) ?? throw new BusinessException(404, "分类不存在");
}
