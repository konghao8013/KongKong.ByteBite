using ByteBite.Infrastructure.Persistence.Entities;

namespace ByteBite.Infrastructure.Interfaces;

/// <summary>
/// 商品仓储接口 - 商品数据访问
/// </summary>
public interface IProductRepository : IGenericRepository<Product>
{
    /// <summary>根据分类ID获取商品列表</summary>
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>根据店铺ID获取商品列表</summary>
    Task<IReadOnlyList<Product>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>根据店铺ID和状态获取商品列表</summary>
    Task<IReadOnlyList<Product>> GetByStoreIdAndStatusAsync(Guid storeId, string status, CancellationToken ct = default);
}
