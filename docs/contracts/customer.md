# 顾客端 API 契约

> 版本：v1.3.0 | 更新日期：2026-05-25

## 店铺菜单

### GET /api/CustomerStore/{storeId}/menu

获取店铺菜单（分类+商品），用于商家端/管理端等已知 storeId 的场景

**响应**：
```json
{
  "store": { "id", "name", "storeCode", "description", "businessStatus", ... },
  "categories": [
    { "id", "name", "icon", "sortOrder", "products": [...] }
  ]
}
```

### GET /api/CustomerStore/code/{storeCode}/menu

根据店铺码获取店铺菜单，用于顾客端短链扫码场景

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| storeCode | string | 是 | 6位店铺码（如 AFXF08） |

**响应**：同上（包含 storeCode 字段）

**错误**：
- 404 店铺不存在

---

## 短链路由

### /A/{storeCode}

店铺点单页短链，前端路由匹配 `/A/:code`，加载店铺菜单

| 路由 | 模块 | 说明 |
|------|------|------|
| /A/{storeCode} | 店铺点单 | 扫码/分享进入店铺 |
| /B/{storeCode} | 优惠活动（预留） | 后续扩展 |
| /C/{storeCode} | 会员页（预留） | 后续扩展 |

### /store/{storeId}

旧路由兼容，301 重定向至 `/A/{storeCode}`

---

## 顾客信息

### GET /api/customers/{id}

获取顾客详情

### POST /api/customers/register

注册（参见 [认证鉴权 API](auth.md)）

### POST /api/customers/login

登录（参见 [认证鉴权 API](auth.md)）

### POST /api/customers/{id}/merge

数据合并

### POST /api/customers/{id}/logout

退出登录