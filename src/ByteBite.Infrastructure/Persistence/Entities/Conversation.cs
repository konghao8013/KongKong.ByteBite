using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

public partial class Conversation
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid StoreId { get; set; }

    public Guid? CustomerId { get; set; }

    public string? DeviceId { get; set; }

    public DateTime LastMessageAt { get; set; }

    public int CustomerUnreadCount { get; set; }

    public int MerchantUnreadCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;

    public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}

