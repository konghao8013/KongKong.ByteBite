using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 每日店铺经营统计快照表
/// </summary>
public partial class DailyStoreStat
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    /// <summary>
    /// 统计日期
    /// </summary>
    public DateOnly StatDate { get; set; }

    /// <summary>
    /// 当日总订单数
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// 当日已完成订单数
    /// </summary>
    public int CompletedOrders { get; set; }

    /// <summary>
    /// 当日已取消订单数
    /// </summary>
    public int CancelledOrders { get; set; }

    /// <summary>
    /// 当日总金额（含优惠前）
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// 当日实收金额（扣除优惠）
    /// </summary>
    public decimal ActualRevenue { get; set; }

    /// <summary>
    /// 当日优惠减免总额
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 当日打包费总额
    /// </summary>
    public decimal PackingFee { get; set; }

    /// <summary>
    /// 当日客单价
    /// </summary>
    public decimal AvgOrderAmount { get; set; }

    /// <summary>
    /// 当日新顾客数
    /// </summary>
    public int NewCustomers { get; set; }

    /// <summary>
    /// 当日回头客数
    /// </summary>
    public int ReturningCustomers { get; set; }

    /// <summary>
    /// 当日高峰时段（0-23）
    /// </summary>
    public int? PeakHour { get; set; }

    /// <summary>
    /// 高峰时段订单数
    /// </summary>
    public int PeakHourOrders { get; set; }

    /// <summary>
    /// 当日最热销商品ID
    /// </summary>
    public Guid? TopProductId { get; set; }

    /// <summary>
    /// 当日最热销商品名称
    /// </summary>
    public string? TopProductName { get; set; }

    /// <summary>
    /// 当日最热销商品销量
    /// </summary>
    public int TopProductQty { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Store Store { get; set; } = null!;
}
