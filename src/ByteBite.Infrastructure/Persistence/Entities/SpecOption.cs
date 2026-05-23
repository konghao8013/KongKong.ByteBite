using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 规格项表（如：小份+0, 大份+10）
/// </summary>
public partial class SpecOption
{
    public Guid Id { get; set; }

    public Guid SpecGroupId { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>
    /// 加价（可为负数，如罐装比瓶装便宜）
    /// </summary>
    public decimal ExtraPrice { get; set; }

    /// <summary>
    /// 库存（null表示不限库存）
    /// </summary>
    public int? Stock { get; set; }

    public int SortOrder { get; set; }

    /// <summary>
    /// 是否为默认选中项
    /// </summary>
    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SpecGroup SpecGroup { get; set; } = null!;
}
