# TEST - 测试体系

> 模块编号：TEST
> 状态：✅ 已完成
> 测试项目：`tests/ByteBite.UnitTests/`、`tests/ByteBite.IntegrationTests/`

---

## 1. 单元测试

### 1.1 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| xUnit | 2.9 | 测试运行器 |
| Moq | 4.20 | Mock 依赖 |
| FluentAssertions | 7.2 | 断言库 |
| ByteBite.Shared | - | PasswordHasher 等 |

### 1.2 测试覆盖

| 服务 | 测试数 | 关键场景 |
|------|--------|---------|
| MerchantService | 7 | 注册/重复手机号/登录/密码验证 |
| StoreService | 8 | 创建/更新/营业状态切换 |
| CategoryService | 7 | 创建/更新/软删除/排序 |
| ProductService | 9 | 创建/更新/批量上下架/分类验证 |
| OrderService | 17 | 下单全流程验证 + 6种状态转换 |
| CustomerService | 13 | 注册/登录/匿名顾客/数据合并 |
| DiscountRuleService | 11 | 创建/更新/软删除/状态切换 |
| AdminService | 14 | 登录/商家审核/配置更新/平台统计 |
| DashboardService | 5 | 概览/商品销售/趋势/时段分布 |
| **合计** | **92** | |

### 1.3 命名规范

`{MethodName}_{Scenario}_{ExpectedResult}`

示例：
- `CreateOrderAsync_StoreClosed_ThrowsInvalidOperationException`
- `AcceptOrderAsync_PendingOrder_ChangesToAccepted`
- `RegisterAsync_DuplicatePhone_ThrowsInvalidOperationException`

### 1.4 运行命令

```bash
dotnet test tests/ByteBite.UnitTests --verbosity normal
```

---

## 2. 集成测试

### 2.1 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| WebApplicationFactory | 10.0 | 内存测试服务器 |
| xUnit | 2.9 | 测试运行器 |
| FluentAssertions | 7.2 | 断言库 |
| 真实 PostgreSQL | 17 | 数据库 |

### 2.2 测试覆盖

| 模块 | 测试数 | 关键场景 |
|------|--------|---------|
| Merchants | 5 | 注册/重复手机号/登录/错误密码/查询 |
| Stores | 4 | 创建/查询/按商家查/营业状态切换 |
| Categories & Products | 6 | 分类CRUD/商品CRUD/菜单查询/软删除 |
| Orders | 5 | 下单/取货码/接单/完整流程/拒单 |
| DiscountRules | 4 | 创建/查询/状态切换/删除 |
| Customers | 4 | 匿名顾客/注册/查询/合并预览 |
| Admin | 5 | 登录/错误密码/公开配置/商家列表/平台统计 |
| Dashboard | 3 | 概览/商品销售/营收趋势 |
| Templates | 3 | 行业分类树/创建模板/模板列表 |
| **合计** | **39** | |

### 2.3 测试基础设施

- `ByteBiteWebAppFactory`：WebApplicationFactory<Program>，连接真实 PostgreSQL
- `TestDataGenerator`：测试数据生成器（唯一手机号、各模块请求对象）
- `ApiResponse<T>`：响应包装类

### 2.4 运行命令

```bash
dotnet test tests/ByteBite.IntegrationTests --verbosity normal
```

---

## 3. 已知问题

### 3.1 集成测试外键约束问题（已修复）

**问题**：`MerchantService.RegisterAsync` 创建商家后立即创建店铺，但商家ID在 `SaveChangesAsync` 前为 `Guid.Empty`，导致 `stores_merchant_id_fkey` 外键约束违反。

**修复**：在 Application 层实体创建时显式设置 `Id = Guid.NewGuid()`，确保在 `SaveChangesAsync` 之前 ID 已可用。

### 3.2 DbContext 新增表配置缺失（已修复）

**问题**：EF Core scaffold 重新生成后，12 个新增表的 DbSet 和 OnModelCreating 配置丢失。

**修复**：手动添加所有新增表的配置到 `ByteBiteDbContext.cs`。

---

## 4. 测试管理员账号

| 用户名 | 密码 | 角色 |
|--------|------|------|
| super_admin | admin123 | super_admin |
| admin | admin123 | admin |
| viewer | admin123 | viewer |
