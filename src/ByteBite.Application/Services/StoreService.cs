using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 店铺服务 - 处理店铺信息查询和更新
/// </summary>
public class StoreService
{
    private readonly ByteBiteDbContext _db;

    public StoreService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 根据ID获取店铺信息
    /// </summary>
    /// <param name="id">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>店铺实体</returns>
    /// <exception cref="BusinessException">404-店铺不存在</exception>
    public async Task<Store> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Stores.FindAsync([id], ct) ?? throw new BusinessException(404, "店铺不存在");

    /// <summary>
    /// 获取商家下的所有店铺列表
    /// </summary>
    /// <param name="merchantId">商家ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>店铺列表</returns>
    public async Task<List<Store>> GetByMerchantIdAsync(Guid merchantId, CancellationToken ct = default)
        => await _db.Stores.Where(s => s.MerchantId == merchantId).ToListAsync(ct);

    /// <summary>
    /// 更新店铺信息 - 支持部分字段更新
    /// </summary>
    /// <param name="id">店铺ID</param>
    /// <param name="name">店铺名称（可选）</param>
    /// <param name="description">店铺描述（可选）</param>
    /// <param name="coverImageUrl">封面图URL（可选）</param>
    /// <param name="businessStatus">营业状态（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的店铺实体</returns>
    /// <exception cref="BusinessException">404-店铺不存在</exception>
    public async Task<Store> UpdateAsync(Guid id, string? name, string? description, string? coverImageUrl, string? businessStatus, CancellationToken ct = default)
    {
        var store = await _db.Stores.FindAsync([id], ct) ?? throw new BusinessException(404, "店铺不存在");
        if (name != null) store.Name = name;
        if (description != null) store.Description = description;
        if (coverImageUrl != null) store.CoverImageUrl = coverImageUrl;
        if (businessStatus != null) store.BusinessStatus = businessStatus;
        store.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return store;
    }
}
