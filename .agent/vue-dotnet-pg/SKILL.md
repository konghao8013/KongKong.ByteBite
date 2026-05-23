---
name: vue-dotnet-pg
description: Vue 3 + .NET 10 + PostgreSQL 全栈项目技术栈 skill。提供项目结构约定、分层架构规则、前后端协作契约、数据库访问规范和代码风格约束。当项目使用 Vue + .NET + PostgreSQL 技术栈或用户提到前端/后端/数据库分层开发时自动激活。
---

# 空空码上点单（KongKong.ByteBite）技术栈 Skill

## 核心定位

本 skill 是 **空空码上点单**（KongKong.ByteBite）项目的技术栈约束层，与 `$sdd-riper-one` / `$sdd-riper-one-light` 配合使用。它不定义流程，只定义技术边界和约定：项目结构、分层规则、命名规范、前后端契约、数据库访问模式和代码风格。

## 技术栈

| 层 | 技术 | 版本约束 |
|---|---|---|
| 前端框架 | Vue 3 (Composition API + `<script setup>`) | 3.5+ |
| 前端构建 | Vite | 6+ |
| 前端状态 | Pinia | 2+ |
| 前端路由 | Vue Router | 4+ |
| 前端 UI | Element Plus | 2.9+ |
| 前端类型 | TypeScript | 5+ |
| 前端 HTTP | Axios | 1+ |
| 后端框架 | .NET | 10 (preview) |
| 后端 ORM | Entity Framework Core | 10 (preview) |
| 后端认证 | ASP.NET Core Identity + JWT | 随 .NET 10 |
| 后端 API 风格 | RESTful (主) + Minimal API (轻量端点) | - |
| 后端验证 | FluentValidation | 11+ |
| 后端映射 | Mapster | 7+ |
| 后端实时通信 | SignalR | 随 .NET 10 |
| 后端日志 | Serilog | 4+ |
| 数据库 | PostgreSQL | 17+ |
| 数据库访问 | EF Core DB First | 随 EF Core 10 |
| 缓存 | Redis (StackExchange.Redis) | 7+ |
| 文件存储 | 本地文件系统 (uploads/) | - |
| 容器化 | Docker + Docker Compose | - |
| API 文档 | Swagger / OpenAPI (Swashbuckle) | 随 .NET 10 |

## 项目结构

