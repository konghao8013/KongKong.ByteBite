# 功能规格（Spec）索引

> 版本：v1.3.0 | 更新日期：2026-05-25

## 按领域组织

| 领域 | Spec | 核心实体 | API 契约 |
|------|------|----------|----------|
| 认证鉴权 | [spec-auth.md](spec-auth.md) | Admin, Merchant, Customer | [auth.md](../contracts/auth.md) |
| 商家-店铺 | [spec-merchant-store.md](spec-merchant-store.md) | Merchant, Store, StoreCode | [merchant.md](../contracts/merchant.md), [customer.md](../contracts/customer.md) |
| 分类-商品 | [spec-category-product.md](spec-category-product.md) | Category, Product, SpecGroup, SpecOption | [merchant.md](../contracts/merchant.md) |
| 订单-取货码 | [spec-order-pickup.md](spec-order-pickup.md) | Order, OrderItem | [order.md](../contracts/order.md) |
| 优惠活动 | [spec-discount.md](spec-discount.md) | DiscountRule | [merchant.md](../contracts/merchant.md) |
| 模板系统 | [spec-template.md](spec-template.md) | IndustryCategory, StoreTemplate, Template* | — |
| 顾客 | [spec-customer.md](spec-customer.md) | Customer, CustomerSession | [customer.md](../contracts/customer.md) |
| 经营数据 | [spec-dashboard.md](spec-dashboard.md) | DailyStoreStat, HourlyOrderStat | [merchant.md](../contracts/merchant.md) |
| 管理后台 | [spec-admin.md](spec-admin.md) | MerchantAuditLog, AdminOperationLog | [admin.md](../contracts/admin.md) |
| 前端 | [spec-frontend.md](spec-frontend.md) | — | — |
| 测试 | [spec-testing.md](spec-testing.md) | — | — |

## 文档体系关系

```
需求 (requirements/)        → 产品要做什么
  ↓
规格 (specs/)               → 每个领域怎么做（业务规则+流程+状态机）
  ↓
契约 (contracts/)           → API 接口定义（请求/响应/错误码）
  ↓
数据 (data/)                → 实体关系+状态枚举
  ↓
架构 (architecture/)        → 技术选型+架构决策+风格指南
  ↓
代码 (src/ + web/)          → 实现
```