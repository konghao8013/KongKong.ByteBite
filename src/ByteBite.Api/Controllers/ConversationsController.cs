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

    [HttpPost("api/merchant/orders/{orderId:guid}/conversation")]
    public async Task<object> GetOrCreateByOrderForMerchant(Guid orderId, [FromBody] StartMerchantConversationRequest request, CancellationToken ct)
        => ToConversationDto(await _conversationService.GetOrCreateByOrderForMerchantAsync(orderId, request.StoreId, request.CustomerId, request.DeviceId, ct));

    [HttpGet("api/stores/{storeId:guid}/conversations")]
    public async Task<List<object>> GetByStore(Guid storeId, CancellationToken ct)
        => (await _conversationService.GetByStoreAsync(storeId, ct)).Select(ToConversationDto).ToList();

    [HttpGet("api/stores/{storeId:guid}/conversations/unread-count")]
    public async Task<object> GetStoreUnreadCount(Guid storeId, CancellationToken ct)
        => new { count = await _conversationService.GetUnreadCountForStoreAsync(storeId, ct) };

    [HttpGet("api/customer-conversations")]
    public async Task<List<object>> GetByCustomer([FromQuery] Guid? customerId, [FromQuery] string? deviceId, CancellationToken ct)
        => (await _conversationService.GetByCustomerAsync(customerId, deviceId, ct)).Select(ToConversationDto).ToList();

    [HttpGet("api/customer-conversations/unread-count")]
    public async Task<object> GetCustomerUnreadCount([FromQuery] Guid? customerId, [FromQuery] string? deviceId, CancellationToken ct)
        => new { count = await _conversationService.GetUnreadCountForCustomerAsync(customerId, deviceId, ct) };

    [HttpGet("api/conversations/{conversationId:guid}/messages")]
    public async Task<List<object>> GetMessages(Guid conversationId, CancellationToken ct)
        => (await _conversationService.GetMessagesAsync(conversationId, ct)).Select(ToMessageDto).ToList();

    [HttpPost("api/conversations/{conversationId:guid}/messages")]
    public async Task<object> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var message = await _conversationService.SendMessageAsync(conversationId, request.SenderType, request.SenderId, request.Content, ct);
        var conversation = await _conversationService.GetByIdAsync(conversationId, ct);
        var messageDto = ToMessageDto(message);
        var conversationDto = ToConversationDto(conversation);
        var payload = new { conversationId, message = messageDto, conversation = conversationDto };

        await _conversationHub.Clients.Group($"conversation_{conversationId}")
            .SendAsync("ConversationMessageReceived", payload, cancellationToken: ct);

        if (message.SenderType == "customer")
        {
            var unreadCount = await _conversationService.GetUnreadCountForStoreAsync(conversation.StoreId, ct);
            var storePayload = new { conversationId, message = messageDto, conversation = conversationDto, unreadCount };
            var unreadPayload = new { scope = "merchant", storeId = conversation.StoreId, count = unreadCount };
            await _conversationHub.Clients.Group($"store_{conversation.StoreId}")
                .SendAsync("CustomerMessageReceived", storePayload, cancellationToken: ct);
            await _conversationHub.Clients.Group($"store_{conversation.StoreId}")
                .SendAsync("ConversationUnreadChanged", unreadPayload, cancellationToken: ct);
            await _storeHub.Clients.Group($"store_{conversation.StoreId}")
                .SendAsync("CustomerMessageReceived", storePayload, cancellationToken: ct);
        }
        else
        {
            var unreadCount = await _conversationService.GetUnreadCountForCustomerAsync(conversation.CustomerId, conversation.DeviceId, ct);
            var customerPayload = new { conversationId, message = messageDto, conversation = conversationDto, unreadCount };
            var unreadPayload = new { scope = "customer", conversation.CustomerId, conversation.DeviceId, count = unreadCount };
            foreach (var groupName in ConversationHub.GetCustomerGroupNames(conversation.CustomerId, conversation.DeviceId))
            {
                await _conversationHub.Clients.Group(groupName)
                    .SendAsync("MerchantMessageReceived", customerPayload, cancellationToken: ct);
                await _conversationHub.Clients.Group(groupName)
                    .SendAsync("ConversationUnreadChanged", unreadPayload, cancellationToken: ct);
            }
            await _orderHub.Clients.Group($"order_{conversation.OrderId}")
                .SendAsync("MerchantMessageReceived", customerPayload, cancellationToken: ct);
        }

        return messageDto;
    }

    [HttpPost("api/conversations/{conversationId:guid}/read")]
    public async Task<object> MarkRead(Guid conversationId, [FromBody] MarkReadRequest request, CancellationToken ct)
    {
        var conversation = await _conversationService.MarkReadAsync(conversationId, request.ReaderType, ct);
        var readerType = request.ReaderType.Trim().ToLowerInvariant();
        if (readerType == "merchant")
        {
            var unreadCount = await _conversationService.GetUnreadCountForStoreAsync(conversation.StoreId, ct);
            var payload = new { scope = "merchant", storeId = conversation.StoreId, count = unreadCount };
            await _conversationHub.Clients.Group($"store_{conversation.StoreId}")
                .SendAsync("ConversationUnreadChanged", payload, cancellationToken: ct);
            return new { unreadCount, conversation = ToConversationDto(conversation) };
        }

        var customerUnreadCount = await _conversationService.GetUnreadCountForCustomerAsync(conversation.CustomerId, conversation.DeviceId, ct);
        var customerPayload = new { scope = "customer", conversation.CustomerId, conversation.DeviceId, count = customerUnreadCount };
        foreach (var groupName in ConversationHub.GetCustomerGroupNames(conversation.CustomerId, conversation.DeviceId))
        {
            await _conversationHub.Clients.Group(groupName)
                .SendAsync("ConversationUnreadChanged", customerPayload, cancellationToken: ct);
        }
        return new { unreadCount = customerUnreadCount, conversation = ToConversationDto(conversation) };
    }

    private static object ToConversationDto(Conversation conversation)
        => new
        {
            conversation.Id,
            conversation.OrderId,
            conversation.StoreId,
            StoreName = conversation.Store?.Name,
            StoreCode = conversation.Store?.StoreCode,
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

    private static object ToMessageDto(ConversationMessage message)
        => new
        {
            message.Id,
            message.ConversationId,
            message.SenderType,
            message.SenderId,
            message.Content,
            message.MessageType,
            message.ReadAt,
            message.CreatedAt
        };
}

public class StartConversationRequest { public Guid? CustomerId { get; set; } public string? DeviceId { get; set; } }
public class StartMerchantConversationRequest { public Guid StoreId { get; set; } public Guid? CustomerId { get; set; } public string? DeviceId { get; set; } }
public class SendMessageRequest { public string SenderType { get; set; } = string.Empty; public Guid? SenderId { get; set; } public string Content { get; set; } = string.Empty; public Guid? StoreId { get; set; } public Guid? OrderId { get; set; } }
public class MarkReadRequest { public string ReaderType { get; set; } = string.Empty; }
