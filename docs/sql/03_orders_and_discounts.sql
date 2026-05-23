-- ============================================================
-- 空空码上点单 (KongKong.ByteBite) - 模块3：订单与优惠
-- 创建日期：2026-05-20
-- 说明：订单、订单项、取货码、优惠活动
-- 依赖：01_users_and_stores.sql, 02_categories_and_products.sql
-- ============================================================

-- ============================================================
-- 1. 优惠活动表 (discount_rules)
-- ============================================================
CREATE TABLE discount_rules (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    name            VARCHAR(100) NOT NULL,
    discount_type   VARCHAR(20) NOT NULL,
    threshold_amount DECIMAL(18,2),
    discount_amount DECIMAL(18,2),
    discount_rate   DECIMAL(5,2),
    apply_scope     VARCHAR(20) NOT NULL DEFAULT 'all',
    apply_scope_id  UUID,
    allow_stack     BOOLEAN NOT NULL DEFAULT false,
    start_time      TIMESTAMPTZ NOT NULL,
    end_time        TIMESTAMPTZ NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'active',
    used_count      INTEGER NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ,

    CONSTRAINT fk_discount_rules_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT ck_discount_rules_type CHECK (discount_type IN ('full_reduction', 'discount')),
    CONSTRAINT ck_discount_rules_apply_scope CHECK (apply_scope IN ('all', 'category', 'product')),
    CONSTRAINT ck_discount_rules_status CHECK (status IN ('active', 'inactive')),
    CONSTRAINT ck_discount_rules_threshold CHECK (threshold_amount IS NULL OR threshold_amount >= 0),
    CONSTRAINT ck_discount_rules_discount_amount CHECK (discount_amount IS NULL OR discount_amount >= 0),
    CONSTRAINT ck_discount_rules_discount_rate CHECK (discount_rate IS NULL OR (discount_rate > 0 AND discount_rate <= 100))
);

COMMENT ON TABLE discount_rules IS '优惠活动表';
COMMENT ON COLUMN discount_rules.discount_type IS '类型：full_reduction-满减, discount-折扣';
COMMENT ON COLUMN discount_rules.threshold_amount IS '满减门槛金额（仅满减类型）';
COMMENT ON COLUMN discount_rules.discount_amount IS '满减减免金额（仅满减类型）';
COMMENT ON COLUMN discount_rules.discount_rate IS '折扣率（如80=8折，仅折扣类型）';
COMMENT ON COLUMN discount_rules.apply_scope IS '适用范围：all-全店, category-指定分类, product-指定商品';
COMMENT ON COLUMN discount_rules.apply_scope_id IS '适用范围的分类ID或商品ID';
COMMENT ON COLUMN discount_rules.allow_stack IS '是否允许与其他优惠叠加';

CREATE INDEX ix_discount_rules_store_id ON discount_rules(store_id);
CREATE INDEX ix_discount_rules_status ON discount_rules(store_id, status);

