# M4 - 优惠活动管理

> 模块编号：M4
> 状态：✅ 已完成
> SQL 文件：`docs/sql/03_orders_and_discounts.sql`

---

## 1. 功能概述

商家创建和管理优惠活动（满减/折扣），顾客下单时自动匹配最优优惠。

---

## 2. 数据库表结构

### discount_rules（优惠规则表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| store_id | UUID | FK→stores | 店铺ID |
| name | VARCHAR(100) | NOT NULL | 活动名称 |
| type | VARCHAR(20) | NOT NULL | 类型：full_reduction/discount |
| threshold_amount | DECIMAL(18,2) | DEFAULT 0 | 门槛金额（满减） |
| discount_amount | DECIMAL(18,2) | DEFAULT 0 | 减免金额（满减） |
| discount_rate | DECIMAL(5,4) | DEFAULT 0 | 折扣率（折扣，如0.8=8折） |
| apply_scope | VARCHAR(20) | DEFAULT 'all' | 适用范围：all/category/product |
| allow_stack | BOOLEAN | DEFAULT false | 是否允许叠加 |
| start_time | TIMESTAMPTZ | | 开始时间 |
| end_time | TIMESTAMPTZ | | 结束时间 |
| status | VARCHAR(20) | DEFAULT 'active' | 状态：active/inactive |
| used_count | INTEGER | DEFAULT 0 | 已使用次数 |
| created_at / updated_at / deleted_at | TIMESTAMPTZ | | 时间戳 |

---

## 3. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `api/stores/{storeId}/discount-rules` | 获取店铺优惠列表 |
| POST | `api/stores/{storeId}/discount-rules` | 创建优惠规则 |
| PUT | `api/discount-rules/{id}` | 更新优惠规则 |
| DELETE | `api/discount-rules/{id}` | 删除优惠规则（软删除） |
| PATCH | `api/discount-rules/{id}/toggle-status` | 切换启停状态 |

---

## 4. 业务规则

- BR-1：满减（full_reduction）：订单金额 >= threshold_amount 时减 discount_amount，单条规则只减一次，不按门槛倍数重复减免
- BR-2：折扣（discount）：订单金额 × discount_rate，如 0.8 = 八折
- BR-3：新创建的规则默认 status=active
- BR-4：allow_stack=false 时，同一订单只能使用一条规则
- BR-5：下单时自动匹配最优优惠（满减优先，折扣次之）
- BR-6：优惠有时间范围限制，超出时间不匹配

---

## 5. 单元测试覆盖（11个）

创建/更新/删除/状态切换/查询
