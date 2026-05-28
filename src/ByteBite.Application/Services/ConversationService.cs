using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

public class ConversationService
{
    private readonly ByteBiteDbContext _db;

    public ConversationService(ByteBiteDbContext db) { _db = db; }

    public async Task<Conversation> GetOrCreateByOrderAsync(Guid orderId, Guid? customerId, string? deviceId, CancellationToken ct = default)
    {
        var order = await _db.Orders.Include(o => o.Store).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new BusinessException(404, "订单不存在");

        if (customerId == null && string.IsNullOrWhiteSpace(deviceId))
            throw new BusinessException(400, "缺少顾客身份信息");
        if (customerId != null && order.CustomerId != customerId && order.DeviceId != deviceId)
            throw new BusinessException(403, "无权访问该订单会话");
        if (customerId == null && order.DeviceId != deviceId)
            throw new BusinessException(403, "无权访问该订单会话");

        var conversation = await _db.Conversations
            .Include(c => c.Order)
            .ThenInclude(o => o.OrderItems)
            .Include(c => c.Store)
            .FirstOrDefaultAsync(c => c.OrderId == orderId, ct);
        if (conversation != null) return conversation;

        var now = DateTime.UtcNow;
        conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            StoreId = order.StoreId,
            CustomerId = order.CustomerId ?? customerId,
            DeviceId = order.CustomerId == null ? order.DeviceId ?? deviceId : null,
            LastMessageAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync(ct);
        conversation.Order = order;
        conversation.Store = order.Store;
        return conversation;
    }

    public async Task<List<Conversation>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Conversations
            .Include(c => c.Order)
            .ThenInclude(o => o.OrderItems)
            .Include(c => c.Customer)
            .Where(c => c.StoreId == storeId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(ct);

    public async Task<List<Conversation>> GetByCustomerAsync(Guid? customerId, string? deviceId, CancellationToken ct = default)
    {
        if (customerId == null && string.IsNullOrWhiteSpace(deviceId)) return [];
        var query = _db.Conversations
            .Include(c => c.Order)
            .ThenInclude(o => o.OrderItems)
            .Include(c => c.Store)
            .AsQueryable();

        query = customerId != null
            ? query.Where(c => c.CustomerId == customerId || c.DeviceId == deviceId)
            : query.Where(c => c.DeviceId == deviceId);

        return await query.OrderByDescending(c => c.LastMessageAt).ToListAsync(ct);
    }

    public async Task<Conversation> GetByIdAsync(Guid conversationId, CancellationToken ct = default)
        => await _db.Conversations
            .Include(c => c.Order)
            .ThenInclude(o => o.OrderItems)
            .Include(c => c.Store)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct)
            ?? throw new BusinessException(404, "会话不存在");

    public async Task<int> GetUnreadCountForStoreAsync(Guid storeId, CancellationToken ct = default)
        => await _db.Conversations
            .Where(c => c.StoreId == storeId)
            .SumAsync(c => c.MerchantUnreadCount, ct);

    public async Task<int> GetUnreadCountForCustomerAsync(Guid? customerId, string? deviceId, CancellationToken ct = default)
    {
        if (customerId == null && string.IsNullOrWhiteSpace(deviceId)) return 0;

        var query = _db.Conversations.AsQueryable();
        query = customerId != null
            ? query.Where(c => c.CustomerId == customerId || c.DeviceId == deviceId)
            : query.Where(c => c.DeviceId == deviceId);

        return await query.SumAsync(c => c.CustomerUnreadCount, ct);
    }

    public async Task<List<ConversationMessage>> GetMessagesAsync(Guid conversationId, CancellationToken ct = default)
        => await _db.ConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task<ConversationMessage> SendMessageAsync(Guid conversationId, string senderType, Guid? senderId, string content, CancellationToken ct = default)
    {
        var normalizedSender = senderType.Trim().ToLowerInvariant();
        if (normalizedSender is not ("customer" or "merchant"))
            throw new BusinessException(400, "消息发送方不正确");
        if (string.IsNullOrWhiteSpace(content))
            throw new BusinessException(400, "消息内容不能为空");

        var conversation = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId, ct)
            ?? throw new BusinessException(404, "会话不存在");

        var now = DateTime.UtcNow;
        var message = new ConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderType = normalizedSender,
            SenderId = senderId,
            Content = content.Trim(),
            MessageType = "text",
            CreatedAt = now
        };

        conversation.LastMessageAt = now;
        conversation.UpdatedAt = now;
        if (normalizedSender == "customer") conversation.MerchantUnreadCount++;
        else conversation.CustomerUnreadCount++;

        _db.ConversationMessages.Add(message);
        await _db.SaveChangesAsync(ct);
        return message;
    }

    public async Task<Conversation> MarkReadAsync(Guid conversationId, string readerType, CancellationToken ct = default)
    {
        var normalizedReader = readerType.Trim().ToLowerInvariant();
        if (normalizedReader is not ("customer" or "merchant"))
            throw new BusinessException(400, "已读方角色不正确");

        var conversation = await GetByIdAsync(conversationId, ct);
        if (normalizedReader == "customer") conversation.CustomerUnreadCount = 0;
        if (normalizedReader == "merchant") conversation.MerchantUnreadCount = 0;
        conversation.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return conversation;
    }
}
