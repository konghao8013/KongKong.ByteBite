-- ============================================================
-- 空空码上点单 (KongKong.ByteBite) - 模块7：顾客注册与数据合并
-- 创建日期：2026-05-20
-- 说明：顾客注册/登录、设备会话管理、匿名数据合并到注册账号
-- 依赖：01_users_and_stores.sql, 03_orders_and_discounts.sql
-- ============================================================

-- ============================================================
-- 1. 修改customers表 - 增加注册相关字段
-- ============================================================
ALTER TABLE customers ADD COLUMN IF NOT EXISTS password_hash VARCHAR(200);
ALTER TABLE customers ADD COLUMN IF NOT EXISTS is_registered BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE customers ADD COLUMN IF NOT EXISTS last_login_at TIMESTAMPTZ;

COMMENT ON COLUMN customers.password_hash IS 'BCrypt加密密码（注册用户才有）';
COMMENT ON COLUMN customers.is_registered IS '是否已注册：true-已注册用户, false-匿名用户';
COMMENT ON COLUMN customers.last_login_at IS '最后登录时间';

-- ============================================================
-- 2. 顾客会话表 (customer_sessions)
-- ============================================================
CREATE TABLE customer_sessions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id     UUID NOT NULL REFERENCES customers(id),
    device_id       VARCHAR(200),
    token           VARCHAR(500),
    last_store_id   UUID,
    last_category_id UUID,
    cart_data       JSONB,
    active_orders   JSONB,
    order_history   JSONB,
    ip_address      VARCHAR(50),
    user_agent      VARCHAR(500),
    expires_at      TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_customer_sessions_customer FOREIGN KEY (customer_id) REFERENCES customers(id)
);

COMMENT ON TABLE customer_sessions IS '顾客会话表 - 记录设备与账号关联、缓存数据';
COMMENT ON COLUMN customer_sessions.customer_id IS '关联的顾客ID';
COMMENT ON COLUMN customer_sessions.device_id IS '设备标识';
COMMENT ON COLUMN customer_sessions.token IS '登录令牌';
COMMENT ON COLUMN customer_sessions.last_store_id IS '最后访问的店铺ID';
COMMENT ON COLUMN customer_sessions.last_category_id IS '最后浏览的分类ID';
COMMENT ON COLUMN customer_sessions.cart_data IS '购物车数据（JSON，按店铺分组）';
COMMENT ON COLUMN customer_sessions.active_orders IS '进行中订单列表（JSON）';
COMMENT ON COLUMN customer_sessions.order_history IS '历史订单列表（JSON）';
COMMENT ON COLUMN customer_sessions.ip_address IS '最后登录IP地址';
COMMENT ON COLUMN customer_sessions.user_agent IS '最后登录浏览器标识';
COMMENT ON COLUMN customer_sessions.expires_at IS '会话过期时间';

CREATE INDEX ix_customer_sessions_customer_id ON customer_sessions(customer_id);
CREATE INDEX ix_customer_sessions_device_id ON customer_sessions(device_id);
CREATE INDEX ix_customer_sessions_token ON customer_sessions(token);

-- ============================================================
-- 3. 数据合并日志表 (data_merge_logs)
-- ============================================================
CREATE TABLE data_merge_logs (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    target_customer_id  UUID NOT NULL REFERENCES customers(id),
    source_device_id    VARCHAR(200),
    source_customer_id  UUID REFERENCES customers(id),
    merge_type          VARCHAR(20) NOT NULL,
    orders_merged       INTEGER NOT NULL DEFAULT 0,
    cart_items_merged   INTEGER NOT NULL DEFAULT 0,
    pickup_codes_merged INTEGER NOT NULL DEFAULT 0,
    conflicts_resolved  INTEGER NOT NULL DEFAULT 0,
    merge_detail        JSONB,
    status              VARCHAR(20) NOT NULL DEFAULT 'completed',
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_data_merge_logs_target_customer FOREIGN KEY (target_customer_id) REFERENCES customers(id),
    CONSTRAINT fk_data_merge_logs_source_customer FOREIGN KEY (source_customer_id) REFERENCES customers(id),
    CONSTRAINT ck_data_merge_logs_type CHECK (merge_type IN ('device_to_account', 'account_to_account', 'session_to_account')),
    CONSTRAINT ck_data_merge_logs_status CHECK (status IN ('pending', 'completed', 'failed', 'rolled_back'))
);

COMMENT ON TABLE data_merge_logs IS '数据合并日志表 - 记录匿名数据合并到注册账号的操作';
COMMENT ON COLUMN data_merge_logs.target_customer_id IS '目标注册顾客ID';
COMMENT ON COLUMN data_merge_logs.source_device_id IS '来源设备标识';
COMMENT ON COLUMN data_merge_logs.source_customer_id IS '来源匿名顾客ID（如有）';
COMMENT ON COLUMN data_merge_logs.merge_type IS '合并类型：device_to_account-设备到账号, account_to_account-账号到账号, session_to_account-会话到账号';
COMMENT ON COLUMN data_merge_logs.orders_merged IS '合并的订单数量';
COMMENT ON COLUMN data_merge_logs.cart_items_merged IS '合并的购物车商品数量';
COMMENT ON COLUMN data_merge_logs.pickup_codes_merged IS '合并的取货码数量';
COMMENT ON COLUMN data_merge_logs.conflicts_resolved IS '解决的冲突数量';
COMMENT ON COLUMN data_merge_logs.merge_detail IS '合并详情（JSON，记录具体合并内容）';
COMMENT ON COLUMN data_merge_logs.status IS '合并状态：pending-待处理, completed-已完成, failed-失败, rolled_back-已回滚';

CREATE INDEX ix_data_merge_logs_target_customer_id ON data_merge_logs(target_customer_id);
CREATE INDEX ix_data_merge_logs_source_device_id ON data_merge_logs(source_device_id);
CREATE INDEX ix_data_merge_logs_created_at ON data_merge_logs(created_at);
