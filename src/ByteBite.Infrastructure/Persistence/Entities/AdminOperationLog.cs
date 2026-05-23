using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 管理员操作日志表
/// </summary>
public partial class AdminOperationLog
{
    public Guid Id { get; set; }

    public Guid AdminId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string Operation { get; set; } = null!;

    /// <summary>
    /// 操作对象类型：merchant, store, template, config等
    /// </summary>
    public string? TargetType { get; set; }

    /// <summary>
    /// 操作对象ID
    /// </summary>
    public Guid? TargetId { get; set; }

    /// <summary>
    /// 操作详情（JSON）
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// 操作者IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Admin Admin { get; set; } = null!;
}
