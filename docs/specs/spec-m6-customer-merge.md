# M6 - 顾客注册/数据合并

> 模块编号：M6
> 状态：✅ 已完成
> SQL 文件：`docs/sql/05_customer_registration.sql`

---

## 1. 功能概述

顾客可通过手机号注册账号，注册后自动合并该设备之前的匿名订单数据。支持多店铺数据合并预览。

---

## 2. 数据库表结构

### 2.1 customer_sessions（顾客会话表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| customer_id | UUID | FK→customers | 顾客ID |
| device_id | VARCHAR(200) | | 设备标识 |
| token | VARCHAR(500) | | 登录令牌 |
| last_store_id | UUID | | 最后访问店铺 |
| last_category_id | UUID | | 最后浏览分类 |
| cart_data | JSONB | | 购物车数据 |
| active_orders | JSONB | | 进行中订单 |
| order_history | JSONB | | 历史订单 |
| ip_address | VARCHAR(50) | | IP地址 |
| user_agent | VARCHAR(500) | | 浏览器标识 |
| expires_at | TIMESTAMPTZ | | 过期时间 |
| created_at / updated_at | TIMESTAMPTZ | | 时间戳 |

### 2.2 data_merge_logs（数据合并日志表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| target_customer_id | UUID | FK→customers | 目标注册顾客 |
| source_device_id | VARCHAR(200) | | 来源设备标识 |
| source_customer_id | UUID | FK→customers | 来源匿名顾客 |
| merge_type | VARCHAR(20) | CHECK | 类型：device_to_account/account_to_account/session_to_account |
| orders_merged | INTEGER | DEFAULT 0 | 合并订单数 |
| cart_items_merged | INTEGER | DEFAULT 0 | 合并购物车项数 |
| pickup_codes_merged | INTEGER | DEFAULT 0 | 合并取货码数 |
| conflicts_resolved | INTEGER | DEFAULT 0 | 解决冲突数 |
| merge_detail | JSONB | | 合并详情 |
| status | VARCHAR(20) | CHECK | 状态：pending/completed/failed/rolled_back |
| created_at | TIMESTAMPTZ | | 创建时间 |

---

## 3. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| POST | `api/customers/register` | 顾客注册 |
| POST | `api/customers/login` | 顾客登录 |
| GET | `api/customers/{id}` | 获取顾客信息 |
| POST | `api/customers/{id}/merge?deviceId=xxx` | 合并匿名数据 |
| GET | `api/customers/merge-preview?deviceId=xxx` | 合并预览 |
| POST | `api/customers/anonymous?deviceId=xxx` | 确保匿名顾客存在 |

---

## 4. 业务规则

- BR-1：首次扫码自动创建匿名顾客（is_registered=false, device_id 标识）
- BR-2：注册时若提供 deviceId，自动触发数据合并
- BR-3：合并逻辑：将 device_id 匹配且 customer_id 为 null 的订单更新为注册顾客
- BR-4：合并预览按店铺分组展示待合并数据摘要
- BR-5：昵称默认取手机号后4位
- BR-6：合并操作记录到 data_merge_logs 表

---

## 5. 单元测试覆盖（13个）

注册/登录/匿名顾客/数据合并/合并预览
