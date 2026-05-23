using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 平台每日统计表
/// </summary>
public partial class PlatformDailyStat
{
    public Guid Id { get; set; }

    public DateOnly StatDate { get; set; }

    /// <summary>
    /// 商家总数
    /// </summary>
    public int TotalMerchants { get; set; }

    /// <summary>
    /// 活跃商家数（当日有订单）
    /// </summary>
    public int ActiveMerchants { get; set; }

    /// <summary>
    /// 当日新增商家数
    /// </summary>
    public int NewMerchants { get; set; }

    /// <summary>
    /// 店铺总数
    /// </summary>
    public int TotalStores { get; set; }

    /// <summary>
    /// 当日平台总订单数
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// 当日平台总营收
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// 顾客总数
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// 当日新增顾客数
    /// </summary>
    public int NewCustomers { get; set; }

    /// <summary>
    /// 当日模板使用次数
    /// </summary>
    public int TemplateUsageCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
