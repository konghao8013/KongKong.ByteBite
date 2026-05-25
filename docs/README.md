# 空空码上点单 (KongKong.ByteBite) - 项目文档

> 版本：v1.1.0 | 更新日期：2026-05-22

## 文档导航

```
docs/
├── README.md                    ← 本文件（文档导航索引）
├── architecture/                ← 架构设计
│   ├── overview.md              ← 系统架构总览
│   ├── decisions.md             ← 架构决策记录（ADR）
│   └── style-guide.md           ← UI 风格指南
├── contracts/                   ← API 契约
│   ├── auth.md                  ← 认证鉴权
│   ├── merchant.md              ← 商家端 API
│   ├── customer.md              ← 顾客端 API
│   ├── admin.md                 ← 管理端 API
│   ├── order.md                 ← 订单 API
│   ├── store.md                 ← 店铺 API
│   ├── menu.md                  ← 分类+商品 API
│   ├── dashboard.md             ← 经营数据 API
│   └── discount.md              ← 优惠活动 API
├── data/                        ← 数据模型
│   ├── overview.md              ← 实体关系总览
│   └── seed-data.md             ← 种子数据说明
├── frontend/                    ← 前端文档
│   ├── overview.md              ← 前端架构总览
│   └── components.md            ← 组件文档
├── specs/                       ← 功能规格（按领域组织）
│   ├── README.md                ← Spec 索引
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
├── requirements/                ← 产品需求
│   └── requirements-overview.md ← 需求清单
└── sql/                         ← 数据库脚本
    ├── 01_users_and_stores.sql
    ├── 02_categories_and_products.sql
    ├── 03_orders_and_discounts.sql
    ├── 04_template_system.sql
    ├── 05_customer_registration.sql
    ├── 06_business_dashboard.sql
    └── 07_platform_admin.sql
```

## 快速入口

| 角色 | 推荐阅读 |
|------|----------|
| 新成员 | [架构总览](architecture/overview.md) → [数据模型](data/overview.md) → [API 契约](contracts/auth.md) |
| 后端开发 | [架构决策](architecture/decisions.md) → [API 契约](contracts/) → [Spec](specs/) |
| 前端开发 | [前端架构](frontend/overview.md) → [API 契约](contracts/) → [风格指南](architecture/style-guide.md) |
| 产品/设计 | [需求清单](requirements/requirements-overview.md) → [Spec](specs/README.md) |

## 技术栈

| 层级 | 技术 |
|------|------|
| 前端 | Vue 3.5 + Vite 8 + TypeScript 6 + Element Plus + Pinia |
| 后端 | .NET 10 + EF Core 10 (DB First) |
| 数据库 | PostgreSQL 17 |
| 实时通信 | SignalR |
| 部署 | 前端端口 3000 (dev) / 后端端口 5044 |
