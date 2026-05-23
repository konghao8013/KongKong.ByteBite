using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 订单服务 - 处理订单创建、状态流转（接单/拒单/制作/完成/取消）等业务逻辑
/// </summary>
public class OrderService
{
    private readonly ByteBiteDbContext _db;

    public OrderService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 创建订单 - 生成取货码，计算金额，创建订单及订单项
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="customerId">顾客ID（可选，匿名下单为空）</param>
    /// <param name="deviceId">设备ID（匿名下单时使用）</param>
    /// <param name="diningMode">就餐方式：dine_in/pack/takeout</param>
    /// <param name="items">订单项列表</param>
    /// <param name="remark">整单备注（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新创建的订单实体（含订单项）</returns>
    /// <exception cref="BusinessException">404-店铺不存在</exception>
    public async Task<Order> CreateOrderAsync(Guid storeId, Guid? customerId, string? deviceId, string diningMode, List<OrderItem> items, string? remark, CancellationToken ct = default)
    {
        var store = await _db.Stores.FindAsync([storeId], ct) ?? throw new BusinessException(404, "店铺不存在");
        // 生成唯一取货码，确保同店铺内不重复
        string pickupCode;
        do { pickupCode = PickupCodeGenerator.Generate(4); } while (await _db.Orders.AnyAsync(o => o.PickupCode == pickupCode && o.StoreId == storeId, ct));
        var totalAmount = items.Sum(i => i.TotalPrice);
        var order = new Order
        {
            Id = Guid.NewGuid(), StoreId = storeId, CustomerId = customerId, DeviceId = deviceId,
            OrderNo = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999),
            PickupCode = pickupCode, DiningMode = diningMode, Status = "pending",
            TotalAmount = totalAmount, DiscountAmount = 0, ActualAmount = totalAmount, PackingFee = 0,
            Remark = remark, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        // 初始化订单项ID和关联关系
        foreach (var item in items) { item.Id = Guid.NewGuid(); item.OrderId = order.Id; item.CreatedAt = DateTime.UtcNow; }
        order.OrderItems = items;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// 根据取货码查询订单 - 顾客凭取货码查看订单状态
    /// </summary>
    /// <param name="pickupCode">取货码</param>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>订单实体（含订单项）</returns>
    /// <exception cref="BusinessException">404-订单不存在</exception>
    public async Task<Order> GetByPickupCodeAsync(string pickupCode, Guid storeId, CancellationToken ct = default)
        => await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.PickupCode == pickupCode && o.StoreId == storeId, ct) ?? throw new BusinessException(404, "订单不存在");

    /// <summary>
    /// 获取店铺订单列表 - 支持按状态筛选和分页
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="status">订单状态筛选（可选）</param>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>订单列表（含订单项）</returns>
    public async Task<List<Order>> GetByStoreIdAsync(Guid storeId, string? status, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Orders.Include(o => o.OrderItems).Where(o => o.StoreId == storeId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
        return await query.OrderByDescending(o => o.CreatedAt).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    /// <summary>
    /// 接单 - 商家确认接单，状态从pending变为accepted
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的订单实体</returns>
    /// <exception cref="BusinessException">404-订单不存在, 400-订单状态不允许接单</exception>
    public async Task<Order> AcceptOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        // 只有待接单状态才能接单
        if (order.Status != "pending") throw new BusinessException(400, "订单状态不允许接单");
        order.Status = "accepted"; order.AcceptedAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// 拒单 - 商家拒绝订单，需填写拒单原因
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="reason">拒单原因</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的订单实体</returns>
    /// <exception cref="BusinessException">404-订单不存在, 400-订单状态不允许拒单</exception>
    public async Task<Order> RejectOrderAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "pending") throw new BusinessException(400, "订单状态不允许拒单");
        order.Status = "rejected"; order.RejectReason = reason; order.CancelledAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// 开始制作 - 状态从accepted变为preparing
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的订单实体</returns>
    /// <exception cref="BusinessException">404-订单不存在, 400-订单状态不允许此操作</exception>
    public async Task<Order> StartPreparingAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "accepted") throw new BusinessException(400, "订单状态不允许此操作");
        order.Status = "preparing"; order.PreparingAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// 标记待取餐 - 制作完成，状态从preparing变为ready
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的订单实体</returns>
    /// <exception cref="BusinessException">404-订单不存在, 400-订单状态不允许此操作</exception>
    public async Task<Order> MarkReadyAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "preparing") throw new BusinessException(400, "订单状态不允许此操作");
        order.Status = "ready"; order.ReadyAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// 完成订单 - 顾客取餐后核销，状态从ready变为completed
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的订单实体</returns>
    /// <exception cref="BusinessException">404-订单不存在, 400-订单状态不允许此操作</exception>
    public async Task<Order> CompleteOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        if (order.Status != "ready") throw new BusinessException(400, "订单状态不允许此操作");
        order.Status = "completed"; order.CompletedAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }

    /// <summary>
    /// 取消订单 - 已完成或已取消的订单不可取消
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的订单实体</returns>
    /// <exception cref="BusinessException">404-订单不存在, 400-订单状态不允许取消</exception>
    public async Task<Order> CancelOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([orderId], ct) ?? throw new BusinessException(404, "订单不存在");
        // 已完成或已取消的订单不可取消
        if (order.Status is "completed" or "cancelled") throw new BusinessException(400, "订单状态不允许取消");
        order.Status = "cancelled"; order.CancelledAt = DateTime.UtcNow; order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return order;
    }
}
