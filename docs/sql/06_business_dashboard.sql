-- ============================================================
-- 空空码上点单 (KongKong.ByteBite) - 模块8：经营数据看板
-- 创建日期：2026-05-20
-- 说明：经营数据统计、报表数据源、数据导出记录
-- 依赖：01_users_and_stores.sql, 02_categories_and_products.sql, 03_orders_and_discounts.sql
-- ============================================================

-- ============================================================
-- 1. 每日经营统计快照表 (daily_store_stats)
-- ============================================================
CREATE TABLE daily_store_stats (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    stat_date       DATE NOT NULL,
    total_orders    INTEGER NOT NULL DEFAULT 0,
    completed_orders INTEGER NOT NULL DEFAULT 0,
    cancelled_orders INTEGER NOT NULL DEFAULT 0,
    total_revenue   DECIMAL(18,2) NOT NULL DEFAULT 0,
    actual_revenue  DECIMAL(18,2) NOT NULL DEFAULT 0,
    discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    packing_fee     DECIMAL(18,2) NOT NULL DEFAULT 0,
    avg_order_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    new_customers   INTEGER NOT NULL DEFAULT 0,
    returning_customers INTEGER NOT NULL DEFAULT 0,
    peak_hour       INTEGER,
    peak_hour_orders INTEGER NOT NULL DEFAULT 0,
    top_product_id  UUID,
    top_product_name VARCHAR(100),
    top_product_qty INTEGER NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_daily_store_stats_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT uq_daily_store_stats_store_date UNIQUE (store_id, stat_date)
);

COMMENT ON TABLE daily_store_stats IS '每日店铺经营统计快照表';
COMMENT ON COLUMN daily_store_stats.stat_date IS '统计日期';
COMMENT ON COLUMN daily_store_stats.total_orders IS '当日总订单数';
COMMENT ON COLUMN daily_store_stats.completed_orders IS '当日已完成订单数';
COMMENT ON COLUMN daily_store_stats.cancelled_orders IS '当日已取消订单数';
COMMENT ON COLUMN daily_store_stats.total_revenue IS '当日总金额（含优惠前）';
COMMENT ON COLUMN daily_store_stats.actual_revenue IS '当日实收金额（扣除优惠）';
COMMENT ON COLUMN daily_store_stats.discount_amount IS '当日优惠减免总额';
COMMENT ON COLUMN daily_store_stats.packing_fee IS '当日打包费总额';
COMMENT ON COLUMN daily_store_stats.avg_order_amount IS '当日客单价';
COMMENT ON COLUMN daily_store_stats.new_customers IS '当日新顾客数';
COMMENT ON COLUMN daily_store_stats.returning_customers IS '当日回头客数';
COMMENT ON COLUMN daily_store_stats.peak_hour IS '当日高峰时段（0-23）';
COMMENT ON COLUMN daily_store_stats.peak_hour_orders IS '高峰时段订单数';
COMMENT ON COLUMN daily_store_stats.top_product_id IS '当日最热销商品ID';
COMMENT ON COLUMN daily_store_stats.top_product_name IS '当日最热销商品名称';
COMMENT ON COLUMN daily_store_stats.top_product_qty IS '当日最热销商品销量';

CREATE INDEX ix_daily_store_stats_store_id ON daily_store_stats(store_id);
CREATE INDEX ix_daily_store_stats_stat_date ON daily_store_stats(stat_date);

-- ============================================================
-- 2. 商品销售统计表 (product_sales_stats)
-- ============================================================
CREATE TABLE product_sales_stats (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    product_id      UUID NOT NULL REFERENCES products(id),
    category_id     UUID REFERENCES categories(id),
    stat_date       DATE NOT NULL,
    sales_quantity  INTEGER NOT NULL DEFAULT 0,
    sales_amount    DECIMAL(18,2) NOT NULL DEFAULT 0,
    order_count     INTEGER NOT NULL DEFAULT 0,
    cancel_count    INTEGER NOT NULL DEFAULT 0,
    avg_unit_price  DECIMAL(18,2) NOT NULL DEFAULT 0,
    spec_distribution JSONB,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_product_sales_stats_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT fk_product_sales_stats_product FOREIGN KEY (product_id) REFERENCES products(id),
    CONSTRAINT fk_product_sales_stats_category FOREIGN KEY (category_id) REFERENCES categories(id),
    CONSTRAINT uq_product_sales_stats_unique UNIQUE (store_id, product_id, stat_date)
);

