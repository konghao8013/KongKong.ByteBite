# 数据模型总览

> 版本：v1.2.0 | 更新日期：2026-05-25

## 实体关系图（核心）

```
┌──────────┐     ┌───────────┐     ┌──────────┐
│  Admin   │     │ Merchant  │     │ Customer │
└──────────┘     └─────┬─────┘     └────┬─────┘
                       │                 │
                       │ 1:N             │ N:M
                       ▼                 ▼
                 ┌───────────┐     ┌──────────┐
                 │   Store   │◄────│  Order   │
                 └─────┬─────┘     └────┬─────┘
                       │                │
           ┌───────────┼──────────┐     │ 1:N
           │           │          │     ▼
           ▼           ▼          ▼  ┌──────────┐
      ┌─────────┐ ┌────────┐ ┌──────┐│ OrderItem│
      │Category │ │Discount│ │Stats │└──────────┘
      └────┬────┘ │ Rule   │ └──────┘
           │      └────────┘
           │ 1:N
           ▼
      ┌─────────┐
      │ Product │
      └────┬────┘
           │ 1:N
           ▼
      ┌──────────┐
      │SpecGroup │
      └────┬─────┘
           │ 1:N
           ▼
      ┌──────────┐
      │SpecOption│
      └──────────┘
```

## 实体清单

### 认证与用户

| 实体 | 表名 | 说明 | 关键字段 |
|------|------|------|----------|
| Admin | admins | 管理员 | username, passwordHash, role, status |
| Merchant | merchants | 商家 | phone, passwordHash, nickname, status |
| Customer | customers | 顾客 | phone, passwordHash, nickname |
| Staff | staffs | 店员（预留） | storeId, role |
| CustomerSession | customer_sessions | 顾客会话 | customerId, deviceId, token |

### 店铺与菜单

| 实体 | 表名 | 说明 | 关键字段 |
|------|------|------|----------|
| Store | stores | 店铺 | name, merchantId, storeCode, businessStatus, industryCategoryId |
| Category | categories | 分类 | name, storeId, sortOrder, icon |
| Product | products | 商品 | name, basePrice, categoryId, storeId, status |
| SpecGroup | spec_groups | 规格组 | name, productId |
| SpecOption | spec_options | 规格选项 | name, extraPrice, specGroupId |
| ComboItem | combo_items | 套餐项（预留） | productId, comboProductId |

### 订单

| 实体 | 表名 | 说明 | 关键字段 |
|------|------|------|----------|
| Order | orders | 订单 | orderNo, pickupCode, storeId, status, actualAmount |
| OrderItem | order_items | 订单项 | productName, quantity, totalPrice, orderId, productId |

### 优惠

| 实体 | 表名 | 说明 | 关键字段 |
|------|------|------|----------|
| DiscountRule | discount_rules | 优惠规则 | name, discountType, thresholdAmount, discountAmount, storeId |
| DiscountEffectStat | discount_effect_stats | 优惠效果统计 | discountRuleId, statDate |

### 模板

| 实体 | 表名 | 说明 | 关键字段 |
|------|------|------|----------|
| IndustryCategory | industry_categories | 行业分类 | name, level, parentId |
| StoreTemplate | store_templates | 店铺模板 | name, industryCategoryId |
| TemplateCategory | template_categories | 模板分类 | name, templateId |
| TemplateProduct | template_products | 模板商品 | name, basePrice, templateCategoryId |
| TemplateSpecGroup | template_spec_groups | 模板规格组 | name, templateProductId |
| TemplateSpecOption | template_spec_options | 模板规格选项 | name, extraPrice |
| TemplateComboItem | template_combo_items | 模板套餐项 | templateProductId |

### 统计

