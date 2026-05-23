# 数据库结构文档

本文件记录空空码上点单（KongKong.ByteBite）的完整数据库结构。

## 连接信息

| 项目 | 值 |
|------|---|
| Host | 192.168.3.22 |
| Port | 5432 |
| Database | kongkong_bytebite |
| Username | konghao |

## 表总览（20 张表）

| # | 表名 | 模块 | 说明 |
|---|------|------|------|
| 1 | merchants | 用户与店铺 | 商家用户表 |
| 2 | stores | 用户与店铺 | 店铺表 |
| 3 | customers | 用户与店铺 | 顾客用户表 |
| 4 | admins | 用户与店铺 | 平台管理员表 |
| 5 | staff | 用户与店铺 | 店员表 |
| 6 | categories | 分类与商品 | 商品分类表 |
| 7 | products | 分类与商品 | 商品表 |
| 8 | spec_groups | 分类与商品 | 规格组表 |
| 9 | spec_options | 分类与商品 | 规格项表 |
| 10 | combo_items | 分类与商品 | 套餐子商品表 |
| 11 | discount_rules | 订单与优惠 | 优惠活动表 |
| 12 | orders | 订单与优惠 | 订单表 |
| 13 | order_items | 订单与优惠 | 订单项表 |
| 14 | industry_categories | 模板系统 | 行业分类表（三级） |
| 15 | store_templates | 模板系统 | 商家模板表 |
| 16 | template_categories | 模板系统 | 模板分类表 |
| 17 | template_products | 模板系统 | 模板商品表 |
| 18 | template_spec_groups | 模板系统 | 模板规格组表 |
| 19 | template_spec_options | 模板系统 | 模板规格项表 |
| 20 | template_combo_items | 模板系统 | 模板套餐子商品表 |

## ER 关系图

```
merchants ──1:N── stores ──1:N── categories ──1:N── products
    │                │                           ├── 1:N ── spec_groups ──1:N── spec_options
    │                │                           └── 1:N ── combo_items (is_combo=true)
    │                │
    │                ├── 1:N ── staff
    │                ├── 1:N ── discount_rules
    │                └── 1:N ── orders ──1:N── order_items
    │                                │
    │                                ├── pickup_code (唯一 per store)
    │                                └── customer_id → customers
    │
    └── customers (独立，可选关联 orders)

admins (独立)

industry_categories (自引用三级树) ──1:N── store_templates
    └── store_templates ──1:N── template_categories ──1:N── template_products
                                                    └── template_products
                                                        ├── 1:N ── template_spec_groups ──1:N── template_spec_options
                                                        └── 1:N ── template_combo_items
```

## 各表详细结构

### merchants（商家用户表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK, DEFAULT gen_random_uuid() | 主键 |
| phone | VARCHAR(20) | NOT NULL, UNIQUE | 手机号（登录账号） |
| password_hash | VARCHAR(200) | NOT NULL | BCrypt 加密密码 |
| nickname | VARCHAR(50) | | 昵称 |
| avatar_url | VARCHAR(500) | | 头像 URL |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'active', CHECK: pending/active/disabled | 状态 |
| last_login_at | TIMESTAMPTZ | | 最后登录时间 |
| created_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() | 创建时间 |
| updated_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() | 更新时间 |
| deleted_at | TIMESTAMPTZ | | 软删除时间 |

### stores（店铺表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| merchant_id | UUID | FK → merchants, NOT NULL | 所属商家 |
| name | VARCHAR(100) | NOT NULL | 店铺名称 |
| description | VARCHAR(500) | | 描述 |
| cover_image_url | VARCHAR(500) | | 封面图 |
| qr_code_url | VARCHAR(500) | | 二维码 |
| business_status | VARCHAR(20) | DEFAULT 'open', CHECK: open/closed | 营业状态 |
| business_hours_start | TIME | | 营业开始 |
| business_hours_end | TIME | | 营业结束 |
| industry_category_id | UUID | | 行业分类 ID |
| dining_mode | VARCHAR(100) | DEFAULT 'dine_in,takeaway' | 就餐方式（逗号分隔） |
| delivery_min_amount | DECIMAL(18,2) | DEFAULT 0 | 外卖最低消费 |
| packing_fee | DECIMAL(18,2) | DEFAULT 0 | 打包费 |
| monthly_sales | INTEGER | DEFAULT 0 | 月销量 |
| created_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() | |
| updated_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() | |
| deleted_at | TIMESTAMPTZ | | |