COMMENT ON TABLE product_sales_stats IS '商品销售统计表';
COMMENT ON COLUMN product_sales_stats.sales_quantity IS '销售数量';
COMMENT ON COLUMN product_sales_stats.sales_amount IS '销售金额';
COMMENT ON COLUMN product_sales_stats.order_count IS '包含该商品的订单数';
COMMENT ON COLUMN product_sales_stats.cancel_count IS '被取消订单中包含该商品的次数';
COMMENT ON COLUMN product_sales_stats.avg_unit_price IS '平均单价';
COMMENT ON COLUMN product_sales_stats.spec_distribution IS '规格分布（JSON，如{"小份":40,"大份":60}）';

CREATE INDEX ix_product_sales_stats_store_id ON product_sales_stats(store_id);
CREATE INDEX ix_product_sales_stats_product_id ON product_sales_stats(product_id);
CREATE INDEX ix_product_sales_stats_stat_date ON product_sales_stats(stat_date);

-- ============================================================
-- 3. 顾客消费统计表 (customer_consumption_stats)
-- ============================================================
CREATE TABLE customer_consumption_stats (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id            UUID NOT NULL REFERENCES stores(id),
    customer_id         UUID REFERENCES customers(id),
    device_id           VARCHAR(200),
    total_orders        INTEGER NOT NULL DEFAULT 0,
    total_amount        DECIMAL(18,2) NOT NULL DEFAULT 0,
    avg_order_amount    DECIMAL(18,2) NOT NULL DEFAULT 0,
    first_order_at      TIMESTAMPTZ,
    last_order_at       TIMESTAMPTZ,
    top_products        JSONB,
    is_returning        BOOLEAN NOT NULL DEFAULT false,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_customer_consumption_stats_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT fk_customer_consumption_stats_customer FOREIGN KEY (customer_id) REFERENCES customers(id),
    CONSTRAINT uq_customer_consumption_stats_unique UNIQUE (store_id, customer_id, device_id)
);

COMMENT ON TABLE customer_consumption_stats IS '顾客消费统计表';
COMMENT ON COLUMN customer_consumption_stats.customer_id IS '顾客ID（注册用户）';
COMMENT ON COLUMN customer_consumption_stats.device_id IS '设备标识（匿名用户）';
COMMENT ON COLUMN customer_consumption_stats.total_orders IS '在当前店铺的总订单数';
COMMENT ON COLUMN customer_consumption_stats.total_amount IS '在当前店铺的总消费金额';
COMMENT ON COLUMN customer_consumption_stats.avg_order_amount IS '平均客单价';
COMMENT ON COLUMN customer_consumption_stats.first_order_at IS '首次下单时间';
COMMENT ON COLUMN customer_consumption_stats.last_order_at IS '最近下单时间';
COMMENT ON COLUMN customer_consumption_stats.top_products IS '常购商品Top3（JSON）';
COMMENT ON COLUMN customer_consumption_stats.is_returning IS '是否为回头客（下单>=2次）';

CREATE INDEX ix_customer_consumption_stats_store_id ON customer_consumption_stats(store_id);
CREATE INDEX ix_customer_consumption_stats_customer_id ON customer_consumption_stats(customer_id);
CREATE INDEX ix_customer_consumption_stats_device_id ON customer_consumption_stats(device_id);

-- ============================================================
-- 4. 时段订单统计表 (hourly_order_stats)
-- ============================================================
CREATE TABLE hourly_order_stats (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    stat_date       DATE NOT NULL,
    hour            INTEGER NOT NULL,
    order_count     INTEGER NOT NULL DEFAULT 0,
    revenue         DECIMAL(18,2) NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_hourly_order_stats_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT uq_hourly_order_stats_unique UNIQUE (store_id, stat_date, hour),
    CONSTRAINT ck_hourly_order_stats_hour CHECK (hour >= 0 AND hour <= 23)
);