```
KongKong.ByteBite/
├── src/
│   ├── ByteBite.Api/                    # 后端 API 层
│   │   ├── Controllers/                 # REST 控制器
│   │   ├── MinimalEndpoints/            # Minimal API 端点
│   │   ├── Middleware/                   # 中间件（异常、日志、认证）
│   │   ├── Filters/                     # Action Filters
│   │   ├── Extensions/                  # 服务注册扩展
│   │   └── Program.cs                   # 启动入口
│   │
│   ├── ByteBite.Application/            # 应用层
│   │   ├── Services/                    # 业务服务接口与实现
│   │   ├── DTOs/                        # 数据传输对象
│   │   ├── Validators/                  # FluentValidation 验证器
│   │   ├── Mappings/                    # Mapster 映射配置
│   │   └── Interfaces/                  # 服务接口定义
│   │
│   ├── ByteBite.Domain/                 # 领域层
│   │   ├── Entities/                    # 领域实体
│   │   ├── Enums/                       # 枚举
│   │   ├── Events/                      # 领域事件
│   │   ├── ValueObjects/                # 值对象
│   │   ├── Exceptions/                  # 领域异常
│   │   └── Interfaces/                  # 仓储接口
│   │
│   ├── ByteBite.Infrastructure/         # 基础设施层
│   │   ├── Persistence/                 # EF Core DbContext（DB First 生成）
│   │   │   ├── Entities/               # DB First 生成的实体类
│   │   │   └── ByteBiteDbContext.cs    # 数据上下文（DB First 生成）
│   │   ├── Repositories/               # 仓储实现
│   │   ├── Cache/                      # Redis 缓存实现
│   │   ├── FileStorage/                # 本地文件存储实现
│   │   ├── ExternalServices/           # 外部服务集成
│   │   └── Identity/                   # 认证与授权实现
│   │
│   └── ByteBite.Shared/                # 共享层
│       ├── Constants/                   # 常量
│       ├── Helpers/                     # 工具类
│       └── Extensions/                  # 扩展方法
│
├── web/                                 # 前端 Vue 项目
│   ├── src/
│   │   ├── api/                        # API 请求封装
│   │   │   ├── index.ts               # Axios 实例与拦截器
│   │   │   └── modules/               # 按业务模块拆分的 API
│   │   ├── assets/                     # 静态资源
│   │   ├── components/                 # 通用组件
│   │   │   └── common/                # 基础通用组件
│   │   ├── composables/                # 组合式函数
│   │   ├── layouts/                    # 布局组件
│   │   ├── pages/                      # 页面视图（按业务模块组织）
│   │   ├── router/                     # 路由配置
│   │   │   ├── index.ts               # 路由实例
│   │   │   └── modules/               # 按模块拆分的路由
│   │   ├── stores/                     # Pinia 状态管理
│   │   │   └── modules/               # 按模块拆分的 Store
│   │   ├── styles/                     # 全局样式
│   │   ├── types/                      # TypeScript 类型定义
│   │   │   ├── api.d.ts               # API 响应类型
│   │   │   └── models/                # 业务模型类型
│   │   ├── utils/                      # 工具函数
│   │   ├── App.vue
│   │   └── main.ts
│   ├── index.html
│   ├── vite.config.ts
│   ├── tsconfig.json
│   └── package.json
│
├── tests/
│   ├── ByteBite.UnitTests/             # 单元测试
│   ├── ByteBite.IntegrationTests/      # 集成测试
│   └── ByteBite.E2ETests/              # 端到端测试
│
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.web
│   └── docker-compose.yml
│
├── uploads/                             # 本地文件存储
│   ├── stores/                          # 店铺资源
│   │   └── {storeId}/                   # 每个商家一个子目录
│   │       ├── qrcode/                  # 商家二维码
│   │       ├── cover/                   # 店铺封面图
│   │       └── products/               # 菜谱/商品图片
│   ├── categories/                      # 分类图标
│   └── temp/                            # 临时上传目录（定期清理）
│
├── docs/                               # 项目文档
├── mydocs/                             # SDD 产物目录
│   ├── specs/
│   ├── codemap/
│   ├── context/
│   ├── project/
│   └── archive/
│
├── ByteBite.sln                        # .NET 解决方案文件
└── Directory.Build.props               # 共享 MSBuild 属性
```

## 分层架构规则

### 依赖方向（严格单向）

```
Api → Application → Domain ← Infrastructure
                    ↑              |
                    └──────────────┘
              (Infrastructure 实现 Domain 接口)
```

| 规则 | 说明 |
|---|---|
| Domain 不依赖任何层 | 实体、值对象、领域事件、仓储接口纯 POCO |
| Application 依赖 Domain | 通过接口调用仓储，不直接依赖 Infrastructure |
| Infrastructure 依赖 Domain | 实现 Domain 定义的仓储接口 |
| Api 依赖 Application | 控制器注入 Application 服务，不直接使用 Infrastructure |
| Shared 被所有层引用 | 常量、工具、扩展方法 |

### 禁止项

- Domain 层禁止引用 EF Core、Dapper 等数据访问库
- Application 层禁止直接使用 DbContext
- Api 层禁止直接注入 DbContext 或 Repository 实现
- 禁止跨层直接抛出 Infrastructure 异常到 Api 层
- 禁止在 Controller 中编写业务逻辑

### 注释规范（强制）

所有类、方法、主要代码块必须编写中文注释，实体类属性必须标注中文说明。

**类注释：**

```csharp
/// <summary>
/// 商家用户服务 - 处理商家注册、登录、信息管理
/// </summary>
public class MerchantService : IMerchantService
```

**方法注释：**

```csharp
/// <summary>
/// 商家注册 - 创建商家账号并关联店铺
/// </summary>
/// <param name="request">注册请求（手机号、密码、店铺名称）</param>
/// <param name="ct">取消令牌</param>
/// <returns>注册结果（商家ID、店铺ID、Token）</returns>
/// <exception cref="DomainException">手机号已注册时抛出</exception>
public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
```

**实体类属性注释：**

```csharp
/// <summary>
/// 商家用户实体
/// </summary>
public partial class Merchant
{
    /// <summary>主键UUID</summary>
    public Guid Id { get; set; }

    /// <summary>手机号，作为登录账号</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>BCrypt加密密码</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>昵称</summary>
    public string? Nickname { get; set; }

    /// <summary>状态：pending-待审核, active-已激活, disabled-已禁用</summary>
    public string Status { get; set; } = "active";

    /// <summary>最后登录时间</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>软删除时间</summary>
    public DateTime? DeletedAt { get; set; }
}
```

