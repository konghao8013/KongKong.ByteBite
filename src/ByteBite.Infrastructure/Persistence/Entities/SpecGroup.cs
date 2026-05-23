using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 规格组表（如：份量、辣度）
/// </summary>
public partial class SpecGroup
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }

    /// <summary>
    /// 是否必选
    /// </summary>
    public bool IsRequired { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<SpecOption> SpecOptions { get; set; } = new List<SpecOption>();
}
