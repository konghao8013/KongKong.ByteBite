using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 商品分类表
/// </summary>
public partial class Category
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public string Name { get; set; } = null!;

    public string? Icon { get; set; }

    /// <summary>
    /// 分类类型：normal-普通, hot-热销, welfare-进店福利, combo-套餐
    /// </summary>
    public string CategoryType { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; }

    /// <summary>
    /// 热销分类自动聚合的Top数量（仅hot类型有效）
    /// </summary>
    public int? HotTopCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ProductSalesStat> ProductSalesStats { get; set; } = new List<ProductSalesStat>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual Store Store { get; set; } = null!;
}
