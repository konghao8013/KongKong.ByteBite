using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

public partial class Admin
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AdminOperationLog> AdminOperationLogs { get; set; } = new List<AdminOperationLog>();

    public virtual ICollection<MerchantAuditLog> MerchantAuditLogs { get; set; } = new List<MerchantAuditLog>();

    public virtual ICollection<StoreTemplate> StoreTemplates { get; set; } = new List<StoreTemplate>();
}
