using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 模板规格项表
/// </summary>
public partial class TemplateSpecOption
{
    public Guid Id { get; set; }

    public Guid TemplateSpecGroupId { get; set; }

    public string Name { get; set; } = null!;

    public decimal ExtraPrice { get; set; }

    public int SortOrder { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TemplateSpecGroup TemplateSpecGroup { get; set; } = null!;
}
