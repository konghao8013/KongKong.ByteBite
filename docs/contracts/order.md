# 订单 API 契约

> 版本：v1.1.0 | 更新日期：2026-05-22

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
| tableNo | string | 否 | 桌号（堂食时） |
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

获取店铺订单列表

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| status | string | 否 | 状态筛选：pending/accepted/preparing/ready/completed/cancelled |
| pageIndex | int | 否 | 页码（默认1） |
| pageSize | int | 否 | 每页数量（默认20） |

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