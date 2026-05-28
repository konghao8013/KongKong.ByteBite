using ByteBite.Api.Hubs;
using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ByteBite.Api.Controllers;

[ApiController]
public class ConversationsController : ControllerBase
{
    private readonly ConversationService _conversationService;
    private readonly IHubContext<ConversationHub> _conversationHub;
    private readonly IHubContext<StoreHub> _storeHub;
    private readonly IHubContext<OrderHub> _orderHub;

    public ConversationsController(
        ConversationService conversationService,
        IHubContext<ConversationHub> conversationHub,
        IHubContext<StoreHub> storeHub,
        IHubContext<OrderHub> orderHub)
    {
        _conversationService = conversationService;
        _conversationHub = conversationHub;
        _storeHub = storeHub;
        _orderHub = orderHub;
    }

    [HttpPost("api/orders/{orderId:guid}/conversation")]
    public async Task<object> GetOrCreateByOrder(Guid orderId, [FromBody] StartConversationRequest request, CancellationToken ct)
        => ToConversationDto(await _conversationService.GetOrCreateByOrderAsync(orderId, request.CustomerId, request.DeviceId, ct));

    [HttpGet("api/stores/{storeId:guid}/conversations")]
    public async Task<List<object>> GetByStore(Guid storeId, CancellationToken ct)
        => (await _conversationService.GetByStoreAsync(storeId, ct)).Select(ToConversationDto).ToList();

    [HttpGet("api/customer-conversations")]
    public async Task<List<object>> GetByCustomer([FromQuery] Guid? customerId, [FromQuery] string? deviceId, CancellationToken ct)
        => (await _conversationService.GetByCustomerAsync(customerId, deviceId, ct)).Select(ToConversationDto).ToList();

    [HttpGet("api/conversations/{conversationId:guid}/messages")]
    public async Task<List<ConversationMessage>> GetMessages(Guid conversationId, CancellationToken ct)
        => await _conversationService.GetMessagesAsync(conversationId, ct);

    [HttpPost("api/conversations/{conversationId:guid}/messages")]
    public async Task<ConversationMessage> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var message = await _conversationService.SendMessageAsync(conversationId, request.SenderType, request.SenderId, request.Content, ct);
        var payload = new { conversationId, message };
        await _conversationHub.Clients.Group($"conversation_{conversationId}").SendAsync("ConversationMessageReceived", payload, cancellationToken: ct);
        if (request.SenderType == "customer" && request.StoreId != null)
            await _storeHub.Clients.Group($"store_{request.StoreId}").SendAsync("CustomerMessageReceived", payload, cancellationToken: ct);
        if (request.SenderType == "merchant" && request.OrderId != null)
            await _orderHub.Clients.Group($"order_{request.OrderId}").SendAsync("MerchantMessageReceived", payload, cancellationToken: ct);
        return message;
    }

    [HttpPost("api/conversations/{conversationId:guid}/read")]
    public async Task<object> MarkRead(Guid conversationId, [FromBody] MarkReadRequest request, CancellationToken ct)
    {
        await _conversationService.MarkReadAsync(conversationId, request.ReaderType, ct);
        return new { };
    }

    private static object ToConversationDto(Conversation conversation)
        => new
        {
            conversation.Id,
            conversation.OrderId,
            conversation.StoreId,
            StoreName = conversation.Store?.Name,
            conversation.CustomerId,
            conversation.DeviceId,
            conversation.LastMessageAt,
            conversation.CustomerUnreadCount,
            conversation.MerchantUnreadCount,
            Order = conversation.Order == null ? null : new
            {
                conversation.Order.Id,
                conversation.Order.OrderNo,
                conversation.Order.PickupCode,
                conversation.Order.Status,
                conversation.Order.DiningMode,
                conversation.Order.TableNo,
                conversation.Order.TotalAmount,
                conversation.Order.ActualAmount,
                conversation.Order.CreatedAt
            },
            Customer = conversation.Customer == null ? null : new
            {
                conversation.Customer.Id,
                conversation.Customer.Phone,
                conversation.Customer.Username,
                conversation.Customer.Nickname
            }
        };
}

public class StartConversationRequest { public Guid? CustomerId { get; set; } public string? DeviceId { get; set; } }
public class SendMessageRequest { public string SenderType { get; set; } = string.Empty; public Guid? SenderId { get; set; } public string Content { get; set; } = string.Empty; public Guid? StoreId { get; set; } public Guid? OrderId { get; set; } }
public class MarkReadRequest { public string ReaderType { get; set; } = string.Empty; }

