using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 数据导出记录表
/// </summary>
public partial class DataExportLog
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid MerchantId { get; set; }

    /// <summary>
    /// 导出类型：product_sales-商品销售, customer_consumption-顾客消费, revenue_trend-营收趋势, order_trend-订单趋势, discount_effect-优惠效果
    /// </summary>
    public string ExportType { get; set; } = null!;

    public DateOnly? DateRangeStart { get; set; }

    public DateOnly? DateRangeEnd { get; set; }

    /// <summary>
    /// 文件格式：csv, excel, pdf
    /// </summary>
    public string FileFormat { get; set; } = null!;

    /// <summary>
    /// 导出文件路径
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// 导出状态：pending-待处理, processing-处理中, completed-已完成, failed-失败
    /// </summary>
    public string Status { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