### categories（商品分类表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| store_id | UUID | FK → stores | 所属店铺 |
| name | VARCHAR(50) | NOT NULL | 分类名称 |
| icon | VARCHAR(50) | | 图标 |
| category_type | VARCHAR(20) | DEFAULT 'normal', CHECK: normal/hot/welfare/combo | 分类类型 |
| sort_order | INTEGER | DEFAULT 0 | 排序 |
| is_visible | BOOLEAN | DEFAULT true | 是否显示 |
| hot_top_count | INTEGER | DEFAULT 10 | 热销 Top 数量 |
| created_at | TIMESTAMPTZ | NOT NULL | |
| updated_at | TIMESTAMPTZ | NOT NULL | |
| deleted_at | TIMESTAMPTZ | | |

### products（商品表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| store_id | UUID | FK → stores | 所属店铺 |
| category_id | UUID | FK → categories | 所属分类 |
| name | VARCHAR(100) | NOT NULL | 商品名称 |
| description | VARCHAR(500) | | 描述 |
| base_price | DECIMAL(18,2) | NOT NULL, >= 0 | 基础价格 |
| image_url | VARCHAR(500) | | 图片 |
| status | VARCHAR(20) | DEFAULT 'off', CHECK: on/off/sold_out | 状态 |
| sort_order | INTEGER | DEFAULT 0 | 排序 |
| min_order_qty | INTEGER | DEFAULT 1, >= 1 | 最低起购数量 |
| monthly_sales | INTEGER | DEFAULT 0 | 月销量 |
| total_sales | INTEGER | DEFAULT 0 | 总销量 |
| is_combo | BOOLEAN | DEFAULT false | 是否套餐 |
| created_at | TIMESTAMPTZ | NOT NULL | |
| updated_at | TIMESTAMPTZ | NOT NULL | |
| deleted_at | TIMESTAMPTZ | | |

### spec_groups（规格组表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| product_id | UUID | FK → products | 所属商品 |
| name | VARCHAR(50) | NOT NULL | 规格组名称（如：份量） |
| sort_order | INTEGER | DEFAULT 0 | 排序 |
| is_required | BOOLEAN | DEFAULT true | 是否必选 |

### spec_options（规格项表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| spec_group_id | UUID | FK → spec_groups | 所属规格组 |
| name | VARCHAR(50) | NOT NULL | 规格名称（如：大份） |
| extra_price | DECIMAL(18,2) | DEFAULT 0 | 加价 |
| stock | INTEGER | | 库存（null=不限） |
| sort_order | INTEGER | DEFAULT 0 | 排序 |
| is_default | BOOLEAN | DEFAULT false | 是否默认选中 |

### combo_items（套餐子商品表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| combo_product_id | UUID | FK → products | 套餐商品 ID |
| product_id | UUID | FK → products | 子商品 ID |
| quantity | INTEGER | DEFAULT 1, >= 1 | 数量 |
| default_spec_option_ids | VARCHAR(500) | | 默认规格项 ID（逗号分隔） |
| allow_change_spec | BOOLEAN | DEFAULT false | 允许替换规格 |
| remark | VARCHAR(100) | | 备注 |
| sort_order | INTEGER | DEFAULT 0 | 排序 |

### orders（订单表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| order_no | VARCHAR(30) | NOT NULL, UNIQUE | 订单编号 |
| store_id | UUID | FK → stores | 所属店铺 |
| customer_id | UUID | FK → customers | 顾客 ID（可 null） |
| device_id | VARCHAR(200) | | 设备标识 |
| pickup_code | VARCHAR(10) | NOT NULL, UNIQUE per store | 取货码 |
| dining_mode | VARCHAR(20) | DEFAULT 'dine_in', CHECK: dine_in/takeaway/delivery | 就餐方式 |
| table_no | VARCHAR(20) | | 桌号 |
| delivery_address | VARCHAR(500) | | 外卖地址 |
| delivery_phone | VARCHAR(20) | | 外卖电话 |
| total_amount | DECIMAL(18,2) | DEFAULT 0 | 商品合计 |
| discount_amount | DECIMAL(18,2) | DEFAULT 0 | 优惠金额 |
| actual_amount | DECIMAL(18,2) | DEFAULT 0 | 应付金额 |
| packing_fee | DECIMAL(18,2) | DEFAULT 0 | 打包费 |
| discount_rule_id | UUID | FK → discount_rules | 使用的优惠活动 |
| remark | VARCHAR(500) | | 整单备注 |
| status | VARCHAR(20) | DEFAULT 'pending', CHECK: pending/accepted/preparing/ready/completed/cancelled | 订单状态 |
| reject_reason | VARCHAR(500) | | 拒单原因 |
| accepted_at | TIMESTAMPTZ | | 接单时间 |
| preparing_at | TIMESTAMPTZ | | 开始制作时间 |
| ready_at | TIMESTAMPTZ | | 备餐完成时间 |
| completed_at | TIMESTAMPTZ | | 完成时间 |
| cancelled_at | TIMESTAMPTZ | | 取消时间 |
| created_at | TIMESTAMPTZ | NOT NULL | |
| updated_at | TIMESTAMPTZ | NOT NULL | |

