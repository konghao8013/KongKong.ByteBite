using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ByteBite.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'admin'::character varying", comment: "角色：super_admin-超级管理员, admin-管理员, viewer-只读"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'active'::character varying"),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("admins_pkey", x => x.id);
                },
                comment: "平台管理员表");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "手机号（注册后才有）"),
                    nickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "设备标识（匿名用户）"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "BCrypt加密密码（注册用户才有）"),
                    is_registered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否已注册：true-已注册用户, false-匿名用户"),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "最后登录时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("customers_pkey", x => x.id);
                },
                comment: "顾客用户表");

            migrationBuilder.CreateTable(
                name: "industry_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "层级：1-一级行业(餐饮), 2-二级行业(烧烤), 3-三级行业(重庆特色烧烤)"),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("industry_categories_pkey", x => x.id);
                    table.ForeignKey(
                        name: "industry_categories_parent_id_fkey",
                        column: x => x.parent_id,
                        principalTable: "industry_categories",
                        principalColumn: "id");
                },
                comment: "行业分类表（三级）");

            migrationBuilder.CreateTable(
                name: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "主键UUID"),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "手机号，作为登录账号"),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "BCrypt加密密码"),
                    nickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "昵称"),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "头像URL"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'active'::character varying", comment: "状态：pending-待审核, active-已激活, disabled-已禁用"),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "最后登录时间"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "软删除时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("merchants_pkey", x => x.id);
                },
                comment: "商家用户表");

            migrationBuilder.CreateTable(
                name: "platform_daily_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    stat_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "统计日期"),
                    total_merchants = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "商家总数"),
                    active_merchants = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "活跃商家数（当日有订单）"),
                    new_merchants = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日新增商家数"),
                    total_stores = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "店铺总数"),
                    total_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日平台总订单数"),
                    total_revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日平台总营收"),
                    total_customers = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "顾客总数"),
                    new_customers = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日新增顾客数"),
                    template_usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日模板使用次数"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("platform_daily_stats_pkey", x => x.id);
                },
                comment: "平台每日统计表");

            migrationBuilder.CreateTable(
                name: "system_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    config_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "配置键名"),
                    config_value = table.Column<string>(type: "text", nullable: false, comment: "配置值"),
                    config_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'string'::character varying", comment: "值类型：string-字符串, number-数字, boolean-布尔, json-JSON对象"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "配置说明"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否公开（前端可读取的配置，如取货码长度）"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("system_configs_pkey", x => x.id);
                },
                comment: "系统配置表");

            migrationBuilder.CreateTable(
                name: "admin_operation_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "操作类型"),
                    target_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "操作对象类型：merchant, store, template, config等"),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "操作对象ID"),
                    detail = table.Column<string>(type: "jsonb", nullable: true, comment: "操作详情（JSON）"),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "操作者IP地址"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("admin_operation_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "admin_operation_logs_admin_id_fkey",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "管理员操作日志表");

            migrationBuilder.CreateTable(
                name: "customer_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "设备标识"),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "登录令牌"),
                    last_store_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "最后访问的店铺ID"),
                    last_category_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "最后浏览的分类ID"),
                    cart_data = table.Column<string>(type: "jsonb", nullable: true, comment: "购物车数据（JSON，按店铺分组）"),
                    active_orders = table.Column<string>(type: "jsonb", nullable: true, comment: "进行中订单列表（JSON）"),
                    order_history = table.Column<string>(type: "jsonb", nullable: true, comment: "历史订单列表（JSON）"),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "最后登录IP地址"),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "最后登录浏览器标识"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "会话过期时间"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_sessions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "customer_sessions_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "顾客会话表");

            migrationBuilder.CreateTable(
                name: "data_merge_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    target_customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "来源设备标识"),
                    source_customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    merge_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "合并类型：device_to_account-设备到账号, account_to_account-账号到账号, session_to_account-会话到账号"),
                    orders_merged = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "合并的订单数量"),
                    cart_items_merged = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "合并的购物车商品数量"),
                    pickup_codes_merged = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "合并的取货码数量"),
                    conflicts_resolved = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "解决的冲突数量"),
                    merge_detail = table.Column<string>(type: "jsonb", nullable: true, comment: "合并详情（JSON，记录具体合并内容）"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'completed'::character varying", comment: "合并状态：pending-待处理, completed-已完成, failed-失败, rolled_back-已回滚"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("data_merge_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_merge_logs_source_customer",
                        column: x => x.source_customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_data_merge_logs_target_customer",
                        column: x => x.target_customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "数据合并日志表");

            migrationBuilder.CreateTable(
                name: "store_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    industry_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    source_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'manual'::character varying", comment: "创建方式：manual-从零创建, from_store-从商家引入, combined-多商家组合"),
                    source_store_ids = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "来源商家ID列表（逗号分隔，从商家引入/组合时记录）"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'active'::character varying"),
                    use_count = table.Column<int>(type: "integer", nullable: false, comment: "被应用次数"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("store_templates_pkey", x => x.id);
                    table.ForeignKey(
                        name: "store_templates_created_by_fkey",
                        column: x => x.created_by,
                        principalTable: "admins",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "store_templates_industry_category_id_fkey",
                        column: x => x.industry_category_id,
                        principalTable: "industry_categories",
                        principalColumn: "id");
                },
                comment: "商家模板表");

            migrationBuilder.CreateTable(
                name: "merchant_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "操作类型：approve-审核通过, reject-审核拒绝, disable-禁用, enable-解禁, delete-删除"),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "操作原因"),
                    previous_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "操作前状态"),
                    new_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "操作后状态"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("merchant_audit_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "merchant_audit_logs_admin_id_fkey",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "merchant_audit_logs_merchant_id_fkey",
                        column: x => x.merchant_id,
                        principalTable: "merchants",
                        principalColumn: "id");
                },
                comment: "商家审核记录表");

            migrationBuilder.CreateTable(
                name: "stores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "所属商家ID"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "店铺名称"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "店铺描述"),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "封面图URL"),
                    qr_code_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "店铺二维码URL"),
                    business_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'open'::character varying", comment: "营业状态：open-营业中, closed-休息中"),
                    business_hours_start = table.Column<TimeOnly>(type: "time without time zone", nullable: true, comment: "营业开始时间"),
                    business_hours_end = table.Column<TimeOnly>(type: "time without time zone", nullable: true, comment: "营业结束时间"),
                    industry_category_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "行业分类ID（关联模板系统）"),
                    dining_mode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "'dine_in,takeaway'::character varying", comment: "就餐方式：逗号分隔 dine_in-堂食,takeaway-打包,delivery-外卖"),
                    delivery_min_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m, comment: "外卖最低消费金额"),
                    packing_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m, comment: "打包费"),
                    monthly_sales = table.Column<int>(type: "integer", nullable: false, comment: "月销量（冗余字段，定时统计更新）"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("stores_pkey", x => x.id);
                    table.ForeignKey(
                        name: "stores_merchant_id_fkey",
                        column: x => x.merchant_id,
                        principalTable: "merchants",
                        principalColumn: "id");
                },
                comment: "店铺表");

            migrationBuilder.CreateTable(
                name: "template_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'normal'::character varying"),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    hot_top_count = table.Column<int>(type: "integer", nullable: true, defaultValue: 10),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("template_categories_pkey", x => x.id);
                    table.ForeignKey(
                        name: "template_categories_template_id_fkey",
                        column: x => x.template_id,
                        principalTable: "store_templates",
                        principalColumn: "id");
                },
                comment: "模板分类表");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    category_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'normal'::character varying", comment: "分类类型：normal-普通, hot-热销, welfare-进店福利, combo-套餐"),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    hot_top_count = table.Column<int>(type: "integer", nullable: true, defaultValue: 10, comment: "热销分类自动聚合的Top数量（仅hot类型有效）"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("categories_pkey", x => x.id);
                    table.ForeignKey(
                        name: "categories_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "商品分类表");

            migrationBuilder.CreateTable(
                name: "customer_consumption_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "顾客ID（注册用户）"),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "设备标识（匿名用户）"),
                    total_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "在当前店铺的总订单数"),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "在当前店铺的总消费金额"),
                    avg_order_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "平均客单价"),
                    first_order_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "首次下单时间"),
                    last_order_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "最近下单时间"),
                    top_products = table.Column<string>(type: "jsonb", nullable: true, comment: "常购商品Top3（JSON）"),
                    is_returning = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否为回头客（下单>=2次）"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_consumption_stats_pkey", x => x.id);
                    table.ForeignKey(
                        name: "customer_consumption_stats_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "customer_consumption_stats_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "顾客消费统计表");

            migrationBuilder.CreateTable(
                name: "daily_store_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stat_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "统计日期"),
                    total_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日总订单数"),
                    completed_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日已完成订单数"),
                    cancelled_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日已取消订单数"),
                    total_revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日总金额（含优惠前）"),
                    actual_revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日实收金额（扣除优惠）"),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日优惠减免总额"),
                    packing_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日打包费总额"),
                    avg_order_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日客单价"),
                    new_customers = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日新顾客数"),
                    returning_customers = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日回头客数"),
                    peak_hour = table.Column<int>(type: "integer", nullable: true, comment: "当日高峰时段（0-23）"),
                    peak_hour_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "高峰时段订单数"),
                    top_product_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "当日最热销商品ID"),
                    top_product_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "当日最热销商品名称"),
                    top_product_qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日最热销商品销量"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("daily_store_stats_pkey", x => x.id);
                    table.ForeignKey(
                        name: "daily_store_stats_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "每日店铺经营统计快照表");

            migrationBuilder.CreateTable(
                name: "data_export_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    export_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "导出类型：product_sales-商品销售, customer_consumption-顾客消费, revenue_trend-营收趋势, order_trend-订单趋势, discount_effect-优惠效果"),
                    date_range_start = table.Column<DateOnly>(type: "date", nullable: true),
                    date_range_end = table.Column<DateOnly>(type: "date", nullable: true),
                    file_format = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "'csv'::character varying", comment: "文件格式：csv, excel, pdf"),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "导出文件路径"),
                    file_size = table.Column<long>(type: "bigint", nullable: true, comment: "文件大小（字节）"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying", comment: "导出状态：pending-待处理, processing-处理中, completed-已完成, failed-失败"),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("data_export_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "data_export_logs_merchant_id_fkey",
                        column: x => x.merchant_id,
                        principalTable: "merchants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "data_export_logs_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "数据导出记录表");

            migrationBuilder.CreateTable(
                name: "discount_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "类型：full_reduction-满减, discount-折扣"),
                    threshold_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, comment: "满减门槛金额（仅满减类型）"),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, comment: "满减减免金额（仅满减类型）"),
                    discount_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true, comment: "折扣率（如80=8折，仅折扣类型）"),
                    apply_scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'all'::character varying", comment: "适用范围：all-全店, category-指定分类, product-指定商品"),
                    apply_scope_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "适用范围的分类ID或商品ID"),
                    allow_stack = table.Column<bool>(type: "boolean", nullable: false, comment: "是否允许与其他优惠叠加"),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'active'::character varying"),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("discount_rules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "discount_rules_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "优惠活动表");

            migrationBuilder.CreateTable(
                name: "hourly_order_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stat_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "统计日期"),
                    hour = table.Column<int>(type: "integer", nullable: false, comment: "时段（0-23）"),
                    order_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "该时段订单数"),
                    revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "该时段营收"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("hourly_order_stats_pkey", x => x.id);
                    table.ForeignKey(
                        name: "hourly_order_stats_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "时段订单统计表");

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    permission = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'order_only'::character varying", comment: "权限：full-全部权限, order_only-仅接单"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'active'::character varying"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("staff_pkey", x => x.id);
                    table.ForeignKey(
                        name: "staff_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "店员表");

            migrationBuilder.CreateTable(
                name: "template_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "参考价格（市场常见价）"),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    min_order_qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("template_products_pkey", x => x.id);
                    table.ForeignKey(
                        name: "template_products_template_category_id_fkey",
                        column: x => x.template_category_id,
                        principalTable: "template_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "template_products_template_id_fkey",
                        column: x => x.template_id,
                        principalTable: "store_templates",
                        principalColumn: "id");
                },
                comment: "模板商品表");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    base_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "基础价格（默认规格价格）"),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'off'::character varying", comment: "状态：on-上架, off-下架, sold_out-售罄"),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    min_order_qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "最低起购数量"),
                    monthly_sales = table.Column<int>(type: "integer", nullable: false, comment: "月销量（冗余，定时统计）"),
                    total_sales = table.Column<int>(type: "integer", nullable: false, comment: "总销量"),
                    is_combo = table.Column<bool>(type: "boolean", nullable: false, comment: "是否为套餐商品"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("products_pkey", x => x.id);
                    table.ForeignKey(
                        name: "products_category_id_fkey",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "products_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "商品表");

            migrationBuilder.CreateTable(
                name: "discount_effect_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    discount_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stat_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "统计日期"),
                    used_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "当日使用次数"),
                    total_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日优惠减免总额"),
                    total_driven_revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日使用优惠的订单总营收"),
                    avg_order_amount_with_discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "当日使用优惠的订单平均金额"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("discount_effect_stats_pkey", x => x.id);
                    table.ForeignKey(
                        name: "discount_effect_stats_discount_rule_id_fkey",
                        column: x => x.discount_rule_id,
                        principalTable: "discount_rules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "discount_effect_stats_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "优惠活动效果统计表");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_no = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "订单编号（如：20260520001）"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pickup_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "取货码（4-6位字母数字）"),
                    dining_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'dine_in'::character varying", comment: "就餐方式：dine_in-堂食, takeaway-打包, delivery-外卖"),
                    table_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    delivery_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    delivery_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "商品合计金额"),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "优惠减免金额"),
                    actual_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "应付金额 = total - discount + packing_fee"),
                    packing_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying", comment: "状态：pending-待接单, accepted-已接单, preparing-制作中, ready-待取餐, completed-已完成, cancelled-已取消"),
                    reject_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preparing_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ready_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("orders_pkey", x => x.id);
                    table.ForeignKey(
                        name: "orders_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "orders_discount_rule_id_fkey",
                        column: x => x.discount_rule_id,
                        principalTable: "discount_rules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "orders_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "订单表");

            migrationBuilder.CreateTable(
                name: "template_combo_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    combo_template_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    remark = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("template_combo_items_pkey", x => x.id);
                    table.ForeignKey(
                        name: "template_combo_items_combo_template_product_id_fkey",
                        column: x => x.combo_template_product_id,
                        principalTable: "template_products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "template_combo_items_template_product_id_fkey",
                        column: x => x.template_product_id,
                        principalTable: "template_products",
                        principalColumn: "id");
                },
                comment: "模板套餐子商品表");

            migrationBuilder.CreateTable(
                name: "template_spec_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    template_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("template_spec_groups_pkey", x => x.id);
                    table.ForeignKey(
                        name: "template_spec_groups_template_product_id_fkey",
                        column: x => x.template_product_id,
                        principalTable: "template_products",
                        principalColumn: "id");
                },
                comment: "模板规格组表");

            migrationBuilder.CreateTable(
                name: "combo_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    combo_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    default_spec_option_ids = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "默认规格项ID列表（逗号分隔）"),
                    allow_change_spec = table.Column<bool>(type: "boolean", nullable: false, comment: "是否允许顾客替换规格"),
                    remark = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("combo_items_pkey", x => x.id);
                    table.ForeignKey(
                        name: "combo_items_combo_product_id_fkey",
                        column: x => x.combo_product_id,
                        principalTable: "products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "combo_items_product_id_fkey",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                },
                comment: "套餐子商品表");

            migrationBuilder.CreateTable(
                name: "product_sales_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stat_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "统计日期"),
                    sales_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "销售数量"),
                    sales_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "销售金额"),
                    order_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "包含该商品的订单数"),
                    cancel_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "被取消订单中包含该商品的次数"),
                    avg_unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m, comment: "平均单价"),
                    spec_distribution = table.Column<string>(type: "jsonb", nullable: true, comment: "规格分布（JSON，如{\"小份\":40,\"大份\":60}）"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_sales_stats_pkey", x => x.id);
                    table.ForeignKey(
                        name: "product_sales_stats_category_id_fkey",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "product_sales_stats_product_id_fkey",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "product_sales_stats_store_id_fkey",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id");
                },
                comment: "商品销售统计表");

            migrationBuilder.CreateTable(
                name: "spec_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "是否必选"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("spec_groups_pkey", x => x.id);
                    table.ForeignKey(
                        name: "spec_groups_product_id_fkey",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                },
                comment: "规格组表（如：份量、辣度）");

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    product_image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    spec_snapshot = table.Column<string>(type: "jsonb", nullable: true, comment: "规格快照（JSON，记录下单时的规格选择）"),
                    remark = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_combo = table.Column<bool>(type: "boolean", nullable: false),
                    combo_items_snapshot = table.Column<string>(type: "jsonb", nullable: true, comment: "套餐子商品快照（JSON，记录套餐内容）"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_items_pkey", x => x.id);
                    table.ForeignKey(
                        name: "order_items_order_id_fkey",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "order_items_product_id_fkey",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                },
                comment: "订单项表");

            migrationBuilder.CreateTable(
                name: "template_spec_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    template_spec_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    extra_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("template_spec_options_pkey", x => x.id);
                    table.ForeignKey(
                        name: "template_spec_options_template_spec_group_id_fkey",
                        column: x => x.template_spec_group_id,
                        principalTable: "template_spec_groups",
                        principalColumn: "id");
                },
                comment: "模板规格项表");

            migrationBuilder.CreateTable(
                name: "spec_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    spec_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    extra_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "加价（可为负数，如罐装比瓶装便宜）"),
                    stock = table.Column<int>(type: "integer", nullable: true, comment: "库存（null表示不限库存）"),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, comment: "是否为默认选中项"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("spec_options_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_spec_options_spec_group",
                        column: x => x.spec_group_id,
                        principalTable: "spec_groups",
                        principalColumn: "id");
                },
                comment: "规格项表（如：小份+0, 大份+10）");

            migrationBuilder.CreateIndex(
                name: "ix_admin_operation_logs_admin_id",
                table: "admin_operation_logs",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "uq_admins_username",
                table: "admins",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_sort_order",
                table: "categories",
                columns: new[] { "store_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_store_id",
                table: "categories",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_combo_items_combo_product_id",
                table: "combo_items",
                column: "combo_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_combo_items_product_id",
                table: "combo_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_consumption_stats_customer_id",
                table: "customer_consumption_stats",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "uq_customer_consumption_stats_store_customer_device",
                table: "customer_consumption_stats",
                columns: new[] { "store_id", "customer_id", "device_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_sessions_customer_id",
                table: "customer_sessions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_sessions_device_id",
                table: "customer_sessions",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_sessions_token",
                table: "customer_sessions",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "ix_customers_device_id",
                table: "customers",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone",
                table: "customers",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "uq_customers_phone",
                table: "customers",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_daily_store_stats_stat_date",
                table: "daily_store_stats",
                column: "stat_date");

            migrationBuilder.CreateIndex(
                name: "ix_daily_store_stats_store_id",
                table: "daily_store_stats",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "uq_daily_store_stats_store_date",
                table: "daily_store_stats",
                columns: new[] { "store_id", "stat_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_export_logs_merchant_id",
                table: "data_export_logs",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_logs_store_id",
                table: "data_export_logs",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_merge_logs_created_at",
                table: "data_merge_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_data_merge_logs_source_customer_id",
                table: "data_merge_logs",
                column: "source_customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_merge_logs_source_device_id",
                table: "data_merge_logs",
                column: "source_device_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_merge_logs_target_customer_id",
                table: "data_merge_logs",
                column: "target_customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_discount_effect_stats_store_id",
                table: "discount_effect_stats",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "uq_discount_effect_stats_rule_date",
                table: "discount_effect_stats",
                columns: new[] { "discount_rule_id", "stat_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_discount_rules_status",
                table: "discount_rules",
                columns: new[] { "store_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_discount_rules_store_id",
                table: "discount_rules",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "uq_hourly_order_stats_store_date_hour",
                table: "hourly_order_stats",
                columns: new[] { "store_id", "stat_date", "hour" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_industry_categories_level",
                table: "industry_categories",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "ix_industry_categories_parent_id",
                table: "industry_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_audit_logs_admin_id",
                table: "merchant_audit_logs",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchant_audit_logs_merchant_id",
                table: "merchant_audit_logs",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchants_phone",
                table: "merchants",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "ix_merchants_status",
                table: "merchants",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "uq_merchants_phone",
                table: "merchants",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_created_at",
                table: "orders",
                columns: new[] { "store_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_discount_rule_id",
                table: "orders",
                column: "discount_rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_pickup_code",
                table: "orders",
                columns: new[] { "pickup_code", "store_id" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_status",
                table: "orders",
                columns: new[] { "store_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_store_id",
                table: "orders",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "uq_orders_order_no",
                table: "orders",
                column: "order_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_orders_pickup_code_store",
                table: "orders",
                columns: new[] { "pickup_code", "store_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_platform_daily_stats_stat_date",
                table: "platform_daily_stats",
                column: "stat_date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_sales_stats_category_id",
                table: "product_sales_stats",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_sales_stats_product_id",
                table: "product_sales_stats",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "uq_product_sales_stats_store_product_date",
                table: "product_sales_stats",
                columns: new[] { "store_id", "product_id", "stat_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_status",
                table: "products",
                columns: new[] { "store_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_products_store_id",
                table: "products",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_groups_product_id",
                table: "spec_groups",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_options_spec_group_id",
                table: "spec_options",
                column: "spec_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_staff_store_id",
                table: "staff",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "uq_staff_phone",
                table: "staff",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_store_templates_created_by",
                table: "store_templates",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_store_templates_industry_category_id",
                table: "store_templates",
                column: "industry_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_store_templates_status",
                table: "store_templates",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_stores_business_status",
                table: "stores",
                column: "business_status");

            migrationBuilder.CreateIndex(
                name: "ix_stores_industry_category_id",
                table: "stores",
                column: "industry_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_stores_merchant_id",
                table: "stores",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "uq_system_configs_config_key",
                table: "system_configs",
                column: "config_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_template_categories_template_id",
                table: "template_categories",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_template_combo_items_combo_id",
                table: "template_combo_items",
                column: "combo_template_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_template_combo_items_template_product_id",
                table: "template_combo_items",
                column: "template_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_template_products_category_id",
                table: "template_products",
                column: "template_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_template_products_template_id",
                table: "template_products",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_template_spec_groups_product_id",
                table: "template_spec_groups",
                column: "template_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_template_spec_options_group_id",
                table: "template_spec_options",
                column: "template_spec_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_operation_logs");

            migrationBuilder.DropTable(
                name: "combo_items");

            migrationBuilder.DropTable(
                name: "customer_consumption_stats");

            migrationBuilder.DropTable(
                name: "customer_sessions");

            migrationBuilder.DropTable(
                name: "daily_store_stats");

            migrationBuilder.DropTable(
                name: "data_export_logs");

            migrationBuilder.DropTable(
                name: "data_merge_logs");

            migrationBuilder.DropTable(
                name: "discount_effect_stats");

            migrationBuilder.DropTable(
                name: "hourly_order_stats");

            migrationBuilder.DropTable(
                name: "merchant_audit_logs");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "platform_daily_stats");

            migrationBuilder.DropTable(
                name: "product_sales_stats");

            migrationBuilder.DropTable(
                name: "spec_options");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "system_configs");

            migrationBuilder.DropTable(
                name: "template_combo_items");

            migrationBuilder.DropTable(
                name: "template_spec_options");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "spec_groups");

            migrationBuilder.DropTable(
                name: "template_spec_groups");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "discount_rules");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "template_products");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "template_categories");

            migrationBuilder.DropTable(
                name: "stores");

            migrationBuilder.DropTable(
                name: "store_templates");

            migrationBuilder.DropTable(
                name: "merchants");

            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "industry_categories");
        }
    }
}
