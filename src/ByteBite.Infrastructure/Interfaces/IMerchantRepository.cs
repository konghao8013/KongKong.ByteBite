using ByteBite.Infrastructure.Persistence.Entities;

namespace ByteBite.Infrastructure.Interfaces;

/// <summary>
/// 商家仓储接口 - 商家数据访问
/// </summary>
public interface IMerchantRepository : IGenericRepository<Merchant>
{
    /// <summary>根据手机号获取商家</summary>
    Task<Merchant?> GetByPhoneAsync(string phone, CancellationToken ct = default);

    /// <summary>检查手机号是否已注册</summary>
    Task<bool> ExistsByPhoneAsync(string phone, CancellationToken ct = default);
}
