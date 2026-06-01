using Microsoft.AspNetCore.SignalR;

namespace ByteBite.Api.Hubs;

public class ConversationHub : Hub
{
    public async Task SubscribeConversation(string? conversationId)
    {
        var groupName = GetEntityGroupName("conversation", conversationId);
        if (groupName != null) await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task UnsubscribeConversation(string? conversationId)
    {
        var groupName = GetEntityGroupName("conversation", conversationId);
        if (groupName != null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SubscribeStore(string? storeId)
    {
        var groupName = GetEntityGroupName("store", storeId);
        if (groupName != null) await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task UnsubscribeStore(string? storeId)
    {
        var groupName = GetEntityGroupName("store", storeId);
        if (groupName != null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SubscribeCustomer(string? customerId, string? deviceId)
    {
        foreach (var groupName in GetCustomerGroupNames(ParseGuid(customerId), deviceId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }

    public async Task UnsubscribeCustomer(string? customerId, string? deviceId)
    {
        foreach (var groupName in GetCustomerGroupNames(ParseGuid(customerId), deviceId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }

    public static IEnumerable<string> GetCustomerGroupNames(Guid? customerId, string? deviceId)
    {
        if (customerId.HasValue)
        {
            yield return $"customer_{customerId.Value}";
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(deviceId)) yield return $"device_{deviceId.Trim()}";
    }

    private static string? GetEntityGroupName(string prefix, string? id)
    {
        var parsedId = ParseGuid(id);
        return parsedId.HasValue ? $"{prefix}_{parsedId.Value}" : null;
    }

    private static Guid? ParseGuid(string? value)
        => Guid.TryParse(value, out var parsed) ? parsed : null;
}
