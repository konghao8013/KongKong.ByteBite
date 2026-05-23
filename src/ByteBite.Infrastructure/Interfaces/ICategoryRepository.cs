using ByteBite.Infrastructure.Persistence.Entities;

namespace ByteBite.Infrastructure.Interfaces;

/// <summary>
/// 分类仓储接口 - 分类数据访问
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>根据店铺ID获取分类列表</summary>
    Task<IReadOnlyList<Category>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>获取分类下的商品数量</summary>
    Task<int> GetProductCountByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
}
