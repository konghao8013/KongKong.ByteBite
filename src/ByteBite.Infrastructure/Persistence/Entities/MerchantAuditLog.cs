using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 商家审核记录表
/// </summary>
public partial class MerchantAuditLog
{
    public Guid Id { get; set; }

    public Guid MerchantId { get; set; }

    public Guid AdminId { get; set; }

    /// <summary>
    /// 操作类型：approve-审核通过, reject-审核拒绝, disable-禁用, enable-解禁, delete-删除
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// 操作原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 操作前状态
    /// </summary>
    public string? PreviousStatus { get; set; }

    /// <summary>
    /// 操作后状态
    /// </summary>
    public string? NewStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Admin Admin { get; set; } = null!;

    public virtual Merchant Merchant { get; set; } = null!;
}
