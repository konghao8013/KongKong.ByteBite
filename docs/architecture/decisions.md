# 架构决策记录 (ADR)

> 记录项目中所有重要的架构决策及其背景、理由和影响

## ADR-001: 极简分层架构（无DTO/无仓储/无接口）

**日期**：2026-05-20
**状态**：已采纳

### 背景

项目初期需要快速迭代，传统分层架构（Controller → DTO → Service → Repository → Interface）对于 MVP 来说过于冗余。

### 决策

采用极简架构：
- Service 直接返回 EF 实体，不定义 DTO
- Service 直接注入 DbContext，不定义仓储层
- Service 直接注册实现类，不定义接口

### 理由

1. MVP 阶段核心是快速交付，过度抽象增加维护成本
2. EF 实体和数据库字段基本一致，DTO 映射是重复劳动
3. DbContext 已经是最佳的"仓储"，再包装一层无意义
4. 单实现类的接口是 YAGNI（You Aren't Gonna Need It）

### 影响

- 优点：代码量减少 40%+，开发速度提升
- 缺点：后期如果需要替换 ORM 或增加缓存层，需要重构 Service 层
- 例外：创建/更新请求入参（如 UnifiedLoginRequest）仍然定义为 DTO；实体敏感字段（PasswordHash）直接在实体中标注 [NotMapped]

---

## ADR-002: Code First 模式 + partial class 扩展

**日期**：2026-05-25
**状态**：已采纳（替代原 ADR-002 DB First 模式）

### 背景

项目初期使用 DB First 模式（SQL 脚本建表 → Scaffold 生成实体），但存在以下问题：
- 手动修改实体中的数据库字段会被下次 Scaffold 覆盖
- 数据库结构变更依赖手动执行 SQL 脚本，容易遗漏
- AI 辅助开发时，新增字段需要同时修改实体、SQL 脚本、DbContext 三处，效率低

### 决策

迁移到 **Code First** 模式，同时保留 partial class 扩展机制：

1. **Code First 迁移管理**：
   - 实体中的数据库字段可自由修改
   - 通过 `dotnet ef migrations add` 生成迁移，`db.Database.MigrateAsync()` 启动时自动应用
   - 迁移文件位于 `Persistence/Migrations/`

2. **partial class 扩展机制**（保留）：
   - EF 实体文件（`Persistence/Entities/`）只包含数据库结构相关属性，保持干净
   - 非数据库字段（如 Token、计算属性等）通过 partial class 扩展
   - 扩展文件位于 `Persistence/Extensions/Entities/`，命名空间与原实体相同
   - 扩展字段标注 `[NotMapped]`

### 理由

1. Code First 让 AI 辅助开发更高效：新增字段只需修改实体 + 生成迁移
2. partial class 扩展让实体职责分离：数据库字段 vs 运行时属性
3. 实体文件只反映数据库结构，阅读更清晰
4. 迁移自动追踪数据库版本，不再依赖手动 SQL 脚本

### 影响

- 新增字段流程：修改实体（仅数据库字段） → OnModelCreating 配置 → `dotnet ef migrations add` → 启动应用
- 非数据库字段：在 `Persistence/Extensions/Entities/` 下创建 `{EntityName}Partial.cs`
- 新增 `ByteBiteDbContextFactory`（Design-time factory，支持 dotnet ef 命令）
- 新增 `Persistence/Migrations/` 目录，包含所有迁移文件
- Program.cs 启动时调用 `db.Database.MigrateAsync()` 自动应用迁移

---

## ADR-003: 全局过滤器统一响应包装

**日期**：2026-05-20
**状态**：已采纳

### 背景

前后端需要统一的 API 响应格式，避免每个控制器重复包装。

### 决策

- 控制器直接返回实体/对象，不做包装
- `ApiResponseWrapperFilter` 自动包装为 `{ code, message, data, detail }`
- `GlobalExceptionFilter` 捕获异常，BusinessException 返回业务错误码，其他异常返回 500 + detail

### 理由

1. 统一格式让前端 API 拦截器可以统一处理
2. 控制器代码更简洁，专注于业务调用
3. 异常处理集中在过滤器，避免 try-catch 散落各处

### 影响

- 前端 Axios 拦截器自动解包 data，request.get/post 返回的是 data 部分
- 业务错误通过 BusinessException(code, message) 表达
- 前端通过 catch 拿到的就是 { code, message } 结构

---

## ADR-004: 统一登录（账号自动识别角色）

**日期**：2026-05-21
**状态**：已采纳

### 背景

原始设计需要用户先选择角色（管理员/商家/顾客）再登录，体验不佳。实际使用中用户只关心"输入账号就能登录"。

### 决策

提供统一登录 API `POST /api/auth/login`，根据账号格式自动识别角色：
- 非11位手机号 → 优先尝试管理员登录
- 11位手机号 → 依次尝试商家 → 顾客

### 理由

1. 用户不需要思考"我是哪种角色"
2. 手机号天然区分商家/顾客，用户名天然区分管理员
3. 降低了登录页面的认知负担

### 影响

