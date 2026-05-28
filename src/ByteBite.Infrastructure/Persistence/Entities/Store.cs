using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 店铺表
/// </summary>
public partial class Store
{
    public Guid Id { get; set; }

    /// <summary>
    /// 所属商家ID
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// 店铺名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 店铺码（6位Base36编码，唯一标识，用于短链分享）
    /// </summary>
    public string StoreCode { get; set; } = null!;

    /// <summary>
    /// 店铺描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 封面图URL
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// 店铺二维码URL
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// 营业状态：open-营业中, closed-休息中
    /// </summary>
    public string BusinessStatus { get; set; } = null!;

    /// <summary>
    /// 营业开始时间
    /// </summary>
    public TimeOnly? BusinessHoursStart { get; set; }

    /// <summary>
    /// 营业结束时间
    /// </summary>
    public TimeOnly? BusinessHoursEnd { get; set; }

    /// <summary>
    /// 行业分类ID（关联模板系统）
    /// </summary>
    public Guid? IndustryCategoryId { get; set; }

    /// <summary>
    /// 就餐方式：逗号分隔 dine_in-堂食,takeaway-打包,delivery-外卖
    /// </summary>
    public string DiningMode { get; set; } = null!;

    /// <summary>
    /// 外卖最低消费金额
    /// </summary>
    public decimal? DeliveryMinAmount { get; set; }

    /// <summary>
    /// 打包费
    /// </summary>
    public decimal? PackingFee { get; set; }

    /// <summary>
    /// 月销量（冗余字段，定时统计更新）
    /// </summary>
    public int MonthlySales { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<CustomerConsumptionStat> CustomerConsumptionStats { get; set; } = new List<CustomerConsumptionStat>();

    public virtual ICollection<DailyStoreStat> DailyStoreStats { get; set; } = new List<DailyStoreStat>();

    public virtual ICollection<DataExportLog> DataExportLogs { get; set; } = new List<DataExportLog>();

    public virtual ICollection<DiscountEffectStat> DiscountEffectStats { get; set; } = new List<DiscountEffectStat>();

    public virtual ICollection<DiscountRule> DiscountRules { get; set; } = new List<DiscountRule>();

    public virtual ICollection<HourlyOrderStat> HourlyOrderStats { get; set; } = new List<HourlyOrderStat>();

    public virtual IndustryCategory? IndustryCategory { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductSalesStat> ProductSalesStats { get; set; } = new List<ProductSalesStat>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
