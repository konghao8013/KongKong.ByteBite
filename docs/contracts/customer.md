# 顾客端 API 契约

> 版本：v1.1.0 | 更新日期：2026-05-22

## 店铺菜单

### GET /api/CustomerStore/{storeId}/menu

获取店铺菜单（分类+商品），用于顾客端浏览

**响应**：
```json
{
  "store": { "id", "name", "description", "businessStatus", ... },
  "categories": [
    { "id", "name", "icon", "sortOrder", "products": [...] }
  ]
}
```

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