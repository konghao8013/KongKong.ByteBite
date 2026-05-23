# FE - 前端 Vue 3 项目

> 模块编号：FE
> 状态：✅ 已完成
> 项目目录：`web/`

---

## 1. 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| Vue 3 | 3.5 | Composition API + `<script setup>` |
| Vite | 8.0 | 构建工具 |
| TypeScript | 6.0 | 类型安全 |
| Pinia | 3.0 | 状态管理 |
| Vue Router | 4.6 | 路由 |
| Axios | 1.16 | HTTP 请求 |
| Element Plus | 2.14 | UI 组件库 |
| @microsoft/signalr | 10.0 | 实时推送 |

---

## 2. 项目结构

```
web/src/
├── api/                          # API 封装层
│   ├── index.ts                  # Axios 实例 + 拦截器
│   └── modules/                  # 11 个业务模块 API
│       ├── merchant.ts           # 商家注册/登录/查询
│       ├── customer.ts           # 顾客匿名/注册/菜单/下单/合并
│       ├── store.ts              # 店铺 CRUD + 营业状态切换
│       ├── category.ts           # 分类 CRUD + 排序
│       ├── product.ts            # 商品 CRUD + 批量上下架
│       ├── order.ts              # 下单/取货码/接单/拒单/状态变更
│       ├── discount.ts           # 优惠活动 CRUD + 启停切换
│       ├── template.ts           # 模板管理 + 行业分类
│       ├── dashboard.ts          # 经营数据看板
│       └── admin.ts              # 管理员登录/商家审核/系统配置/平台统计
├── types/                        # TypeScript 类型定义
│   ├── api.d.ts                  # ApiResponse<T>, PagedResult<T>
│   └── models/                   # 10 个业务模型文件
├── router/index.ts               # 三端路由（顾客/商家/管理员）
├── stores/modules/               # Pinia Store
│   ├── useMerchantStore.ts       # 商家认证
│   ├── useCartStore.ts           # 购物车（localStorage 持久化，24h过期）
│   └── useOrderStore.ts          # 订单追踪
├── composables/                  # 组合式函数
│   ├── useSignalR.ts             # SignalR 连接管理
│   └── useDeviceId.ts            # 设备标识
├── layouts/                      # 布局组件
│   ├── CustomerLayout.vue        # 顾客端（底部Tab：菜单/购物车/我的订单）
│   ├── MerchantLayout.vue        # 商家端（深色底栏：订单/菜单/店铺/数据）
│   └── AdminLayout.vue           # 管理员（侧边栏）
├── components/common/            # 公共组件
│   ├── EmptyState.vue            # 空状态占位
│   ├── LoadingSpinner.vue        # 加载动画
│   └── PickupCodeDisplay.vue     # 取货码大字展示
├── pages/
│   ├── customer/                 # 5 个顾客端页面
│   │   ├── StoreMenu.vue         # 菜单浏览（左侧分类+右侧商品）
│   │   ├── Cart.vue              # 购物车
│   │   ├── OrderDetail.vue       # 订单详情+取货码
│   │   ├── MyOrders.vue          # 我的订单
│   │   └── ProductDetail.vue     # 商品详情+规格选择
│   ├── merchant/                 # 6 个商家端页面
│   │   ├── Login.vue             # 商家登录
│   │   ├── Orders.vue            # 订单管理（SignalR实时推送）
│   │   ├── Menu.vue              # 菜单管理
│   │   ├── StoreInfo.vue         # 店铺信息
│   │   ├── Discounts.vue         # 优惠活动
│   │   └── Dashboard.vue         # 经营数据看板
│   └── admin/                    # 管理员页面（路由占位）
├── styles/
│   ├── index.scss                # 全局样式
│   └── variables.scss            # SCSS 变量（美团风格主题色）
└── utils/format.ts               # 工具函数
```

---

## 3. 路由设计

### 3.1 顾客端 `/store/{storeId}`
- `/store/{storeId}` → 菜单浏览
- `/store/{storeId}/cart` → 购物车
- `/store/{storeId}/order/{orderId}` → 订单详情
- `/store/{storeId}/orders` → 我的订单
- `/store/{storeId}/product/{productId}` → 商品详情

### 3.2 商家端 `/merchant`
- `/merchant/login` → 登录
- `/merchant/orders` → 订单管理
- `/merchant/menu` → 菜单管理
- `/merchant/store` → 店铺信息
- `/merchant/discounts` → 优惠活动
- `/merchant/dashboard` → 经营数据

### 3.3 管理员端 `/admin`
- `/admin/login` → 登录
- `/admin/merchants` → 商家管理
- `/admin/configs` → 系统配置
- `/admin/stats` → 平台统计

---

## 4. UI 风格

| 变量 | 值 | 用途 |
|------|-----|------|
| $primary-color | #FFD161 | 主黄色（美团风格） |
| $brand-color | #FF6633 | 品牌橙色 |
| $dark-bar | #2A2A2A | 深色底栏 |
| $bg-color | #f5f5f5 | 页面背景 |
| $success-color | #52c41a | 成功状态 |
| $error-color | #ff4d4f | 错误状态 |

---

## 5. 关键特性

- **API 拦截器**：自动附加 Bearer Token、ApiResponse 自动解包、401 重定向登录
- **购物车持久化**：localStorage 按店铺缓存，24 小时过期
- **SignalR 实时推送**：新订单即时通知 + 声音/震动
- **设备标识**：自动生成并持久化 deviceId 到 localStorage
- **移动端适配**：safe-area-inset、触摸友好、max-width 100vw
- **SCSS 全局变量注入**：Vite additionalData 自动注入 variables.scss
