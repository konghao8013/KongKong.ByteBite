# 前端架构总览

> 版本：v1.1.0 | 更新日期：2026-06-01

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
│   ├── useCustomerIdentity.ts # 游客/登录顾客身份
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
---

## 消息模块前端落点

- 顾客路由：`/A/:code/messages`，页面 `web/src/pages/customer/Messages.vue`，用于查看商家回复、继续回复、打开关联订单；订单详情通过 `orderId/returnTo` 查询参数跳入消息页并自动创建或打开会话。
- 商家路由：`/merchant/messages`，页面 `web/src/pages/merchant/Messages.vue`，用于集中处理顾客消息、回复消息、打开关联订单；订单管理页通过 `orderId/customerId/deviceId/returnTo` 查询参数跳入消息页，并用订单里的顾客身份自动创建或打开会话。
- 消息交互：顾客端和商家端进入消息路由先展示会话列表，消息列表保留各端底部模块菜单；点击具体会话进入单独聊天详情后隐藏底部菜单以固定输入区；携带 `orderId` 时自动进入对应会话；详情页固定顶部名称和订单信息、底部输入区，仅中间消息区域滚动，商家回复输入区吸附在底部，并在加载、发送、实时接收后自动滚动到底；返回按钮优先回到 `returnTo` 来源页。
- 导航角标：`CustomerLayout.vue` 和 `MerchantLayout.vue` 通过 `useConversationStore` 维护未读数，并通过 `ConversationHub` 实时更新。
- 顾客身份：`useCustomerIdentity` 首次访问时调用匿名顾客接口，为游客生成并缓存 `customer_id`，消息中转优先按顾客 ID 分组。
- 实时连接：`useSignalR` 防止同一组件重复启动连接，断线后自动重连并触发页面/布局恢复门店、顾客和会话订阅。
- API 封装：`conversationApi` 增加顾客/商家未读统计接口；商家按订单创建会话时复用通用订单会话接口并携带订单顾客身份；`models/conversation.ts` 增加 SignalR 事件载荷类型。