- 登录页只需一个输入框 + 一个密码框
- 返回 `{ role, data, storeId }` 前端根据 role 自动跳转
- 原有的 /merchants/login、/admin/login 保留作为兼容接口

---

## ADR-005: 瑚红（Coral）色彩体系

**日期**：2026-05-22
**状态**：已采纳

### 背景

原项目使用暗黑风格（#1a1a2e / #2A2A2A），偏向技术感。产品面向餐饮商家，需要更温暖、更有食欲感的视觉风格。

### 决策

采用珊瑚红（Coral）色彩体系：
- 主色：#FF6B6B 珊瑚红
- 渐变：#FF6B6B → #FF8E53 暖橙
- 强调：#FFBE0B 暖金
- 背景：#F7F7F7 浅灰 + #FFFFFF 白底卡片
- 文字：#1A1A2E 深蓝黑 + #8C8C8C 辅助灰

### 理由

1. 珊瑚红+暖橙传达热情、食欲感，适合餐饮场景
2. 白底+微阴影的卡片风格接近美团/大众点评，用户认知成本低
3. 渐变按钮和Header增加视觉层次感
4. 暗黑风格对餐饮商家来说过于冷感

### 影响

- SCSS 变量体系全面更新（variables.scss）
- 所有页面（15个 Vue 文件）色彩替换
- CSS 变量兼容：保留 $brand-color/$dark-bar 别名映射

---

## ADR-006: 数据初始化模块化（IDataSeeder）

**日期**：2026-05-25
**状态**：已采纳（替代原 ADR-006 种子数据自动初始化）

### 背景

原有 SeedData 是一个 450+ 行的大方法，所有初始化逻辑耦合在一起。新增功能后需要在庞大的方法中找到合适位置插入逻辑，容易出错。AI 辅助开发时，新功能的初始化逻辑应该独立、自包含。

### 决策

采用模块化数据初始化体系：
- 定义 `IDataSeeder` 接口（`Order` 属性 + `SeedAsync` 方法）
- 每个业务领域创建独立 Seeder 类
- `SeedData.InitializeAsync` 按 Order 顺序执行所有 Seeder
- 每个 Seeder 内部先检查是否已初始化（幂等性）

### 理由

1. 新增功能只需创建一个 Seeder 文件，无需修改现有代码
2. Order 属性控制执行顺序，依赖关系清晰
3. 每个 Seeder 独立、自包含，可单独测试
4. AI 辅助开发时，一个功能的初始化代码集中在一个文件中

### 影响

- 当前 Seeder 列表：AdminSeeder(10) → IndustryCategorySeeder(20) → TemplateSeeder(30) → StoreMenuSeeder(40) → OrderSeeder(50) → StoreCodeSeeder(100)
- 新增功能时创建 `Seeders/XxxSeeder.cs`，在 `SeedData.cs` 中注册
- 原 SeedData.cs 的大方法拆分为 `file class`（TemplateSeeder、StoreMenuSeeder、OrderSeeder）

---

## ADR-007: 店铺码自增编码 + 短链分享体系

**日期**：2026-05-25
**状态**：已采纳

### 背景

店铺分享链接当前使用 UUID 格式（如 `/store/550e8400-e29b-41d4-a716-446655440000`），链接过长，不适合线下扫码、口口相传等场景。需要一种更短的店铺标识方式。

### 决策

1. **店铺码（StoreCode）**：基于数据库 int 自增 ID，通过 Base36 编码为6位简码显示
   - 字符集：A-Z（26字母）+ 0-9（10数字）= 36进制
   - 编码示例：自增ID 1 → `000001`，自增ID 1000000 → `LFLS0S`
   - 数据库存储 `store_code` 字段（VARCHAR(6), UNIQUE），创建店铺时自动生成
   - 最大容量：36^6 = 2,176,782,336 个店铺（远超需求）

2. **短链分享体系**：`/{模块代号}/{店铺码}`
   - 模块代号：A-Z（26字母）+ 0-9（10数字）= 36个模块位
   - 当前使用：`/A/` = 店铺点单页
   - 分享链接示例：`http://host/A/AFXF08`
   - 后续可扩展：`/B/` = 优惠活动页、`/C/` = 会员页等

3. **前端路由**：`/A/:code` 替代原 `/store/:storeId`

### 理由

1. UUID 链接长度 50+ 字符，6位店铺码链接仅 10 字符，扫码/口传体验大幅提升
2. 自增 ID + Base36 编码是确定性算法，无需随机碰撞检测，比取货码的循环检查更高效
3. 模块化短链设计（/A/、/B/...）为后续业务扩展预留了 36 个模块位
4. Base36 是行业标准（YouTube、短链服务均采用），认知成本低

### 影响

- Store 实体新增 `StoreCode` 字段（VARCHAR(6), UNIQUE, NOT NULL）
- CustomerStoreController 新增按 StoreCode 查询菜单的 API
- 前端顾客端路由从 `/store/:storeId` 改为 `/A/:code`
- StoreShareDialog 分享链接使用短链格式
- 旧路由 `/store/:storeId` 需要兼容重定向