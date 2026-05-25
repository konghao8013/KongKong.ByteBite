# M8 - 平台管理员端

> 模块编号：M8
> 状态：✅ 已完成
> SQL 文件：`docs/sql/07_platform_admin.sql`

---

## 1. 功能概述

平台管理员登录后台，管理商家（审核/禁用/解禁），管理系统配置，查看平台统计数据。

---

## 2. 数据库表结构

### 2.1 admins（管理员表，已在01中创建）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| username | VARCHAR(50) | UNIQUE, NOT NULL | 用户名 |
| password_hash | VARCHAR(200) | NOT NULL | BCrypt密码 |
| display_name | VARCHAR(50) | | 显示名称 |
| role | VARCHAR(20) | NOT NULL | 角色：super_admin/admin/viewer |
| status | VARCHAR(20) | DEFAULT 'active' | 状态：active/disabled |
| last_login_at | TIMESTAMPTZ | | 最后登录 |

### 2.2 system_configs（系统配置表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| config_key | VARCHAR(100) | UNIQUE, NOT NULL | 配置键名 |
| config_value | TEXT | NOT NULL | 配置值 |
| config_type | VARCHAR(20) | DEFAULT 'string' | 值类型：string/number/boolean/json |
| description | VARCHAR(500) | | 配置说明 |
| is_public | BOOLEAN | DEFAULT false | 是否公开（前端可读） |

**初始配置（11条）**：require_merchant_review, pickup_code_length, order_timeout_minutes, max_upload_size_mb, allowed_upload_formats, sms_provider, sms_api_key, sms_api_url, system_announcement, cart_cache_hours, order_history_limit

### 2.3 merchant_audit_logs（商家审核记录表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| merchant_id | UUID | FK→merchants | 商家ID |
| admin_id | UUID | FK→admins | 操作管理员 |
| action | VARCHAR(20) | CHECK | 操作：approve/reject/disable/enable/delete |
| reason | VARCHAR(500) | | 原因（reject/disable必填） |
| previous_status | VARCHAR(20) | | 操作前状态 |
| new_status | VARCHAR(20) | | 操作后状态 |

### 2.4 platform_daily_stats（平台每日统计表）

| 列名 | 类型 | 说明 |
|------|------|------|
| stat_date | DATE, UNIQUE | 统计日期 |
| total_merchants / active_merchants / new_merchants | INTEGER | 商家统计 |
| total_stores | INTEGER | 店铺总数 |
| total_orders | INTEGER | 订单总数 |
| total_revenue | DECIMAL(18,2) | 总营收 |
| total_customers / new_customers | INTEGER | 顾客统计 |
| template_usage_count | INTEGER | 模板使用次数 |

### 2.5 admin_operation_logs（管理员操作日志表）

| 列名 | 类型 | 说明 |
|------|------|------|
| admin_id | UUID | 操作管理员 |
| operation | VARCHAR(50) | 操作类型 |
| target_type | VARCHAR(50) | 对象类型 |
| target_id | UUID | 对象ID |
| detail | JSONB | 操作详情 |
| ip_address | VARCHAR(50) | IP地址 |

---

## 3. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| POST | `api/admin/login` | 管理员登录 |
| GET | `api/admin/{id}` | 获取管理员信息 |
| GET | `api/admin/merchants` | 商家列表（筛选+分页） |
| POST | `api/admin/merchants/{merchantId}/audit` | 审核商家 |
| GET | `api/admin/configs` | 所有系统配置 |
| GET | `api/admin/configs/public` | 公开配置 |
| PUT | `api/admin/configs/{configId}` | 更新配置 |
| GET | `api/admin/stats` | 平台统计 |

---

## 4. 业务规则

- BR-1：管理员角色：super_admin（全部权限）、admin（常规管理）、viewer（只读）
- BR-2：商家审核状态流转：approve→active, reject→rejected, disable→disabled, enable→active
- BR-3：reject 和 disable 操作必须填写 reason
- BR-4：登录时记录操作日志（admin_operation_logs）
- BR-5：审核操作同时记录审核日志（merchant_audit_logs）和操作日志
- BR-6：公开配置（is_public=true）可被前端无鉴权读取
- BR-7：系统配置值类型校验：string/number/boolean/json

---

## 5. 单元测试覆盖（14个）

登录（有效/无效/禁用/密码错误）、商家审核（approve/reject/disable/enable/无原因/无效操作/不存在）、配置更新、平台统计
