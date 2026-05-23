using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 优惠活动表
/// </summary>
public partial class DiscountRule
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>
    /// 类型：full_reduction-满减, discount-折扣
    /// </summary>
    public string DiscountType { get; set; } = null!;

    /// <summary>
    /// 满减门槛金额（仅满减类型）
    /// </summary>
    public decimal? ThresholdAmount { get; set; }

    /// <summary>
    /// 满减减免金额（仅满减类型）
    /// </summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// 折扣率（如80=8折，仅折扣类型）
    /// </summary>
    public decimal? DiscountRate { get; set; }

    /// <summary>
    /// 适用范围：all-全店, category-指定分类, product-指定商品
    /// </summary>
    public string ApplyScope { get; set; } = null!;

    /// <summary>
    /// 适用范围的分类ID或商品ID
    /// </summary>
    public Guid? ApplyScopeId { get; set; }

    /// <summary>
    /// 是否允许与其他优惠叠加
    /// </summary>
    public bool AllowStack { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = null!;

    public int UsedCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<DiscountEffectStat> DiscountEffectStats { get; set; } = new List<DiscountEffectStat>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Store Store { get; set; } = null!;
}