**主要代码块注释：**

```csharp
public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
{
    // 1. 校验店铺营业状态
    var store = await _storeRepository.GetByIdAsync(request.StoreId, ct);
    if (store.BusinessStatus != "open")
        throw new DomainException("商家休息中，暂不接单");

    // 2. 校验商品并计算价格
    var orderItems = new List<OrderItem>();
    foreach (var item in request.Items)
    {
        var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
        var finalPrice = CalculateFinalPrice(product, item.SpecOptionIds);
        orderItems.Add(new OrderItem(product, item.Quantity, finalPrice, item.SpecOptionIds));
    }

    // 3. 匹配优惠活动
    var discount = await MatchDiscountAsync(request.StoreId, orderItems, ct);

    // 4. 生成取货码
    var pickupCode = await GeneratePickupCodeAsync(request.StoreId, ct);

    // 5. 创建订单
    var order = Order.Create(store, orderItems, pickupCode, request.DiningMode, discount);
    await _orderRepository.AddAsync(order, ct);

    return order.Adapt<OrderDto>();
}
```

**注释要求：**

| 对象 | 要求 | 语言 |
|------|------|------|
| 类 | `<summary>` 说明类的职责 | 中文 |
| 方法 | `<summary>` 说明功能 + `<param>` + `<returns>` + `<exception>` | 中文 |
| 实体属性 | `<summary>` 说明属性含义，枚举值列出所有选项 | 中文 |
| 主要代码块 | 步骤编号 + 简要说明（如：// 1. 校验店铺营业状态） | 中文 |
| 接口 | 同类注释要求 | 中文 |
| 枚举 | 每个 enum 值加 `<summary>` | 中文 |
| DTO | 同实体属性注释要求 | 中文 |

**DB First 实体类注释：**

由于 DB First Scaffold 生成的实体类会被覆盖，属性注释通过以下方式维护：

1. Scaffold 生成后，立即为所有属性添加 `<summary>` 注释
2. 后续 Scaffold 重新生成时，使用 partial class 扩展，不修改生成文件
3. 或使用 `--xml-comment-file` 配合自定义 T4 模板自动生成注释

## 后端编码规范

### 命名约定

| 类型 | 规则 | 示例 |
|---|---|---|
| 实体类 | PascalCase | `User`, `OrderItem` |
| DTO | PascalCase + 后缀 | `UserDto`, `CreateUserRequest`, `UpdateUserRequest` |
| 服务接口 | I + PascalCase + Service | `IUserService` |
| 服务实现 | PascalCase + Service | `UserService` |
| 仓储接口 | I + PascalCase + Repository | `IUserRepository` |
| 仓储实现 | PascalCase + Repository | `UserRepository` |
| 控制器 | PascalCase + Controller | `UsersController` |
| Minimal Endpoint | PascalCase + Endpoint | `CreateUserEndpoint` |
| EF 配置 | 实体名 + Configuration | `UserConfiguration` |
| 迁移 | EF 自动生成 | `AddUserTable` |
| 枚举 | PascalCase（单数） | `UserStatus` |
| 值对象 | PascalCase | `Money`, `Address` |
| 常量 | PascalCase | `MaxRetryCount` |
| 私有字段 | _camelCase | `_userRepository` |

### Controller 规范

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userService.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }
}
```

### 服务层规范

```csharp
public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### EF Core 规范（DB First 模式）

本项目使用 DB First 模式，先创建数据库表结构，再通过 EF Core Scaffold 生成实体类。

**开发流程：**

1. 编写 SQL DDL 创建/修改表结构
2. 执行 `dotnet ef dbcontext scaffold` 生成实体类和 DbContext
3. 在 Application 层编写业务逻辑
4. 编写单元测试

**Scaffold 命令：**

```bash
dotnet ef dbcontext scaffold "Host=192.168.3.22;Port=5432;Database=kongkong_bytebite;Username=konghao;Password=hitek.123" Npgsql.EntityFrameworkCore.PostgreSQL --project src/ByteBite.Infrastructure --startup-project src/ByteBite.Api --output-dir Persistence/Entities --context-dir Persistence --context ByteBiteDbContext --force --no-onconfiguring
```

**数据库表结构规范：**

