using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService) { _orderService = orderService; }

    [HttpPost("api/orders")]
    public async Task<Order> CreateOrder([FromBody] CreateOrderRequest request) => await _orderService.CreateOrderAsync(request.StoreId, request.CustomerId, request.DeviceId, request.DiningMode, request.Items, request.Remark);

    [HttpGet("api/orders/pickup/{pickupCode}/store/{storeId:guid}")]
    public async Task<Order> GetByPickupCode(string pickupCode, Guid storeId) => await _orderService.GetByPickupCodeAsync(pickupCode, storeId);

    [HttpGet("api/stores/{storeId:guid}/orders")]
    public async Task<List<Order>> GetByStoreId(Guid storeId, [FromQuery] string? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        => await _orderService.GetByStoreIdAsync(storeId, status, pageIndex, pageSize);

    [HttpPatch("api/orders/{orderId:guid}/accept")]
    public async Task<Order> AcceptOrder(Guid orderId) => await _orderService.AcceptOrderAsync(orderId);

    [HttpPatch("api/orders/{orderId:guid}/reject")]
    public async Task<Order> RejectOrder(Guid orderId, [FromBody] RejectOrderRequest request) => await _orderService.RejectOrderAsync(orderId, request.Reason);

    [HttpPatch("api/orders/{orderId:guid}/prepare")]
    public async Task<Order> StartPreparing(Guid orderId) => await _orderService.StartPreparingAsync(orderId);

    [HttpPatch("api/orders/{orderId:guid}/ready")]
    public async Task<Order> MarkReady(Guid orderId) => await _orderService.MarkReadyAsync(orderId);

    [HttpPatch("api/orders/{orderId:guid}/complete")]
    public async Task<Order> CompleteOrder(Guid orderId) => await _orderService.CompleteOrderAsync(orderId);

    [HttpPatch("api/orders/{orderId:guid}/cancel")]
    public async Task<Order> CancelOrder(Guid orderId) => await _orderService.CancelOrderAsync(orderId);
}

public class CreateOrderRequest { public Guid StoreId { get; set; } public Guid? CustomerId { get; set; } public string? DeviceId { get; set; } public string DiningMode { get; set; } = "dine_in"; public List<OrderItem> Items { get; set; } = []; public string? Remark { get; set; } }
public class RejectOrderRequest { public string Reason { get; set; } = string.Empty; }