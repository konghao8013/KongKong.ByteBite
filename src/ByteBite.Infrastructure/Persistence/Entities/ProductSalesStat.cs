using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 商品销售统计表
/// </summary>
public partial class ProductSalesStat
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? CategoryId { get; set; }

    public DateOnly StatDate { get; set; }

    /// <summary>
    /// 销售数量
    /// </summary>
    public int SalesQuantity { get; set; }

    /// <summary>
    /// 销售金额
    /// </summary>
    public decimal SalesAmount { get; set; }

    /// <summary>
    /// 包含该商品的订单数
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// 被取消订单中包含该商品的次数
    /// </summary>
    public int CancelCount { get; set; }

    /// <summary>
    /// 平均单价
    /// </summary>
    public decimal AvgUnitPrice { get; set; }

    /// <summary>
    /// 规格分布（JSON，如{&quot;小份&quot;:40,&quot;大份&quot;:60}）
    /// </summary>
    public string? SpecDistribution { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