| 实体 | 表名 | 说明 | 关键字段 |
|------|------|------|----------|
| DailyStoreStat | daily_store_stats | 每日店铺统计 | storeId, statDate, orderCount, revenue |
| HourlyOrderStat | hourly_order_stats | 时段统计 | storeId, statDate, hour, orderCount |
| ProductSalesStat | product_sales_stats | 商品销售统计 | storeId, productId, statDate, salesQuantity |
| PlatformDailyStat | platform_daily_stats | 平台每日统计 | statDate, totalMerchants, totalOrders |
| CustomerConsumptionStat | customer_consumption_stats | 顾客消费统计 | customerId, totalOrders, totalSpent |

### 审计与日志

| 实体 | 表名 | 说明 | 关键字段 |
|------|------|------|----------|
| MerchantAuditLog | merchant_audit_logs | 商家审计日志 | merchantId, adminId, action, reason |
| AdminOperationLog | admin_operation_logs | 管理员操作日志 | adminId, action |
| DataMergeLog | data_merge_logs | 数据合并日志 | customerId |
| DataExportLog | data_export_logs | 数据导出日志 | adminId |
| SystemConfig | system_configs | 系统配置 | key, value |

## 状态枚举

### 商家状态 (Merchant.Status)

| 值 | 说明 | 可登录 |
|----|------|--------|
| active | 正常运营 | ✅ |
| pending | 待审核 | ❌ |
| suspended | 已封禁 | ❌ |
| disabled | 已禁用 | ❌ |

### 商品状态 (Product.Status)

| 值 | 说明 | 顾客端可见 |
|----|------|-----------|
| on | 上架 | ✅ 正常展示 |
| off | 下架 | ❌ 不展示 |
| sold_out | 售罄 | ✅ 灰色标记 |

### 订单状态 (Order.Status)

参见 [订单 API 契约](../contracts/order.md)

### 店铺营业状态 (Store.BusinessStatus)

| 值 | 说明 | 顾客端影响 |
|----|------|-----------|
| open | 营业中 | 可正常下单 |
| closed | 休息中 | 可浏览但不可下单 |

### 优惠类型 (DiscountRule.DiscountType)

| 值 | 说明 |
|----|------|
| full_reduction | 满减（满X减Y） |
| discount | 折扣（X折） |

### 优惠状态 (DiscountRule.Status)

| 值 | 说明 |
|----|------|
| active | 生效中 |
| inactive | 已停用 |

## Partial Class 扩展

| 扩展文件 | 原实体 | 扩展字段 | 用途 |
|----------|--------|----------|------|
| AdminPartial.cs | Admin | Token | 登录后生成鉴权Token |
| MerchantPartial.cs | Merchant | Token | 登录后生成鉴权Token |
| CustomerPartial.cs | Customer | Token | 登录后生成鉴权Token |

扩展文件位于 `ByteBite.Infrastructure/Persistence/Extensions/Entities/`，命名空间与原实体相同，非数据库字段标注 `[NotMapped]`。

> **设计原则**：EF 实体文件（`Persistence/Entities/`）只包含数据库结构相关属性，保持干净；非数据库字段通过 partial class 扩展添加。</toolcall_result> </toolcall_result>
<tool_changes>
The tool made the following changes to the file `g:\code\KongKong.ByteBite\docs\data/overview.md`:
```
@@ -158,7 +161,7 @@
  |----------|--------|----------|------|
- | AdminPartial.cs | Admin | Token | 登录后生成鉴权Token |
- | MerchantPartial.cs | Merchant | Token | 登录后生成鉴权Token |
- | CustomerPartial.cs | Customer | Token | 登录后生成鉴权Token |
+ | Admin.cs | Admin Token (登录后生成鉴权Token", `[NotMapped]`)
+ | Merchant.cs | Merchant Token (登录后生成鉴权Token", `[NotMapped]`)
+ | Customer.cs | Customer Token (登录后生成鉴权Token", `[NotMapped]`)

- 扩展文件位于 `ByteBite.Infrastructure/Extensions/Entities/``命名空间与原实体相同,非数据库字段标注 `[NotMapped]`。
+ 以上非数据库字段已合并到实体本体中，不再使用 partial class 扩展。