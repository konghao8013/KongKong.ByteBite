using ByteBite.Application.Services;
using ByteBite.Api.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrderNotificationService _notificationService;

    public OrdersController(OrderService orderService, OrderNotificationService notificationService)
    {
        _orderService = orderService;
        _notificationService = notificationService;
    }

    [HttpPost("api/orders")]
    public async Task<Order> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CreateOrderAsync(request.StoreId, request.CustomerId, request.DeviceId, request.DiningMode, request.Items, request.Remark, request.TableNo, request.DeliveryAddress, request.DeliveryPhone, ct);
        await _notificationService.SendNewOrderNotificationAsync(order.StoreId, order, ct);
        return order;
    }

    [HttpGet("api/orders/pickup/{pickupCode}/store/{storeId:guid}")]
    public async Task<Order> GetByPickupCode(string pickupCode, Guid storeId) => await _orderService.GetByPickupCodeAsync(pickupCode, storeId);

    [HttpGet("api/stores/{storeId:guid}/orders")]
    public async Task<List<Order>> GetByStoreId(Guid storeId, [FromQuery] string? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        => await _orderService.GetByStoreIdAsync(storeId, status, pageIndex, pageSize);

    [HttpGet("api/stores/{storeId:guid}/customer-orders")]
    public async Task<List<Order>> GetCustomerOrders(Guid storeId, [FromQuery] string? deviceId, [FromQuery] Guid? customerId, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => await _orderService.GetCustomerOrdersAsync(storeId, deviceId, customerId, pageSize, ct);

    [HttpPatch("api/orders/{orderId:guid}/accept")]
    public async Task<Order> AcceptOrder(Guid orderId, CancellationToken ct) => await UpdateAndNotify(orderId, _orderService.AcceptOrderAsync, ct);

    [HttpPatch("api/orders/{orderId:guid}/reject")]
    public async Task<Order> RejectOrder(Guid orderId, [FromBody] RejectOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.RejectOrderAsync(orderId, request.Reason, ct);
        await _notificationService.SendOrderStatusChangedAsync(order.Id, order.Status, ct);
        await _notificationService.SendOrderCancelledAsync(order.Id, ct);
        return order;
    }

    [HttpPatch("api/orders/{orderId:guid}/prepare")]
    public async Task<Order> StartPreparing(Guid orderId, CancellationToken ct) => await UpdateAndNotify(orderId, _orderService.StartPreparingAsync, ct);

    [HttpPatch("api/orders/{orderId:guid}/ready")]
    public async Task<Order> MarkReady(Guid orderId, CancellationToken ct) => await UpdateAndNotify(orderId, _orderService.MarkReadyAsync, ct);

    [HttpPatch("api/orders/{orderId:guid}/complete")]
    public async Task<Order> CompleteOrder(Guid orderId, CancellationToken ct) => await UpdateAndNotify(orderId, _orderService.CompleteOrderAsync, ct);

    [HttpPatch("api/orders/{orderId:guid}/cancel")]
    public async Task<Order> CancelOrder(Guid orderId, CancellationToken ct)
    {
        var order = await _orderService.CancelOrderAsync(orderId, ct);
        await _notificationService.SendOrderStatusChangedAsync(order.Id, order.Status, ct);
        await _notificationService.SendOrderCancelledAsync(order.Id, ct);
        return order;
    }

    private async Task<Order> UpdateAndNotify(Guid orderId, Func<Guid, CancellationToken, Task<Order>> update, CancellationToken ct)
    {
        var order = await update(orderId, ct);
        await _notificationService.SendOrderStatusChangedAsync(order.Id, order.Status, ct);
        return order;
    }
}

public class CreateOrderRequest { public Guid StoreId { get; set; } public Guid? CustomerId { get; set; } public string? DeviceId { get; set; } public string DiningMode { get; set; } = "dine_in"; public string? TableNo { get; set; } public string? DeliveryAddress { get; set; } public string? DeliveryPhone { get; set; } public List<CreateOrderItemInput> Items { get; set; } = []; public string? Remark { get; set; } }
public class RejectOrderRequest { public string Reason { get; set; } = string.Empty; }
