using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 顾客消费统计表
/// </summary>
public partial class CustomerConsumptionStat
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    /// <summary>
    /// 顾客ID（注册用户）
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// 设备标识（匿名用户）
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 在当前店铺的总订单数
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// 在当前店铺的总消费金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 平均客单价
    /// </summary>
    public decimal AvgOrderAmount { get; set; }

    /// <summary>
    /// 首次下单时间
    /// </summary>
    public DateTime? FirstOrderAt { get; set; }

    /// <summary>
    /// 最近下单时间
    /// </summary>
    public DateTime? LastOrderAt { get; set; }

    /// <summary>
    /// 常购商品Top3（JSON）
    /// </summary>
    public string? TopProducts { get; set; }

    /// <summary>
    /// 是否为回头客（下单&gt;=2次）
    /// </summary>
    public bool IsReturning { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Store Store { get; set; } = null!;
}