| 规则 | 说明 |
|------|------|
| 表名 | snake_case 复数（`users`, `stores`, `products`） |
| 列名 | snake_case（`created_at`, `store_id`） |
| 主键 | `id`（UUID / `uuid`），默认 `gen_random_uuid()` |
| 外键 | `<关联表单数>_id`（`store_id`） |
| 创建时间 | `created_at`（`timestamptz`，默认 `now()`） |
| 更新时间 | `updated_at`（`timestamptz`） |
| 软删除 | `deleted_at`（`timestamptz`，nullable） |
| 金额 | `decimal(18,2)`，禁止 float/double |
| 枚举 | 存储为 `varchar`，不使用 PostgreSQL enum type |
| 字符串 | 必须指定长度，禁止无限制 text |
| 索引 | `ix_<表名>_<列名>` |
| 唯一约束 | `uq_<表名>_<列名>` |
| 外键约束 | `fk_<表名>_<关联表名>` |
| 所有表 | 必须有 `created_at` 和 `updated_at` 列 |

**禁止项：**

- 禁止使用 EF Core Migrations（Code First），所有表结构变更通过 SQL DDL
- 禁止手动修改 Scaffold 生成的实体类（如需扩展，使用 partial class）
- 禁止在实体类中添加业务逻辑（业务逻辑在 Application 层）
- 字符串默认指定长度，禁止无限制 `nvarchar(max)`

### 数据库规范

- 表名：snake_case 复数（`user_accounts`, `order_items`）
- 列名：snake_case（`created_at`, `user_id`）
- 主键：`id`（UUID / `uuid`）
- 外键：`<关联表单数>_id`（`user_id`）
- 创建时间：`created_at`（`timestamptz`，默认 `now()`）
- 更新时间：`updated_at`（`timestamptz`，触发器或 EF 自动更新）
- 软删除：`deleted_at`（`timestamptz`，nullable）
- 索引命名：`ix_<表名>_<列名>`
- 唯一约束：`uq_<表名>_<列名>`
- 外键约束：`fk_<表名>_<关联表名>`

## 前端编码规范

### 命名约定

| 类型 | 规则 | 示例 |
|---|---|---|
| Vue 文件 | PascalCase | `UserList.vue`, `OrderDetail.vue` |
| 组合式函数 | use + PascalCase | `useUserList`, `useAuth` |
| Store | use + 模块名 + Store | `useUserStore` |
| API 模块 | camelCase | `userApi`, `orderApi` |
| 类型/接口 | PascalCase | `UserInfo`, `PageParams` |
| 常量 | SCREAMING_SNAKE_CASE | `API_BASE_URL` |
| 工具函数 | camelCase | `formatDate`, `parseToken` |
| CSS 类 | kebab-case | `user-list`, `btn-primary` |
| 事件名 | kebab-case | `@item-click`, `@form-submit` |

### 组件规范

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUserStore } from '@/stores/modules/useUserStore'
import type { UserInfo } from '@/types/models/user'

const userStore = useUserStore()
const loading = ref(false)

const handleSave = async (user: UserInfo) => {
  loading.value = true
  try {
    await userStore.updateUser(user)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  userStore.fetchUsers()
})
</script>

<template>
  <div class="user-list">
    <!-- template content -->
  </div>
</template>

<style scoped>
.user-list {
  /* styles */
}
</style>
```

### API 封装规范

```typescript
import request from '@/api'
import type { PagedResult, PageParams } from '@/types/api'
import type { UserInfo, CreateUserRequest, UpdateUserRequest } from '@/types/models/user'

export const userApi = {
  getList: (params: PageParams) =>
    request.get<PagedResult<UserInfo>>('/api/users', { params }),

  getById: (id: string) =>
    request.get<UserInfo>(`/api/users/${id}`),

  create: (data: CreateUserRequest) =>
    request.post<UserInfo>('/api/users', data),

  update: (id: string, data: UpdateUserRequest) =>
    request.put(`/api/users/${id}`, data),

  delete: (id: string) =>
    request.delete(`/api/users/${id}`),
}
```

### Store 规范

```typescript
import { defineStore } from 'pinia'
import { userApi } from '@/api/modules/user'
import type { UserInfo } from '@/types/models/user'

