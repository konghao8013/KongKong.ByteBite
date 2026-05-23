using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 模板分类表
/// </summary>
public partial class TemplateCategory
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }

    public string Name { get; set; } = null!;

    public string CategoryType { get; set; } = null!;

    public string? Icon { get; set; }

    public int SortOrder { get; set; }

    public int? HotTopCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual StoreTemplate Template { get; set; } = null!;

    public virtual ICollection<TemplateProduct> TemplateProducts { get; set; } = new List<TemplateProduct>();
}
