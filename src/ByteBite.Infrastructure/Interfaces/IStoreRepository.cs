using ByteBite.Infrastructure.Persistence.Entities;

namespace ByteBite.Infrastructure.Interfaces;

/// <summary>
/// 店铺仓储接口 - 店铺数据访问
/// </summary>
public interface IStoreRepository : IGenericRepository<Store>
{
    /// <summary>根据商家ID获取其所有店铺</summary>
    Task<IReadOnlyList<Store>> GetByMerchantIdAsync(Guid merchantId, CancellationToken ct = default);

    /// <summary>检查商家是否已拥有同名店铺</summary>
    Task<bool> ExistsByNameAsync(Guid merchantId, string name, CancellationToken ct = default);
}