export const useUserStore = defineStore('user', () => {
  const users = ref<UserInfo[]>([])
  const loading = ref(false)

  const fetchUsers = async () => {
    loading.value = true
    try {
      const result = await userApi.getList({ page: 1, pageSize: 20 })
      users.value = result.items
    } finally {
      loading.value = false
    }
  }

  return { users, loading, fetchUsers }
})
```

## 前后端协作契约

### API 响应格式（统一）

```json
{
  "code": 200,
  "message": "Success",
  "data": { }
}
```

分页响应：

```json
{
  "code": 200,
  "message": "Success",
  "data": {
    "items": [],
    "totalCount": 100,
    "pageIndex": 1,
    "pageSize": 20
  }
}
```

错误响应：

```json
{
  "code": 400,
  "message": "Validation failed",
  "errors": [
    { "field": "email", "message": "Invalid email format" }
  ]
}
```

### 前后端类型同步

- 后端 DTO 属性名使用 camelCase 序列化（`System.Text.Json` 默认配置）
- 前端 TypeScript 类型必须与后端 DTO 属性名保持一致
- API 变更必须先更新后端 DTO，再同步前端类型
- 禁止前端硬编码 API 路径，统一在 `api/modules/` 中管理

### 认证流程

1. 前端登录 → 后端返回 JWT Access Token + Refresh Token
2. 前端 Axios 拦截器自动附加 `Authorization: Bearer <token>`
3. Token 过期 → 拦截器自动使用 Refresh Token 刷新
4. 刷新失败 → 清除本地状态，跳转登录页

## 组件复用检查规则（强制）

在编写或引入任何通用组件、组合式函数、工具函数、后端服务/帮助类之前，**必须先检查项目中是否已存在同等功能的实现**。

### 检查范围

| 检查时机 | 检查位置 | 说明 |
|----------|---------|------|
| 写前端通用组件 | `web/src/components/common/` | 弹窗、表单、列表、空状态、加载等 |
| 写组合式函数 | `web/src/composables/` | 分页、表单、加载状态等 |
| 写工具函数 | `web/src/utils/` | 格式化、校验、转换等 |
| 写后端帮助类 | `src/ByteBite.Shared/Helpers/` | 日期、字符串、加密等 |
| 写后端扩展方法 | `src/ByteBite.Shared/Extensions/` | 集合、字符串、Task 等 |
| 写后端服务 | `src/ByteBite.Application/Services/` | 业务逻辑服务 |
| 写后端仓储 | `src/ByteBite.Infrastructure/Repositories/` | 数据访问实现 |

### 检查流程

1. **识别**：当前要写的组件/函数/类是否具有通用性（可被多个页面/模块复用）
2. **搜索**：在上述对应目录中搜索是否已存在同名或同功能的文件
3. **判断**：
   - 已存在且功能匹配 → **复用现有实现**，不重复创建
   - 已存在但功能不完整 → **扩展现有实现**，不新建替代
   - 不存在 → **新建**，放入对应通用目录
4. **记录**：在 Spec 的 Plan 中记录复用/新建决策

### 禁止项

- 禁止在业务页面目录中创建通用组件（应放入 `components/common/`）
- 禁止创建功能重复的工具函数（如已有 `formatPrice` 就不再写 `formatMoney`）
- 禁止在 Controller 中直接编写可复用逻辑（应提取到 Application Service）

## 通用约束

### 安全

- 密码使用 BCrypt 或 Argon2 哈希，禁止明文或 MD5/SHA
- JWT 密钥从配置读取，禁止硬编码
- API 输入必须经过 FluentValidation 验证
- 敏感数据（密码、Token）禁止写入日志
- SQL 必须通过 EF Core 参数化查询，禁止字符串拼接
- 跨域配置必须在 `Program.cs` 显式声明，禁止 `*`

### 异常处理

- 后端全局异常中间件统一捕获，返回标准错误响应
- 领域异常继承自自定义 `DomainException`
- 禁止在 Controller 中 try-catch 后返回非标准格式
- 前端 Axios 拦截器统一处理错误提示

### 日志

- 后端使用 Serilog 结构化日志
- 请求日志中间件记录请求/响应摘要
- 前端关键操作记录到后端审计日志

### 可测试性编码规则（强制）

后端所有接口和方法必须支持单元测试。代码设计必须遵循可测试性原则，确保每个业务逻辑单元都能被独立验证。

**核心原则：**

- **所有 Application 层服务必须有对应的单元测试**
- **所有 API 端点必须有对应的集成测试**
- **禁止编写无法被单元测试的代码**（如：在业务逻辑中直接 new 依赖、静态方法调用外部资源）

**依赖注入规则：**

| 规则 | 说明 |
|------|------|
| 构造函数注入 | 所有依赖通过构造函数注入，禁止在服务内部 new 依赖 |
| 接口隔离 | 依赖抽象（接口），不依赖具体实现 |
| 禁止静态依赖 | 禁止在业务逻辑中调用 `DateTime.Now`、`Guid.NewGuid()` 等静态方法，通过 `IDateTimeProvider`、`IGuidProvider` 等接口包装 |
| 禁止直接依赖 DbContext | Application 层通过 `IRepository<T>` 接口访问数据，不直接依赖 `ByteBiteDbContext` |
| 禁止直接依赖 HttpClient | 外部服务调用通过 `IHttpClientFactory` + 接口包装 |

**测试分层：**

| 层 | 测试类型 | 覆盖范围 | 框架 |
|---|---|---|---|
| Domain | 单元测试 | 实体行为、值对象、领域事件、领域异常 | xUnit + FluentAssertions |
| Application | 单元测试 | 业务服务逻辑、验证规则、映射配置 | xUnit + FluentAssertions + Moq |
| Infrastructure | 集成测试 | 仓储实现、DbContext 配置、缓存 | xUnit + Testcontainers PostgreSQL |
| Api | 集成测试 | API 端点、中间件、过滤器 | xUnit + WebApplicationFactory |
| 端到端 | E2E 测试 | 核心业务流程 | Playwright |

**测试命名：**

- 方法名_场景_预期结果：`CreateOrder_WhenStoreIsClosed_ThrowsDomainException`
- 测试类命名：`{被测类名}Tests`：`OrderServiceTests`

**测试文件组织：**

```
tests/
├── ByteBite.UnitTests/
│   ├── Domain/
│   │   ├── Entities/
│   │   └── ValueObjects/
│   └── Application/
│       ├── Services/
│       └── Validators/
├── ByteBite.IntegrationTests/
│   ├── Api/
│   ├── Infrastructure/
│   └── Shared/              # 共享的 WebApplicationFactory、数据库种子
└── ByteBite.E2ETests/
    └── Pages/
