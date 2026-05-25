---
name: "sdd-doc-structure"
description: "Defines the SDD (Specification-Driven Development) documentation structure and writing conventions. Invoke when creating, updating, or reorganizing any project documentation, specs, API contracts, or architecture docs."
---

# SDD 文档结构规范

本 Skill 定义了空空码上点单项目的文档组织方式和编写规范。所有文档必须按此规范创建和维护。

## 1. 文档目录结构

```
docs/
├── README.md                    ← 文档导航索引（必须维护）
├── architecture/                ← 架构层
│   ├── overview.md              ← 系统架构总览
│   ├── decisions.md             ← 架构决策记录（ADR）
│   └── style-guide.html         ← UI 风格指南
├── contracts/                   ← API 契约层（按领域组织）
│   ├── auth.md
│   ├── merchant.md
│   ├── customer.md
│   ├── admin.md
│   └── order.md
├── data/                        ← 数据模型层
│   ├── overview.md              ← 实体关系总览+状态枚举
│   └── seed-data.md             ← 种子数据说明
├── frontend/                    ← 前端层
│   ├── overview.md              ← 前端架构总览
│   └── components.md            ← 组件文档
├── specs/                       ← 功能规格层（按领域组织）
│   ├── README.md                ← Spec 索引（必须维护）
│   ├── spec-auth.md             ← 认证鉴权领域
│   ├── spec-merchant-store.md   ← 商家-店铺领域
│   ├── spec-category-product.md ← 分类-商品领域
│   ├── spec-order-pickup.md     ← 订单-取货码领域
│   ├── spec-discount.md         ← 优惠活动领域
│   ├── spec-template.md         ← 模板系统领域
│   ├── spec-customer.md         ← 顾客领域
│   ├── spec-dashboard.md        ← 经营数据领域
│   ├── spec-admin.md            ← 管理后台领域
│   ├── spec-frontend.md         ← 前端规格
│   └── spec-testing.md          ← 测试规格
├── requirements/                ← 需求层
│   └── requirements-overview.md ← 需求清单
└── sql/                         ← DDL脚本
```

## 2. 文档层级关系（自顶向下）

```
需求 (requirements/)     → 产品要做什么
  ↓
规格 (specs/)            → 每个领域怎么做（业务规则+流程+状态机）
  ↓
契约 (contracts/)        → API 接口定义（请求/响应/错误码）
  ↓
数据 (data/)             → 实体关系+状态枚举+种子数据
  ↓
架构 (architecture/)     → 技术选型+架构决策+风格指南
  ↓
代码 (src/ + web/)       → 实现
```

**原则**：上层文档变更时，必须检查下层文档是否需要同步更新。

## 3. 文档编写规范

### 3.1 通用格式

每个文档必须包含：
- **标题**：一级标题，格式为 `# 领域名称 文档类型`
- **版本头**：`> 版本：vX.Y.Z | 更新日期：YYYY-MM-DD`
- **章节编号**：使用 `## 1.` `## 2.` `### 2.1` 层级编号

示例：
```markdown
# 订单 API 契约

> 版本：v1.1.0 | 更新日期：2026-05-22

## 1. 创建订单

### 1.1 POST /api/orders

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
```

### 3.2 文档类型规范

#### architecture/ - 架构文档

- **overview.md**：系统定位、角色体系、后端分层、前端架构、数据库、部署
- **decisions.md**：架构决策记录（ADR），每个决策包含：
  - 日期、状态
  - 背景 → 决策 → 理由 → 影响

#### contracts/ - API 契约

按领域拆分，每个文件包含：
- API 列表（HTTP方法 + 路径 + 说明）
- 请求参数表格（参数/类型/必填/说明）
- 响应示例（JSON）
- 错误码说明
- 状态流转图（如有）

**命名规则**：与后端 Controller 对应，一个领域一个文件

#### data/ - 数据模型

- **overview.md**：实体关系图（ASCII art）+ 实体清单表格 + 状态枚举 + Partial Class 扩展
- **seed-data.md**：种子数据清单 + 初始化策略

#### specs/ - 功能规格

按领域组织，每个 spec 包含：
- 领域概述
- 核心实体
- 业务规则（表格+流程图）
- API 契约交叉引用
- 前端实现交叉引用

**命名规则**：`spec-{领域名}.md`，不加编号前缀

#### frontend/ - 前端文档

- **overview.md**：技术栈、目录结构、路由体系、API封装机制、色彩体系、默认账号
- **components.md**：布局组件+公共组件+组合式函数+页面组件

### 3.3 索引文件维护规则

以下文件必须在文档增删时同步更新：
1. `docs/README.md` - 目录树 + 快速入口表
2. `docs/specs/README.md` - Spec 索引表（领域/Spec/核心实体/API契约 四列）

### 3.4 新增领域的操作流程

当项目新增一个功能领域时：
1. 在 `specs/` 创建 `spec-{领域名}.md`
2. 在 `contracts/` 创建对应 API 契约文档（如有新API）
3. 在 `data/overview.md` 补充新实体到关系图和清单
4. 更新 `docs/README.md` 目录树
5. 更新 `docs/specs/README.md` 索引表
6. 如涉及架构变更，在 `architecture/decisions.md` 新增 ADR

### 3.5 禁止事项

- ❌ 不使用 m1/m2/m3 模块编号前缀命名 spec 文件
- ❌ 不创建单一大文件塞入所有内容（如 FEATURE_SPEC.md）
- ❌ 不在文档中写代码实现细节（那是代码的事）
- ❌ 不让文档与代码不同步（API 变更必须同步更新契约文档）
- ❌ 不创建孤立的文档（必须有索引引用）

## 4. 交叉引用规范

文档之间通过相对路径交叉引用：
- Spec 引用契约：`[auth.md](../contracts/auth.md)`
- 契约引用数据：`[overview.md](../data/overview.md)`
- 前端引用契约：`[auth.md](../contracts/auth.md)`
- README 引用子文档：`[overview.md](architecture/overview.md)`

## 5. 版本号规则

- 主版本（X.0.0）：架构大变更、新增大领域
- 次版本（1.Y.0）：新增API、新增实体、新增spec章节
- 修订版（1.1.Z）：文档纠错、措辞调整、补充说明

所有同一批变更的文档使用相同版本号。