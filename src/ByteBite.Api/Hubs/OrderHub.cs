using Microsoft.AspNetCore.SignalR;

namespace ByteBite.Api.Hubs;

/// <summary>
/// 顾客端SignalR Hub - 订阅订单状态变更
/// </summary>
public class OrderHub : Hub
{
    /// <summary>订阅指定订单的状态变更通知</summary>
    public async Task SubscribeOrder(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    /// <summary>取消订阅指定订单的状态变更通知</summary>
    public async Task UnsubscribeOrder(Guid orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }
}
