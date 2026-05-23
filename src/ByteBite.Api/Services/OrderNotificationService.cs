using ByteBite.Api.Hubs;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.SignalR;

namespace ByteBite.Api.Services;

public class OrderNotificationService
{
    private readonly IHubContext<StoreHub> _storeHubContext;
    private readonly IHubContext<OrderHub> _orderHubContext;

    public OrderNotificationService(IHubContext<StoreHub> storeHubContext, IHubContext<OrderHub> orderHubContext)
    { _storeHubContext = storeHubContext; _orderHubContext = orderHubContext; }

    public async Task SendNewOrderNotificationAsync(Guid storeId, Order order, CancellationToken ct = default)
    {
        await _storeHubContext.Clients.Group($"store_{storeId}").SendAsync("NewOrder", order, cancellationToken: ct);
    }

    public async Task SendOrderStatusChangedAsync(Guid orderId, string status, CancellationToken ct = default)
    {
        await _orderHubContext.Clients.Group($"order_{orderId}").SendAsync("OrderStatusChanged", new { orderId, status }, cancellationToken: ct);
        await _storeHubContext.Clients.All.SendAsync("OrderStatusUpdated", new { orderId, status }, cancellationToken: ct);
    }

    public async Task SendOrderCancelledAsync(Guid orderId, CancellationToken ct = default)
    {
        await _orderHubContext.Clients.Group($"order_{orderId}").SendAsync("OrderCancelled", new { orderId }, cancellationToken: ct);
        await _storeHubContext.Clients.All.SendAsync("OrderCancelled", new { orderId }, cancellationToken: ct);
    }
}