### order_items（订单项表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| order_id | UUID | FK → orders | 所属订单 |
| product_id | UUID | FK → products | 商品 ID |
| product_name | VARCHAR(100) | NOT NULL | 商品名称（快照） |
| product_image | VARCHAR(500) | | 商品图片（快照） |
| quantity | INTEGER | DEFAULT 1, >= 1 | 数量 |
| unit_price | DECIMAL(18,2) | NOT NULL, >= 0 | 单价 |
| total_price | DECIMAL(18,2) | NOT NULL, >= 0 | 小计 |
| spec_snapshot | JSONB | | 规格快照 |
| remark | VARCHAR(200) | | 备注 |
| is_combo | BOOLEAN | DEFAULT false | 是否套餐 |
| combo_items_snapshot | JSONB | | 套餐子商品快照 |

### discount_rules（优惠活动表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| store_id | UUID | FK → stores | 所属店铺 |
| name | VARCHAR(100) | NOT NULL | 活动名称 |
| discount_type | VARCHAR(20) | CHECK: full_reduction/discount | 类型 |
| threshold_amount | DECIMAL(18,2) | | 满减门槛 |
| discount_amount | DECIMAL(18,2) | | 满减金额 |
| discount_rate | DECIMAL(5,2) | | 折扣率（80=8折） |
| apply_scope | VARCHAR(20) | DEFAULT 'all', CHECK: all/category/product | 适用范围 |
| apply_scope_id | UUID | | 范围 ID |
| allow_stack | BOOLEAN | DEFAULT false | 允许叠加 |
| start_time | TIMESTAMPTZ | NOT NULL | 开始时间 |
| end_time | TIMESTAMPTZ | NOT NULL | 结束时间 |
| status | VARCHAR(20) | DEFAULT 'active' | 状态 |
| used_count | INTEGER | DEFAULT 0 | 使用次数 |
| created_at | TIMESTAMPTZ | NOT NULL | |
| updated_at | TIMESTAMPTZ | NOT NULL | |
| deleted_at | TIMESTAMPTZ | | |

### industry_categories（行业分类表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| parent_id | UUID | FK → industry_categories (自引用) | 父分类 |
| name | VARCHAR(50) | NOT NULL | 分类名称 |
| level | INTEGER | CHECK: 1/2/3 | 层级 |
| sort_order | INTEGER | DEFAULT 0 | 排序 |
| icon | VARCHAR(50) | | 图标 |
| is_visible | BOOLEAN | DEFAULT true | 是否显示 |

### store_templates（商家模板表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| name | VARCHAR(100) | NOT NULL | 模板名称 |
| industry_category_id | UUID | FK → industry_categories | 行业分类 |
| cover_image_url | VARCHAR(500) | | 封面图 |
| description | VARCHAR(500) | | 描述 |
| source_type | VARCHAR(20) | DEFAULT 'manual', CHECK: manual/from_store/combined | 创建方式 |
| source_store_ids | VARCHAR(500) | | 来源商家 ID 列表 |
| status | VARCHAR(20) | DEFAULT 'active' | 状态 |
| use_count | INTEGER | DEFAULT 0 | 使用次数 |
| created_by | UUID | FK → admins | 创建者 |
| created_at | TIMESTAMPTZ | NOT NULL | |
| updated_at | TIMESTAMPTZ | NOT NULL | |
| deleted_at | TIMESTAMPTZ | | |

### customers / admins / staff

详见 SQL 文件：`docs/sql/01_users_and_stores.sql`

### template_* 系列表

模板表结构与业务表对应关系：

| 业务表 | 模板表 | 差异 |
|--------|--------|------|
| categories | template_categories | 无 store_id，有 template_id |
| products | template_products | base_price → reference_price，无 status |
| spec_groups | template_spec_groups | 同结构 |
| spec_options | template_spec_options | 无 stock |
| combo_items | template_combo_items | 同结构 |

## SQL 文件清单

| 文件 | 说明 |
|------|------|
| `docs/sql/01_users_and_stores.sql` | 商家、店铺、顾客、管理员、店员 |
| `docs/sql/02_categories_and_products.sql` | 分类、商品、规格组、规格项、套餐 |
| `docs/sql/03_orders_and_discounts.sql` | 优惠活动、订单、订单项 |
| `docs/sql/04_template_system.sql` | 行业分类、模板、模板分类、模板商品、模板规格 |
