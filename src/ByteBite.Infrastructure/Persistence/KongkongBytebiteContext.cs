using System;
using System.Collections.Generic;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Infrastructure.Persistence;

public partial class KongkongBytebiteContext : DbContext
{
    public KongkongBytebiteContext(DbContextOptions<KongkongBytebiteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<AdminOperationLog> AdminOperationLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ComboItem> ComboItems { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerConsumptionStat> CustomerConsumptionStats { get; set; }

    public virtual DbSet<CustomerSession> CustomerSessions { get; set; }

    public virtual DbSet<DailyStoreStat> DailyStoreStats { get; set; }

    public virtual DbSet<DataExportLog> DataExportLogs { get; set; }

    public virtual DbSet<DataMergeLog> DataMergeLogs { get; set; }

    public virtual DbSet<DiscountEffectStat> DiscountEffectStats { get; set; }

    public virtual DbSet<DiscountRule> DiscountRules { get; set; }

    public virtual DbSet<HourlyOrderStat> HourlyOrderStats { get; set; }

    public virtual DbSet<IndustryCategory> IndustryCategories { get; set; }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<MerchantAuditLog> MerchantAuditLogs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<PlatformDailyStat> PlatformDailyStats { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductSalesStat> ProductSalesStats { get; set; }

    public virtual DbSet<SpecGroup> SpecGroups { get; set; }

    public virtual DbSet<SpecOption> SpecOptions { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreTemplate> StoreTemplates { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<TemplateCategory> TemplateCategories { get; set; }

    public virtual DbSet<TemplateComboItem> TemplateComboItems { get; set; }

    public virtual DbSet<TemplateProduct> TemplateProducts { get; set; }

    public virtual DbSet<TemplateSpecGroup> TemplateSpecGroups { get; set; }

    public virtual DbSet<TemplateSpecOption> TemplateSpecOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admins_pkey");

            entity.ToTable("admins", tb => tb.HasComment("平台管理员表"));

            entity.HasIndex(e => e.Username, "uq_admins_username").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(50)
                .HasColumnName("display_name");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'admin'::character varying")
                .HasComment("角色：super_admin-超级管理员, admin-管理员, viewer-只读")
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AdminOperationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_operation_logs_pkey");

            entity.ToTable("admin_operation_logs", tb => tb.HasComment("管理员操作日志表"));

            entity.HasIndex(e => e.AdminId, "ix_admin_operation_logs_admin_id");

            entity.HasIndex(e => e.CreatedAt, "ix_admin_operation_logs_created_at");

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "ix_admin_operation_logs_target");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Detail)
                .HasComment("操作详情（JSON）")
                .HasColumnType("jsonb")
                .HasColumnName("detail");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasComment("操作者IP地址")
                .HasColumnName("ip_address");
            entity.Property(e => e.Operation)
                .HasMaxLength(50)
                .HasComment("操作类型")
                .HasColumnName("operation");
            entity.Property(e => e.TargetId)
                .HasComment("操作对象ID")
                .HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .HasComment("操作对象类型：merchant, store, template, config等")
                .HasColumnName("target_type");

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminOperationLogs)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_admin_operation_logs_admin");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories", tb => tb.HasComment("商品分类表"));

            entity.HasIndex(e => new { e.StoreId, e.SortOrder }, "ix_categories_sort_order");

            entity.HasIndex(e => e.StoreId, "ix_categories_store_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CategoryType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'normal'::character varying")
                .HasComment("分类类型：normal-普通, hot-热销, welfare-进店福利, combo-套餐")
                .HasColumnName("category_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.HotTopCount)
                .HasDefaultValue(10)
                .HasComment("热销分类自动聚合的Top数量（仅hot类型有效）")
                .HasColumnName("hot_top_count");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(true)
                .HasColumnName("is_visible");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.Categories)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_categories_store");
        });

        modelBuilder.Entity<ComboItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("combo_items_pkey");

            entity.ToTable("combo_items", tb => tb.HasComment("套餐子商品表"));

            entity.HasIndex(e => e.ComboProductId, "ix_combo_items_combo_product_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AllowChangeSpec)
                .HasComment("是否允许顾客替换规格")
                .HasColumnName("allow_change_spec");
            entity.Property(e => e.ComboProductId).HasColumnName("combo_product_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DefaultSpecOptionIds)
                .HasMaxLength(500)
                .HasComment("默认规格项ID列表（逗号分隔）")
                .HasColumnName("default_spec_option_ids");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.Remark)
                .HasMaxLength(100)
                .HasColumnName("remark");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ComboProduct).WithMany(p => p.ComboItemComboProducts)
                .HasForeignKey(d => d.ComboProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_combo_items_combo");

            entity.HasOne(d => d.Product).WithMany(p => p.ComboItemProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_combo_items_product");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customers_pkey");

            entity.ToTable("customers", tb => tb.HasComment("顾客用户表"));

            entity.HasIndex(e => e.DeviceId, "ix_customers_device_id");

            entity.HasIndex(e => e.Phone, "ix_customers_phone");

            entity.HasIndex(e => e.Phone, "uq_customers_phone").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasComment("设备标识（匿名用户）")
                .HasColumnName("device_id");
            entity.Property(e => e.IsRegistered)
                .HasComment("是否已注册：true-已注册用户, false-匿名用户")
                .HasColumnName("is_registered");
            entity.Property(e => e.LastLoginAt)
                .HasComment("最后登录时间")
                .HasColumnName("last_login_at");
            entity.Property(e => e.Nickname)
                .HasMaxLength(50)
                .HasColumnName("nickname");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .HasComment("BCrypt加密密码（注册用户才有）")
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasComment("手机号（注册后才有）")
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<CustomerConsumptionStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_consumption_stats_pkey");

            entity.ToTable("customer_consumption_stats", tb => tb.HasComment("顾客消费统计表"));

            entity.HasIndex(e => e.CustomerId, "ix_customer_consumption_stats_customer_id");

            entity.HasIndex(e => e.DeviceId, "ix_customer_consumption_stats_device_id");

            entity.HasIndex(e => e.StoreId, "ix_customer_consumption_stats_store_id");

            entity.HasIndex(e => new { e.StoreId, e.CustomerId, e.DeviceId }, "uq_customer_consumption_stats_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvgOrderAmount)
                .HasPrecision(18, 2)
                .HasComment("平均客单价")
                .HasColumnName("avg_order_amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId)
                .HasComment("顾客ID（注册用户）")
                .HasColumnName("customer_id");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasComment("设备标识（匿名用户）")
                .HasColumnName("device_id");
            entity.Property(e => e.FirstOrderAt)
                .HasComment("首次下单时间")
                .HasColumnName("first_order_at");
            entity.Property(e => e.IsReturning)
                .HasComment("是否为回头客（下单>=2次）")
                .HasColumnName("is_returning");
            entity.Property(e => e.LastOrderAt)
                .HasComment("最近下单时间")
                .HasColumnName("last_order_at");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TopProducts)
                .HasComment("常购商品Top3（JSON）")
                .HasColumnType("jsonb")
                .HasColumnName("top_products");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasComment("在当前店铺的总消费金额")
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalOrders)
                .HasComment("在当前店铺的总订单数")
                .HasColumnName("total_orders");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerConsumptionStats)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("fk_customer_consumption_stats_customer");

            entity.HasOne(d => d.Store).WithMany(p => p.CustomerConsumptionStats)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customer_consumption_stats_store");
        });

        modelBuilder.Entity<CustomerSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_sessions_pkey");

            entity.ToTable("customer_sessions", tb => tb.HasComment("顾客会话表 - 记录设备与账号关联、缓存数据"));

            entity.HasIndex(e => e.CustomerId, "ix_customer_sessions_customer_id");

            entity.HasIndex(e => e.DeviceId, "ix_customer_sessions_device_id");

            entity.HasIndex(e => e.Token, "ix_customer_sessions_token");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActiveOrders)
                .HasComment("进行中订单列表（JSON）")
                .HasColumnType("jsonb")
                .HasColumnName("active_orders");
            entity.Property(e => e.CartData)
                .HasComment("购物车数据（JSON，按店铺分组）")
                .HasColumnType("jsonb")
                .HasColumnName("cart_data");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId)
                .HasComment("关联的顾客ID")
                .HasColumnName("customer_id");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasComment("设备标识")
                .HasColumnName("device_id");
            entity.Property(e => e.ExpiresAt)
                .HasComment("会话过期时间")
                .HasColumnName("expires_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasComment("最后登录IP地址")
                .HasColumnName("ip_address");
            entity.Property(e => e.LastCategoryId)
                .HasComment("最后浏览的分类ID")
                .HasColumnName("last_category_id");
            entity.Property(e => e.LastStoreId)
                .HasComment("最后访问的店铺ID")
                .HasColumnName("last_store_id");
            entity.Property(e => e.OrderHistory)
                .HasComment("历史订单列表（JSON）")
                .HasColumnType("jsonb")
                .HasColumnName("order_history");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasComment("登录令牌")
                .HasColumnName("token");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasComment("最后登录浏览器标识")
                .HasColumnName("user_agent");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerSessions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customer_sessions_customer");
        });

        modelBuilder.Entity<DailyStoreStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("daily_store_stats_pkey");

            entity.ToTable("daily_store_stats", tb => tb.HasComment("每日店铺经营统计快照表"));

            entity.HasIndex(e => e.StatDate, "ix_daily_store_stats_stat_date");

            entity.HasIndex(e => e.StoreId, "ix_daily_store_stats_store_id");

            entity.HasIndex(e => new { e.StoreId, e.StatDate }, "uq_daily_store_stats_store_date").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActualRevenue)
                .HasPrecision(18, 2)
                .HasComment("当日实收金额（扣除优惠）")
                .HasColumnName("actual_revenue");
            entity.Property(e => e.AvgOrderAmount)
                .HasPrecision(18, 2)
                .HasComment("当日客单价")
                .HasColumnName("avg_order_amount");
            entity.Property(e => e.CancelledOrders)
                .HasComment("当日已取消订单数")
                .HasColumnName("cancelled_orders");
            entity.Property(e => e.CompletedOrders)
                .HasComment("当日已完成订单数")
                .HasColumnName("completed_orders");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasComment("当日优惠减免总额")
                .HasColumnName("discount_amount");
            entity.Property(e => e.NewCustomers)
                .HasComment("当日新顾客数")
                .HasColumnName("new_customers");
            entity.Property(e => e.PackingFee)
                .HasPrecision(18, 2)
                .HasComment("当日打包费总额")
                .HasColumnName("packing_fee");
            entity.Property(e => e.PeakHour)
                .HasComment("当日高峰时段（0-23）")
                .HasColumnName("peak_hour");
            entity.Property(e => e.PeakHourOrders)
                .HasComment("高峰时段订单数")
                .HasColumnName("peak_hour_orders");
            entity.Property(e => e.ReturningCustomers)
                .HasComment("当日回头客数")
                .HasColumnName("returning_customers");
            entity.Property(e => e.StatDate)
                .HasComment("统计日期")
                .HasColumnName("stat_date");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TopProductId)
                .HasComment("当日最热销商品ID")
                .HasColumnName("top_product_id");
            entity.Property(e => e.TopProductName)
                .HasMaxLength(100)
                .HasComment("当日最热销商品名称")
                .HasColumnName("top_product_name");
            entity.Property(e => e.TopProductQty)
                .HasComment("当日最热销商品销量")
                .HasColumnName("top_product_qty");
            entity.Property(e => e.TotalOrders)
                .HasComment("当日总订单数")
                .HasColumnName("total_orders");
            entity.Property(e => e.TotalRevenue)
                .HasPrecision(18, 2)
                .HasComment("当日总金额（含优惠前）")
                .HasColumnName("total_revenue");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.DailyStoreStats)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_daily_store_stats_store");
        });

        modelBuilder.Entity<DataExportLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("data_export_logs_pkey");

            entity.ToTable("data_export_logs", tb => tb.HasComment("数据导出记录表"));

            entity.HasIndex(e => e.CreatedAt, "ix_data_export_logs_created_at");

            entity.HasIndex(e => e.MerchantId, "ix_data_export_logs_merchant_id");

            entity.HasIndex(e => e.StoreId, "ix_data_export_logs_store_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DateRangeEnd).HasColumnName("date_range_end");
            entity.Property(e => e.DateRangeStart).HasColumnName("date_range_start");
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500)
                .HasColumnName("error_message");
            entity.Property(e => e.ExportType)
                .HasMaxLength(50)
                .HasComment("导出类型：product_sales-商品销售, customer_consumption-顾客消费, revenue_trend-营收趋势, order_trend-订单趋势, discount_effect-优惠效果")
                .HasColumnName("export_type");
            entity.Property(e => e.FileFormat)
                .HasMaxLength(10)
                .HasDefaultValueSql("'csv'::character varying")
                .HasComment("文件格式：csv, excel, pdf")
                .HasColumnName("file_format");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasComment("导出文件路径")
                .HasColumnName("file_path");
            entity.Property(e => e.FileSize)
                .HasComment("文件大小（字节）")
                .HasColumnName("file_size");
            entity.Property(e => e.MerchantId).HasColumnName("merchant_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasComment("导出状态：pending-待处理, processing-处理中, completed-已完成, failed-失败")
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");

            entity.HasOne(d => d.Merchant).WithMany(p => p.DataExportLogs)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_data_export_logs_merchant");

            entity.HasOne(d => d.Store).WithMany(p => p.DataExportLogs)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_data_export_logs_store");
        });

        modelBuilder.Entity<DataMergeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("data_merge_logs_pkey");

            entity.ToTable("data_merge_logs", tb => tb.HasComment("数据合并日志表 - 记录匿名数据合并到注册账号的操作"));

            entity.HasIndex(e => e.CreatedAt, "ix_data_merge_logs_created_at");

            entity.HasIndex(e => e.SourceDeviceId, "ix_data_merge_logs_source_device_id");

            entity.HasIndex(e => e.TargetCustomerId, "ix_data_merge_logs_target_customer_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CartItemsMerged)
                .HasComment("合并的购物车商品数量")
                .HasColumnName("cart_items_merged");
            entity.Property(e => e.ConflictsResolved)
                .HasComment("解决的冲突数量")
                .HasColumnName("conflicts_resolved");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.MergeDetail)
                .HasComment("合并详情（JSON，记录具体合并内容）")
                .HasColumnType("jsonb")
                .HasColumnName("merge_detail");
            entity.Property(e => e.MergeType)
                .HasMaxLength(20)
                .HasComment("合并类型：device_to_account-设备到账号, account_to_account-账号到账号, session_to_account-会话到账号")
                .HasColumnName("merge_type");
            entity.Property(e => e.OrdersMerged)
                .HasComment("合并的订单数量")
                .HasColumnName("orders_merged");
            entity.Property(e => e.PickupCodesMerged)
                .HasComment("合并的取货码数量")
                .HasColumnName("pickup_codes_merged");
            entity.Property(e => e.SourceCustomerId)
                .HasComment("来源匿名顾客ID（如有）")
                .HasColumnName("source_customer_id");
            entity.Property(e => e.SourceDeviceId)
                .HasMaxLength(200)
                .HasComment("来源设备标识")
                .HasColumnName("source_device_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'completed'::character varying")
                .HasComment("合并状态：pending-待处理, completed-已完成, failed-失败, rolled_back-已回滚")
                .HasColumnName("status");
            entity.Property(e => e.TargetCustomerId)
                .HasComment("目标注册顾客ID")
                .HasColumnName("target_customer_id");

            entity.HasOne(d => d.SourceCustomer).WithMany(p => p.DataMergeLogSourceCustomers)
                .HasForeignKey(d => d.SourceCustomerId)
                .HasConstraintName("fk_data_merge_logs_source_customer");

            entity.HasOne(d => d.TargetCustomer).WithMany(p => p.DataMergeLogTargetCustomers)
                .HasForeignKey(d => d.TargetCustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_data_merge_logs_target_customer");
        });

        modelBuilder.Entity<DiscountEffectStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("discount_effect_stats_pkey");

            entity.ToTable("discount_effect_stats", tb => tb.HasComment("优惠活动效果统计表"));

            entity.HasIndex(e => e.DiscountRuleId, "ix_discount_effect_stats_discount_rule_id");

            entity.HasIndex(e => e.StatDate, "ix_discount_effect_stats_stat_date");

            entity.HasIndex(e => e.StoreId, "ix_discount_effect_stats_store_id");

            entity.HasIndex(e => new { e.DiscountRuleId, e.StatDate }, "uq_discount_effect_stats_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvgOrderAmountWithDiscount)
                .HasPrecision(18, 2)
                .HasComment("当日使用优惠的订单平均金额")
                .HasColumnName("avg_order_amount_with_discount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountRuleId).HasColumnName("discount_rule_id");
            entity.Property(e => e.StatDate).HasColumnName("stat_date");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TotalDiscountAmount)
                .HasPrecision(18, 2)
                .HasComment("当日优惠减免总额")
                .HasColumnName("total_discount_amount");
            entity.Property(e => e.TotalDrivenRevenue)
                .HasPrecision(18, 2)
                .HasComment("当日使用优惠的订单总营收")
                .HasColumnName("total_driven_revenue");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsedCount)
                .HasComment("当日使用次数")
                .HasColumnName("used_count");

            entity.HasOne(d => d.DiscountRule).WithMany(p => p.DiscountEffectStats)
                .HasForeignKey(d => d.DiscountRuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_discount_effect_stats_discount_rule");

            entity.HasOne(d => d.Store).WithMany(p => p.DiscountEffectStats)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_discount_effect_stats_store");
        });

        modelBuilder.Entity<DiscountRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("discount_rules_pkey");

            entity.ToTable("discount_rules", tb => tb.HasComment("优惠活动表"));

            entity.HasIndex(e => new { e.StoreId, e.Status }, "ix_discount_rules_status");

            entity.HasIndex(e => e.StoreId, "ix_discount_rules_store_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AllowStack)
                .HasComment("是否允许与其他优惠叠加")
                .HasColumnName("allow_stack");
            entity.Property(e => e.ApplyScope)
                .HasMaxLength(20)
                .HasDefaultValueSql("'all'::character varying")
                .HasComment("适用范围：all-全店, category-指定分类, product-指定商品")
                .HasColumnName("apply_scope");
            entity.Property(e => e.ApplyScopeId)
                .HasComment("适用范围的分类ID或商品ID")
                .HasColumnName("apply_scope_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasComment("满减减免金额（仅满减类型）")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountRate)
                .HasPrecision(5, 2)
                .HasComment("折扣率（如80=8折，仅折扣类型）")
                .HasColumnName("discount_rate");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .HasComment("类型：full_reduction-满减, discount-折扣")
                .HasColumnName("discount_type");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ThresholdAmount)
                .HasPrecision(18, 2)
                .HasComment("满减门槛金额（仅满减类型）")
                .HasColumnName("threshold_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");

            entity.HasOne(d => d.Store).WithMany(p => p.DiscountRules)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_discount_rules_store");
        });

        modelBuilder.Entity<HourlyOrderStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hourly_order_stats_pkey");

            entity.ToTable("hourly_order_stats", tb => tb.HasComment("时段订单统计表 - 按小时统计订单分布"));

            entity.HasIndex(e => e.StatDate, "ix_hourly_order_stats_stat_date");

            entity.HasIndex(e => e.StoreId, "ix_hourly_order_stats_store_id");

            entity.HasIndex(e => new { e.StoreId, e.StatDate, e.Hour }, "uq_hourly_order_stats_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Hour)
                .HasComment("时段（0-23）")
                .HasColumnName("hour");
            entity.Property(e => e.OrderCount)
                .HasComment("该时段订单数")
                .HasColumnName("order_count");
            entity.Property(e => e.Revenue)
                .HasPrecision(18, 2)
                .HasComment("该时段营收")
                .HasColumnName("revenue");
            entity.Property(e => e.StatDate).HasColumnName("stat_date");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.HourlyOrderStats)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hourly_order_stats_store");
        });

        modelBuilder.Entity<IndustryCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("industry_categories_pkey");

            entity.ToTable("industry_categories", tb => tb.HasComment("行业分类表（三级）"));

            entity.HasIndex(e => e.Level, "ix_industry_categories_level");

            entity.HasIndex(e => e.ParentId, "ix_industry_categories_parent_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(true)
                .HasColumnName("is_visible");
            entity.Property(e => e.Level)
                .HasDefaultValue(1)
                .HasComment("层级：1-一级行业(餐饮), 2-二级行业(烧烤), 3-三级行业(重庆特色烧烤)")
                .HasColumnName("level");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("industry_categories_parent_id_fkey");
        });

        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("merchants_pkey");

            entity.ToTable("merchants", tb => tb.HasComment("商家用户表"));

            entity.HasIndex(e => e.Phone, "ix_merchants_phone");

            entity.HasIndex(e => e.Status, "ix_merchants_status");

            entity.HasIndex(e => e.Phone, "uq_merchants_phone").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasComment("主键UUID")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasComment("头像URL")
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasComment("软删除时间")
                .HasColumnName("deleted_at");
            entity.Property(e => e.LastLoginAt)
                .HasComment("最后登录时间")
                .HasColumnName("last_login_at");
            entity.Property(e => e.Nickname)
                .HasMaxLength(50)
                .HasComment("昵称")
                .HasColumnName("nickname");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .HasComment("BCrypt加密密码")
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasComment("手机号，作为登录账号")
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasComment("状态：pending-待审核, active-已激活, disabled-已禁用")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MerchantAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("merchant_audit_logs_pkey");

            entity.ToTable("merchant_audit_logs", tb => tb.HasComment("商家审核记录表"));

            entity.HasIndex(e => e.AdminId, "ix_merchant_audit_logs_admin_id");

            entity.HasIndex(e => e.CreatedAt, "ix_merchant_audit_logs_created_at");

            entity.HasIndex(e => e.MerchantId, "ix_merchant_audit_logs_merchant_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .HasComment("操作类型：approve-审核通过, reject-审核拒绝, disable-禁用, enable-解禁, delete-删除")
                .HasColumnName("action");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.MerchantId).HasColumnName("merchant_id");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(20)
                .HasComment("操作后状态")
                .HasColumnName("new_status");
            entity.Property(e => e.PreviousStatus)
                .HasMaxLength(20)
                .HasComment("操作前状态")
                .HasColumnName("previous_status");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasComment("操作原因")
                .HasColumnName("reason");

            entity.HasOne(d => d.Admin).WithMany(p => p.MerchantAuditLogs)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_merchant_audit_logs_admin");

            entity.HasOne(d => d.Merchant).WithMany(p => p.MerchantAuditLogs)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_merchant_audit_logs_merchant");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders", tb => tb.HasComment("订单表"));

            entity.HasIndex(e => new { e.StoreId, e.CreatedAt }, "ix_orders_created_at");

            entity.HasIndex(e => e.CustomerId, "ix_orders_customer_id");

            entity.HasIndex(e => new { e.PickupCode, e.StoreId }, "ix_orders_pickup_code");

            entity.HasIndex(e => new { e.StoreId, e.Status }, "ix_orders_status");

            entity.HasIndex(e => e.StoreId, "ix_orders_store_id");

            entity.HasIndex(e => e.OrderNo, "uq_orders_order_no").IsUnique();

            entity.HasIndex(e => new { e.PickupCode, e.StoreId }, "uq_orders_pickup_code_store").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AcceptedAt).HasColumnName("accepted_at");
            entity.Property(e => e.ActualAmount)
                .HasPrecision(18, 2)
                .HasComment("应付金额 = total - discount + packing_fee")
                .HasColumnName("actual_amount");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeliveryAddress)
                .HasMaxLength(500)
                .HasColumnName("delivery_address");
            entity.Property(e => e.DeliveryPhone)
                .HasMaxLength(20)
                .HasColumnName("delivery_phone");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("device_id");
            entity.Property(e => e.DiningMode)
                .HasMaxLength(20)
                .HasDefaultValueSql("'dine_in'::character varying")
                .HasComment("就餐方式：dine_in-堂食, takeaway-打包, delivery-外卖")
                .HasColumnName("dining_mode");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2)
                .HasComment("优惠减免金额")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountRuleId).HasColumnName("discount_rule_id");
            entity.Property(e => e.OrderNo)
                .HasMaxLength(30)
                .HasComment("订单编号（如：20260520001）")
                .HasColumnName("order_no");
            entity.Property(e => e.PackingFee)
                .HasPrecision(18, 2)
                .HasColumnName("packing_fee");
            entity.Property(e => e.PickupCode)
                .HasMaxLength(10)
                .HasComment("取货码（4-6位字母数字）")
                .HasColumnName("pickup_code");
            entity.Property(e => e.PreparingAt).HasColumnName("preparing_at");
            entity.Property(e => e.ReadyAt).HasColumnName("ready_at");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(500)
                .HasColumnName("reject_reason");
            entity.Property(e => e.Remark)
                .HasMaxLength(500)
                .HasColumnName("remark");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasComment("状态：pending-待接单, accepted-已接单, preparing-制作中, ready-待取餐, completed-已完成, cancelled-已取消")
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TableNo)
                .HasMaxLength(20)
                .HasColumnName("table_no");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .HasComment("商品合计金额")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("fk_orders_customer");

            entity.HasOne(d => d.DiscountRule).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DiscountRuleId)
                .HasConstraintName("fk_orders_discount_rule");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_store");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_items_pkey");

            entity.ToTable("order_items", tb => tb.HasComment("订单项表"));

            entity.HasIndex(e => e.OrderId, "ix_order_items_order_id");

            entity.HasIndex(e => e.ProductId, "ix_order_items_product_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ComboItemsSnapshot)
                .HasComment("套餐子商品快照（JSON，记录套餐内容）")
                .HasColumnType("jsonb")
                .HasColumnName("combo_items_snapshot");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCombo).HasColumnName("is_combo");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(500)
                .HasColumnName("product_image");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.Remark)
                .HasMaxLength(200)
                .HasColumnName("remark");
            entity.Property(e => e.SpecSnapshot)
                .HasComment("规格快照（JSON，记录下单时的规格选择）")
                .HasColumnType("jsonb")
                .HasColumnName("spec_snapshot");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(18, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_items_order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_items_product");
        });

        modelBuilder.Entity<PlatformDailyStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("platform_daily_stats_pkey");

            entity.ToTable("platform_daily_stats", tb => tb.HasComment("平台每日统计表"));

            entity.HasIndex(e => e.StatDate, "ix_platform_daily_stats_stat_date");

            entity.HasIndex(e => e.StatDate, "uq_platform_daily_stats_date").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActiveMerchants)
                .HasComment("活跃商家数（当日有订单）")
                .HasColumnName("active_merchants");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.NewCustomers)
                .HasComment("当日新增顾客数")
                .HasColumnName("new_customers");
            entity.Property(e => e.NewMerchants)
                .HasComment("当日新增商家数")
                .HasColumnName("new_merchants");
            entity.Property(e => e.StatDate).HasColumnName("stat_date");
            entity.Property(e => e.TemplateUsageCount)
                .HasComment("当日模板使用次数")
                .HasColumnName("template_usage_count");
            entity.Property(e => e.TotalCustomers)
                .HasComment("顾客总数")
                .HasColumnName("total_customers");
            entity.Property(e => e.TotalMerchants)
                .HasComment("商家总数")
                .HasColumnName("total_merchants");
            entity.Property(e => e.TotalOrders)
                .HasComment("当日平台总订单数")
                .HasColumnName("total_orders");
            entity.Property(e => e.TotalRevenue)
                .HasPrecision(18, 2)
                .HasComment("当日平台总营收")
                .HasColumnName("total_revenue");
            entity.Property(e => e.TotalStores)
                .HasComment("店铺总数")
                .HasColumnName("total_stores");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products", tb => tb.HasComment("商品表"));

            entity.HasIndex(e => e.CategoryId, "ix_products_category_id");

            entity.HasIndex(e => new { e.StoreId, e.Status }, "ix_products_status");

            entity.HasIndex(e => e.StoreId, "ix_products_store_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BasePrice)
                .HasPrecision(18, 2)
                .HasComment("基础价格（默认规格价格）")
                .HasColumnName("base_price");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsCombo)
                .HasComment("是否为套餐商品")
                .HasColumnName("is_combo");
            entity.Property(e => e.MinOrderQty)
                .HasDefaultValue(1)
                .HasComment("最低起购数量")
                .HasColumnName("min_order_qty");
            entity.Property(e => e.MonthlySales)
                .HasComment("月销量（冗余，定时统计）")
                .HasColumnName("monthly_sales");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'off'::character varying")
                .HasComment("状态：on-上架, off-下架, sold_out-售罄")
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TotalSales)
                .HasComment("总销量")
                .HasColumnName("total_sales");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_category");

            entity.HasOne(d => d.Store).WithMany(p => p.Products)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_store");
        });

        modelBuilder.Entity<ProductSalesStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_sales_stats_pkey");

            entity.ToTable("product_sales_stats", tb => tb.HasComment("商品销售统计表"));

            entity.HasIndex(e => e.ProductId, "ix_product_sales_stats_product_id");

            entity.HasIndex(e => e.StatDate, "ix_product_sales_stats_stat_date");

            entity.HasIndex(e => e.StoreId, "ix_product_sales_stats_store_id");

            entity.HasIndex(e => new { e.StoreId, e.ProductId, e.StatDate }, "uq_product_sales_stats_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvgUnitPrice)
                .HasPrecision(18, 2)
                .HasComment("平均单价")
                .HasColumnName("avg_unit_price");
            entity.Property(e => e.CancelCount)
                .HasComment("被取消订单中包含该商品的次数")
                .HasColumnName("cancel_count");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderCount)
                .HasComment("包含该商品的订单数")
                .HasColumnName("order_count");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SalesAmount)
                .HasPrecision(18, 2)
                .HasComment("销售金额")
                .HasColumnName("sales_amount");
            entity.Property(e => e.SalesQuantity)
                .HasComment("销售数量")
                .HasColumnName("sales_quantity");
            entity.Property(e => e.SpecDistribution)
                .HasComment("规格分布（JSON，如{\"小份\":40,\"大份\":60}）")
                .HasColumnType("jsonb")
                .HasColumnName("spec_distribution");
            entity.Property(e => e.StatDate).HasColumnName("stat_date");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.ProductSalesStats)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("fk_product_sales_stats_category");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSalesStats)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_sales_stats_product");

            entity.HasOne(d => d.Store).WithMany(p => p.ProductSalesStats)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_sales_stats_store");
        });

        modelBuilder.Entity<SpecGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("spec_groups_pkey");

            entity.ToTable("spec_groups", tb => tb.HasComment("规格组表（如：份量、辣度）"));

            entity.HasIndex(e => e.ProductId, "ix_spec_groups_product_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(true)
                .HasComment("是否必选")
                .HasColumnName("is_required");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Product).WithMany(p => p.SpecGroups)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_spec_groups_product");
        });

        modelBuilder.Entity<SpecOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("spec_options_pkey");

            entity.ToTable("spec_options", tb => tb.HasComment("规格项表（如：小份+0, 大份+10）"));

            entity.HasIndex(e => e.SpecGroupId, "ix_spec_options_spec_group_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExtraPrice)
                .HasPrecision(18, 2)
                .HasComment("加价（可为负数，如罐装比瓶装便宜）")
                .HasColumnName("extra_price");
            entity.Property(e => e.IsDefault)
                .HasComment("是否为默认选中项")
                .HasColumnName("is_default");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.SpecGroupId).HasColumnName("spec_group_id");
            entity.Property(e => e.Stock)
                .HasComment("库存（null表示不限库存）")
                .HasColumnName("stock");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.SpecGroup).WithMany(p => p.SpecOptions)
                .HasForeignKey(d => d.SpecGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("spec_options_spec_group_id_fkey");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("staff_pkey");

            entity.ToTable("staff", tb => tb.HasComment("店员表"));

            entity.HasIndex(e => e.StoreId, "ix_staff_store_id");

            entity.HasIndex(e => e.Phone, "uq_staff_phone").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .HasColumnName("password_hash");
            entity.Property(e => e.Permission)
                .HasMaxLength(20)
                .HasDefaultValueSql("'order_only'::character varying")
                .HasComment("权限：full-全部权限, order_only-仅接单")
                .HasColumnName("permission");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.Staff)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_staff_store");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stores_pkey");

            entity.ToTable("stores", tb => tb.HasComment("店铺表"));

            entity.HasIndex(e => e.BusinessStatus, "ix_stores_business_status");

            entity.HasIndex(e => e.IndustryCategoryId, "ix_stores_industry_category_id");

            entity.HasIndex(e => e.MerchantId, "ix_stores_merchant_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BusinessHoursEnd)
                .HasComment("营业结束时间")
                .HasColumnName("business_hours_end");
            entity.Property(e => e.BusinessHoursStart)
                .HasComment("营业开始时间")
                .HasColumnName("business_hours_start");
            entity.Property(e => e.BusinessStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'open'::character varying")
                .HasComment("营业状态：open-营业中, closed-休息中")
                .HasColumnName("business_status");
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(500)
                .HasComment("封面图URL")
                .HasColumnName("cover_image_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeliveryMinAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m)
                .HasComment("外卖最低消费金额")
                .HasColumnName("delivery_min_amount");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("店铺描述")
                .HasColumnName("description");
            entity.Property(e => e.DiningMode)
                .HasMaxLength(100)
                .HasDefaultValueSql("'dine_in,takeaway'::character varying")
                .HasComment("就餐方式：逗号分隔 dine_in-堂食,takeaway-打包,delivery-外卖")
                .HasColumnName("dining_mode");
            entity.Property(e => e.IndustryCategoryId)
                .HasComment("行业分类ID（关联模板系统）")
                .HasColumnName("industry_category_id");
            entity.Property(e => e.MerchantId)
                .HasComment("所属商家ID")
                .HasColumnName("merchant_id");
            entity.Property(e => e.MonthlySales)
                .HasComment("月销量（冗余字段，定时统计更新）")
                .HasColumnName("monthly_sales");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasComment("店铺名称")
                .HasColumnName("name");
            entity.Property(e => e.PackingFee)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m)
                .HasComment("打包费")
                .HasColumnName("packing_fee");
            entity.Property(e => e.QrCodeUrl)
                .HasMaxLength(500)
                .HasComment("店铺二维码URL")
                .HasColumnName("qr_code_url");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Stores)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_stores_merchant");
        });

        modelBuilder.Entity<StoreTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("store_templates_pkey");

            entity.ToTable("store_templates", tb => tb.HasComment("商家模板表"));

            entity.HasIndex(e => e.IndustryCategoryId, "ix_store_templates_industry_category_id");

            entity.HasIndex(e => e.Status, "ix_store_templates_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(500)
                .HasColumnName("cover_image_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IndustryCategoryId).HasColumnName("industry_category_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SourceStoreIds)
                .HasMaxLength(500)
                .HasComment("来源商家ID列表（逗号分隔，从商家引入/组合时记录）")
                .HasColumnName("source_store_ids");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'manual'::character varying")
                .HasComment("创建方式：manual-从零创建, from_store-从商家引入, combined-多商家组合")
                .HasColumnName("source_type");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UseCount)
                .HasComment("被应用次数")
                .HasColumnName("use_count");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StoreTemplates)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("store_templates_created_by_fkey");

            entity.HasOne(d => d.IndustryCategory).WithMany(p => p.StoreTemplates)
                .HasForeignKey(d => d.IndustryCategoryId)
                .HasConstraintName("fk_store_templates_industry");
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("system_configs_pkey");

            entity.ToTable("system_configs", tb => tb.HasComment("系统配置表"));

            entity.HasIndex(e => e.ConfigKey, "uq_system_configs_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ConfigKey)
                .HasMaxLength(100)
                .HasComment("配置键名")
                .HasColumnName("config_key");
            entity.Property(e => e.ConfigType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'string'::character varying")
                .HasComment("值类型：string-字符串, number-数字, boolean-布尔, json-JSON对象")
                .HasColumnName("config_type");
            entity.Property(e => e.ConfigValue)
                .HasComment("配置值")
                .HasColumnName("config_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("配置说明")
                .HasColumnName("description");
            entity.Property(e => e.IsPublic)
                .HasComment("是否公开（前端可读取的配置，如取货码长度）")
                .HasColumnName("is_public");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<TemplateCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("template_categories_pkey");

            entity.ToTable("template_categories", tb => tb.HasComment("模板分类表"));

            entity.HasIndex(e => e.TemplateId, "ix_template_categories_template_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CategoryType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'normal'::character varying")
                .HasColumnName("category_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.HotTopCount)
                .HasDefaultValue(10)
                .HasColumnName("hot_top_count");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Template).WithMany(p => p.TemplateCategories)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_template_categories_template");
        });

        modelBuilder.Entity<TemplateComboItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("template_combo_items_pkey");

            entity.ToTable("template_combo_items", tb => tb.HasComment("模板套餐子商品表"));

            entity.HasIndex(e => e.ComboTemplateProductId, "ix_template_combo_items_combo_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ComboTemplateProductId).HasColumnName("combo_template_product_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.Remark)
                .HasMaxLength(100)
                .HasColumnName("remark");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TemplateProductId).HasColumnName("template_product_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ComboTemplateProduct).WithMany(p => p.TemplateComboItemComboTemplateProducts)
                .HasForeignKey(d => d.ComboTemplateProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_template_combo_items_combo");

            entity.HasOne(d => d.TemplateProduct).WithMany(p => p.TemplateComboItemTemplateProducts)
                .HasForeignKey(d => d.TemplateProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_template_combo_items_product");
        });

        modelBuilder.Entity<TemplateProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("template_products_pkey");

            entity.ToTable("template_products", tb => tb.HasComment("模板商品表"));

            entity.HasIndex(e => e.TemplateCategoryId, "ix_template_products_category_id");

            entity.HasIndex(e => e.TemplateId, "ix_template_products_template_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.MinOrderQty)
                .HasDefaultValue(1)
                .HasColumnName("min_order_qty");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ReferencePrice)
                .HasPrecision(18, 2)
                .HasComment("参考价格（市场常见价）")
                .HasColumnName("reference_price");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TemplateCategoryId).HasColumnName("template_category_id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.TemplateCategory).WithMany(p => p.TemplateProducts)
                .HasForeignKey(d => d.TemplateCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_template_products_category");

            entity.HasOne(d => d.Template).WithMany(p => p.TemplateProducts)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_template_products_template");
        });

        modelBuilder.Entity<TemplateSpecGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("template_spec_groups_pkey");

            entity.ToTable("template_spec_groups", tb => tb.HasComment("模板规格组表"));

            entity.HasIndex(e => e.TemplateProductId, "ix_template_spec_groups_product_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(true)
                .HasColumnName("is_required");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TemplateProductId).HasColumnName("template_product_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.TemplateProduct).WithMany(p => p.TemplateSpecGroups)
                .HasForeignKey(d => d.TemplateProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_template_spec_groups_product");
        });

        modelBuilder.Entity<TemplateSpecOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("template_spec_options_pkey");

            entity.ToTable("template_spec_options", tb => tb.HasComment("模板规格项表"));

            entity.HasIndex(e => e.TemplateSpecGroupId, "ix_template_spec_options_group_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExtraPrice)
                .HasPrecision(18, 2)
                .HasColumnName("extra_price");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TemplateSpecGroupId).HasColumnName("template_spec_group_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.TemplateSpecGroup).WithMany(p => p.TemplateSpecOptions)
                .HasForeignKey(d => d.TemplateSpecGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_template_spec_options_group");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
