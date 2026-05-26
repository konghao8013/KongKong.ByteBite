# 功能实现清单

本清单用于跟踪 `requirements-overview.md` 中功能的落地状态。后续每实现、修复或验收一个功能，都需要同步更新本文件。

最后盘点日期：2026-05-26

维护技能：`docs/skills/bytebite-feature-checklist/SKILL.md`

## 维护规则

- 状态只使用：`已实现`、`部分实现`、`未实现`、`待核验`。
- 标记 `已实现` 前，需要同时确认：后端接口/服务、前端入口、数据存储或状态流转、基础测试或手动验证证据。
- 只有后端或只有前端时标记为 `部分实现`。
- 新增需求时先补一行 `未实现`，实现过程中改为 `部分实现`，验收通过后改为 `已实现`。
- 不删除历史功能项；需求废弃时在“待补齐/备注”中写明原因。

## 顾客端

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| CUST-001 | 已实现 | P0 | 短链扫码入店 `/A/{storeCode}` | `web/src/router/index.ts`、`CustomerStoreController.GetStoreMenuByCode` | 继续保留 `/store/{storeId}` 兼容跳转 |
| CUST-002 | 已实现 | P0 | 按店铺码加载菜单 | `CustomerStoreService.GetStoreMenuByCodeAsync`、`StoreMenu.vue` | - |
| CUST-003 | 已实现 | P0 | 菜单分类与菜品列表展示 | `StoreMenu.vue`、`CustomerStoreService.BuildStoreMenuAsync` | - |
| CUST-004 | 已实现 | P0 | 左侧分类独立滚动 | `StoreMenu.vue` 中分类侧栏独立 `overflow-y` | - |
| CUST-005 | 已实现 | P0 | 商品规格选择 | `ProductDetail.vue`、商品规格实体与下单校验 | - |
| CUST-006 | 已实现 | P0 | 购物车增删改、备注、按店铺缓存 | `useCartStore.ts`、`Cart.vue` | - |
| CUST-007 | 已实现 | P0 | 堂食/打包/外卖下单 | `Cart.vue`、`OrderService.CreateOrderAsync` | 外卖地址/电话仍需更完整校验 |
| CUST-008 | 部分实现 | P0 | 商家活动提示与优惠计算 | 后端下单时计算最优优惠；订单详情展示优惠金额 | 顾客菜单页/购物车页缺少活动条和凑单提示 |
| CUST-009 | 已实现 | P0 | 下单生成订单并进入订单详情 | `Cart.vue` 调用 `customerApi.createOrder`，成功后跳转订单详情 | - |
| CUST-010 | 部分实现 | P0 | 取货码生成和展示 | 当前订单表存 `PickupCode` 字符串，页面展示取货码 | 新需求要求数据库改为 `int pickupCodeValue`，页面显示 6 位 Base36 |
| CUST-011 | 未实现 | P0 | 取货码二维码展示 | 依赖 `qrcode.vue` 已安装，但订单详情未展示取货二维码 | 需在订单详情同屏展示二维码，内容为商家核销链接 |
| CUST-012 | 已实现 | P0 | 订单详情与状态时间线 | `OrderDetail.vue` | 页面中文存在乱码显示，需要单独修复 |
| CUST-013 | 部分实现 | P0 | 订单状态实时推送 | `OrderHub`、`StoreHub`、`OrderNotificationService`、前端 `useSignalR` | 需要补断线轮询降级与更完整测试 |
| CUST-014 | 部分实现 | P0 | 顾客取消订单 | 前端和后端均有取消接口 | 当前允许 `accepted` 后取消，需求要求仅 `pending` 可取消 |
| CUST-015 | 部分实现 | P0 | 我的订单展示 | `MyOrders.vue`、`GET /api/stores/{storeId}/customer-orders` | 当前按当前店铺查询；新需求要求登录账号跨店铺分组展示 |
| CUST-016 | 部分实现 | P0 | 顾客手机号注册/登录 | `CustomersController`、`CustomerService.RegisterAsync/LoginAsync`、统一登录可识别顾客 | 顾客端缺少直接注册页面/入口；验证码模式未实现 |
| CUST-017 | 未实现 | P0 | 顾客账号名注册/登录 | 当前顾客实体和登录逻辑主要按手机号 | 需新增账号名字段、唯一性和前端注册入口 |
| CUST-018 | 部分实现 | P0 | 匿名设备标识与数据合并 | `useDeviceId.ts`、`CustomerService.EnsureAnonymousAsync/MergeDataAsync` | 目前主要合并订单；购物车、最近店铺、会话合并未完整落地 |
| CUST-019 | 未实现 | P1 | 店铺搜索 | 无顾客店铺搜索路由/API | 需支持店铺名、店铺码、行业关键词搜索 |
| CUST-020 | 未实现 | P1 | 最近店铺跨设备同步 | 当前主要依赖 localStorage 当前店铺 | 需账号级最近访问/下单店铺记录 |
| CUST-021 | 未实现 | P1 | 订单消息会话 | 无 Conversation/Message 实体、API、页面或 Hub | 顾客从已下单订单发起，商家可回复并打开订单 |
| CUST-022 | 部分实现 | P1 | 商品搜索 | 菜单页有静态搜索框样式 | 缺少输入、筛选和结果展示逻辑 |
| CUST-023 | 未实现 | P1 | 再来一单 | 我的订单页未提供再来一单操作 | - |

