using Microsoft.AspNetCore.SignalR;

namespace ByteBite.Api.Hubs;

/// <summary>
/// 商家端SignalR Hub - 订阅店铺新订单通知
/// </summary>
public class StoreHub : Hub
{
    /// <summary>订阅指定店铺的新订单通知</summary>
    public async Task SubscribeStore(Guid storeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"store_{storeId}");
    }

    /// <summary>取消订阅指定店铺的新订单通知</summary>
    public async Task UnsubscribeStore(Guid storeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"store_{storeId}");
    }
}
