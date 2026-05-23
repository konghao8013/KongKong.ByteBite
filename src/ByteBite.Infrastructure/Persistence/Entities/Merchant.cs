using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 商家用户表
/// </summary>
public partial class Merchant
{
    /// <summary>
    /// 主键UUID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 手机号，作为登录账号
    /// </summary>
    public string Phone { get; set; } = null!;

    /// <summary>
    /// BCrypt加密密码
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 状态：pending-待审核, active-已激活, disabled-已禁用
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 软删除时间
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<DataExportLog> DataExportLogs { get; set; } = new List<DataExportLog>();

    public virtual ICollection<MerchantAuditLog> MerchantAuditLogs { get; set; } = new List<MerchantAuditLog>();

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
