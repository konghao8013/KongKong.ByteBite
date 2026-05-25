using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

public partial class Customer
{
    public Guid Id { get; set; }

    public string? Phone { get; set; }

    public string? Nickname { get; set; }

    public string? AvatarUrl { get; set; }

    public string? DeviceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? PasswordHash { get; set; }

    public bool IsRegistered { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<CustomerConsumptionStat> CustomerConsumptionStats { get; set; } = new List<CustomerConsumptionStat>();

    public virtual ICollection<CustomerSession> CustomerSessions { get; set; } = new List<CustomerSession>();

    public virtual ICollection<DataMergeLog> DataMergeLogSourceCustomers { get; set; } = new List<DataMergeLog>();

    public virtual ICollection<DataMergeLog> DataMergeLogTargetCustomers { get; set; } = new List<DataMergeLog>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
