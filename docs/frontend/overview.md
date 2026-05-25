# 前端架构总览

> 版本：v1.1.0 | 更新日期：2026-05-22

## 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| Vue | 3.5 | 前端框架 |
| Vite | 8 | 构建工具 |
| TypeScript | 6 | 类型系统 |
| Element Plus | 2.14 | UI组件库 |
| Pinia | 3.0 | 状态管理 |
| Axios | 1.16 | HTTP客户端 |
| SignalR | — | 实时通信 |
| SCSS | — | 样式预处理 |

## 目录结构

```
web/src/
├── api/                    # API 封装层
│   ├── index.ts            # Axios 实例 + 拦截器
│   └── modules/            # 10个 API 模块
│       ├── admin.ts        # 管理端 API
│       ├── merchant.ts     # 商家 API
│       ├── customer.ts     # 顾客 API
│       ├── store.ts        # 店铺 API
│       ├── category.ts     # 分类 API
│       ├── product.ts      # 商品 API
│       ├── order.ts        # 订单 API
│       ├── dashboard.ts    # 经营数据 API
│       ├── discount.ts     # 优惠活动 API
│       └── template.ts     # 模板 API
├── components/             # 公共组件
│   └── common/
│       ├── EmptyState.vue  # 空状态
│       ├── LoadingSpinner.vue # 加载中
│       └── PickupCodeDisplay.vue # 取货码展示
├── composables/            # 组合式函数
│   ├── useDeviceId.ts      # 设备ID
│   └── useSignalR.ts       # SignalR 连接
├── layouts/                # 布局组件
│   ├── MerchantLayout.vue  # 商家端（顶栏+TabBar）
│   ├── AdminLayout.vue     # 管理端（侧栏布局）
│   └── CustomerLayout.vue  # 顾客端（顶部导航）
├── pages/                  # 页面组件
│   ├── Home.vue            # 首页（入口选择）
│   ├── merchant/           # 商家端页面
│   │   ├── Login.vue       # 统一登录页
│   │   ├── Orders.vue      # 订单管理
│   │   ├── Menu.vue        # 菜单管理
│   │   ├── StoreInfo.vue   # 店铺信息
│   │   ├── Dashboard.vue   # 经营数据
│   │   └── Discounts.vue   # 优惠活动
│   ├── admin/              # 管理端页面
│   │   ├── Login.vue       # 管理端登录（兼容）
│   │   ├── Merchants.vue   # 商家管理
│   │   ├── Configs.vue     # 系统配置
│   │   └── Stats.vue       # 平台统计
│   └── customer/           # 顾客端页面
│       ├── StoreMenu.vue   # 店铺菜单
│       ├── ProductDetail.vue # 商品详情
│       ├── Cart.vue        # 购物车
│       ├── MyOrders.vue    # 我的订单
│       └── OrderDetail.vue # 订单详情
├── styles/                 # 全局样式
│   ├── variables.scss      # SCSS 变量（色彩体系）
│   └── index.scss          # 全局样式 + CSS 变量
├── router/index.ts         # 路由配置
├── App.vue                 # 根组件
└── main.ts                 # 入口
```

## 路由体系

| 路由 | 页面 | 布局 | 需要登录 |
|------|------|------|----------|
| / | Home | — | 否 |
| /login | 统一登录 | — | 否 |
| /merchant/* | 商家端页面 | MerchantLayout | merchant_token |
| /admin/* | 管理端页面 | AdminLayout | admin_token |
| /store/:storeId/* | 顾客端页面（旧路由，重定向至 /A/:code） | CustomerLayout | 否 |
| /A/:code/* | 顾客端页面（短链，按店铺码） | CustomerLayout | 否 |

**路由守卫**：检查 localStorage 中的 token 判断登录状态，未登录跳转 /login

## API 封装机制

```
前端调用                          后端响应
─────────                        ─────────
request.get('/stores/123')  ←→   { code:200, message:"success", data:{...} }
                                ↓
                          Axios 拦截器自动解包
                                ↓
                          返回 data 部分（即 Store 实体）
```

- 成功时：request.get/post 返回的就是 data 部分
- 失败时：Axios 拦截器抛出错误，包含 { code, message }

## 色彩体系

参见 [架构总览](../architecture/overview.md#44-色彩体系) 和 [SCSS 变量](../../web/src/styles/variables.scss)

## 默认账号

| 角色 | 账号 | 密码 |
|------|------|------|
| 管理员 | admin | admin123 |
| 商家 | 18523978013 | 123456 |