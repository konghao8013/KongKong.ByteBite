# 认证鉴权领域 Spec

> 版本：v1.1.0 | 更新日期：2026-05-22

## 领域概述

统一认证系统，根据账号自动识别角色（管理员/商家/顾客），提供登录、注册、退出功能。

## 核心实体

- Admin（管理员）
- Merchant（商家）
- Customer（顾客）

## 业务规则

### 1. 统一登录规则

| 账号格式 | 识别策略 |
|----------|----------|
| 非11位数字 | 优先管理员（用户名匹配），失败返回401 |
| 11位手机号 | 依次尝试商家 → 顾客，全部失败返回401 |

### 2. 登录状态限制

| 角色 | 禁止登录的状态 | 返回错误 |
|------|---------------|----------|
| 商家 | pending/disabled/suspended | 403 + "账号被禁用或待审核" |
| 管理员 | disabled | 403 |
| 顾客 | 无限制 | — |

### 3. Token 机制

- 生成：`Convert.ToBase64String(Guid.NewGuid().ToByteArray())`
- 存储：实体 Token 字段 + 前端 localStorage
- 传输：`Authorization: Bearer {token}`
- 验证：Service 查询实体 Token 字段匹配
- 无过期机制（MVP阶段）

### 4. 商家注册

- 手机号 + 密码 + 店铺名称
- BCrypt 加密密码
- MVP 阶段注册后直接 active（无审核流程）
- 同手机号重复注册返回错误

### 5. 退出登录

- 前端清除 localStorage（token/id/info/storeId）
- 调用后端 Logout API（服务端清除 Token）
- 商家退出时断开 SignalR 连接

## API 契约

详见 [认证鉴权 API 契约](../contracts/auth.md)

## 前端实现

- 统一登录页：`web/src/pages/merchant/Login.vue`
- 登录成功后根据 role 自动跳转：
  - admin → /admin/merchants
  - merchant → /merchant/orders
  - customer → /