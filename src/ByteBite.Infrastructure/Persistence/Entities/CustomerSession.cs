using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 顾客会话表 - 记录设备与账号关联、缓存数据
/// </summary>
public partial class CustomerSession
{
    public Guid Id { get; set; }

    /// <summary>
    /// 关联的顾客ID
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// 设备标识
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 登录令牌
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// 最后访问的店铺ID
    /// </summary>
    public Guid? LastStoreId { get; set; }

    /// <summary>
    /// 最后浏览的分类ID
    /// </summary>
    public Guid? LastCategoryId { get; set; }

    /// <summary>
    /// 购物车数据（JSON，按店铺分组）
    /// </summary>
    public string? CartData { get; set; }

    /// <summary>
    /// 进行中订单列表（JSON）
    /// </summary>
    public string? ActiveOrders { get; set; }

    /// <summary>
    /// 历史订单列表（JSON）
    /// </summary>
    public string? OrderHistory { get; set; }

    /// <summary>
    /// 最后登录IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 最后登录浏览器标识
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 会话过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