```

**单元测试模板：**

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IStoreRepository> _storeRepoMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _storeRepoMock = new Mock<IStoreRepository>();
        _sut = new OrderService(_orderRepoMock.Object, _storeRepoMock.Object);
    }

    [Fact]
    public async Task CreateOrder_WhenStoreIsClosed_ThrowsDomainException()
    {
        var store = Store.Create("测试店铺", StoreStatus.Closed);
        _storeRepoMock.Setup(r => r.GetByIdAsync(store.Id, default)).ReturnsAsync(store);

        var act = () => _sut.CreateOrderAsync(store.Id, new CreateOrderRequest());

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*营业*");
    }
}
```

**集成测试模板：**

```csharp
public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ByteBiteDbContext>>();
                services.AddDbContext<ByteBiteDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new { /* ... */ });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

**覆盖率要求：**

| 层 | 最低覆盖率 | 说明 |
|---|---|---|
| Domain | 90% | 纯逻辑，必须高覆盖 |
| Application | 80% | 核心业务逻辑 |
| Api | 60% | 端点集成测试覆盖主要流程 |
| Infrastructure | 50% | 仓储和外部服务集成测试 |

**禁止项：**

- 禁止在业务逻辑中使用 `new` 创建可变依赖（必须通过 DI 注入）
- 禁止在测试中依赖外部服务（使用 Mock 替代）
- 禁止跳过失败的测试（必须修复或删除）
- 禁止在 CI 中排除测试项目
- 禁止提交没有对应测试的新增 Application Service

## 与 SDD-RIPER 的协作

本 skill 不定义流程，只定义技术边界。使用方式：

- 当 `$sdd-riper-one` 或 `$sdd-riper-one-light` 进入 Plan/Execute 阶段时，自动加载本 skill 的技术约束
- Codemap 生成时，按本 skill 定义的项目结构组织索引
- Spec 中涉及文件路径时，遵循本 skill 的目录约定
- 前后端契约变更时，Spec 必须记录 Contract Interface
- 多项目模式下，`web/` 和 `src/` 作为两个项目，本 skill 定义它们的协作边界

## 按需参考

- `references/ef-core-patterns.md`：EF Core 常用模式与踩坑
- `references/vue-patterns.md`：Vue 3 组合式 API 常用模式
- `references/api-conventions.md`：API 设计详细约定
- `references/deployment.md`：Docker 部署与配置
- `references/testing-patterns.md`：测试模式、Provider 接口、Mock 模式、集成测试基类
- `references/database-schema.md`：数据库结构文档（20 张表，ER 关系，各表详细列定义）
