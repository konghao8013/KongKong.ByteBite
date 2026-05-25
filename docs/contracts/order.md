# 订单 API 契约

> 版本：v1.2.0 | 更新日期：2026-05-25

## 顾客端 - 创建订单

### POST /api/orders

创建新订单

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| storeId | string | 是 | 店铺ID |
| items | array | 是 | 订单项列表 |
| items[].productId | string | 是 | 商品ID |
| items[].quantity | int | 是 | 数量 |
| items[].unitPrice | decimal | 是 | 单价 |
| items[].totalPrice | decimal | 是 | 小计 |
| items[].productName | string | 是 | 商品名称 |
| items[].specOptions | array | 否 | 选中规格 |
| diningMode | string | 是 | 就餐方式：dine_in/takeaway/delivery |
| tableNo | string | 否 | 桌号（堂食时选填） |
| deliveryAddress | string | 否 | 配送地址（外卖时必填） |
| deliveryPhone | string | 否 | 联系电话（外卖时必填） |
| remark | string | 否 | 整单备注 |
| customerId | string | 否 | 顾客ID（已登录时） |

**响应**：Order 实体（含 pickupCode 取货码）

---

## 顾客端 - 查询订单

### GET /api/orders/pickup/{pickupCode}/store/{storeId}

通过取货码查询订单

---

## 商家端 - 订单列表

### GET /api/stores/{storeId}/orders

获取店铺订单列表（含 OrderItems 订单项明细）

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| status | string | 否 | 状态筛选：pending/accepted/preparing/ready/completed/cancelled |
| pageIndex | int | 否 | 页码（默认1） |
| pageSize | int | 否 | 每页数量（默认20） |

**响应字段说明**：返回 Order 实体列表，每个 Order 包含以下关键字段：

| 字段 | 说明 | 商家端展示位置 |
|------|------|----------------|
| pickupCode | 取货码 | 订单卡片 + 详情弹窗标题 |
| diningMode | 就餐方式 | 订单卡片标签 + 详情弹窗就餐方式区 |
| tableNo | 桌号 | 订单卡片标签 + 详情弹窗就餐方式区 |
| deliveryAddress | 配送地址 | 详情弹窗配送信息区（外卖时） |
| deliveryPhone | 联系电话 | 详情弹窗配送信息区（外卖时，支持一键拨打） |
| orderItems | 订单项列表 | 订单卡片摘要（前3项）+ 详情弹窗商品明细 |
| totalAmount | 商品合计 | 详情弹窗金额明细 |
| discountAmount | 优惠金额 | 详情弹窗金额明细 |
| packingFee | 打包费 | 详情弹窗金额明细 |
| actualAmount | 实付金额 | 订单卡片合计 + 详情弹窗金额明细 |
| remark | 备注 | 订单卡片备注 + 详情弹窗备注区 |
| rejectReason | 拒单原因 | 详情弹窗拒单原因区 |
| createdAt | 下单时间 | 详情弹窗订单时间线 |
| acceptedAt/preparingAt/readyAt/completedAt | 状态变更时间 | 详情弹窗订单时间线 |

---

## 商家端 - 订单状态流转

### PATCH /api/orders/{orderId}/accept

接单

### PATCH /api/orders/{orderId}/reject

拒单

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| reason | string | 是 | 拒单原因 |

### PATCH /api/orders/{orderId}/prepare

开始制作

### PATCH /api/orders/{orderId}/ready

制作完成（通知顾客取餐）

### PATCH /api/orders/{orderId}/complete

核销完成（顾客取餐确认）

### PATCH /api/orders/{orderId}/cancel

取消订单

---

## 订单状态流转图

```
pending ──→ accepted ──→ preparing ──→ ready ──→ completed
  │
  ├──→ rejected（拒单，需填原因）
  │
  └──→ cancelled（任意状态可取消）
```

## 订单状态颜色映射

| 状态 | 标签 | 颜色 | 说明 |
|------|------|------|------|
| pending | 待接单 | #FF6B6B 珊瑚红 | 需要商家操作 |
| accepted | 已接单 | #52C41A 绿色 | 已确认 |
| preparing | 制作中 | #FF9800 橙色 | 制作进行中 |
| ready | 待取餐 | #1890FF 蓝色 | 等待顾客取餐 |
| completed | 已完成 | #8C8C8C 灰色 | 订单关闭 |
| cancelled | 已取消 | #8C8C8C 灰色 | 订单取消 |
| rejected | 已拒单 | #FF4D4F 红色 | 商家拒绝 |