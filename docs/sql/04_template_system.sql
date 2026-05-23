-- ============================================================
-- 空空码上点单 (KongKong.ByteBite) - 模块4：模板系统
-- 创建日期：2026-05-20
-- 说明：行业分类、模板、模板分类、模板商品、模板套餐
-- 依赖：01_users_and_stores.sql
-- ============================================================

-- ============================================================
-- 1. 行业分类表 (industry_categories)
-- ============================================================
CREATE TABLE industry_categories (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id       UUID REFERENCES industry_categories(id),
    name            VARCHAR(50) NOT NULL,
    level           INTEGER NOT NULL DEFAULT 1,
    sort_order      INTEGER NOT NULL DEFAULT 0,
    icon            VARCHAR(50),
    is_visible      BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT ck_industry_categories_level CHECK (level IN (1, 2, 3))
);

COMMENT ON TABLE industry_categories IS '行业分类表（三级）';
COMMENT ON COLUMN industry_categories.level IS '层级：1-一级行业(餐饮), 2-二级行业(烧烤), 3-三级行业(重庆特色烧烤)';

CREATE INDEX ix_industry_categories_parent_id ON industry_categories(parent_id);
CREATE INDEX ix_industry_categories_level ON industry_categories(level);

-- ============================================================
-- 2. 商家模板表 (store_templates)
-- ============================================================
CREATE TABLE store_templates (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(100) NOT NULL,
    industry_category_id UUID REFERENCES industry_categories(id),
    cover_image_url     VARCHAR(500),
    description         VARCHAR(500),
    source_type         VARCHAR(20) NOT NULL DEFAULT 'manual',
    source_store_ids    VARCHAR(500),
    status              VARCHAR(20) NOT NULL DEFAULT 'active',
    use_count           INTEGER NOT NULL DEFAULT 0,
    created_by          UUID REFERENCES admins(id),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at          TIMESTAMPTZ,

    CONSTRAINT fk_store_templates_industry FOREIGN KEY (industry_category_id) REFERENCES industry_categories(id),
    CONSTRAINT ck_store_templates_source_type CHECK (source_type IN ('manual', 'from_store', 'combined')),
    CONSTRAINT ck_store_templates_status CHECK (status IN ('active', 'inactive'))
);

COMMENT ON TABLE store_templates IS '商家模板表';
COMMENT ON COLUMN store_templates.source_type IS '创建方式：manual-从零创建, from_store-从商家引入, combined-多商家组合';
COMMENT ON COLUMN store_templates.source_store_ids IS '来源商家ID列表（逗号分隔，从商家引入/组合时记录）';
COMMENT ON COLUMN store_templates.use_count IS '被应用次数';

CREATE INDEX ix_store_templates_industry_category_id ON store_templates(industry_category_id);
CREATE INDEX ix_store_templates_status ON store_templates(status);

-- ============================================================
-- 3. 模板分类表 (template_categories)
-- ============================================================
CREATE TABLE template_categories (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_id     UUID NOT NULL REFERENCES store_templates(id),
    name            VARCHAR(50) NOT NULL,
    category_type   VARCHAR(20) NOT NULL DEFAULT 'normal',
    icon            VARCHAR(50),
    sort_order      INTEGER NOT NULL DEFAULT 0,
    hot_top_count   INTEGER DEFAULT 10,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_template_categories_template FOREIGN KEY (template_id) REFERENCES store_templates(id),
    CONSTRAINT ck_template_categories_type CHECK (category_type IN ('normal', 'hot', 'welfare', 'combo'))
);

COMMENT ON TABLE template_categories IS '模板分类表';

CREATE INDEX ix_template_categories_template_id ON template_categories(template_id);

-- ============================================================
-- 4. 模板商品表 (template_products)
-- ============================================================
CREATE TABLE template_products (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_id     UUID NOT NULL REFERENCES store_templates(id),
    template_category_id UUID NOT NULL REFERENCES template_categories(id),
    name            VARCHAR(100) NOT NULL,
    description     VARCHAR(500),
    reference_price DECIMAL(18,2) NOT NULL,
    image_url       VARCHAR(500),
    sort_order      INTEGER NOT NULL DEFAULT 0,
    min_order_qty   INTEGER NOT NULL DEFAULT 1,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_template_products_template FOREIGN KEY (template_id) REFERENCES store_templates(id),
    CONSTRAINT fk_template_products_category FOREIGN KEY (template_category_id) REFERENCES template_categories(id),
    CONSTRAINT ck_template_products_reference_price CHECK (reference_price >= 0)
);

COMMENT ON TABLE template_products IS '模板商品表';
COMMENT ON COLUMN template_products.reference_price IS '参考价格（市场常见价）';

CREATE INDEX ix_template_products_template_id ON template_products(template_id);
CREATE INDEX ix_template_products_category_id ON template_products(template_category_id);

-- ============================================================
-- 5. 模板规格组表 (template_spec_groups)
-- ============================================================
CREATE TABLE template_spec_groups (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_product_id UUID NOT NULL REFERENCES template_products(id),
    name            VARCHAR(50) NOT NULL,
    sort_order      INTEGER NOT NULL DEFAULT 0,
    is_required     BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_template_spec_groups_product FOREIGN KEY (template_product_id) REFERENCES template_products(id)
);

COMMENT ON TABLE template_spec_groups IS '模板规格组表';

CREATE INDEX ix_template_spec_groups_product_id ON template_spec_groups(template_product_id);

-- ============================================================
-- 6. 模板规格项表 (template_spec_options)
-- ============================================================
CREATE TABLE template_spec_options (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_spec_group_id UUID NOT NULL REFERENCES template_spec_groups(id),
    name            VARCHAR(50) NOT NULL,
    extra_price     DECIMAL(18,2) NOT NULL DEFAULT 0,
    sort_order      INTEGER NOT NULL DEFAULT 0,
    is_default      BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_template_spec_options_group FOREIGN KEY (template_spec_group_id) REFERENCES template_spec_groups(id)
);

COMMENT ON TABLE template_spec_options IS '模板规格项表';

CREATE INDEX ix_template_spec_options_group_id ON template_spec_options(template_spec_group_id);

-- ============================================================
-- 7. 模板套餐子商品表 (template_combo_items)
-- ============================================================
CREATE TABLE template_combo_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    combo_template_product_id UUID NOT NULL REFERENCES template_products(id),
    template_product_id UUID NOT NULL REFERENCES template_products(id),
    quantity        INTEGER NOT NULL DEFAULT 1,
    remark          VARCHAR(100),
    sort_order      INTEGER NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT fk_template_combo_items_combo FOREIGN KEY (combo_template_product_id) REFERENCES template_products(id),
    CONSTRAINT fk_template_combo_items_product FOREIGN KEY (template_product_id) REFERENCES template_products(id),
    CONSTRAINT ck_template_combo_items_quantity CHECK (quantity >= 1)
);

COMMENT ON TABLE template_combo_items IS '模板套餐子商品表';

CREATE INDEX ix_template_combo_items_combo_id ON template_combo_items(combo_template_product_id);
