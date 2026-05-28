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

    public async Task<DiscountRule> CreateAsync(UpsertDiscountRuleInput input, CancellationToken ct = default)
    {
        await ValidateAsync(input, null, ct);
        var now = DateTime.UtcNow;
        var entity = new DiscountRule
        {
            Id = Guid.NewGuid(),
            StoreId = input.StoreId!.Value,
            Name = input.Name!.Trim(),
            DiscountType = input.DiscountType!,
            ThresholdAmount = input.DiscountType == "full_reduction" ? input.ThresholdAmount : null,
            DiscountAmount = input.DiscountType == "full_reduction" ? input.DiscountAmount : null,
            DiscountRate = input.DiscountType == "discount" ? input.DiscountRate : null,
            ApplyScope = input.ApplyScope!,
            ApplyScopeId = input.ApplyScope == "all" ? null : input.ApplyScopeId,
            AllowStack = input.AllowStack.GetValueOrDefault(),
            StartTime = input.StartTime!.Value,
            EndTime = input.EndTime!.Value,
            Status = string.IsNullOrWhiteSpace(input.Status) ? "active" : input.Status!,
            UsedCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.DiscountRules.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<DiscountRule> UpdateAsync(Guid id, UpsertDiscountRuleInput input, CancellationToken ct = default)
    {
        var entity = await _db.DiscountRules.FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "优惠规则不存在");

        input.StoreId ??= entity.StoreId;
        input.Name ??= entity.Name;
        input.DiscountType ??= entity.DiscountType;
        input.ThresholdAmount ??= entity.ThresholdAmount;
        input.DiscountAmount ??= entity.DiscountAmount;
        input.DiscountRate ??= entity.DiscountRate;
        input.ApplyScope ??= entity.ApplyScope;
        input.ApplyScopeId ??= entity.ApplyScopeId;
        input.AllowStack ??= entity.AllowStack;
        input.StartTime ??= entity.StartTime;
        input.EndTime ??= entity.EndTime;
        input.Status ??= entity.Status;

        await ValidateAsync(input, id, ct);

        entity.StoreId = input.StoreId!.Value;
        entity.Name = input.Name!.Trim();
        entity.DiscountType = input.DiscountType!;
        entity.ThresholdAmount = input.DiscountType == "full_reduction" ? input.ThresholdAmount : null;
        entity.DiscountAmount = input.DiscountType == "full_reduction" ? input.DiscountAmount : null;
        entity.DiscountRate = input.DiscountType == "discount" ? input.DiscountRate : null;
        entity.ApplyScope = input.ApplyScope!;
        entity.ApplyScopeId = input.ApplyScope == "all" ? null : input.ApplyScopeId;
        entity.AllowStack = input.AllowStack.GetValueOrDefault();
        entity.StartTime = input.StartTime!.Value;
        entity.EndTime = input.EndTime!.Value;
        entity.Status = input.Status!;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.DiscountRules.FindAsync([id], ct) ?? throw new BusinessException(404, "优惠规则不存在");
        entity.DeletedAt = DateTime.UtcNow;
        entity.Status = "inactive";
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// 获取店铺所有优惠规则 - 按创建时间倒序
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>优惠规则列表</returns>
    public async Task<List<DiscountRule>> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.DiscountRules.Where(d => d.StoreId == storeId && d.DeletedAt == null).OrderByDescending(d => d.CreatedAt).ToListAsync(ct);

    /// <summary>
    /// 获取店铺当前生效的优惠规则 - 状态为active且在有效期内的
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>生效中的优惠规则列表</returns>
    public async Task<List<DiscountRule>> GetActiveByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => await _db.DiscountRules.Where(d => d.StoreId == storeId && d.DeletedAt == null && d.Status == "active" && d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow).ToListAsync(ct);

    private async Task ValidateAsync(UpsertDiscountRuleInput input, Guid? currentId, CancellationToken ct)
    {
        if (!input.StoreId.HasValue) throw new BusinessException(400, "店铺ID不能为空");
        if (!await _db.Stores.AnyAsync(s => s.Id == input.StoreId.Value && s.DeletedAt == null, ct))
            throw new BusinessException(404, "店铺不存在");
        if (string.IsNullOrWhiteSpace(input.Name)) throw new BusinessException(400, "活动名称不能为空");
        if (input.DiscountType is not ("full_reduction" or "discount")) throw new BusinessException(400, "优惠类型不正确");
        if (input.ApplyScope is not ("all" or "category" or "product")) throw new BusinessException(400, "适用范围不正确");
        if (!input.StartTime.HasValue || !input.EndTime.HasValue || input.EndTime <= input.StartTime)
            throw new BusinessException(400, "活动结束时间必须晚于开始时间");
        if (input.Status is not ("active" or "inactive")) throw new BusinessException(400, "活动状态不正确");

        if (input.DiscountType == "full_reduction")
        {
            if (input.ThresholdAmount.GetValueOrDefault() <= 0) throw new BusinessException(400, "满减门槛必须大于0");
            if (input.DiscountAmount.GetValueOrDefault() <= 0) throw new BusinessException(400, "满减金额必须大于0");
            if (input.DiscountAmount >= input.ThresholdAmount) throw new BusinessException(400, "满减金额必须小于门槛金额");
        }
        else
        {
            if (input.DiscountRate.GetValueOrDefault() <= 0 || input.DiscountRate >= 100)
                throw new BusinessException(400, "折扣率需要在1到99之间，例如80表示8折");
        }

        if (input.ApplyScope == "category")
        {
            if (!input.ApplyScopeId.HasValue) throw new BusinessException(400, "请选择适用分类");
            var exists = await _db.Categories.AnyAsync(c => c.Id == input.ApplyScopeId.Value && c.StoreId == input.StoreId && c.DeletedAt == null, ct);
            if (!exists) throw new BusinessException(404, "适用分类不存在");
        }
        else if (input.ApplyScope == "product")
        {
            if (!input.ApplyScopeId.HasValue) throw new BusinessException(400, "请选择适用商品");
            var exists = await _db.Products.AnyAsync(p => p.Id == input.ApplyScopeId.Value && p.StoreId == input.StoreId && p.DeletedAt == null, ct);
            if (!exists) throw new BusinessException(404, "适用商品不存在");
        }
    }
}

public sealed class UpsertDiscountRuleInput
{
    public Guid? StoreId { get; set; }
    public string? Name { get; set; }
    public string? DiscountType { get; set; }
    public decimal? ThresholdAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountRate { get; set; }
    public string? ApplyScope { get; set; }
    public Guid? ApplyScopeId { get; set; }
    public bool? AllowStack { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Status { get; set; }
}
