using System;

namespace ByteBite.Infrastructure.Persistence.Entities;

public partial class ConversationMessage
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public string SenderType { get; set; } = null!;

    public Guid? SenderId { get; set; }

    public string Content { get; set; } = null!;

    public string MessageType { get; set; } = "text";

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}

