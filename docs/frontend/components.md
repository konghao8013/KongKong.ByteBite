# 前端组件文档

> 版本：v1.1.0 | 更新日期：2026-05-22

## 布局组件

### MerchantLayout

商家端 H5 移动布局：白底顶栏 + 内容区 + 白底 TabBar

| 区域 | 内容 |
|------|------|
| 顶栏 | "空空码上点单" 标题（珊瑚红） + 退出按钮（药丸形） |
| 内容区 | `<router-view />` |
| TabBar | 4个Tab：订单📋/菜单🍽️/店铺🏪/数据📊，珊瑚红激活色 |

### AdminLayout

管理端 PC 侧栏布局：白底侧栏 + 内容区

| 区域 | 内容 |
|------|------|
| 侧栏头 | "管理后台" 标题（珊瑚红） |
| 侧栏菜单 | 商家管理👥/系统配置⚙️/平台统计📈 |
| 侧栏底 | 退出登录🚪（红色） |
| 内容区 | `<router-view />` |

### CustomerLayout

顾客端 H5 移动布局：顶部导航 + 内容区

---

## 公共组件

### EmptyState

空状态占位组件

| Prop | 类型 | 说明 |
|------|------|------|
| icon | string | Emoji 图标 |
| message | string | 提示文字 |

### LoadingSpinner

加载中旋转动画

| Prop | 类型 | 说明 |
|------|------|------|
| size | number | 尺寸（px） |
| color | string | 颜色（默认 #FF6B6B） |

### PickupCodeDisplay

取货码展示组件（渐变背景 + 大号取货码）

| Prop | 类型 | 说明 |
|------|------|------|
| code | string | 取货码 |
| status | string | 订单状态 |

---

## 组合式函数

### useDeviceId

生成并缓存浏览器设备唯一标识（localStorage）

### useSignalR

SignalR 连接管理，自动重连、断线检测

---

## 页面组件

### 商家端

| 页面 | 功能 | 数据源 |
|------|------|--------|
| Orders | 订单列表、状态筛选、接单/拒单/制作/核销 | orderApi |
| Menu | 分类侧栏、商品列表、上下架切换 | categoryApi + productApi |
| StoreInfo | 店铺信息展示/编辑、营业状态切换 | storeApi |
| Dashboard | 经营概览、图表占位、近期订单 | dashboardApi + orderApi |
| Discounts | 优惠活动列表、启用/停用/删除 | discountApi |

### 管理端

| 页面 | 功能 | 数据源 |
|------|------|--------|
| Merchants | 商家列表、搜索筛选、审核/封禁/解封 | adminApi |
| Configs | 管理员信息、系统参数、审计日志 | adminApi |
| Stats | 平台统计数据卡片 | adminApi.getPlatformStats |

### 顾客端

| 页面 | 功能 | 数据源 |
|------|------|--------|
| StoreMenu | 分类导航、商品列表、加入购物车 | customerStoreApi |
| ProductDetail | 商品详情、规格选择、数量调整 | productApi |
| Cart | 购物车管理、优惠匹配、下单 | orderApi |
| MyOrders | 顾客订单列表 | orderApi |
| OrderDetail | 订单详情、取货码展示 | orderApi |