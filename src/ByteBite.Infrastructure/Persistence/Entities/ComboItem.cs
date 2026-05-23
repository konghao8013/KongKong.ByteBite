using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 套餐子商品表
/// </summary>
public partial class ComboItem
{
    public Guid Id { get; set; }

    public Guid ComboProductId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// 默认规格项ID列表（逗号分隔）
    /// </summary>
    public string? DefaultSpecOptionIds { get; set; }

    /// <summary>
    /// 是否允许顾客替换规格
    /// </summary>
    public bool AllowChangeSpec { get; set; }

    public string? Remark { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product ComboProduct { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
