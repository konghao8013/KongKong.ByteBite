using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 模板套餐子商品表
/// </summary>
public partial class TemplateComboItem
{
    public Guid Id { get; set; }

    public Guid ComboTemplateProductId { get; set; }

    public Guid TemplateProductId { get; set; }

    public int Quantity { get; set; }

    public string? Remark { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TemplateProduct ComboTemplateProduct { get; set; } = null!;

    public virtual TemplateProduct TemplateProduct { get; set; } = null!;
}
