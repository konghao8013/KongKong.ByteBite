-- ============================================================
-- 空空码上点单 (KongKong.ByteBite) - 模块1：用户与店铺基础表
-- 创建日期：2026-05-20
-- 说明：商家用户、店铺、顾客用户、平台管理员
-- ============================================================

-- 启用 UUID 扩展
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- 1. 商家用户表 (merchants)
-- ============================================================
CREATE TABLE merchants (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phone           VARCHAR(20) NOT NULL,
    password_hash   VARCHAR(200) NOT NULL,
    nickname        VARCHAR(50),
    avatar_url      VARCHAR(500),
    status          VARCHAR(20) NOT NULL DEFAULT 'active',
    last_login_at   TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ,

    CONSTRAINT uq_merchants_phone UNIQUE (phone),
    CONSTRAINT ck_merchants_status CHECK (status IN ('pending', 'active', 'disabled'))
);

COMMENT ON TABLE merchants IS '商家用户表';
COMMENT ON COLUMN merchants.id IS '主键UUID';
COMMENT ON COLUMN merchants.phone IS '手机号，作为登录账号';
COMMENT ON COLUMN merchants.password_hash IS 'BCrypt加密密码';
COMMENT ON COLUMN merchants.nickname IS '昵称';
COMMENT ON COLUMN merchants.avatar_url IS '头像URL';
COMMENT ON COLUMN merchants.status IS '状态：pending-待审核, active-已激活, disabled-已禁用';
COMMENT ON COLUMN merchants.last_login_at IS '最后登录时间';
COMMENT ON COLUMN merchants.deleted_at IS '软删除时间';

CREATE INDEX ix_merchants_phone ON merchants(phone);
CREATE INDEX ix_merchants_status ON merchants(status);

-- ============================================================
-- 2. 店铺表 (stores)
-- ============================================================
CREATE TABLE stores (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    merchant_id         UUID NOT NULL REFERENCES merchants(id),
    name                VARCHAR(100) NOT NULL,
    description         VARCHAR(500),
    cover_image_url     VARCHAR(500),
    qr_code_url         VARCHAR(500),
    business_status     VARCHAR(20) NOT NULL DEFAULT 'open',
    business_hours_start TIME,
    business_hours_end   TIME,
    industry_category_id UUID,
    dining_mode         VARCHAR(100) NOT NULL DEFAULT 'dine_in,takeaway',
    delivery_min_amount  DECIMAL(18,2) DEFAULT 0,
    packing_fee         DECIMAL(18,2) DEFAULT 0,
    monthly_sales       INTEGER NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at          TIMESTAMPTZ,

    CONSTRAINT fk_stores_merchant FOREIGN KEY (merchant_id) REFERENCES merchants(id),
    CONSTRAINT ck_stores_business_status CHECK (business_status IN ('open', 'closed')),
    CONSTRAINT ck_stores_delivery_min_amount CHECK (delivery_min_amount >= 0),
    CONSTRAINT ck_stores_packing_fee CHECK (packing_fee >= 0)
);

COMMENT ON TABLE stores IS '店铺表';
COMMENT ON COLUMN stores.merchant_id IS '所属商家ID';
COMMENT ON COLUMN stores.name IS '店铺名称';
COMMENT ON COLUMN stores.description IS '店铺描述';
COMMENT ON COLUMN stores.cover_image_url IS '封面图URL';
COMMENT ON COLUMN stores.qr_code_url IS '店铺二维码URL';
COMMENT ON COLUMN stores.business_status IS '营业状态：open-营业中, closed-休息中';
COMMENT ON COLUMN stores.business_hours_start IS '营业开始时间';
COMMENT ON COLUMN stores.business_hours_end IS '营业结束时间';
COMMENT ON COLUMN stores.industry_category_id IS '行业分类ID（关联模板系统）';
COMMENT ON COLUMN stores.dining_mode IS '就餐方式：逗号分隔 dine_in-堂食,takeaway-打包,delivery-外卖';
COMMENT ON COLUMN stores.delivery_min_amount IS '外卖最低消费金额';
COMMENT ON COLUMN stores.packing_fee IS '打包费';
COMMENT ON COLUMN stores.monthly_sales IS '月销量（冗余字段，定时统计更新）';

CREATE INDEX ix_stores_merchant_id ON stores(merchant_id);
CREATE INDEX ix_stores_business_status ON stores(business_status);
CREATE INDEX ix_stores_industry_category_id ON stores(industry_category_id);

-- ============================================================
-- 3. 顾客用户表 (customers)
-- ============================================================
CREATE TABLE customers (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phone           VARCHAR(20),
    nickname        VARCHAR(50),
    avatar_url      VARCHAR(500),
    device_id       VARCHAR(200),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ,

    CONSTRAINT uq_customers_phone UNIQUE (phone)
);

COMMENT ON TABLE customers IS '顾客用户表';
COMMENT ON COLUMN customers.phone IS '手机号（注册后才有）';
COMMENT ON COLUMN customers.device_id IS '设备标识（匿名用户）';

CREATE INDEX ix_customers_phone ON customers(phone);
CREATE INDEX ix_customers_device_id ON customers(device_id);

-- ============================================================
-- 4. 平台管理员表 (admins)
-- ============================================================
CREATE TABLE admins (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username        VARCHAR(50) NOT NULL,
    password_hash   VARCHAR(200) NOT NULL,
    display_name    VARCHAR(50),
    role            VARCHAR(20) NOT NULL DEFAULT 'admin',
    status          VARCHAR(20) NOT NULL DEFAULT 'active',
    last_login_at   TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_admins_username UNIQUE (username),
    CONSTRAINT ck_admins_role CHECK (role IN ('super_admin', 'admin', 'viewer')),
    CONSTRAINT ck_admins_status CHECK (status IN ('active', 'disabled'))
);

COMMENT ON TABLE admins IS '平台管理员表';
COMMENT ON COLUMN admins.role IS '角色：super_admin-超级管理员, admin-管理员, viewer-只读';

-- 初始化超级管理员（密码：Admin@123，BCrypt hash）
INSERT INTO admins (username, password_hash, display_name, role)
VALUES ('admin', '$2a$11$placeholder_bcrypt_hash_for_admin', '系统管理员', 'super_admin');

-- ============================================================
-- 5. 店员表 (staff)
-- ============================================================
CREATE TABLE staff (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    name            VARCHAR(50) NOT NULL,
    phone           VARCHAR(20) NOT NULL,
    password_hash   VARCHAR(200) NOT NULL,
    permission      VARCHAR(20) NOT NULL DEFAULT 'order_only',
    status          VARCHAR(20) NOT NULL DEFAULT 'active',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ,

    CONSTRAINT fk_staff_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT uq_staff_phone UNIQUE (phone),
    CONSTRAINT ck_staff_permission CHECK (permission IN ('full', 'order_only')),
    CONSTRAINT ck_staff_status CHECK (status IN ('active', 'disabled'))
);

COMMENT ON TABLE staff IS '店员表';
COMMENT ON COLUMN staff.permission IS '权限：full-全部权限, order_only-仅接单';

CREATE INDEX ix_staff_store_id ON staff(store_id);
