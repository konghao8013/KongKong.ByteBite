using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 时段订单统计表 - 按小时统计订单分布
/// </summary>
public partial class HourlyOrderStat
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public DateOnly StatDate { get; set; }

    /// <summary>
    /// 时段（0-23）
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// 该时段订单数
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// 该时段营收
    /// </summary>
    public decimal Revenue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Store Store { get; set; } = null!;
}
