using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 顾客用户表
/// </summary>
public partial class Customer
{
    public Guid Id { get; set; }

    /// <summary>
    /// 手机号（注册后才有）
    /// </summary>
    public string? Phone { get; set; }

    public string? Nickname { get; set; }

    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 设备标识（匿名用户）
    /// </summary>
    public string? DeviceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// BCrypt加密密码（注册用户才有）
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// 是否已注册：true-已注册用户, false-匿名用户
    /// </summary>
    public bool IsRegistered { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<CustomerConsumptionStat> CustomerConsumptionStats { get; set; } = new List<CustomerConsumptionStat>();

    public virtual ICollection<CustomerSession> CustomerSessions { get; set; } = new List<CustomerSession>();

    public virtual ICollection<DataMergeLog> DataMergeLogSourceCustomers { get; set; } = new List<DataMergeLog>();

    public virtual ICollection<DataMergeLog> DataMergeLogTargetCustomers { get; set; } = new List<DataMergeLog>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
