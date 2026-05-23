using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 数据合并日志表 - 记录匿名数据合并到注册账号的操作
/// </summary>
public partial class DataMergeLog
{
    public Guid Id { get; set; }

    /// <summary>
    /// 目标注册顾客ID
    /// </summary>
    public Guid TargetCustomerId { get; set; }

    /// <summary>
    /// 来源设备标识
    /// </summary>
    public string? SourceDeviceId { get; set; }

    /// <summary>
    /// 来源匿名顾客ID（如有）
    /// </summary>
    public Guid? SourceCustomerId { get; set; }

    /// <summary>
    /// 合并类型：device_to_account-设备到账号, account_to_account-账号到账号, session_to_account-会话到账号
    /// </summary>
    public string MergeType { get; set; } = null!;

    /// <summary>
    /// 合并的订单数量
    /// </summary>
    public int OrdersMerged { get; set; }

    /// <summary>
    /// 合并的购物车商品数量
    /// </summary>
    public int CartItemsMerged { get; set; }

    /// <summary>
    /// 合并的取货码数量
    /// </summary>
    public int PickupCodesMerged { get; set; }

    /// <summary>
    /// 解决的冲突数量
    /// </summary>
    public int ConflictsResolved { get; set; }

    /// <summary>
    /// 合并详情（JSON，记录具体合并内容）
    /// </summary>
    public string? MergeDetail { get; set; }

    /// <summary>
    /// 合并状态：pending-待处理, completed-已完成, failed-失败, rolled_back-已回滚
    /// </summary>
    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Customer? SourceCustomer { get; set; }

    public virtual Customer TargetCustomer { get; set; } = null!;
}
