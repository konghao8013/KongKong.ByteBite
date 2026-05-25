# 商家端 API 契约

> 版本：v1.1.0 | 更新日期：2026-05-22

## 商家信息

### GET /api/merchants/{id}

获取商家详情

**响应**：Merchant 实体（含 [NotMapped] Token 字段）

### GET /api/merchants/{merchantId}/stores

获取商家下的店铺列表

---

## 店铺管理

### GET /api/stores/{id}

获取店铺详情

**响应**：Store 实体

### GET /api/stores/merchant/{merchantId}

按商家ID获取店铺列表

### PUT /api/stores/{id}

更新店铺信息

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 否 | 店铺名称 |
| description | string | 否 | 店铺描述 |
| coverImageUrl | string | 否 | 封面图URL |
| businessStatus | string | 否 | 营业状态：open/closed |

---

## 分类管理

### GET /api/categories/store/{storeId}

获取店铺分类列表

### GET /api/categories/{id}

获取分类详情

### POST /api/categories

创建分类

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| storeId | string | 是 | 店铺ID |
| name | string | 是 | 分类名称 |
| sortOrder | int | 否 | 排序权重 |
| icon | string | 否 | 分类图标 |

### PUT /api/categories/{id}

更新分类

### DELETE /api/categories/{id}

删除分类（分类下有商品时返回错误）

---

## 商品管理

### GET /api/products/store/{storeId}

获取店铺商品列表（含 SpecGroups + SpecOptions 导航属性）

### GET /api/products/category/{categoryId}

按分类获取商品列表

### GET /api/products/{id}

获取商品详情

### POST /api/products

创建商品

### PUT /api/products/{id}

更新商品（含 status 切换：on/off/sold_out）

### DELETE /api/products/{id}

删除商品

---

## 优惠活动

### GET /api/discount-rules/store/{storeId}

获取店铺优惠规则列表

### GET /api/discount-rules/store/{storeId}/active

获取生效中的优惠规则

### POST /api/discount-rules

创建优惠规则

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 是 | 活动名称 |
| discountType | string | 是 | full_reduction/discount |
| thresholdAmount | decimal | 否 | 满减门槛金额 |
| discountAmount | decimal | 否 | 满减优惠金额 |
| discountRate | int | 否 | 折扣率（如 80 = 8折） |
| applyScope | string | 否 | all/category/product |
| storeId | string | 是 | 店铺ID |
| startTime | datetime | 否 | 开始时间 |
| endTime | datetime | 否 | 结束时间 |

### PUT /api/discount-rules/{id}

更新优惠规则（name/status）

### DELETE /api/discount-rules/{id}

删除优惠规则

---

## 经营数据

### GET /api/dashboard/{storeId}/overview

经营概览

**响应**：
```json
{
  "pendingOrderCount": 5,
  "yesterdayOrderCount": 18,
  "yesterdayRevenue": 1102.00,
  "todayOrderCount": 0,
  "todayRevenue": 0,
  "totalOrderCount": 18,
  "totalRevenue": 1102.00
}
```

### GET /api/dashboard/{storeId}/category-sales

分类销售统计

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| startDate | date | 否 | 开始日期 |
| endDate | date | 否 | 结束日期 |

### GET /api/dashboard/{storeId}/hourly

时段分布统计

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| date | date | 否 | 统计日期 |