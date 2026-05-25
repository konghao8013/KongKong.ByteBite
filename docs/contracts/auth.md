# 认证鉴权 API 契约

> 版本：v1.1.0 | 更新日期：2026-05-22

## 统一登录

### POST /api/auth/login

自动识别角色的统一登录接口。

**请求**：
```json
{ "account": "admin", "password": "admin123" }
```

**账号识别规则**：
| 账号格式 | 优先尝试 |
|----------|----------|
| 非11位手机号 | 管理员 → 报错 |
| 11位手机号 | 商家 → 顾客 → 报错 |

**响应**：
```json
{
  "role": "admin|merchant|customer",
  "data": { ... },
  "storeId": "uuid"  // 仅商家角色返回
}
```

**角色跳转**：
| role | 前端跳转 | localStorage key |
|------|----------|-----------------|
| admin | /admin/merchants | admin_token, admin_id |
| merchant | /merchant/orders | merchant_token, merchant_id, merchant_store_id |
| customer | / | customer_token, customer_id |

**错误**：code=401, message="账号或密码错误"

---

## 商家登录/注册

### POST /api/merchants/login

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| phone | string | 是 | 11位手机号 |
| password | string | 是 | 密码 |

**状态限制**：pending/disabled 状态不允许登录，返回 403

### POST /api/merchants/register

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| phone | string | 是 | 11位手机号 |
| password | string | 是 | 密码（≥6位） |
| storeName | string | 是 | 店铺名称 |

注册后商家状态为 active（MVP阶段无需审核）

### POST /api/merchants/{id}/logout

服务端清除 Token

---

## 管理员登录

### POST /api/admin/login

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| username | string | 是 | 管理员用户名 |
| password | string | 是 | 密码 |

### POST /api/admin/{id}/logout

服务端清除 Token

---

## 顾客登录/注册

### POST /api/customers/register

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| phone | string | 是 | 手机号 |
| password | string | 是 | 密码 |

### POST /api/customers/login

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| phone | string | 是 | 手机号 |
| password | string | 是 | 密码 |

### POST /api/customers/{id}/merge

将浏览器缓存中的匿名数据合并到注册账号中。

### POST /api/customers/{id}/logout

服务端清除 Token

---

## Token 机制

| 特性 | 说明 |
|------|------|
| 生成方式 | `Convert.ToBase64String(Guid.NewGuid().ToByteArray())` |
| 存储位置 | 前端 localStorage，后端实体的 Token 字段 |
| 传输方式 | `Authorization: Bearer {token}` |
| 验证方式 | Service 层查询实体 Token 字段匹配 |
| 有效期 | 无过期机制（MVP阶段） |