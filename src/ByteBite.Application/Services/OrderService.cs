using System.Text.Json;
using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 订单服务 - 处理订单创建、状态流转和金额计算。
/// </summary>
public class OrderService
{
    private readonly ByteBiteDbContext _db;

    public OrderService(ByteBiteDbContext db) { _db = db; }

    public async Task<Order> CreateOrderAsync(Guid storeId, Guid? customerId, string? deviceId, string diningMode, List<CreateOrderItemInput> items, string? remark, string? tableNo = null, string? deliveryAddress = null, string? deliveryPhone = null, CancellationToken ct = default)
    {
        if (items.Count == 0) throw new BusinessException(400, "订单商品不能为空");

        var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == storeId && s.DeletedAt == null, ct)
            ?? throw new BusinessException(404, "店铺不存在");
        if (store.BusinessStatus != "open") throw new BusinessException(400, "商家休息中，暂不可下单");

        var normalizedDiningMode = string.IsNullOrWhiteSpace(diningMode) ? "dine_in" : diningMode.Trim();
        var allowedDiningModes = store.DiningMode
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!allowedDiningModes.Contains(normalizedDiningMode))
            throw new BusinessException(400, "当前店铺不支持该就餐方式");

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Include(p => p.SpecGroups)
            .ThenInclude(sg => sg.SpecOptions)
            .Where(p => productIds.Contains(p.Id) && p.StoreId == storeId && p.DeletedAt == null)
            .ToDictionaryAsync(p => p.Id, ct);
        if (products.Count != productIds.Count) throw new BusinessException(400, "订单中包含不存在的商品");

        var orderItems = new List<OrderItem>();
        foreach (var item in items)
        {
            if (item.Quantity <= 0) throw new BusinessException(400, "商品数量必须大于0");
            var product = products[item.ProductId];
            if (product.Status != "on") throw new BusinessException(400, $"{product.Name}暂不可购买");
            if (item.Quantity < Math.Max(1, product.MinOrderQty))
                throw new BusinessException(400, $"{product.Name}至少{product.MinOrderQty}份起购");

            var selectedOptionIds = item.SelectedSpecOptionIds.Distinct().ToHashSet();
            var productOptionIds = product.SpecGroups.SelectMany(sg => sg.SpecOptions).Select(o => o.Id).ToHashSet();
            if (selectedOptionIds.Any(id => !productOptionIds.Contains(id)))
                throw new BusinessException(400, $"{product.Name}包含无效规格");

            foreach (var group in product.SpecGroups.Where(g => g.IsRequired))
            {
                if (!group.SpecOptions.Any(option => selectedOptionIds.Contains(option.Id)))
                    throw new BusinessException(400, $"{product.Name}请选择{group.Name}");
            }

            var selectedOptions = product.SpecGroups
                .SelectMany(group => group.SpecOptions.Select(option => new { Group = group, Option = option }))
                .Where(x => selectedOptionIds.Contains(x.Option.Id))
                .ToList();
            var unitPrice = product.BasePrice + selectedOptions.Sum(x => x.Option.ExtraPrice);
            var totalPrice = unitPrice * item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImage = product.ImageUrl,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                SpecSnapshot = BuildSpecSnapshot(selectedOptions.Select(x => new SpecSnapshotItem(x.Group.Id, x.Group.Name, x.Option.Id, x.Option.Name, x.Option.ExtraPrice))),
                Remark = item.Remark,
                IsCombo = product.IsCombo
            });
        }

        var totalAmount = orderItems.Sum(i => i.TotalPrice);
        if (normalizedDiningMode == "delivery" && store.DeliveryMinAmount.GetValueOrDefault() > totalAmount)
            throw new BusinessException(400, $"外卖满{store.DeliveryMinAmount!.Value:0.##}元起送");

        var packingFee = normalizedDiningMode is "takeaway" or "delivery" ? store.PackingFee.GetValueOrDefault() : 0m;
        var (discountAmount, discountRule) = await CalculateBestDiscountAsync(storeId, orderItems, products, totalAmount, ct);
        var actualAmount = Math.Max(0, totalAmount - discountAmount + packingFee);

        string pickupCode;
        do { pickupCode = PickupCodeGenerator.Generate(); } while (await _db.Orders.AnyAsync(o => o.PickupCode == pickupCode && o.StoreId == storeId, ct));

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            CustomerId = customerId,
            DeviceId = deviceId,
            OrderNo = $"{now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}",
            PickupCode = pickupCode,
            DiningMode = normalizedDiningMode,
            Status = "pending",
            TableNo = tableNo,
            DeliveryAddress = deliveryAddress,
            DeliveryPhone = deliveryPhone,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            ActualAmount = actualAmount,
            PackingFee = packingFee,
            DiscountRuleId = discountRule?.Id,
            Remark = remark,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var item in orderItems)
        {
            item.Id = Guid.NewGuid();
            item.OrderId = order.Id;
            item.CreatedAt = now;
        }
        order.OrderItems = orderItems;

        if (discountRule != null) discountRule.UsedCount++;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> GetByPickupCodeAsync(string pickupCode, Guid storeId, CancellationToken ct = default)
        => await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.PickupCode == pickupCode && o.StoreId == storeId, ct) ?? throw new BusinessException(404, "订单不存在");

    public async Task<List<Order>> GetByStoreIdAsync(Guid storeId, string? status, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Orders.Include(o => o.OrderItems).Where(o => o.StoreId == storeId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
        return await query.OrderByDescending(o => o.CreatedAt).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(Guid storeId, string? deviceId, Guid? customerId, int pageSize, CancellationToken ct = default)
    {
        if (customerId == null && string.IsNullOrWhiteSpace(deviceId))
            throw new BusinessException(400, "缺少顾客身份信息");

        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Store)
            .Where(o => o.StoreId == storeId);

        if (customerId != null && !string.IsNullOrWhiteSpace(deviceId))
        {
            query = query.Where(o => o.CustomerId == customerId || o.DeviceId == deviceId);
        }
        else if (customerId != null)
        {
            query = query.Where(o => o.CustomerId == customerId);
        }
        else
        {
            query = query.Where(o => o.DeviceId == deviceId);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Order> AcceptOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "pending") throw new BusinessException(400, "订单状态不允许接单");
        order.Status = "accepted"; order.AcceptedAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> RejectOrderAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "pending") throw new BusinessException(400, "订单状态不允许拒单");
        order.Status = "rejected"; order.RejectReason = reason; order.CancelledAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> StartPreparingAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "accepted") throw new BusinessException(400, "订单状态不允许此操作");
        order.Status = "preparing"; order.PreparingAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> MarkReadyAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "preparing") throw new BusinessException(400, "订单状态不允许此操作");
        order.Status = "ready"; order.ReadyAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> CompleteOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "ready") throw new BusinessException(400, "订单状态不允许此操作");
        order.Status = "completed"; order.CompletedAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order> CancelOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status is "completed" or "cancelled" or "rejected") throw new BusinessException(400, "订单状态不允许取消");
        order.Status = "cancelled"; order.CancelledAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    private async Task<(decimal Amount, DiscountRule? Rule)> CalculateBestDiscountAsync(Guid storeId, List<OrderItem> orderItems, Dictionary<Guid, Product> products, decimal totalAmount, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var activeRules = await _db.DiscountRules
            .Where(d => d.StoreId == storeId && d.Status == "active" && d.DeletedAt == null && d.StartTime <= now && d.EndTime >= now)
            .ToListAsync(ct);

        DiscountRule? bestRule = null;
        var bestAmount = 0m;
        foreach (var rule in activeRules)
        {
            var eligibleAmount = GetEligibleAmount(rule, orderItems, products);
            var amount = rule.DiscountType switch
            {
                "full_reduction" when eligibleAmount >= rule.ThresholdAmount.GetValueOrDefault() => rule.DiscountAmount.GetValueOrDefault(),
                "discount" when rule.DiscountRate.HasValue => eligibleAmount * (100m - rule.DiscountRate.Value) / 100m,
                _ => 0m
            };
            amount = Math.Round(Math.Min(amount, totalAmount), 2, MidpointRounding.AwayFromZero);
            if (amount > bestAmount)
            {
                bestAmount = amount;
                bestRule = rule;
            }
        }

        return (bestAmount, bestRule);
    }

    private static decimal GetEligibleAmount(DiscountRule rule, List<OrderItem> orderItems, Dictionary<Guid, Product> products)
        => rule.ApplyScope switch
        {
            "category" when rule.ApplyScopeId.HasValue => orderItems.Where(i => products[i.ProductId].CategoryId == rule.ApplyScopeId.Value).Sum(i => i.TotalPrice),
            "product" when rule.ApplyScopeId.HasValue => orderItems.Where(i => i.ProductId == rule.ApplyScopeId.Value).Sum(i => i.TotalPrice),
            _ => orderItems.Sum(i => i.TotalPrice)
        };

    private static string? BuildSpecSnapshot(IEnumerable<SpecSnapshotItem> specs)
    {
        var selected = specs.ToList();
        return selected.Count == 0 ? null : JsonSerializer.Serialize(selected);
    }
}

public sealed class CreateOrderItemInput
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public List<Guid> SelectedSpecOptionIds { get; set; } = [];
    public string? Remark { get; set; }
}

public sealed record SpecSnapshotItem(Guid SpecGroupId, string SpecGroupName, Guid OptionId, string OptionName, decimal ExtraPrice);
