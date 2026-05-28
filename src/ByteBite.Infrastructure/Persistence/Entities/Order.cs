using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ByteBite.Shared.Helpers;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 订单表
/// </summary>
public partial class Order
{
    public Guid Id { get; set; }

    /// <summary>
    /// 订单编号（如：20260520001）
    /// </summary>
    public string OrderNo { get; set; } = null!;

    public Guid StoreId { get; set; }

    public Guid? CustomerId { get; set; }

    public string? DeviceId { get; set; }

    /// <summary>
    /// 取货码整数码值，页面以 6 位 Base36 展示。
    /// </summary>
    public int PickupCodeValue { get; set; }

    [NotMapped]
    public string PickupCode => PickupCodeGenerator.ToDisplayCode(PickupCodeValue);

    /// <summary>
    /// 就餐方式：dine_in-堂食, takeaway-打包, delivery-外卖
    /// </summary>
    public string DiningMode { get; set; } = null!;

    public string? TableNo { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? DeliveryPhone { get; set; }

    /// <summary>
    /// 商品合计金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 优惠减免金额
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 应付金额 = total - discount + packing_fee
    /// </summary>
    public decimal ActualAmount { get; set; }

    public decimal PackingFee { get; set; }

    public Guid? DiscountRuleId { get; set; }

    public string? Remark { get; set; }

    /// <summary>
    /// 状态：pending-待接单, accepted-已接单, preparing-制作中, ready-待取餐, completed-已完成, cancelled-已取消
    /// </summary>
    public string Status { get; set; } = null!;

    public string? RejectReason { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? PreparingAt { get; set; }

    public DateTime? ReadyAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual DiscountRule? DiscountRule { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Store Store { get; set; } = null!;
}
