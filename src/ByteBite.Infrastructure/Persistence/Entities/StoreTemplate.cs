using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 商家模板表
/// </summary>
public partial class StoreTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid? IndustryCategoryId { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// 创建方式：manual-从零创建, from_store-从商家引入, combined-多商家组合
    /// </summary>
    public string SourceType { get; set; } = null!;

    /// <summary>
    /// 来源商家ID列表（逗号分隔，从商家引入/组合时记录）
    /// </summary>
    public string? SourceStoreIds { get; set; }

    public string Status { get; set; } = null!;

    /// <summary>
    /// 被应用次数
    /// </summary>
    public int UseCount { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Admin? CreatedByNavigation { get; set; }

    public virtual IndustryCategory? IndustryCategory { get; set; }

    public virtual ICollection<TemplateCategory> TemplateCategories { get; set; } = new List<TemplateCategory>();

    public virtual ICollection<TemplateProduct> TemplateProducts { get; set; } = new List<TemplateProduct>();
}
