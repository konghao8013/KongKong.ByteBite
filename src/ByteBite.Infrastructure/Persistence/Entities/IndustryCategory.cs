using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 行业分类表（三级）
/// </summary>
public partial class IndustryCategory
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>
    /// 层级：1-一级行业(餐饮), 2-二级行业(烧烤), 3-三级行业(重庆特色烧烤)
    /// </summary>
    public int Level { get; set; }

    public int SortOrder { get; set; }

    public string? Icon { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<IndustryCategory> InverseParent { get; set; } = new List<IndustryCategory>();

    public virtual IndustryCategory? Parent { get; set; }

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();

    public virtual ICollection<StoreTemplate> StoreTemplates { get; set; } = new List<StoreTemplate>();
}
