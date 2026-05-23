using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 优惠规则服务 - 处理满减/折扣等优惠活动的创建、更新和查询
/// </summary>
public class DiscountRuleService
{
    private readonly ByteBiteDbContext _db;

    public DiscountRuleService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 创建优惠规则 - 满减/折扣活动
    /// </summary>
    /// <param name="entity">优惠规则实体</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的优惠规则实体</returns>
    public async Task<DiscountRule> CreateAsync(DiscountRule entity, CancellationToken ct = default)
    {
        entity.Id = Guid.NewGuid(); entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        _db.DiscountRules.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// 更新优惠规则 - 修改名称或状态
    /// </summary>
    /// <param name="id">优惠规则ID</param>
    /// <param name="name">规则名称（可选）</param>
    /// <param name="status">状态（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的优惠规则实体</returns>
    /// <exception cref="BusinessException">404-优惠规则不存在</exception>
    public async Task<DiscountRule> UpdateAsync(Guid id, string? name, string? status, CancellationToken ct = default)
    {
        var entity = await _db.DiscountRules.FindAsync([id], ct) ?? throw new BusinessException(404, "优惠规则不存在");
        if (name != null) entity.Name = name;
        if (status != null) entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// 获取店铺所有优惠规则 - 按创建时间倒序
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>优惠规则列表</returns>
    public async Task<List<DiscountRule>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.DiscountRules.Where(d => d.StoreId == storeId).OrderByDescending(d => d.CreatedAt).ToListAsync(ct);

    /// <summary>
    /// 获取店铺当前生效的优惠规则 - 状态为active且在有效期内的
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>生效中的优惠规则列表</returns>
    public async Task<List<DiscountRule>> GetActiveByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.DiscountRules.Where(d => d.StoreId == storeId && d.Status == "active" && d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow).ToListAsync(ct);
}