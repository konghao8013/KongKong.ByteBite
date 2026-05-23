using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 模板商品表
/// </summary>
public partial class TemplateProduct
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }

    public Guid TemplateCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// 参考价格（市场常见价）
    /// </summary>
    public decimal ReferencePrice { get; set; }

    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }

    public int MinOrderQty { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual StoreTemplate Template { get; set; } = null!;

    public virtual TemplateCategory TemplateCategory { get; set; } = null!;

    public virtual ICollection<TemplateComboItem> TemplateComboItemComboTemplateProducts { get; set; } = new List<TemplateComboItem>();

    public virtual ICollection<TemplateComboItem> TemplateComboItemTemplateProducts { get; set; } = new List<TemplateComboItem>();

    public virtual ICollection<TemplateSpecGroup> TemplateSpecGroups { get; set; } = new List<TemplateSpecGroup>();
}
