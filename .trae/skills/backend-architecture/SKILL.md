---
name: "backend-architecture"
description: "Defines the minimal backend architecture rules for KongKong.ByteBite. Invoke when writing or refactoring any .NET backend code, adding controllers, services, or repositories."
---

# 后端极简架构规范

## 项目分层

```
ByteBite.Api          → 控制器、过滤器、Hub、Program.cs
ByteBite.Application  → 服务类、验证器、异常定义
ByteBite.Infrastructure → EF Core DbContext、实体、实体扩展
ByteBite.Shared       → 公共工具类（密码哈希、取货码生成等）
```

依赖关系：Api → Application → Infrastructure → Shared（Api 也可直接引用 Infrastructure 和 Shared）

## 核心规则

### 1. 控制器直接返回实体，不包装 ApiResponse

```csharp
// ✅ 正确 - 直接返回实体
[HttpGet("{id:guid}")]
public async Task<Merchant> GetById(Guid id)
{
    return await _merchantService.GetByIdAsync(id);
}

// ❌ 错误 - 不要包装
public async Task<ActionResult<ApiResponse<MerchantDto>>> GetById(Guid id)
```

### 2. 全局过滤器统一包装响应和错误

- 所有 HTTP 响应统一返回 200
- 通过全局 `ApiResponseWrapperFilter` 自动将返回值包装为 `{ code, message, data }` 格式
- 通过全局 `GlobalExceptionFilter` 捕获所有异常，返回 `{ code: 500, message: "错误信息", detail: "详细堆栈", data: null }`
- 业务异常使用 `BusinessException` 类，携带业务错误码和消息
- 非业务异常通过 `detail` 字段返回详细错误信息（异常类型+消息+堆栈）

### 3. 服务类不需要接口

```csharp
// ✅ 正确 - 直接注册实现类
builder.Services.AddScoped<MerchantService>();

// ❌ 错误 - 过度抽象
builder.Services.AddScoped<IMerchantService, MerchantService>();
```

### 4. 不要定义仓储层，Service 直接操作 DbContext

```csharp
// ✅ 正确 - Service 直接注入 DbContext 操作数据
public class MerchantService
{
    private readonly ByteBiteDbContext _db;

    public async Task<Merchant> GetByIdAsync(Guid id)
    {
        return await _db.Merchants.FindAsync(id) ?? throw new BusinessException(404, "商家不存在");
    }
}

// ❌ 错误 - 不要定义无意义的仓储层
public class MerchantRepository { ... }
```

### 5. 不要大量定义 DTO，直接返回 EF 实体到前端

```csharp
// ✅ 正确 - 直接返回 EF 实体
public async Task<Merchant> LoginAsync(string phone, string password)
{
    var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Phone == phone);
    return merchant;
}

// ❌ 错误 - 不要定义无意义的 DTO 做映射
public class MerchantDto { ... }
```

**例外**：仅在以下情况定义 DTO：
- 创建/更新请求的入参（如 `CreateMerchantRequest`、`UpdateStoreRequest`）
- 实体包含敏感字段（如 PasswordHash）需要过滤时
- 前端需要的字段与实体差异很大时

### 6. Code First 模式 + partial class 扩展

项目已从 DB First 迁移到 **Code First** 模式，同时保留 partial class 扩展机制。

**Code First 迁移管理**：
1. 实体中的数据库字段可自由修改
2. 在 `ByteBiteDbContext.OnModelCreating` 中添加字段配置（列名映射、注释、默认值等）
3. 执行 `dotnet ef migrations add <MigrationName>` 生成迁移
4. 应用启动时 `db.Database.MigrateAsync()` 自动应用迁移

**partial class 扩展机制**（保留）：
- EF 实体文件（`Persistence/Entities/`）**只包含数据库结构相关属性**，保持干净
- 非数据库字段（如 Token、计算属性等）通过 partial class 扩展
- 扩展文件位于 `Persistence/Extensions/Entities/`，命名空间与原实体相同
- 扩展字段标注 `[NotMapped]`

**规则**：
- 数据库字段 → 直接在实体文件 `Persistence/Entities/{EntityName}.cs` 中添加
- 非数据库字段 → 在扩展文件 `Persistence/Extensions/Entities/{EntityName}Partial.cs` 中添加
- 新增数据库字段必须在 `OnModelCreating` 中配置 `HasColumnName`，确保 snake_case 列名映射
- 每次新增数据库字段后必须生成迁移：`dotnet ef migrations add Xxx --output-dir Persistence/Migrations --context ByteBiteDbContext`
- 迁移文件位于 `ByteBite.Infrastructure/Persistence/Migrations/`
- 迁移工具：`ByteBiteDbContextFactory`（Design-time factory，用于 `dotnet ef` 命令）

