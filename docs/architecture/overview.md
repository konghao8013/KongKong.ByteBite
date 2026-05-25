# 系统架构总览

> 版本：v1.1.0 | 更新日期：2026-05-22

## 1. 系统定位

**空空码上点单**（KongKong.ByteBite）是面向烧烤店、小卖部、饭馆的轻量级扫码点单系统。

核心流程：商家创建店铺 → 顾客扫码 → 浏览菜单 → 下单 → 商家接单 → 顾客取餐

## 2. 角色体系

| 角色 | 登录方式 | 入口 | 核心职责 |
|------|----------|------|----------|
| 管理员 | 用户名+密码 | /admin/* | 审核商家、平台统计、系统配置 |
| 商家 | 手机号+密码 | /merchant/* | 管理店铺、菜单、接单、经营数据 |
| 顾客 | 扫码进入（无需登录） | /store/:storeId/* | 浏览菜单、下单、查看取货码 |

**统一登录**：`POST /api/auth/login` 根据账号自动识别角色，返回 `{ role, data, storeId }`

## 3. 后端架构

### 3.1 项目分层

```
ByteBite.Api          → 控制器、过滤器、Hub、Program.cs
ByteBite.Application  → 服务类、异常定义
ByteBite.Infrastructure → EF Core DbContext、实体、实体扩展
ByteBite.Shared       → 公共工具类（密码哈希、取货码生成等）
ByteBite.Domain       ← 预留领域层（当前为空）
```

依赖方向：Api → Application → Infrastructure → Shared

### 3.2 架构规则

| 规则 | 说明 |
|------|------|
| 无 DTO 层 | Service 直接返回 EF 实体，全局过滤器自动包装 ApiResponse |
| 无仓储层 | Service 直接注入 DbContext 操作数据 |
| 无接口层 | Service 直接注册实现类，不定义 IService/Repository 接口 |
| DB First | EF 实体由 Scaffold 生成，不手动修改；扩展用 partial class + [NotMapped] |
| 全局过滤 | ApiResponseWrapperFilter 包装响应 + GlobalExceptionFilter 捕获异常 |
| 业务异常 | Service 层抛出 BusinessException(code, message)，控制器无需 try-catch |

### 3.3 全局响应格式

```json
{ "code": 200, "message": "success", "data": { ... }, "detail": null }
```

- 成功：code=200，data 包含业务数据
- 业务异常：code=4xx（401 未授权、403 禁止、404 不存在）
- 系统异常：code=500，detail 包含堆栈信息

### 3.4 鉴权机制

登录成功后服务端生成 Base64 编码 Token，前端存 localStorage，请求时 `Authorization: Bearer {token}` 携带。

| 角色 | Token 存储 key |
|------|---------------|
| 管理员 | admin_token |
| 商家 | merchant_token |
| 顾客 | customer_token |

### 3.5 实时通信

| Hub | 路径 | 用途 |
|-----|------|------|
| OrderHub | /hubs/order | 订单状态变更推送（商家端接收新订单） |
| StoreHub | /hubs/store | 店铺状态变更推送（顾客端感知营业/打烊） |

## 4. 前端架构

### 4.1 技术栈

Vue 3.5 + Vite 8 + TypeScript 6 + Element Plus + Pinia + Axios + SignalR

### 4.2 三端布局

| 端 | 布局 | 导航方式 |
|----|------|----------|
| 商家端 | H5移动端 | 底部TabBar（订单/菜单/店铺/数据） |
| 管理端 | PC侧栏布局 | 左侧菜单（商家管理/系统配置/平台统计） |
| 顾客端 | H5移动端 | 顶部导航 + 浮动购物车 |

### 4.3 API 封装

- Axios 实例自动解包 ApiResponse.data，request.get/post 返回的是 data 部分
- API 模块化组织：admin / merchant / store / category / product / order / dashboard / discount / customer / template

### 4.4 色彩体系

| 角色 | 主色 | 强调色 | 渐变 |
|------|------|--------|------|
| 全局 | #FF6B6B 珊瑚红 | #FFBE0B 暖金 | #FF6B6B → #FF8E53 |
| 背景 | #F7F7F7 | — | — |
| 卡片 | #FFFFFF | — | box-shadow 微阴影 |
| 文字主 | #1A1A2E | — | — |
| 文字副 | #8C8C8C | — | — |

## 5. 数据库

PostgreSQL 17，连接：192.168.3.22:5432，数据库 kongkong_bytebite

32 个实体，核心实体关系：

```
Admin ─┐
       │
Merchant ─── Store ─┬── Category ─── Product ─── SpecGroup ─── SpecOption
                     ├── Order ─── OrderItem
                     ├── DiscountRule
                     ├── DailyStoreStat / HourlyOrderStat / ProductSalesStat
                     └── MerchantAuditLog

Customer ─── CustomerSession / CustomerConsumptionStat
IndustryCategory ─── StoreTemplate ─── TemplateCategory ─── TemplateProduct
```

## 6. 种子数据

应用启动时 `SeedData.InitializeAsync()` 自动初始化：
- 管理员账号：admin/admin123
- 商家 18523978013/123456 → 重庆老灶烧烤（观音桥店）
- 行业分类（6个三级分类）
- 店铺模板（重庆烧烤模板）
- 店铺菜单（8分类 + 26商品 + 5规格组）
- 优惠规则（3个）
- 订单数据（18个）

## 7. 部署

| 服务 | 端口 | 说明 |
|------|------|------|
| 前端 dev | 3000 | vite dev server |
| 后端 API | 5044 | Kestrel |
| PostgreSQL | 5432 | 外部数据库 |