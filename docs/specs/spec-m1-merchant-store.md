# M1 - 商家注册/登录/店铺管理

> 模块编号：M1
> 状态：✅ 已完成
> SQL 文件：`docs/sql/01_users_and_stores.sql`

---

## 1. 功能概述

商家通过手机号注册账号，注册时自动创建默认店铺。商家可登录系统、管理店铺信息、切换营业状态。

---

## 2. 数据库表结构

### 2.1 merchants（商家用户表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK, DEFAULT gen_random_uuid() | 主键 |
| phone | VARCHAR(20) | UNIQUE, NOT NULL | 手机号 |
| password_hash | VARCHAR(200) | NOT NULL | BCrypt加密密码 |
| nickname | VARCHAR(50) | | 昵称 |
| avatar_url | VARCHAR(500) | | 头像URL |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'active' | 状态：active/reviewing/disabled/rejected |
| last_login_at | TIMESTAMPTZ | | 最后登录时间 |
| created_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() | 创建时间 |
| updated_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() | 更新时间 |
| deleted_at | TIMESTAMPTZ | | 软删除时间 |

索引：`ix_merchants_phone`, `ix_merchants_status`, `uq_merchants_phone`

### 2.2 stores（店铺表）

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| id | UUID | PK | 主键 |
| merchant_id | UUID | FK→merchants, NOT NULL | 所属商家ID |
| name | VARCHAR(100) | NOT NULL | 店铺名称 |
| description | TEXT | | 店铺描述 |
| cover_image_url | VARCHAR(500) | | 封面图URL |
| address | VARCHAR(300) | | 地址 |
| business_status | VARCHAR(20) | NOT NULL, DEFAULT 'open' | 营业状态：open/closed |
| business_hours | VARCHAR(100) | | 营业时间 |
| dining_mode | VARCHAR(50) | DEFAULT 'dine_in,takeaway' | 就餐方式 |
| delivery_min_amount | DECIMAL(18,2) | DEFAULT 0 | 外卖最低消费 |
| packing_fee | DECIMAL(18,2) | DEFAULT 0 | 打包费 |
| monthly_sales | INTEGER | DEFAULT 0 | 月销量 |
| industry_category_id | UUID | | 行业分类ID |
| created_at / updated_at / deleted_at | TIMESTAMPTZ | | 时间戳 |

---

## 3. API 端点

### 3.1 商家注册
- **POST** `api/merchants/register`
- 请求体：`{ phone, password, storeName, industryCategoryId? }`
- 业务逻辑：
  1. 检查手机号唯一性
  2. BCrypt 加密密码
  3. 创建商家记录（status=active）
  4. 自动创建默认店铺（businessStatus=open, diningMode=dine_in,takeaway）
  5. 返回 MerchantDto

### 3.2 商家登录
- **POST** `api/merchants/login`
- 请求体：`{ phone, password }`
- 业务逻辑：
  1. 按手机号查找商家
  2. BCrypt 验证密码
  3. 更新 LastLoginAt
  4. 返回 MerchantDto

### 3.3 获取商家信息
- **GET** `api/merchants/{id}`

### 3.4 创建店铺
- **POST** `api/stores`
- 请求体：`{ merchantId, name, description, ... }`

### 3.5 更新店铺
- **PUT** `api/stores/{id}`

### 3.6 获取店铺信息
- **GET** `api/stores/{id}`

### 3.7 按商家查店铺
- **GET** `api/stores/merchant/{merchantId}`

### 3.8 切换营业状态
- **PATCH** `api/stores/{id}/toggle-status`
- 业务逻辑：open↔closed 互切

---

## 4. 代码文件索引

| 层 | 文件 | 说明 |
|----|------|------|
| Controller | `ByteBite.Api/Controllers/MerchantsController.cs` | 商家API |
| Controller | `ByteBite.Api/Controllers/StoresController.cs` | 店铺API |
| Service接口 | `ByteBite.Application/Interfaces/IMerchantService.cs` | 商家服务接口 |
| Service接口 | `ByteBite.Application/Interfaces/IStoreService.cs` | 店铺服务接口 |
| Service实现 | `ByteBite.Application/Services/MerchantService.cs` | 商家服务实现 |
| Service实现 | `ByteBite.Application/Services/StoreService.cs` | 店铺服务实现 |
| 仓储接口 | `ByteBite.Application/Interfaces/IMerchantRepository.cs` | 商家仓储接口+轻量实体 |
| 仓储接口 | `ByteBite.Application/Interfaces/IStoreRepository.cs` | 店铺仓储接口+轻量实体 |
| 仓储实现 | `ByteBite.Infrastructure/Repositories/MerchantRepository.cs` | 商家仓储实现 |
| 仓储实现 | `ByteBite.Infrastructure/Repositories/StoreRepository.cs` | 店铺仓储实现 |
| DTO | `ByteBite.Application/DTOs/Merchant/` | 商家相关DTO |
| DTO | `ByteBite.Application/DTOs/Store/` | 店铺相关DTO |
| 验证器 | `ByteBite.Application/Validators/RegisterMerchantRequestValidator.cs` | 注册验证 |
| 验证器 | `ByteBite.Application/Validators/LoginMerchantRequestValidator.cs` | 登录验证 |
| 实体 | `ByteBite.Infrastructure/Persistence/Entities/Merchant.cs` | 商家持久化实体 |
| 实体 | `ByteBite.Infrastructure/Persistence/Entities/Store.cs` | 店铺持久化实体 |

---

## 5. 业务规则

- BR-1：手机号全局唯一，重复注册抛出 `InvalidOperationException("该手机号已注册")`
- BR-2：密码使用 BCrypt 加密存储（`ByteBite.Shared.Helpers.PasswordHasher`）
- BR-3：注册时自动创建默认店铺，名称取自 `request.StoreName`
- BR-4：新商家状态默认 `active`（后期改为 `reviewing` 需管理员审核）
- BR-5：新店铺营业状态默认 `open`
- BR-6：店铺切换营业状态为 open↔closed 互切
- BR-7：实体ID在Application层使用 `Guid.NewGuid()` 预生成，避免外键约束问题

---

## 6. 单元测试覆盖

| 测试 | 说明 |
|------|------|
| RegisterAsync_ValidRequest_ReturnsMerchantDto | 验证注册流程完整 |
| RegisterAsync_DuplicatePhone_ThrowsInvalidOperationException | 重复手机号 |
| LoginAsync_ValidCredentials_ReturnsMerchantDto | 正确密码登录 |
| LoginAsync_InvalidPassword_ReturnsNull | 错误密码 |
| LoginAsync_PhoneNotFound_ReturnsNull | 手机号不存在 |
| GetByIdAsync_ExistingId_ReturnsMerchantDto | 按ID查询 |
| GetByIdAsync_NonExistingId_ReturnsNull | ID不存在 |
