using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 模板规格组表
/// </summary>
public partial class TemplateSpecGroup
{
    public Guid Id { get; set; }

    public Guid TemplateProductId { get; set; }

    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsRequired { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TemplateProduct TemplateProduct { get; set; } = null!;

    public virtual ICollection<TemplateSpecOption> TemplateSpecOptions { get; set; } = new List<TemplateSpecOption>();
}
