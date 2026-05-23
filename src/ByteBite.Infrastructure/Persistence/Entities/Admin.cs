using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 平台管理员表
/// </summary>
public partial class Admin
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? DisplayName { get; set; }

    /// <summary>
    /// 角色：super_admin-超级管理员, admin-管理员, viewer-只读
    /// </summary>
    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AdminOperationLog> AdminOperationLogs { get; set; } = new List<AdminOperationLog>();

    public virtual ICollection<MerchantAuditLog> MerchantAuditLogs { get; set; } = new List<MerchantAuditLog>();

    public virtual ICollection<StoreTemplate> StoreTemplates { get; set; } = new List<StoreTemplate>();
}
