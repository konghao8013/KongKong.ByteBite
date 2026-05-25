# M5 - 商家模板系统

> 模块编号：M5
> 状态：✅ 已完成
> SQL 文件：`docs/sql/04_template_system.sql`

---

## 1. 功能概述

系统管理员设置行业分类树和商家模板，商家可一键应用模板快速初始化店铺菜单。支持三种模板创建方式：从零创建、从商家引入、多商家组合。

---

## 2. 数据库表结构

### 2.1 industry_categories（行业分类表，三级自引用）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| name | VARCHAR(50) | NOT NULL | 分类名称 |
| parent_id | UUID | FK→industry_categories | 父级ID（顶级为null） |
| level | INTEGER | NOT NULL | 层级：1/2/3 |
| sort_order | INTEGER | DEFAULT 0 | 排序 |

### 2.2 store_templates（店铺模板表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| name | VARCHAR(100) | NOT NULL | 模板名称 |
| source_type | VARCHAR(20) | NOT NULL | 来源：manual/from_store/combined |
| industry_category_id | UUID | FK→industry_categories | 行业分类 |
| source_store_id | UUID | | 来源店铺ID |
| description | TEXT | | 描述 |
| usage_count | INTEGER | DEFAULT 0 | 使用次数 |
| status | VARCHAR(20) | DEFAULT 'active' | 状态 |

### 2.3 模板子表

- `template_categories`：模板分类（对应 store_categories）
- `template_products`：模板商品（对应 store_products）
- `template_spec_groups` / `template_spec_options`：模板规格
- `template_combo_items`：模板套餐项

---

## 3. API 端点

### 行业分类

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `api/industry-categories/tree` | 获取分类树 |
| POST | `api/industry-categories` | 创建分类 |
| PUT | `api/industry-categories/{id}` | 更新分类 |
| DELETE | `api/industry-categories/{id}` | 删除分类 |

### 模板管理

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `api/templates` | 模板列表 |
| GET | `api/templates/{id}` | 模板详情 |
| POST | `api/templates/from-scratch` | 从零创建模板 |
| POST | `api/templates/from-store` | 从商家引入模板 |
| POST | `api/templates/combined` | 多商家组合模板 |
| PUT | `api/templates/{id}` | 更新模板 |
| POST | `api/templates/{id}/categories` | 添加模板分类 |
| POST | `api/templates/{id}/products` | 添加模板商品 |
| DELETE | `api/templates/{templateId}/categories/{categoryId}` | 删除模板分类 |
| DELETE | `api/templates/{templateId}/products/{productId}` | 删除模板商品 |
| POST | `api/templates/apply` | 应用模板到店铺 |

---

## 4. 业务规则

- BR-1：行业分类支持三级自引用（如：餐饮→烧烤→重庆特色烧烤）
- BR-2：模板来源三种：manual（手动创建）、from_store（从商家引入）、combined（多商家组合）
- BR-3：应用模板时，将模板的分类和商品复制到目标店铺
- BR-4：应用模板后 usage_count 递增
- BR-5：组合模板可从多个店铺选取商品

---

## 5. 代码文件索引

| 层 | 文件 |
|----|------|
| Controller | `IndustryCategoriesController.cs`, `TemplatesController.cs` |
| Service | `IndustryCategoryService.cs`, `TemplateService.cs` |
| 仓储 | `IndustryCategoryRepository.cs`, `TemplateRepository.cs` |
| 实体 | `IndustryCategory.cs`, `StoreTemplate.cs`, `TemplateCategory.cs`, `TemplateProduct.cs`, `TemplateSpecGroup.cs`, `TemplateSpecOption.cs`, `TemplateComboItem.cs` |