-- ============================================================
-- 2. 订单表 (orders)
-- ============================================================
CREATE TABLE orders (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_no        VARCHAR(30) NOT NULL,
    store_id        UUID NOT NULL REFERENCES stores(id),
    customer_id     UUID REFERENCES customers(id),
    device_id       VARCHAR(200),
    pickup_code     VARCHAR(10) NOT NULL,
    dining_mode     VARCHAR(20) NOT NULL DEFAULT 'dine_in',
    table_no        VARCHAR(20),
    delivery_address VARCHAR(500),
    delivery_phone  VARCHAR(20),
    total_amount    DECIMAL(18,2) NOT NULL DEFAULT 0,
    discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    actual_amount   DECIMAL(18,2) NOT NULL DEFAULT 0,
    packing_fee     DECIMAL(18,2) NOT NULL DEFAULT 0,
    discount_rule_id UUID REFERENCES discount_rules(id),
    remark          VARCHAR(500),
    status          VARCHAR(20) NOT NULL DEFAULT 'pending',
    reject_reason   VARCHAR(500),
    accepted_at     TIMESTAMPTZ,
    preparing_at    TIMESTAMPTZ,
    ready_at        TIMESTAMPTZ,
    completed_at    TIMESTAMPTZ,
    cancelled_at    TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_orders_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT fk_orders_customer FOREIGN KEY (customer_id) REFERENCES customers(id),
    CONSTRAINT fk_orders_discount_rule FOREIGN KEY (discount_rule_id) REFERENCES discount_rules(id),
    CONSTRAINT uq_orders_order_no UNIQUE (order_no),
    CONSTRAINT uq_orders_pickup_code_store UNIQUE (pickup_code, store_id),
    CONSTRAINT ck_orders_dining_mode CHECK (dining_mode IN ('dine_in', 'takeaway', 'delivery')),
    CONSTRAINT ck_orders_status CHECK (status IN ('pending', 'accepted', 'preparing', 'ready', 'completed', 'cancelled')),
    CONSTRAINT ck_orders_total_amount CHECK (total_amount >= 0),
    CONSTRAINT ck_orders_actual_amount CHECK (actual_amount >= 0)
);

COMMENT ON TABLE orders IS '订单表';
COMMENT ON COLUMN orders.order_no IS '订单编号（如：20260520001）';
COMMENT ON COLUMN orders.pickup_code IS '取货码（4-6位字母数字）';
COMMENT ON COLUMN orders.dining_mode IS '就餐方式：dine_in-堂食, takeaway-打包, delivery-外卖';
COMMENT ON COLUMN orders.total_amount IS '商品合计金额';
COMMENT ON COLUMN orders.discount_amount IS '优惠减免金额';
COMMENT ON COLUMN orders.actual_amount IS '应付金额 = total - discount + packing_fee';
COMMENT ON COLUMN orders.status IS '状态：pending-待接单, accepted-已接单, preparing-制作中, ready-待取餐, completed-已完成, cancelled-已取消';

CREATE INDEX ix_orders_store_id ON orders(store_id);
CREATE INDEX ix_orders_customer_id ON orders(customer_id);
CREATE INDEX ix_orders_status ON orders(store_id, status);
CREATE INDEX ix_orders_pickup_code ON orders(pickup_code, store_id);
CREATE INDEX ix_orders_created_at ON orders(store_id, created_at);

-- ============================================================
-- 3. 订单项表 (order_items)
-- ============================================================
CREATE TABLE order_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id        UUID NOT NULL REFERENCES orders(id),
    product_id      UUID NOT NULL REFERENCES products(id),
    product_name    VARCHAR(100) NOT NULL,
    product_image   VARCHAR(500),
    quantity        INTEGER NOT NULL DEFAULT 1,
    unit_price      DECIMAL(18,2) NOT NULL,
    total_price     DECIMAL(18,2) NOT NULL,
    spec_snapshot   JSONB,
    remark          VARCHAR(200),
    is_combo        BOOLEAN NOT NULL DEFAULT false,
    combo_items_snapshot JSONB,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_order_items_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT fk_order_items_product FOREIGN KEY (product_id) REFERENCES products(id),
    CONSTRAINT ck_order_items_quantity CHECK (quantity >= 1),
    CONSTRAINT ck_order_items_unit_price CHECK (unit_price >= 0),
    CONSTRAINT ck_order_items_total_price CHECK (total_price >= 0)
);

COMMENT ON TABLE order_items IS '订单项表';
COMMENT ON COLUMN order_items.spec_snapshot IS '规格快照（JSON，记录下单时的规格选择）';
COMMENT ON COLUMN order_items.combo_items_snapshot IS '套餐子商品快照（JSON，记录套餐内容）';

CREATE INDEX ix_order_items_order_id ON order_items(order_id);
CREATE INDEX ix_order_items_product_id ON order_items(product_id);
