# M2 - 分类/商品管理

> 模块编号：M2
> 状态：✅ 已完成
> SQL 文件：`docs/sql/02_categories_and_products.sql`

---

## 1. 功能概述

商家管理店铺内的商品分类和商品信息。支持分类排序、商品多规格（大份/小份）、套餐商品、最低起购数量。

---

## 2. 数据库表结构

### 2.1 categories（分类表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| store_id | UUID | FK→stores, NOT NULL | 所属店铺ID |
| name | VARCHAR(50) | NOT NULL | 分类名称 |
| category_type | VARCHAR(20) | NOT NULL, DEFAULT 'normal' | 类型：normal/hot/welfare/combo |
| sort_order | INTEGER | NOT NULL, DEFAULT 0 | 排序值 |
| created_at / updated_at / deleted_at | TIMESTAMPTZ | | 时间戳 |

### 2.2 products（商品表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| store_id | UUID | FK→stores | 所属店铺 |
| category_id | UUID | FK→categories | 所属分类 |
| name | VARCHAR(100) | NOT NULL | 商品名称 |
| base_price | DECIMAL(18,2) | NOT NULL | 基础价格 |
| description | TEXT | | 描述 |
| image_url | VARCHAR(500) | | 图片URL |
| status | VARCHAR(20) | DEFAULT 'off' | 状态：on/off |
| is_sold_out | BOOLEAN | DEFAULT false | 是否售罄 |
| min_order_qty | INTEGER | DEFAULT 1 | 最低起购数量 |
| is_combo | BOOLEAN | DEFAULT false | 是否套餐 |
| sort_order | INTEGER | DEFAULT 0 | 排序 |
| created_at / updated_at / deleted_at | TIMESTAMPTZ | | 时间戳 |

### 2.3 spec_groups（规格组表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| product_id | UUID | FK→products | 所属商品 |
| name | VARCHAR(50) | NOT NULL | 规格组名称（如"份量"） |
| is_required | BOOLEAN | DEFAULT true | 是否必选 |
| max_select | INTEGER | DEFAULT 1 | 最多可选数量 |

### 2.4 spec_options（规格选项表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| spec_group_id | UUID | FK→spec_groups | 所属规格组 |
| name | VARCHAR(50) | NOT NULL | 选项名称（如"大份"） |
| extra_price | DECIMAL(18,2) | DEFAULT 0 | 额外价格（可为负） |

### 2.5 combo_items（套餐项表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| combo_product_id | UUID | FK→products | 所属套餐商品 |
| included_product_id | UUID | FK→products | 包含的商品 |
| quantity | INTEGER | DEFAULT 1 | 数量 |
| default_spec_option_ids | JSONB | | 默认规格选项ID列表 |

---

## 3. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `api/stores/{storeId}/categories` | 获取店铺分类列表 |
| POST | `api/stores/{storeId}/categories` | 创建分类 |
| PUT | `api/categories/{id}` | 更新分类 |
| DELETE | `api/categories/{id}` | 删除分类（软删除） |
| PATCH | `api/categories/{id}/sort` | 更新分类排序 |
| GET | `api/categories/{categoryId}/products` | 按分类查商品 |
| GET | `api/stores/{storeId}/products` | 按店铺查商品 |
| POST | `api/stores/{storeId}/products` | 创建商品 |
| PUT | `api/products/{id}` | 更新商品 |
| DELETE | `api/products/{id}` | 删除商品（软删除） |
| PATCH | `api/products/batch-status` | 批量上下架 |

---

## 4. 代码文件索引

| 层 | 文件 |
|----|------|
| Controller | `CategoriesController.cs`, `ProductsController.cs` |
| Service | `CategoryService.cs`, `ProductService.cs` |
| 仓储 | `CategoryRepository.cs`, `ProductRepository.cs` |
| DTO | `DTOs/Category/`, `DTOs/Product/` |
| 验证器 | `CreateCategoryRequestValidator.cs`, `CreateProductRequestValidator.cs` |
| 实体 | `Category.cs`, `Product.cs`, `SpecGroup.cs`, `SpecOption.cs`, `ComboItem.cs` |

---

## 5. 业务规则

- BR-1：分类类型支持 normal（普通）、hot（热销）、welfare（福利）、combo（套餐）
- BR-2：商品状态 on=上架、off=下架，新创建默认 off
- BR-3：规格选项的 extra_price 可为负数（如小份减价）
- BR-4：套餐商品通过 is_combo=true 标识，combo_items 记录包含的商品
- BR-5：min_order_qty 控制最低起购数量，下单时校验
- BR-6：删除为软删除（设置 deleted_at）
- BR-7：批量上下架通过 PATCH batch-status 接口

---

## 6. 单元测试覆盖（16个）

- CategoryService: 7个（创建/更新/删除/排序/查询）
- ProductService: 9个（创建/更新/删除/批量上下架/分类验证/查询）
