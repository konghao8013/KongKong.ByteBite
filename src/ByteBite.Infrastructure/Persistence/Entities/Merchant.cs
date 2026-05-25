using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

public partial class Merchant
{
    public Guid Id { get; set; }

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Nickname { get; set; }

    public string? AvatarUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<DataExportLog> DataExportLogs { get; set; } = new List<DataExportLog>();

    public virtual ICollection<MerchantAuditLog> MerchantAuditLogs { get; set; } = new List<MerchantAuditLog>();

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
