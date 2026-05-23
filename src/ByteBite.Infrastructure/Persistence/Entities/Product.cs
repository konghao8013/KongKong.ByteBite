using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 商品表
/// </summary>
public partial class Product
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// 基础价格（默认规格价格）
    /// </summary>
    public decimal BasePrice { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>
    /// 状态：on-上架, off-下架, sold_out-售罄
    /// </summary>
    public string Status { get; set; } = null!;

    public int SortOrder { get; set; }

    /// <summary>
    /// 最低起购数量
    /// </summary>
    public int MinOrderQty { get; set; }

    /// <summary>
    /// 月销量（冗余，定时统计）
    /// </summary>
    public int MonthlySales { get; set; }

    /// <summary>
    /// 总销量
    /// </summary>
    public int TotalSales { get; set; }

    /// <summary>
    /// 是否为套餐商品
    /// </summary>
    public bool IsCombo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ComboItem> ComboItemComboProducts { get; set; } = new List<ComboItem>();

    public virtual ICollection<ComboItem> ComboItemProducts { get; set; } = new List<ComboItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductSalesStat> ProductSalesStats { get; set; } = new List<ProductSalesStat>();

    public virtual ICollection<SpecGroup> SpecGroups { get; set; } = new List<SpecGroup>();

    public virtual Store Store { get; set; } = null!;
}
