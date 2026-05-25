# 订单-取货码领域 Spec

> 版本：v1.2.0 | 更新日期：2026-05-25

## 领域概述

顾客扫码进入店铺，浏览菜单，加入购物车，提交订单，获取取货码。商家通过 SignalR 实时接收新订单通知，查看订单详情（含配送信息），按流程处理订单。

---

## 2. 数据库表结构

### 2.1 customers（顾客表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| phone | VARCHAR(20) | | 手机号（注册后才有） |
| nickname | VARCHAR(50) | | 昵称 |
| avatar_url | VARCHAR(500) | | 头像 |
| device_id | VARCHAR(200) | | 设备标识（匿名用户） |
| is_registered | BOOLEAN | DEFAULT false | 是否已注册 |
| password_hash | VARCHAR(200) | | BCrypt密码（注册用户） |
| last_login_at | TIMESTAMPTZ | | 最后登录时间 |
| created_at / updated_at / deleted_at | TIMESTAMPTZ | | 时间戳 |

### 2.2 orders（订单表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| store_id | UUID | FK→stores | 店铺ID |
| customer_id | UUID | FK→customers | 顾客ID |
| device_id | VARCHAR(200) | | 设备标识（匿名用户） |
| order_no | VARCHAR(20) | | 订单编号（如：20260520001） |
| pickup_code | VARCHAR(6) | | 取货码（店铺内唯一） |
| dining_mode | VARCHAR(20) | DEFAULT 'dine_in' | 就餐方式：dine_in/takeaway/delivery |
| table_no | VARCHAR(20) | | 桌号（堂食时填写） |
| delivery_address | VARCHAR(500) | | 配送地址（外卖时必填） |
| delivery_phone | VARCHAR(20) | | 联系电话（外卖时必填） |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'pending' | 状态 |
| total_amount | DECIMAL(18,2) | NOT NULL | 总金额 |
| discount_amount | DECIMAL(18,2) | DEFAULT 0 | 优惠金额 |
| actual_amount | DECIMAL(18,2) | NOT NULL | 实付金额 |
| packing_fee | DECIMAL(18,2) | DEFAULT 0 | 打包费 |
| discount_rule_id | UUID | | 使用的优惠规则ID |
| remark | VARCHAR(500) | | 备注 |
| reject_reason | VARCHAR(500) | | 拒单原因 |
| cancel_reason | VARCHAR(500) | | 取消原因 |
| accepted_at / preparing_at / ready_at / completed_at / cancelled_at | TIMESTAMPTZ | | 状态变更时间 |
| created_at / updated_at | TIMESTAMPTZ | | 时间戳 |

**订单状态流转**：`pending → accepted → preparing → ready → completed`
**取消路径**：`pending → cancelled`（顾客取消/商家拒单）

### 2.3 order_items（订单项表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| order_id | UUID | FK→orders | 订单ID |
| product_id | UUID | | 商品ID |
| product_name | VARCHAR(100) | | 商品名称快照 |
| quantity | INTEGER | NOT NULL | 数量 |
| unit_price | DECIMAL(18,2) | NOT NULL | 单价 |
| spec_snapshot | JSONB | | 规格快照 |
| combo_items_snapshot | JSONB | | 套餐项快照 |
| remark | VARCHAR(200) | | 备注 |

---

## 3. API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `api/customer/stores/{storeId}/menu` | 顾客端菜单 |
| POST | `api/orders` | 创建订单 |
| GET | `api/orders/pickup/{pickupCode}/store/{storeId}` | 取货码查订单 |
| GET | `api/stores/{storeId}/orders` | 店铺订单列表 |
| PATCH | `api/orders/{orderId}/accept` | 接单 |
| PATCH | `api/orders/{orderId}/reject` | 拒单（需RejectReason） |
| PATCH | `api/orders/{orderId}/prepare` | 开始制作 |
| PATCH | `api/orders/{orderId}/ready` | 备餐完毕 |
| PATCH | `api/orders/{orderId}/complete` | 完成订单 |
| PATCH | `api/orders/{orderId}/cancel` | 取消订单（需CancelReason） |

---

## 4. SignalR 推送

### 4.1 OrderHub（顾客端 `/hubs/order`）
- `SubscribeOrder(orderId)` → 接收订单状态变更通知

### 4.2 StoreHub（商家端 `/hubs/store`）
- `SubscribeStore(storeId)` → 接收新订单通知

### 4.3 推送场景
1. 顾客下单 → 商家端收到新订单通知
2. 商家接单 → 顾客端收到状态更新
3. 商家备餐完毕 → 顾客端收到取餐通知
4. 商家拒单 → 顾客端收到拒单通知

---

## 5. 取货码机制

- 长度：4-6位（默认4位，可通过系统配置调整）
- 字符集：排除易混淆字符 `0/O/1/I/L`
- 生成器：`ByteBite.Shared.Helpers.PickupCodeGenerator`
- 唯一性：同一店铺内唯一，生成时检查 `PickupCodeExistsAsync`
- 展示：前端使用 `PickupCodeDisplay` 组件大字展示

---

## 6. 代码文件索引

| 层 | 文件 |
|----|------|
| Controller | `CustomerStoreController.cs`, `OrdersController.cs` |
| Hub | `OrderHub.cs`, `StoreHub.cs` |
| Service | `CustomerStoreService.cs`, `OrderService.cs`, `OrderNotificationService.cs` |
| 仓储 | `OrderRepository.cs` |
| DTO | `DTOs/Customer/`, `DTOs/Order/` |
| 共享 | `PasswordHasher.cs`, `PickupCodeGenerator.cs` |

---

## 7. 业务规则

- BR-1：下单时验证店铺营业状态（closed 不可下单）
- BR-2：下单时验证商品状态（off/sold_out 不可下单）
- BR-3：下单时验证数量 >= min_order_qty
- BR-4：打包费仅在 takeaway/delivery 模式下收取
- BR-5：先做后付模式，订单无支付环节
- BR-6：订单状态严格按流转路径变更，非法变更抛异常
- BR-7：取货码在店铺内唯一，生成时循环检查直到找到可用码
- BR-8：堂食模式下可填桌号（选填），外卖模式下配送地址和联系电话为必填
- BR-9：商家端订单详情弹窗展示完整信息：就餐方式、配送信息（外卖）、商品明细、金额明细、备注、订单时间线
- BR-10：外卖配送信息中联系电话支持 tel: 协议一键拨打

---

## 8. 单元测试覆盖（17个）

- CreateOrderAsync 各种验证（店铺/商品/数量/优惠/打包费）
- 6种状态转换测试（accept/reject/prepare/ready/complete/cancel）
- 非法状态转换抛异常
