using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 优惠活动效果统计表
/// </summary>
public partial class DiscountEffectStat
{
    public Guid Id { get; set; }

    public Guid DiscountRuleId { get; set; }

    public Guid StoreId { get; set; }

    public DateOnly StatDate { get; set; }

    /// <summary>
    /// 当日使用次数
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// 当日优惠减免总额
    /// </summary>
    public decimal TotalDiscountAmount { get; set; }

    /// <summary>
    /// 当日使用优惠的订单总营收
    /// </summary>
    public decimal TotalDrivenRevenue { get; set; }

    /// <summary>
    /// 当日使用优惠的订单平均金额
    /// </summary>
    public decimal AvgOrderAmountWithDiscount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual DiscountRule DiscountRule { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
