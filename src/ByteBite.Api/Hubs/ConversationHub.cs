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

    public async Task SubscribeStore(Guid storeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"store_{storeId}");
    }

    public async Task UnsubscribeStore(Guid storeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"store_{storeId}");
    }

    public async Task SubscribeCustomer(Guid? customerId, string? deviceId)
    {
        foreach (var groupName in GetCustomerGroupNames(customerId, deviceId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }

    public async Task UnsubscribeCustomer(Guid? customerId, string? deviceId)
    {
        foreach (var groupName in GetCustomerGroupNames(customerId, deviceId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }

    public static IEnumerable<string> GetCustomerGroupNames(Guid? customerId, string? deviceId)
    {
        if (customerId.HasValue) yield return $"customer_{customerId.Value}";
        if (!string.IsNullOrWhiteSpace(deviceId)) yield return $"device_{deviceId.Trim()}";
    }
}
