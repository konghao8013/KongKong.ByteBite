# API 设计详细约定

本文件按需加载，不是常驻协议。

## URL 设计

| 操作 | HTTP 方法 | URL 模式 | 示例 |
|---|---|---|---|
| 列表查询 | GET | `/api/{resources}` | `GET /api/users?pageIndex=1&pageSize=20` |
| 详情查询 | GET | `/api/{resources}/{id}` | `GET /api/users/550e8400-e29b-41d4-a716-446655440000` |
| 创建 | POST | `/api/{resources}` | `POST /api/users` |
| 全量更新 | PUT | `/api/{resources}/{id}` | `PUT /api/users/550e8400-...` |
| 部分更新 | PATCH | `/api/{resources}/{id}` | `PATCH /api/users/550e8400-...` |
| 删除 | DELETE | `/api/{resources}/{id}` | `DELETE /api/users/550e8400-...` |
| 子资源列表 | GET | `/api/{resources}/{id}/{sub-resources}` | `GET /api/users/550e8400-.../orders` |
| 操作/动作 | POST | `/api/{resources}/{id}/actions/{action}` | `POST /api/orders/550e8400-.../actions/cancel` |

## URL 规则

- 资源名使用复数名词（`users` 不用 `user`）
- 使用 kebab-case（`user-profiles` 不用 `userProfiles`）
- ID 使用 UUID，路径参数使用 `:guid` 约束
- 查询参数使用 camelCase（`pageIndex`, `pageSize`, `sortBy`）
- 嵌套不超过 2 层（`/api/users/{id}/orders` 可以，`/api/users/{id}/orders/{oid}/items` 拆分）
- 动作类操作使用 `/actions/{verb}` 后缀

## 请求参数

### 查询参数（GET）

| 参数 | 类型 | 说明 |
|---|---|---|
| `pageIndex` | int | 页码（从 1 开始） |
| `pageSize` | int | 每页条数（默认 20，最大 100） |
| `sortBy` | string | 排序字段 |
| `sortOrder` | string | `asc` / `desc` |
| `keyword` | string | 关键词搜索 |

### 请求体（POST/PUT/PATCH）

- Content-Type: `application/json`
- 属性名 camelCase
- 必填字段在 DTO 上标注 `[Required]`
- 字符串长度在 DTO 上标注 `[MaxLength]`

## HTTP 状态码

| 状态码 | 场景 |
|---|---|
| 200 | 成功（查询/更新） |
| 201 | 创建成功 |
| 204 | 删除成功（无返回体） |
| 400 | 请求参数错误 / 验证失败 |
| 401 | 未认证 |
| 403 | 无权限 |
| 404 | 资源不存在 |
| 409 | 冲突（重复创建等） |
| 422 | 业务规则验证失败 |
| 500 | 服务器内部错误 |

## 版本策略

- 当前不使用 URL 版本前缀（`/api/v1/`）
- 破坏性变更通过新端点或请求头协商
- 未来如需版本化，在 `Program.cs` 统一配置

## Minimal API 使用场景

仅在以下场景使用 Minimal API 替代 Controller：

- 健康检查端点（`/health`）
- 单一操作的轻量端点
- Webhook 回调端点
- 内部服务间调用端点

其他业务端点统一使用 Controller。
