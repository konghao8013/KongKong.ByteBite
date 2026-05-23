using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 订单项表
/// </summary>
public partial class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductImage { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    /// <summary>
    /// 规格快照（JSON，记录下单时的规格选择）
    /// </summary>
    public string? SpecSnapshot { get; set; }

    public string? Remark { get; set; }

    public bool IsCombo { get; set; }

    /// <summary>
    /// 套餐子商品快照（JSON，记录套餐内容）
    /// </summary>
    public string? ComboItemsSnapshot { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