## 商家端

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| MER-001 | 已实现 | P0 | 商家注册/登录 | `MerchantService`、`MerchantsController`、`web/src/pages/merchant/Login.vue` | 文案仍有乱码 |
| MER-002 | 已实现 | P0 | 统一登录后进入商家工作台 | `AuthController.UnifiedLogin`、`router.beforeEach` | 同手机号商家/顾客身份切换未实现 |
| MER-003 | 已实现 | P0 | 店铺码生成 | `Base36Encoder`、`MerchantService.GenerateUniqueStoreCodeAsync`、迁移 `AddStoreCode` | - |
| MER-004 | 已实现 | P0 | 店铺信息维护 | `StoreService.UpdateAsync`、`StoreInfo.vue` | 图片当前主要是 URL 输入，文件上传未落地 |
| MER-005 | 已实现 | P0 | 店铺二维码和短链分享 | `StoreShareDialog.vue` 使用 `qrcode.vue` 生成 `/A/{storeCode}` | - |
| MER-006 | 已实现 | P0 | 分类管理 | `CategoriesController`、`CategoryService`、`Menu.vue` | - |
| MER-007 | 已实现 | P0 | 商品管理 | `ProductsController`、`ProductService`、`Menu.vue` | - |
| MER-008 | 部分实现 | P0 | 商品规格管理 | 实体、服务和商家菜单页支持规格组/规格项 | 需要补更完整的规格库存、默认项校验 |
| MER-009 | 部分实现 | P0 | 套餐商品 | 实体中有 `ComboItem`/`IsCombo`，菜单能显示套餐标签 | 缺少完整套餐创建/编辑 UI 和规则校验 |
| MER-010 | 已实现 | P0 | 订单列表与状态流转 | `Orders.vue`、`OrdersController` 接单/拒单/制作/待取餐/完成 | - |
| MER-011 | 部分实现 | P0 | 取货码核销 | 商家订单详情和列表可对 ready 订单点击核销 | 缺少输入取货码查询核销、扫码核销、权限确认页 |
| MER-012 | 未实现 | P0 | 取货二维码扫码核销 | 无 `/merchant/verify` 路由/API | 需商家登录、店铺归属、状态校验、二次确认 |
| MER-013 | 部分实现 | P1 | 优惠活动管理 | `DiscountRuleService`、`Discounts.vue` 可列表/启停/删除 | 缺少创建/编辑完整表单和顾客端活动提示 |
| MER-014 | 已实现 | P1 | 经营数据看板 | `DashboardController`、`DashboardService`、`Dashboard.vue` | 数据准确性需继续用集成测试覆盖 |
| MER-015 | 未实现 | P1 | 顾客消息会话处理 | 无商家会话列表/详情/回复功能 | 需与 CUST-021 联动 |
| MER-016 | 未实现 | P2 | 店员管理 | 仅有 `Staff` 实体，未见页面/API | - |

## 管理端

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| ADM-001 | 已实现 | P0 | 管理员登录 | `AdminController.Login`、统一登录、`AdminLayout.vue` | - |
| ADM-002 | 已实现 | P0 | 商家列表与状态管理 | `AdminController.GetMerchants/UpdateMerchantStatus`、`Merchants.vue` | - |
| ADM-003 | 部分实现 | P0 | 行业分类管理 | 后端有 `IndustryCategoriesController` 和初始化数据 | 管理端页面入口缺失 |
| ADM-004 | 部分实现 | P0 | 模板系统 | 后端 `TemplatesController/TemplateService` 存在 | 管理端模板 UI 缺失或未接入主导航 |
| ADM-005 | 部分实现 | P1 | 系统配置 | 有 `SystemConfig` 实体和 `Configs.vue` | 需核验真实 API 和保存能力 |
| ADM-006 | 部分实现 | P1 | 平台统计 | `AdminController.GetPlatformStats`、`Stats.vue` | 需补更多统计指标和数据验收 |
| ADM-007 | 部分实现 | P1 | 审计/操作日志 | `AdminController` 有日志接口，实体存在 | 页面展示与关键操作记录需核验 |

## 系统级

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| SYS-001 | 已实现 | P0 | 全局 API 响应包装 | `GlobalFilters.cs`、前端 API 解包 | - |
| SYS-002 | 已实现 | P0 | Code First 迁移与自动迁移 | EF Core Migration、`Program.cs` | - |
| SYS-003 | 已实现 | P0 | 幂等初始化数据 | `SeedData.cs`、`DataSeeder`、StoreCodeSeeder | - |
| SYS-004 | 部分实现 | P0 | SignalR 实时通信 | `OrderHub`、`StoreHub`、前端 `useSignalR` | 缺少 ConversationHub 和轮询降级 |
| SYS-005 | 部分实现 | P0 | 基础测试覆盖 | 单元测试和集成测试项目存在 | 需要按新增功能补测试，并修复/复核当前集成测试结果 |
| SYS-006 | 未实现 | P1 | 文件上传 | 文档有要求，当前店铺/商品多为 URL 输入 | 需后端上传接口和前端文件选择 |
| SYS-007 | 未实现 | P2 | 短信通知 | 文档列为 P2，未见实现 | - |
