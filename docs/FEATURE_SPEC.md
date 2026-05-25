# 空空码上点单 (KongKong.ByteBite) 功能文档

> 版本：v1.0.0 | 更新日期：2026-05-21

## 一、系统概述

空空码上点单是一个线上扫码点单系统，支持商家管理店铺菜单和处理订单、管理员审核商家和查看平台数据、顾客扫码点餐下单。

### 技术栈

| 层级 | 技术 |
|------|------|
| 前端 | Vue 3.5 + Vite 8 + TypeScript 6 + Element Plus + Pinia |
| 后端 | .NET 10 + EF Core 10 (DB First) |
| 数据库 | PostgreSQL 17 |
| 实时通信 | SignalR |
| 部署 | 前端端口 3000 (dev) / 后端端口 5044 |

### 系统架构

```
ByteBite.Api          → 控制器、过滤器、Hub、Program.cs
ByteBite.Application  → 服务类、异常定义
ByteBite.Infrastructure → EF Core DbContext、实体、实体扩展
ByteBite.Shared       → 公共工具类（密码哈希、取货码生成等）
```

依赖关系：Api → Application → Infrastructure → Shared

---

## 二、角色体系

| 角色 | 登录方式 | 入口 |
|------|----------|------|
| 管理员 | 用户名 + 密码 | /admin/* |
| 商家 | 手机号 + 密码 | /merchant/* |
| 顾客 | 扫码进入（无需登录）/ 手机号注册登录 | /store/:storeId/* |

### 统一登录

系统提供统一登录接口 `POST /api/auth/login`，根据账号自动识别角色：
- 非11位手机号 → 优先尝试管理员登录（用户名匹配）
- 11位手机号 → 依次尝试商家登录 → 顾客登录

返回 `{ role: "admin"|"merchant"|"customer", data: {...}, storeId?: "..." }`

---

## 三、功能模块详述

### 3.1 统一认证模块

| API | 方法 | 说明 |
|-----|------|------|
| `/api/auth/login` | POST | 统一登录（自动识别角色） |
| `/api/merchants/login` | POST | 商家登录（手机号+密码） |
| `/api/merchants/register` | POST | 商家注册（手机号+密码+店铺名称） |
| `/api/merchants/{id}/logout` | POST | 商家退出登录 |
| `/api/admin/login` | POST | 管理员登录（用户名+密码） |
| `/api/admin/{id}/logout` | POST | 管理员退出登录 |
| `/api/customers/register` | POST | 顾客注册（手机号+密码） |
| `/api/customers/login` | POST | 顾客登录（手机号+密码） |
| `/api/customers/{id}/logout` | POST | 顾客退出登录 |
| `/api/customers/{id}/merge` | POST | 顾客数据合并（设备ID关联） |

**鉴权机制**：登录成功后服务端生成 Base64 编码的 Token，前端存储在 localStorage（`merchant_token`/`admin_token`/`customer_token`），请求时通过 `Authorization: Bearer {token}` 携带。

**账号状态**：
- 商家：`active`(正常) / `pending`(待审核) / `suspended`(已封禁) / `disabled`(禁用)
- 管理员：`active`(正常) / `disabled`(禁用)
- 待审核和禁用状态的商家不允许登录

---

### 3.2 商家端功能

商家端采用 H5 移动端布局，底部 TabBar 导航，包含4个主功能 Tab。

#### 3.2.1 订单管理 (Orders)

**页面路由**：`/merchant/orders`

**功能**：
- 订单列表展示（按状态筛选：待接单/已接单/制作中/待取餐/已完成）
- 待接单订单数量 Badge 提示
- 15秒自动刷新订单列表
- 订单状态流转操作：
  - 待接单 → 接单 / 拒单
  - 已接单 → 开始制作
  - 制作中 → 制作完成
  - 待取餐 → 核销完成
- 订单详情展示：取餐码、用餐模式、桌号、商品明细、金额、备注

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/stores/{storeId}/orders` | GET | 获取店铺订单列表（支持 status/pageIndex/pageSize 参数） |
| `/api/orders/{orderId}/accept` | PATCH | 接单 |
| `/api/orders/{orderId}/reject` | PATCH | 拒单（需传 reason） |
| `/api/orders/{orderId}/prepare` | PATCH | 开始制作 |
| `/api/orders/{orderId}/ready` | PATCH | 制作完成 |
| `/api/orders/{orderId}/complete` | PATCH | 核销完成 |
| `/api/orders/{orderId}/cancel` | PATCH | 取消订单 |

**订单状态流转**：
```
pending → accepted → preparing → ready → completed
pending → rejected
任意状态 → cancelled
```

#### 3.2.2 菜单管理 (Menu)

**页面路由**：`/merchant/menu`

**功能**：
- 左侧分类侧栏 + 右侧商品列表
- 分类切换筛选商品
- 添加新分类（弹窗输入名称）
- 删除分类
- 商品上架/下架切换
- 商品信息展示：名称、描述、价格、月销量、状态、规格组

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/categories/store/{storeId}` | GET | 获取店铺分类列表 |
| `/api/categories` | POST | 创建分类（storeId + name + sortOrder） |
| `/api/categories/{id}` | PUT | 更新分类 |
| `/api/categories/{id}` | DELETE | 删除分类 |
| `/api/products/store/{storeId}` | GET | 获取店铺商品列表 |
| `/api/products/category/{categoryId}` | GET | 获取分类下商品列表 |
| `/api/products/{id}` | GET | 获取商品详情 |
| `/api/products` | POST | 创建商品 |
| `/api/products/{id}` | PUT | 更新商品（含 status 切换） |
| `/api/products/{id}` | DELETE | 删除商品 |

#### 3.2.3 店铺信息 (StoreInfo)

**页面路由**：`/merchant/store`

**功能**：
- 店铺封面图 + 营业状态 Badge
- 店铺信息展示：名称、描述、营业时间、用餐模式、月销量
- 编辑模式：修改店铺名称、描述、封面图
- 营业/打烊状态切换

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/stores/{id}` | GET | 获取店铺详情 |
| `/api/stores/{id}` | PUT | 更新店铺信息（name/description/coverImageUrl/businessStatus） |
| `/api/merchants/{merchantId}/stores` | GET | 获取商家下店铺列表 |

#### 3.2.4 经营数据 (Dashboard)

**页面路由**：`/merchant/dashboard`

**功能**：
- 今日/昨日订单数和营收
- 待处理订单数
- 累计营收
- 近期订单列表（最近5单）
- 营收趋势图表区域（预留 ECharts 集成）
- 订单趋势图表区域（预留 ECharts 集成）

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/dashboard/{storeId}/overview` | GET | 经营概览（pendingOrderCount/yesterdayOrderCount/yesterdayRevenue） |
| `/api/dashboard/{storeId}/category-sales` | GET | 分类销售统计（startDate/endDate 参数） |
| `/api/dashboard/{storeId}/hourly` | GET | 时段分布统计（date 参数） |

#### 3.2.5 优惠活动 (Discounts)

**页面路由**：`/merchant/discounts`（目前不在 TabBar 中，可通过路由访问）

**功能**：
- 优惠活动列表（满减/折扣两种类型）
- 活动状态展示：生效中/已停用/已过期
- 启用/停用切换
- 删除优惠活动
- 活动使用次数展示

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/discount-rules/store/{storeId}` | GET | 获取店铺优惠规则列表 |
| `/api/discount-rules/store/{storeId}/active` | GET | 获取生效中的优惠规则 |
| `/api/discount-rules` | POST | 创建优惠规则 |
| `/api/discount-rules/{id}` | PUT | 更新优惠规则（name/status） |
| `/api/discount-rules/{id}` | DELETE | 删除优惠规则 |

---

### 3.3 管理端功能

管理端采用 PC 端侧栏布局，左侧导航菜单，右侧内容区。

#### 3.3.1 商家管理 (Merchants)

**页面路由**：`/admin/merchants`

**功能**：
- 商家列表展示（头像、昵称、手机号、状态、注册时间、最近登录）
- 搜索筛选（关键词搜索 + 状态筛选：全部/正常/待审核/已封禁）
- 审核操作：通过审核、封禁（需输入原因）、解封

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/admin/merchants` | GET | 获取商家列表（status/keyword 参数） |
| `/api/admin/merchants/{merchantId}/status` | PATCH | 更新商家状态（status/operatorId/reason） |

#### 3.3.2 系统配置 (Configs)

**页面路由**：`/admin/configs`

**功能**：
- 管理员信息展示（用户名、显示名称、角色、最近登录时间）
- 系统参数展示（版本号、API地址、数据库、运行环境）
- 操作审计日志列表（状态变更记录、操作原因）

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/admin/audit-logs` | GET | 获取商家审计日志（merchantId 参数可选） |
| `/api/admin/operation-logs` | GET | 获取管理员操作日志（adminId 参数可选） |
| `/api/admin/{adminId}` | PATCH | 更新管理员信息（displayName/role） |

#### 3.3.3 平台统计 (Stats)

**页面路由**：`/admin/stats`

**功能**：
- 商家概览：商家总数 / 正常运营 / 待审核
- 店铺概览：店铺总数 / 营业中
- 订单数据：订单总数 / 已完成 / 今日订单
- 营收数据：累计营收 / 今日营收
- 商品数据：商品总数 / 分类总数

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/admin/platform-stats` | GET | 获取平台统计数据 |

---

### 3.4 顾客端功能

顾客端采用 H5 移动端布局，顾客通过扫描商家二维码直接进入店铺。

#### 3.4.1 店铺菜单 (StoreMenu)

**页面路由**：`/store/:storeId`

**功能**：
- 店铺信息展示（名称、描述、营业状态）
- 分类侧栏 + 商品列表
- 商品加入购物车
- 购物车浮层（商品数量调整、去结算）

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/CustomerStore/{storeId}/menu` | GET | 获取店铺菜单（categories + products） |

#### 3.4.2 商品详情 (ProductDetail)

**页面路由**：`/store/:storeId/product/:productId`

**功能**：
- 商品图片、名称、描述、价格
- 规格选择（如辣度、份量等）
- 数量选择 + 加入购物车

#### 3.4.3 购物车 (Cart)

**页面路由**：`/store/:storeId/cart`

**功能**：
- 购物车商品列表
- 数量增减
- 优惠规则自动匹配
- 下单提交

#### 3.4.4 我的订单 (MyOrders)

**页面路由**：`/store/:storeId/orders`

**功能**：
- 顾客订单列表
- 订单状态跟踪

#### 3.4.5 订单详情 (OrderDetail)

**页面路由**：`/store/:storeId/order/:orderId`

**功能**：
- 订单详细信息
- 取餐码展示
- 订单状态跟踪

**API**：

| API | 方法 | 说明 |
|-----|------|------|
| `/api/orders` | POST | 创建订单 |
| `/api/orders/pickup/{pickupCode}/store/{storeId}` | GET | 通过取餐码查询订单 |

---

### 3.5 基础数据模块

#### 3.5.1 行业分类

| API | 方法 | 说明 |
|-----|------|------|
| `/api/industrycategories` | GET | 获取所有行业分类 |
| `/api/industrycategories/level/{level}` | GET | 按层级获取分类（1/2/3） |

**初始化数据**（三级分类）：
- 餐饮 → 烧烤 → 重庆特色烧烤 / 东北烧烤
- 餐饮 → 小吃快餐
- 餐饮 → 饮品甜点

#### 3.5.2 店铺模板

| API | 方法 | 说明 |
|-----|------|------|
| `/api/templates` | GET | 获取模板列表 |
| `/api/templates/{id}` | GET | 获取模板详情 |
| `/api/templates/apply` | POST | 应用模板到店铺 |

**初始化模板**：重庆烧烤模板（8个分类 + 20+ 商品 + 规格组）

#### 3.5.3 顾客服务

| API | 方法 | 说明 |
|-----|------|------|
| `/api/customers/register` | POST | 顾客注册 |
| `/api/customers/login` | POST | 顾客登录 |
| `/api/customers/{id}` | GET | 获取顾客信息 |
| `/api/customers/{id}/merge` | POST | 合并游客数据 |
| `/api/customers/{id}/logout` | POST | 退出登录 |

---

## 四、数据库实体

| 实体 | 说明 | 关键字段 |
|------|------|----------|
| Admin | 管理员 | username, passwordHash, role, status |
| Merchant | 商家 | phone, passwordHash, nickname, status |
| Store | 店铺 | name, merchantId, businessStatus, industryCategoryId |
| Category | 分类 | name, storeId, sortOrder, icon |
| Product | 商品 | name, basePrice, categoryId, storeId, status |
| SpecGroup | 规格组 | name, productId |
| SpecOption | 规格选项 | name, extraPrice, specGroupId |
| Order | 订单 | orderNo, pickupCode, storeId, status, actualAmount |
| OrderItem | 订单项 | productName, quantity, totalPrice, orderId, productId |
| DiscountRule | 优惠规则 | name, discountType, thresholdAmount, discountAmount, storeId |
| IndustryCategory | 行业分类 | name, level, parentId |
| StoreTemplate | 店铺模板 | name, industryCategoryId |
| Customer | 顾客 | phone, passwordHash, nickname |
| MerchantAuditLog | 商家审计日志 | merchantId, adminId, action, reason |
| AdminOperationLog | 管理员操作日志 | adminId, action |
| DailyStoreStat | 每日店铺统计 | storeId, statDate, orderCount, revenue |
| HourlyOrderStat | 时段统计 | storeId, statDate, hour, orderCount |
| ProductSalesStat | 商品销售统计 | storeId, productId, statDate, salesQuantity |
| PlatformDailyStat | 平台每日统计 | statDate, totalMerchants, totalOrders |
| CustomerConsumptionStat | 顾客消费统计 | customerId, totalOrders, totalSpent |

---

## 五、全局机制

### 5.1 响应包装

所有 API 响应统一返回格式：
```json
{
  "code": 200,
  "message": "success",
  "data": { ... },
  "detail": null
}
```

- 成功：`code: 200`
- 业务异常：`code: 4xx`（如 401 未授权、403 禁止、404 不存在）
- 系统异常：`code: 500`（detail 包含堆栈信息）

### 5.2 EF 循环引用处理

已配置 `ReferenceHandler.IgnoreCycles`，EF 导航属性序列化时自动忽略循环引用。

### 5.3 种子数据初始化

应用启动时自动执行 `SeedData.InitializeAsync()`：
- 行业分类（6个三级分类）
- 管理员账号（admin/admin123，密码不正确时自动重置）
- 重庆烧烤模板（8分类 + 26商品 + 规格组）
- 商家 18523978013 状态确保为 active
- 店铺"重庆老灶烧烤（观音桥店）"
- 店铺菜单（8分类 + 26商品 + 5规格组 + 11规格选项 + 3优惠规则）
- 订单数据（18个订单，覆盖各种状态）

### 5.4 SignalR 实时通信

| Hub | 路径 | 用途 |
|-----|------|------|
| OrderHub | /hubs/order | 订单状态变更实时推送 |
| StoreHub | /hubs/store | 店铺状态变更实时推送 |

---

## 六、前端路由

| 路由 | 页面 | 需要登录 |
|------|------|----------|
| `/` | Home 首页 | 否 |
| `/login` | 统一登录页 | 否 |
| `/merchant/orders` | 商家-订单管理 | 商家Token |
| `/merchant/menu` | 商家-菜单管理 | 商家Token |
| `/merchant/store` | 商家-店铺信息 | 商家Token |
| `/merchant/discounts` | 商家-优惠活动 | 商家Token |
| `/merchant/dashboard` | 商家-经营数据 | 商家Token |
| `/admin/merchants` | 管理端-商家管理 | 管理员Token |
| `/admin/configs` | 管理端-系统配置 | 管理员Token |
| `/admin/stats` | 管理端-平台统计 | 管理员Token |
| `/store/:storeId` | 顾客-店铺菜单 | 否 |
| `/store/:storeId/cart` | 顾客-购物车 | 否 |
| `/store/:storeId/product/:productId` | 顾客-商品详情 | 否 |
| `/store/:storeId/orders` | 顾客-我的订单 | 否 |
| `/store/:storeId/order/:orderId` | 顾客-订单详情 | 否 |

---

## 七、默认账号

| 角色 | 账号 | 密码 | 说明 |
|------|------|------|------|
| 管理员 | admin | admin123 | 超级管理员 |
| 商家 | 18523978013 | 123456 | 重庆老灶烧烤（观音桥店） |

---

## 八、项目文件结构

```
KongKong.ByteBite/
├── src/
│   ├── ByteBite.Api/                    # API层
│   │   ├── Controllers/                 # 13个控制器
│   │   ├── Data/SeedData.cs             # 种子数据初始化
│   │   ├── Filters/                     # 全局过滤器
│   │   │   ├── ApiResponseWrapperFilter # 响应包装
│   │   │   └── GlobalExceptionFilter    # 全局异常
│   │   ├── Hubs/                        # SignalR Hub
│   │   └── Program.cs                   # 启动配置
│   ├── ByteBite.Application/            # 业务逻辑层
│   │   ├── Services/                    # 12个服务类
│   │   └── Exceptions/BusinessException # 业务异常
│   ├── ByteBite.Infrastructure/        # 基础设施层
│   │   ├── Persistence/                 # DbContext + EF实体
│   │   │   └── Entities/               # 32个数据库实体
│   │   └── Extensions/Entities/         # 实体 partial class 扩展
│   └── ByteBite.Shared/                 # 公共工具
│       └── Helpers/
│           ├── PasswordHasher           # BCrypt密码加密
│           └── PickupCodeGenerator      # 取餐码生成
├── web/                                 # 前端项目
│   ├── src/
│   │   ├── api/                         # API模块
│   │   │   ├── index.ts                 # Axios实例+拦截器
│   │   │   └── modules/                # 10个API模块
│   │   ├── pages/                       # 页面
│   │   │   ├── merchant/               # 商家端5个页面
│   │   │   ├── admin/                  # 管理端3个页面
│   │   │   └── customer/              # 顾客端5个页面
│   │   ├── layouts/                     # 布局组件
│   │   └── router/index.ts             # 路由配置
│   └── package.json
└── scripts/
    └── init-seed-data.sql               # SQL初始化脚本（备用）
```
