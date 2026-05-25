# M7 - 经营数据看板

> 模块编号：M7
> 状态：✅ 已完成
> SQL 文件：`docs/sql/06_business_dashboard.sql`

---

## 1. 功能概述

商家查看店铺经营数据，包括今日概览、商品销售排行、分类占比、营收趋势、时段分布、周对比、优惠效果分析。

---

## 2. 数据库表结构

| 表名 | 说明 |
|------|------|
| daily_store_stats | 每日店铺经营统计快照（唯一约束：store_id+stat_date） |
| product_sales_stats | 商品销售统计（唯一约束：store_id+product_id+stat_date） |
| customer_consumption_stats | 顾客消费统计（唯一约束：store_id+customer_id+device_id） |
| hourly_order_stats | 时段订单统计（唯一约束：store_id+stat_date+hour） |
| discount_effect_stats | 优惠活动效果统计（唯一约束：discount_rule_id+stat_date） |
| data_export_logs | 数据导出记录 |

---

## 3. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `api/dashboard/{storeId}/overview` | 经营概览（今日+昨日对比+待处理） |
| GET | `api/dashboard/{storeId}/product-sales` | 商品销售分析 |
| GET | `api/dashboard/{storeId}/category-sales` | 分类销售占比 |
| GET | `api/dashboard/{storeId}/customers` | 顾客消费记录（分页） |
| GET | `api/dashboard/{storeId}/revenue-trend` | 营收趋势 |
| GET | `api/dashboard/{storeId}/order-trend` | 订单量趋势 |
| GET | `api/dashboard/{storeId}/avg-order-trend` | 客单价趋势 |
| GET | `api/dashboard/{storeId}/hourly-distribution` | 时段分布 |
| GET | `api/dashboard/{storeId}/week-compare` | 本周vs上周对比 |
| GET | `api/dashboard/{storeId}/discount-effect` | 优惠效果分析（含ROI） |

---

## 4. 业务规则

- BR-1：概览页自动对比昨日数据，计算变化百分比
- BR-2：快捷时间范围：today/yesterday/last7days/last30days/custom
- BR-3：商品销售排行按销量降序
- BR-4：分类占比计算各分类销售额占总销售额的百分比
- BR-5：时段分布按小时（0-23）统计订单数和营收
- BR-6：周对比包含本周和上周数据，含 CompareValue
- BR-7：优惠效果 ROI = total_driven_revenue / total_discount_amount

---

## 5. 单元测试覆盖（5个）

概览/商品销售/趋势/时段分布