### 7. 统一错误处理

```csharp
// 业务异常 - 在 Service 层抛出
throw new BusinessException(401, "用户名或密码错误");
throw new BusinessException(404, "商家不存在");

// 控制器不需要 try-catch，全局过滤器自动处理
```

### 8. DI 注册简化

```csharp
// 服务 - 直接注册类
builder.Services.AddScoped<MerchantService>();
builder.Services.AddScoped<OrderService>();
```

### 9. 代码注释规范

每个类、方法、重要逻辑都必须写注释，确保代码可读性和可维护性。

**类级别注释**：每个类上方必须用 `///` 文档注释说明类的用途

```csharp
/// <summary>
/// 商家服务 - 处理商家登录、注册、信息查询等业务逻辑
/// </summary>
public class MerchantService
```

**方法级别注释**：每个 public 方法上方必须用 `///` 文档注释说明功能

```csharp
/// <summary>
/// 商家登录 - 验证手机号和密码，生成Token返回
/// </summary>
/// <param name="phone">手机号</param>
/// <param name="password">密码</param>
/// <param name="ct">取消令牌</param>
/// <returns>登录成功的商家实体（含Token）</returns>
/// <exception cref="BusinessException">401-手机号或密码错误, 403-账号被禁用或待审核</exception>
public async Task<Merchant> LoginAsync(string phone, string password, CancellationToken ct = default)
```

**重要逻辑注释**：关键业务逻辑行内必须用 `//` 注释说明意图

```csharp
// 生成Base64编码的Token用于前端鉴权
admin.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

// 检查商家状态，禁用和待审核状态不允许登录
if (merchant.Status == "disabled") throw new BusinessException(403, "账号已被禁用");

// 审计日志写入失败时回滚该条记录，确保商家状态更新不受影响
try { ... } catch { ... }
```

**注释规则**：
- 类注释：`/// <summary>` 说明类的职责和用途
- 方法注释：`/// <summary>` 说明功能 + `/// <param>` 说明参数 + `/// <returns>` 说明返回值 + `/// <exception>` 说明可能抛出的异常
- 逻辑注释：用 `//` 简短说明为什么这样做，而不是做了什么（代码本身已经说明了做了什么）
- 不要写无意义的注释（如 `// 赋值`、`// 返回结果`）
- 注释语言：中文

## 文件组织

```
ByteBite.Api/
  Controllers/        ← 控制器，只做参数接收和调用 Service
  Data/               ← 数据初始化模块
    IDataSeeder.cs    ← 数据初始化接口（Order + SeedAsync）
    SeedData.cs       ← 初始化入口（协调各 Seeder）
    Seeders/          ← 各模块独立 Seeder
  Filters/            ← 全局过滤器（ApiResponseWrapperFilter, GlobalExceptionFilter）
  Hubs/               ← SignalR Hub
  Program.cs

ByteBite.Application/
  Services/           ← 业务逻辑，直接注入 DbContext 操作数据
  Requests/           ← 创建/更新请求的入参 DTO（仅在有需要时定义）
  Exceptions/         ← BusinessException 等自定义异常

ByteBite.Infrastructure/
  Persistence/
    ByteBiteDbContext.cs     ← EF Core DbContext（OnModelCreating 配置所有实体）
    ByteBiteDbContextFactory.cs ← Design-time factory（用于 dotnet ef 命令）
    Entities/                ← EF 实体（Code First，仅包含数据库结构字段）
    Extensions/Entities/     ← 实体 partial class 扩展（非数据库字段，如 Token）
    Migrations/              ← EF Core 迁移文件

ByteBite.Shared/
  Helpers/            ← PasswordHasher, PickupCodeGenerator, Base36Encoder
```

## 数据初始化模块

新增功能后，需要初始化数据时，创建一个新的 Seeder 类：

```csharp
// 文件位置: ByteBite.Api/Data/Seeders/XxxSeeder.cs
public class XxxSeeder : IDataSeeder
{
    public int Order => 60; // 越小越先执行
    public async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();
        // 初始化逻辑（先检查是否已存在，幂等性）
    }
}
```

然后在 `SeedData.cs` 的 `InitializeAsync` 方法中注册新的 Seeder。

**Order 参考**：
- 10: AdminSeeder
- 20: IndustryCategorySeeder
- 30: TemplateSeeder
- 40: StoreMenuSeeder
- 50: OrderSeeder
- 100: StoreCodeSeeder