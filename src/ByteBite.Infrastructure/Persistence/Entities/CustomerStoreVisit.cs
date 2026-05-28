using System;

namespace ByteBite.Infrastructure.Persistence.Entities;

public partial class CustomerStoreVisit
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid? CustomerId { get; set; }

    public string? DeviceId { get; set; }

    public DateTime LastVisitedAt { get; set; }

    public DateTime? LastOrderedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Store Store { get; set; } = null!;
}

