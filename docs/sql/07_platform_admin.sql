-- ============================================================
-- 空空码上点单 (KongKong.ByteBite) - 模块9：平台管理员端
-- 创建日期：2026-05-20
-- 说明：系统配置、商家审核、平台数据统计
-- 依赖：01_users_and_stores.sql, 02_categories_and_products.sql, 03_orders_and_discounts.sql
-- ============================================================

-- ============================================================
-- 1. 系统配置表 (system_configs)
-- ============================================================
CREATE TABLE system_configs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    config_key      VARCHAR(100) NOT NULL,
    config_value    TEXT NOT NULL,
    config_type     VARCHAR(20) NOT NULL DEFAULT 'string',
    description     VARCHAR(500),
    is_public       BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_system_configs_key UNIQUE (config_key),
    CONSTRAINT ck_system_configs_type CHECK (config_type IN ('string', 'number', 'boolean', 'json'))
);

COMMENT ON TABLE system_configs IS '系统配置表';
COMMENT ON COLUMN system_configs.config_key IS '配置键名';
COMMENT ON COLUMN system_configs.config_value IS '配置值';
COMMENT ON COLUMN system_configs.config_type IS '值类型：string-字符串, number-数字, boolean-布尔, json-JSON对象';
COMMENT ON COLUMN system_configs.description IS '配置说明';
COMMENT ON COLUMN system_configs.is_public IS '是否公开（前端可读取的配置，如取货码长度）';

-- 初始化默认配置
INSERT INTO system_configs (config_key, config_value, config_type, description, is_public) VALUES
('require_merchant_review', 'false', 'boolean', '新注册商家是否需要审核', false),
('pickup_code_length', '4', 'number', '取货码默认长度（4-6位）', true),
('order_timeout_minutes', '10', 'number', '商家未接单超时提醒时间（分钟）', false),
('max_upload_size_mb', '5', 'number', '文件上传最大大小（MB）', false),
('allowed_upload_formats', 'jpg,jpeg,png,gif,webp', 'string', '允许上传的文件格式', false),
('sms_provider', '', 'string', '短信服务商标识', false),
('sms_api_key', '', 'string', '短信服务API密钥', false),
('sms_api_url', '', 'string', '短信服务API地址', false),
('system_announcement', '', 'string', '系统公告内容（所有商家端可见）', true),
('cart_cache_hours', '24', 'number', '购物车缓存有效时长（小时）', true),
('order_history_limit', '20', 'number', '保留最近已完成订单条数', true);

-- ============================================================
-- 2. 商家审核记录表 (merchant_audit_logs)
-- ============================================================
CREATE TABLE merchant_audit_logs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    merchant_id     UUID NOT NULL REFERENCES merchants(id),
    admin_id        UUID NOT NULL REFERENCES admins(id),
    action          VARCHAR(20) NOT NULL,
    reason          VARCHAR(500),
    previous_status VARCHAR(20),
    new_status      VARCHAR(20),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_merchant_audit_logs_merchant FOREIGN KEY (merchant_id) REFERENCES merchants(id),
    CONSTRAINT fk_merchant_audit_logs_admin FOREIGN KEY (admin_id) REFERENCES admins(id),
    CONSTRAINT ck_merchant_audit_logs_action CHECK (action IN ('approve', 'reject', 'disable', 'enable', 'delete'))
);

COMMENT ON TABLE merchant_audit_logs IS '商家审核记录表';
COMMENT ON COLUMN merchant_audit_logs.action IS '操作类型：approve-审核通过, reject-审核拒绝, disable-禁用, enable-解禁, delete-删除';
COMMENT ON COLUMN merchant_audit_logs.reason IS '操作原因';
COMMENT ON COLUMN merchant_audit_logs.previous_status IS '操作前状态';
COMMENT ON COLUMN merchant_audit_logs.new_status IS '操作后状态';

CREATE INDEX ix_merchant_audit_logs_merchant_id ON merchant_audit_logs(merchant_id);
CREATE INDEX ix_merchant_audit_logs_admin_id ON merchant_audit_logs(admin_id);
CREATE INDEX ix_merchant_audit_logs_created_at ON merchant_audit_logs(created_at);

-- ============================================================
-- 3. 平台每日统计表 (platform_daily_stats)
-- ============================================================
CREATE TABLE platform_daily_stats (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    stat_date           DATE NOT NULL,
    total_merchants     INTEGER NOT NULL DEFAULT 0,
    active_merchants    INTEGER NOT NULL DEFAULT 0,
    new_merchants       INTEGER NOT NULL DEFAULT 0,
    total_stores        INTEGER NOT NULL DEFAULT 0,
    total_orders        INTEGER NOT NULL DEFAULT 0,
    total_revenue       DECIMAL(18,2) NOT NULL DEFAULT 0,
    total_customers     INTEGER NOT NULL DEFAULT 0,
    new_customers       INTEGER NOT NULL DEFAULT 0,
    template_usage_count INTEGER NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_platform_daily_stats_date UNIQUE (stat_date)
);

COMMENT ON TABLE platform_daily_stats IS '平台每日统计表';
COMMENT ON COLUMN platform_daily_stats.total_merchants IS '商家总数';
COMMENT ON COLUMN platform_daily_stats.active_merchants IS '活跃商家数（当日有订单）';
COMMENT ON COLUMN platform_daily_stats.new_merchants IS '当日新增商家数';
COMMENT ON COLUMN platform_daily_stats.total_stores IS '店铺总数';
COMMENT ON COLUMN platform_daily_stats.total_orders IS '当日平台总订单数';
COMMENT ON COLUMN platform_daily_stats.total_revenue IS '当日平台总营收';
COMMENT ON COLUMN platform_daily_stats.total_customers IS '顾客总数';
COMMENT ON COLUMN platform_daily_stats.new_customers IS '当日新增顾客数';
COMMENT ON COLUMN platform_daily_stats.template_usage_count IS '当日模板使用次数';

CREATE INDEX ix_platform_daily_stats_stat_date ON platform_daily_stats(stat_date);

-- ============================================================
-- 4. 管理员操作日志表 (admin_operation_logs)
-- ============================================================
CREATE TABLE admin_operation_logs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    admin_id        UUID NOT NULL REFERENCES admins(id),
    operation       VARCHAR(50) NOT NULL,
    target_type     VARCHAR(50),
    target_id       UUID,
    detail          JSONB,
    ip_address      VARCHAR(50),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_admin_operation_logs_admin FOREIGN KEY (admin_id) REFERENCES admins(id)
);

COMMENT ON TABLE admin_operation_logs IS '管理员操作日志表';
COMMENT ON COLUMN admin_operation_logs.operation IS '操作类型';
COMMENT ON COLUMN admin_operation_logs.target_type IS '操作对象类型：merchant, store, template, config等';
COMMENT ON COLUMN admin_operation_logs.target_id IS '操作对象ID';
COMMENT ON COLUMN admin_operation_logs.detail IS '操作详情（JSON）';
COMMENT ON COLUMN admin_operation_logs.ip_address IS '操作者IP地址';

CREATE INDEX ix_admin_operation_logs_admin_id ON admin_operation_logs(admin_id);
CREATE INDEX ix_admin_operation_logs_created_at ON admin_operation_logs(created_at);
CREATE INDEX ix_admin_operation_logs_target ON admin_operation_logs(target_type, target_id);
