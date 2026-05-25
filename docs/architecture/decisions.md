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
- 例外：创建/更新请求入参（如 UnifiedLoginRequest）仍然定义为 DTO；实体敏感字段（PasswordHash）用 partial class + [NotMapped] 处理

---

## ADR-002: DB First 模式 + partial class 扩展

**日期**：2026-05-20
**状态**：已采纳

### 背景

数据库由 SQL 脚本定义，EF Core 通过 Scaffold 生成实体类。如果手动修改生成的实体，下次 Scaffold 会覆盖。

### 决策

- EF 实体由 Scaffold 生成，放在 `Persistence/Entities/`
- 不手动修改生成的实体文件
- 需要添加非数据库字段时，使用 partial class 扩展，放在 `Extensions/Entities/`
- 扩展字段标注 `[NotMapped]`

### 理由

1. Scaffold 生成的代码应被视为"不可修改"的生成产物
2. partial class 是 C# 语言特性，天然支持扩展而不修改原文件
3. [NotMapped] 确保 EF 不尝试将扩展字段映射到数据库

### 影响

- 扩展文件命名规则：`{EntityName}Partial.cs`
- 扩展文件命名空间必须与原实体相同
- Token 字段、计算属性等全部通过此方式添加

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

## ADR-006: 种子数据自动初始化

**日期**：2026-05-20
**状态**：已采纳

### 背景

开发和演示需要初始数据，手动执行 SQL 不方便且容易遗忘。

### 决策

应用启动时在 Program.cs 中调用 `SeedData.InitializeAsync()`，自动检查并创建缺失数据。

### 理由

1. 开发环境一键启动就有数据，无需手动操作
2. 检查式创建（if not exist then create）不会重复插入
3. 管理员密码自动重置（如果 BCrypt 验证不匹配则更新），确保 admin/admin123 始终可用

### 影响

- 每次启动会执行 6-8 条 SELECT 检查查询（性能影响微乎其微）
- 数据变更需修改 SeedData.cs 而非 SQL 脚本