# 管理端 API 契约

> 版本：v1.1.0 | 更新日期：2026-05-22

## 商家管理

### GET /api/admin/merchants

获取商家列表

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| status | string | 否 | 状态筛选：active/pending/suspended/disabled |
| keyword | string | 否 | 搜索关键词（昵称/手机号） |

**响应**：Merchant 实体列表

### PATCH /api/admin/merchants/{merchantId}/status

更新商家状态（审核/封禁/解封）

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| status | string | 是 | 目标状态：active/suspended |
| operatorId | string | 是 | 操作管理员ID |
| reason | string | 否 | 操作原因（封禁时必填） |

**副作用**：自动创建 MerchantAuditLog 审计记录

---

## 系统配置

### PATCH /api/admin/{adminId}

更新管理员信息

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| displayName | string | 否 | 显示名称 |
| role | string | 否 | 角色 |

### GET /api/admin/audit-logs

获取商家审计日志

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| merchantId | string | 否 | 按商家ID筛选 |

**响应**：MerchantAuditLog 列表

### GET /api/admin/operation-logs

获取管理员操作日志

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| adminId | string | 否 | 按管理员ID筛选 |

**响应**：AdminOperationLog 列表

---

## 平台统计

### GET /api/admin/platform-stats

获取平台统计数据

**响应**：
```json
{
  "totalMerchants": 30,
  "activeMerchants": 28,
  "pendingMerchants": 1,
  "totalStores": 1,
  "openStores": 1,
  "totalOrders": 18,
  "completedOrders": 18,
  "totalRevenue": 1102.00,
  "todayOrders": 0,
  "todayRevenue": 0,
  "totalProducts": 26,
  "totalCategories": 8
}
```

---

## 管理员登录/退出

参见 [认证鉴权 API](auth.md)