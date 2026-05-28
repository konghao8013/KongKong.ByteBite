using Microsoft.AspNetCore.SignalR;

namespace ByteBite.Api.Hubs;

public class ConversationHub : Hub
{
    public async Task SubscribeConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task UnsubscribeConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }
}

