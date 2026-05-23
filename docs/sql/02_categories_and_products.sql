-- ============================================================
-- 空空码上点单 (KongKong.ByteBite) - 模块2：分类与商品
-- 创建日期：2026-05-20
-- 说明：商品分类、商品、规格组、规格项、套餐
-- 依赖：01_users_and_stores.sql
-- ============================================================

-- ============================================================
-- 1. 商品分类表 (categories)
-- ============================================================
CREATE TABLE categories (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    name            VARCHAR(50) NOT NULL,
    icon            VARCHAR(50),
    category_type   VARCHAR(20) NOT NULL DEFAULT 'normal',
    sort_order      INTEGER NOT NULL DEFAULT 0,
    is_visible      BOOLEAN NOT NULL DEFAULT true,
    hot_top_count   INTEGER DEFAULT 10,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ,

    CONSTRAINT fk_categories_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT ck_categories_type CHECK (category_type IN ('normal', 'hot', 'welfare', 'combo'))
);

COMMENT ON TABLE categories IS '商品分类表';
COMMENT ON COLUMN categories.category_type IS '分类类型：normal-普通, hot-热销, welfare-进店福利, combo-套餐';
COMMENT ON COLUMN categories.hot_top_count IS '热销分类自动聚合的Top数量（仅hot类型有效）';

CREATE INDEX ix_categories_store_id ON categories(store_id);
CREATE INDEX ix_categories_sort_order ON categories(store_id, sort_order);

-- ============================================================
-- 2. 商品表 (products)
-- ============================================================
CREATE TABLE products (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    store_id        UUID NOT NULL REFERENCES stores(id),
    category_id     UUID NOT NULL REFERENCES categories(id),
    name            VARCHAR(100) NOT NULL,
    description     VARCHAR(500),
    base_price      DECIMAL(18,2) NOT NULL,
    image_url       VARCHAR(500),
    status          VARCHAR(20) NOT NULL DEFAULT 'off',
    sort_order      INTEGER NOT NULL DEFAULT 0,
    min_order_qty   INTEGER NOT NULL DEFAULT 1,
    monthly_sales   INTEGER NOT NULL DEFAULT 0,
    total_sales     INTEGER NOT NULL DEFAULT 0,
    is_combo        BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ,

    CONSTRAINT fk_products_store FOREIGN KEY (store_id) REFERENCES stores(id),
    CONSTRAINT fk_products_category FOREIGN KEY (category_id) REFERENCES categories(id),
    CONSTRAINT ck_products_status CHECK (status IN ('on', 'off', 'sold_out')),
    CONSTRAINT ck_products_base_price CHECK (base_price >= 0),
    CONSTRAINT ck_products_min_order_qty CHECK (min_order_qty >= 1)
);

COMMENT ON TABLE products IS '商品表';
COMMENT ON COLUMN products.base_price IS '基础价格（默认规格价格）';
COMMENT ON COLUMN products.status IS '状态：on-上架, off-下架, sold_out-售罄';
COMMENT ON COLUMN products.min_order_qty IS '最低起购数量';
COMMENT ON COLUMN products.monthly_sales IS '月销量（冗余，定时统计）';
COMMENT ON COLUMN products.total_sales IS '总销量';
COMMENT ON COLUMN products.is_combo IS '是否为套餐商品';

CREATE INDEX ix_products_store_id ON products(store_id);
CREATE INDEX ix_products_category_id ON products(category_id);
CREATE INDEX ix_products_status ON products(store_id, status);

-- ============================================================
-- 3. 规格组表 (spec_groups)
-- ============================================================
CREATE TABLE spec_groups (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id      UUID NOT NULL REFERENCES products(id),
    name            VARCHAR(50) NOT NULL,
    sort_order      INTEGER NOT NULL DEFAULT 0,
    is_required     BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_spec_groups_product FOREIGN KEY (product_id) REFERENCES products(id)
);

COMMENT ON TABLE spec_groups IS '规格组表（如：份量、辣度）';
COMMENT ON COLUMN spec_groups.is_required IS '是否必选';

CREATE INDEX ix_spec_groups_product_id ON spec_groups(product_id);

-- ============================================================
-- 4. 规格项表 (spec_options)
-- ============================================================
CREATE TABLE spec_options (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    spec_group_id   UUID NOT NULL REFERENCES spec_groups(id),
    name            VARCHAR(50) NOT NULL,
    extra_price     DECIMAL(18,2) NOT NULL DEFAULT 0,
    stock           INTEGER,
    sort_order      INTEGER NOT NULL DEFAULT 0,
    is_default      BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_spec_options_spec_group FOREIGN KEY (spec_group_id) REFERENCES spec_groups(id),
    CONSTRAINT ck_spec_options_extra_price CHECK (extra_price >= -9999.99)
);

COMMENT ON TABLE spec_options IS '规格项表（如：小份+0, 大份+10）';
COMMENT ON COLUMN spec_options.extra_price IS '加价（可为负数，如罐装比瓶装便宜）';
COMMENT ON COLUMN spec_options.stock IS '库存（null表示不限库存）';
COMMENT ON COLUMN spec_options.is_default IS '是否为默认选中项';

CREATE INDEX ix_spec_options_spec_group_id ON spec_options(spec_group_id);

-- ============================================================
-- 5. 套餐子商品表 (combo_items)
-- ============================================================
CREATE TABLE combo_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    combo_product_id UUID NOT NULL REFERENCES products(id),
    product_id      UUID NOT NULL REFERENCES products(id),
    quantity        INTEGER NOT NULL DEFAULT 1,
    default_spec_option_ids VARCHAR(500),
    allow_change_spec BOOLEAN NOT NULL DEFAULT false,
    remark          VARCHAR(100),
    sort_order      INTEGER NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_combo_items_combo FOREIGN KEY (combo_product_id) REFERENCES products(id),
    CONSTRAINT fk_combo_items_product FOREIGN KEY (product_id) REFERENCES products(id),
    CONSTRAINT ck_combo_items_quantity CHECK (quantity >= 1)
);

COMMENT ON TABLE combo_items IS '套餐子商品表';
COMMENT ON COLUMN combo_items.default_spec_option_ids IS '默认规格项ID列表（逗号分隔）';
COMMENT ON COLUMN combo_items.allow_change_spec IS '是否允许顾客替换规格';

CREATE INDEX ix_combo_items_combo_product_id ON combo_items(combo_product_id);
