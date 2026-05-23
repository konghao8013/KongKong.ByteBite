# 空空码上点单 (KongKong.ByteBite) - 功能模块 Spec 归档

> 生成日期：2026-05-20
> 项目版本：v0.1.0

## 目录

| 模块 | 名称 | Spec 文件 |
|------|------|-----------|
| M1 | 商家注册/登录/店铺管理 | [spec-m1-merchant-store.md](spec-m1-merchant-store.md) |
| M2 | 分类/商品管理 | [spec-m2-category-product.md](spec-m2-category-product.md) |
| M3 | 顾客浏览/下单/取货码 | [spec-m3-order-pickup.md](spec-m3-order-pickup.md) |
| M4 | 优惠活动管理 | [spec-m4-discount.md](spec-m4-discount.md) |
| M5 | 商家模板系统 | [spec-m5-template.md](spec-m5-template.md) |
| M6 | 顾客注册/数据合并 | [spec-m6-customer-merge.md](spec-m6-customer-merge.md) |
| M7 | 经营数据看板 | [spec-m7-dashboard.md](spec-m7-dashboard.md) |
| M8 | 平台管理员端 | [spec-m8-admin.md](spec-m8-admin.md) |
| FE | 前端 Vue 3 项目 | [spec-frontend.md](spec-frontend.md) |
| TEST | 测试体系 | [spec-testing.md](spec-testing.md) |

## 技术栈

| 层 | 技术 | 版本 |
|----|------|------|
| 后端框架 | .NET | 10 |
| ORM | EF Core (DB First) | 10 |
| 数据库 | PostgreSQL | 17 |
| 实时推送 | SignalR | - |
| 前端框架 | Vue 3 + Vite | 3.5 / 8.0 |
| UI 组件库 | Element Plus | 2.14 |
| 状态管理 | Pinia | 3.0 |
| HTTP 客户端 | Axios | 1.16 |
| 单元测试 | xUnit + Moq + FluentAssertions | - |
| 集成测试 | WebApplicationFactory | - |

## 项目结构

```
KongKong.ByteBite/
├── src/
│   ├── ByteBite.Api/              # API层：控制器、SignalR Hub、Program.cs
│   ├── ByteBite.Application/      # 应用层：服务、DTO、仓储接口、验证器
│   ├── ByteBite.Domain/           # 领域层（当前为空，预留）
│   ├── ByteBite.Infrastructure/  # 基础设施层：仓储实现、DbContext、实体
│   └── ByteBite.Shared/           # 共享层：工具类、扩展方法
├── tests/
│   ├── ByteBite.UnitTests/        # 单元测试（92个）
│   └── ByteBite.IntegrationTests/ # 集成测试（39个）
├── web/                           # Vue 3 前端项目
├── docs/
│   ├── sql/                       # 数据库DDL（7个SQL文件）
│   ├── requirements/              # 需求文档
│   └── specs/                     # 功能Spec归档（本目录）
└── tools/                         # 工具脚本
```