COMMENT ON TABLE hourly_order_stats IS '时段订单统计表 - 按小时统计订单分布';
COMMENT ON COLUMN hourly_order_stats.hour IS '时段（0-23）';
COMMENT ON COLUMN hourly_order_stats.order_count IS '该时段订单数';
COMMENT ON COLUMN hourly_order_stats.revenue IS '该时段营收';

CREATE INDEX ix_hourly_order_stats_store_id ON hourly_order_stats(store_id);
CREATE INDEX ix_hourly_order_stats_stat_date ON hourly_order_stats(stat_date);

-- ============================================================
-- 5. 优惠活动效果统计表 (discount_effect_stats)
-- ============================================================
CREATE TABLE discount_effect_stats (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    discount_rule_id    UUID NOT NULL REFERENCES discount_rules(id),
    store_id            UUID NOT NULL REFERENCES stores(id),
    stat_date           DATE NOT NULL,
    used_count          INTEGER NOT NULL DEFAULT 0,
    total_discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    total_driven_revenue DECIMAL(18,2) NOT NULL DEFAULT 0,
    avg_order_amount_with_discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_discount_effect_stats_discount_rule FOREIGN KEY (discount_rule_id) REFERENCES discount_rules(id),
    CONSTRAINT fk_discount_effect_stats_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT uq_discount_effect_stats_unique UNIQUE (discount_rule_id, stat_date)
);

COMMENT ON TABLE discount_effect_stats IS '优惠活动效果统计表';
COMMENT ON COLUMN discount_effect_stats.used_count IS '当日使用次数';
COMMENT ON COLUMN discount_effect_stats.total_discount_amount IS '当日优惠减免总额';
COMMENT ON COLUMN discount_effect_stats.total_driven_revenue IS '当日使用优惠的订单总营收';
COMMENT ON COLUMN discount_effect_stats.avg_order_amount_with_discount IS '当日使用优惠的订单平均金额';

CREATE INDEX ix_discount_effect_stats_discount_rule_id ON discount_effect_stats(discount_rule_id);
CREATE INDEX ix_discount_effect_stats_store_id ON discount_effect_stats(store_id);
CREATE INDEX ix_discount_effect_stats_stat_date ON discount_effect_stats(stat_date);

-- ============================================================
-- 6. 数据导出记录表 (data_export_logs)
-- ============================================================
CREATE TABLE data_export_logs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    merchant_id     UUID NOT NULL REFERENCES merchants(id),
    export_type     VARCHAR(50) NOT NULL,
    date_range_start DATE,
    date_range_end   DATE,
    file_format     VARCHAR(10) NOT NULL DEFAULT 'csv',
    file_path       VARCHAR(500),
    file_size       BIGINT,
    status          VARCHAR(20) NOT NULL DEFAULT 'pending',
    error_message   VARCHAR(500),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_data_export_logs_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT fk_data_export_logs_merchant FOREIGN KEY (merchant_id) REFERENCES merchants(id),
    CONSTRAINT ck_data_export_logs_format CHECK (file_format IN ('csv', 'excel', 'pdf')),
    CONSTRAINT ck_data_export_logs_status CHECK (status IN ('pending', 'processing', 'completed', 'failed'))
);

COMMENT ON TABLE data_export_logs IS '数据导出记录表';
COMMENT ON COLUMN data_export_logs.export_type IS '导出类型：product_sales-商品销售, customer_consumption-顾客消费, revenue_trend-营收趋势, order_trend-订单趋势, discount_effect-优惠效果';
COMMENT ON COLUMN data_export_logs.file_format IS '文件格式：csv, excel, pdf';
COMMENT ON COLUMN data_export_logs.file_path IS '导出文件路径';
COMMENT ON COLUMN data_export_logs.file_size IS '文件大小（字节）';
COMMENT ON COLUMN data_export_logs.status IS '导出状态：pending-待处理, processing-处理中, completed-已完成, failed-失败';

CREATE INDEX ix_data_export_logs_store_id ON data_export_logs(store_id);
CREATE INDEX ix_data_export_logs_merchant_id ON data_export_logs(merchant_id);
CREATE INDEX ix_data_export_logs_created_at ON data_export_logs(created_at);
