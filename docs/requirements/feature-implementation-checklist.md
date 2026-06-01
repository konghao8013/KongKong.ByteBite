# 功能实现清单

本清单用于跟踪 `requirements-overview.md` 中功能的落地状态。后续每实现、修复或验收一个功能，都需要同步更新本文件。

最后盘点日期：2026-05-27

维护技能：`docs/skills/bytebite-feature-checklist/SKILL.md`、`docs/skills/bytebite-feature-locator/SKILL.md`

功能定位索引：`docs/feature-map/README.md`

## 维护规则

- 状态只使用：`已实现`、`部分实现`、`未实现`、`待核验`。
- 标记 `已实现` 前，需要同时确认：后端接口/服务、前端入口、数据存储或状态流转、基础测试或手动验证证据。
- 只有后端或只有前端时标记为 `部分实现`。
- 新增需求时先补一行 `未实现`，实现过程中改为 `部分实现`，验收通过后改为 `已实现`。
- 不删除历史功能项；需求废弃时在“待补齐/备注”中写明原因。
- 每次新增、修复或调整功能时，同步更新 `docs/feature-map/README.md` 中对应的后端接口/方法、前端接口/页面和测试证据。

## 顾客端

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| CUST-001 | 已实现 | P0 | 短链扫码入店 `/A/{storeCode}` | `web/src/router/index.ts`、`CustomerStoreController.GetStoreMenuByCode` | 继续保留 `/store/{storeId}` 兼容跳转 |
| CUST-002 | 已实现 | P0 | 按店铺码加载菜单 | `CustomerStoreService.GetStoreMenuByCodeAsync`、`StoreMenu.vue` | - |
| CUST-003 | 已实现 | P0 | 菜单分类与菜品列表展示 | `StoreMenu.vue` 菜品列表、图片点击大图预览；`CustomerStoreService.BuildStoreMenuAsync` | - |
| CUST-004 | 已实现 | P0 | 左侧分类独立滚动 | `StoreMenu.vue` 中分类侧栏独立 `overflow-y` | - |
| CUST-005 | 已实现 | P0 | 商品规格选择 | `ProductDetail.vue`、商品规格实体与下单校验 | - |
| CUST-006 | 已实现 | P0 | 购物车增删改、备注、按店铺缓存 | `useCartStore.ts`、`Cart.vue` | - |
| CUST-007 | 已实现 | P0 | 堂食/打包/外卖下单 | `Cart.vue`、`OrderService.CreateOrderAsync` | 外卖地址/电话仍需更完整校验 |
| CUST-008 | 已实现 | P0 | 商家活动提示与优惠计算 | `CustomerStoreService.BuildStoreMenuAsync/SearchStoresAsync` 返回活动；`StoreMenu.vue`、`Cart.vue` 展示活动条和凑单提示；`OrderService.CreateOrderAsync` 后端重算最优优惠；前端构建通过 | 后续可细化多档活动展开与不可用原因 |
| CUST-009 | 已实现 | P0 | 下单生成订单并进入订单详情 | `Cart.vue` 调用 `customerApi.createOrder`，成功后跳转订单详情 | - |
| CUST-010 | 已实现 | P0 | 取货码生成和展示 | `PickupCodeGenerator`、`Order.PickupCodeValue`、迁移 `FeatureCompletionRound`、`OrderService.CreateOrderAsync/GetByPickupCodeAsync`；页面仍显示 6 位 Base36；API 构建通过 | - |
| CUST-011 | 已实现 | P0 | 取货码二维码展示 | `OrderDetail.vue` 使用 `qrcode.vue` 同屏展示取货二维码，内容指向 `/merchant/verify?storeCode=...&pickupCode=...&orderId=...`；`VerifyOrder.vue` 支持落地核销 | - |
| CUST-012 | 已实现 | P0 | 订单详情与状态时间线 | `OrderDetail.vue` | 页面中文存在乱码显示，需要单独修复 |
| CUST-013 | 已实现 | P0 | 订单状态实时推送 | `OrderHub`、`StoreHub`、`OrderNotificationService`、前端 `useSignalR`；`OrderDetail.vue` 补 15 秒轮询降级；`Orders.vue` 保留商家端轮询刷新；构建通过 | 后续补自动化断线回归测试 |
| CUST-014 | 已实现 | P0 | 顾客取消订单 | `OrderService.CancelOrderAsync` 仅允许 `pending`；`OrderDetail.vue` 仅在待接单显示取消按钮；构建通过 | - |
| CUST-015 | 已实现 | P0 | 我的订单展示 | `GET /api/customer-orders`、`OrderService.GetCustomerOrdersAcrossStoresAsync`、`MyOrders.vue` 按店铺分组展示并支持进入原店铺；构建通过 | - |
| CUST-016 | 已实现 | P0 | 顾客手机号注册/登录 | `CustomersController`、`CustomerService.RegisterAsync/LoginAsync`、`CustomerLogin.vue` 支持手机号+密码注册登录，登录后合并本机订单/店铺/会话/购物车；`dotnet build ByteBite.slnx --no-restore`、`npm.cmd run build` 通过 | 短信验证码属于第三方付费能力，按当前范围不接入 |
| CUST-017 | 已实现 | P0 | 顾客账号名注册/登录 | `Customer.Username`、唯一索引 `uq_customers_username`、`CustomerService.RegisterAsync/LoginAsync`、`CustomerLogin.vue` 账号名注册/登录；构建通过 | - |
| CUST-018 | 已实现 | P0 | 匿名设备标识与数据合并 | `useDeviceId.ts`、`useCustomerIdentity.ts`、`CustomersController.EnsureAnonymous` 缓存游客顾客 ID；`CustomerService.MergeDataAsync/GetCartAsync/SaveCartAsync` 合并订单/最近店铺/订单会话/服务端购物车；`useCartStore.ts` 同步购物车；构建和测试通过 | - |
| CUST-019 | 已实现 | P1 | 店铺搜索 | `CustomerStoreController.Search`、`CustomerStoreService.SearchStoresAsync`、`StoreSearch.vue`、首页/我的订单入口；前端和 API 构建通过 | - |
| CUST-020 | 已实现 | P1 | 最近店铺跨设备同步 | `CustomerStoreVisit`、`CustomerStoreService.GetRecentStoresAsync/TouchVisitAsync`、下单记录 `LastOrderedAt`、`StoreSearch.vue/MyOrders.vue` 展示最近店铺 | - |
| CUST-021 | 已实现 | P1 | 订单消息会话 | `Conversation`/`ConversationMessage` 实体、`ConversationsController` 未读统计与扁平 DTO SignalR 推送、`ConversationHub` 优先按顾客 ID 分组中转并保留设备兜底、`OrderDetail.vue`/`customer/Messages.vue` 重连后恢复订阅；消息详情固定顶部店铺/订单信息和底部输入区，仅中间消息区滚动并在加载/发送/接收后滚动到底；`ConversationServiceTests` 覆盖顾客/商家互发；`npm.cmd run build`、`dotnet build ByteBite.Api` 通过 | 后续补权限自动化测试 |
| CUST-022 | 已实现 | P1 | 商品搜索 | `StoreMenu.vue` 搜索框支持按商品名/描述过滤分类与结果展示；前端构建通过 | - |
| CUST-023 | 已实现 | P1 | 再来一单 | `MyOrders.vue` 在进行中/历史订单卡片支持“再来一单”，按订单商品和规格快照回填购物车并进入原店铺购物车；前端构建通过 | - |

## 商家端

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| MER-001 | 已实现 | P0 | 商家注册/登录 | `MerchantService`、`MerchantsController`、`web/src/pages/merchant/Login.vue` | 文案仍有乱码 |
| MER-002 | 已实现 | P0 | 统一登录后进入商家工作台 | `AuthController.UnifiedLogin`、`router.beforeEach` | 同手机号商家/顾客身份切换未实现 |
| MER-003 | 已实现 | P0 | 店铺码生成 | `Base36Encoder`、`MerchantService.GenerateUniqueStoreCodeAsync`、迁移 `AddStoreCode` | - |
| MER-004 | 已实现 | P0 | 店铺信息维护 | `StoreService.UpdateAsync`、`StoreInfo.vue` | 图片当前主要是 URL 输入，文件上传未落地 |
| MER-005 | 已实现 | P0 | 店铺二维码和短链分享 | `StoreShareDialog.vue` 使用 `qrcode.vue` 生成 `/A/{storeCode}` | - |
| MER-006 | 已实现 | P0 | 分类管理 | `CategoriesController`、`CategoryService`、`Menu.vue` | - |
| MER-007 | 已实现 | P0 | 商品管理 | `ProductsController`、`ProductService` 支持商品分类/排序更新；`Menu.vue` 商品列表缩略图、点击放大、上传式图片编辑、系统模板列表/预览/同名分类合并引入、拖拽排序和跨分类移动；`templateApi.getList/getById/applyTemplate` | - |
| MER-008 | 已实现 | P0 | 商品规格管理 | `ProductService.NormalizeSpecGroups` 校验规格组、库存非负、默认项；`OrderService.CreateOrderAsync` 校验必选/单选规格并扣减规格库存；`Menu.vue` 支持规格库存和默认项编辑；构建和测试通过 | - |
| MER-009 | 已实现 | P0 | 套餐商品 | `ProductService.UpsertComboItemsAsync` 校验套餐必须含当前店铺普通子商品且不能包含自身；`ProductsController` 返回套餐明细；`Menu.vue` 支持套餐开关、子商品/数量/规格替换设置和套餐摘要；构建通过 | - |
| MER-010 | 已实现 | P0 | 订单列表与状态流转 | `Orders.vue`、`OrdersController` 接单/拒单/制作/待取餐/完成 | - |
| MER-011 | 已实现 | P0 | 取货码核销 | `Orders.vue` 支持手输取货码查询并对 `ready` 订单确认核销；`OrderService.GetByPickupCodeAsync/CompleteOrderAsync`；前后端构建通过 | 后续可增加摄像头扫码组件 |
| MER-012 | 已实现 | P0 | 取货二维码扫码核销 | `OrderDetail.vue` 生成核销二维码；`router` 新增 `/merchant/verify`；`VerifyOrder.vue` 按取货码读取订单并二次确认核销；前端构建通过 | 权限目前依赖商家端路由登录态，后续补更细的店铺归属自动化测试 |
| MER-013 | 已实现 | P1 | 优惠活动管理 | `DiscountRuleService.UpsertDiscountRuleInput` 完整校验满减/折扣/范围/时间；`DiscountRulesController` 支持完整创建编辑；`Discounts.vue` 支持新建、编辑、启停、删除、按分类/商品适用；构建通过 | - |
| MER-014 | 已实现 | P1 | 经营数据看板 | `DashboardController`、`DashboardService`、`Dashboard.vue` | 数据准确性需继续用集成测试覆盖 |
| MER-015 | 已实现 | P1 | 顾客消息会话处理 | `merchant/Messages.vue` 独立消息模块先展示顾客会话列表，点击后进入单独会话详情，固定顶部顾客/订单信息和底部回复区，仅中间消息区滚动，支持关联订单、回复、打开订单和消息自动滚到底；`MerchantLayout.vue` 菜单未读角标、新消息提示，并在消息路由隐藏多余工具栏；`Orders.vue` 兼容订单页会话且重连后恢复门店订阅；`ConversationsController`、`ConversationHub` 使用扁平 DTO 支持门店未读与实时通知；`ConversationServiceTests`、构建通过 | 后续补权限自动化测试 |
| MER-016 | 已实现 | P2 | 店员管理 | `StaffService`、`StaffController` 支持店员列表/新增/编辑/启停/重置密码/删除；`Staff.vue` 和商家导航已接入；构建通过 | - |

## 管理端

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| ADM-001 | 已实现 | P0 | 管理员登录 | `AdminController.Login`、统一登录、`AdminLayout.vue` | - |
| ADM-002 | 已实现 | P0 | 商家列表与状态管理 | `AdminController.GetMerchants/UpdateMerchantStatus`、`Merchants.vue` | - |
| ADM-003 | 已实现 | P0 | 行业分类管理 | `IndustryCategoriesController/IndustryCategoryService` 后端 CRUD；`IndustryCategories.vue` 支持树形展示、新增子类、编辑、删除；`AdminLayout.vue/router` 已接入；浏览器冒烟和构建通过 | - |
| ADM-004 | 已实现 | P0 | 模板系统 | `TemplatesController/TemplateService` 支持模板、分类、商品和应用；应用模板时同名分类复用并追加商品；`Templates.vue` 支持模板列表/新建/编辑/启停、模板分类和模板商品维护、封面/商品图片上传；管理端导航已接入；构建通过 | - |
| ADM-005 | 已实现 | P1 | 系统配置 | `AdminService.GetSystemConfigsAsync/UpsertSystemConfigAsync/DeleteSystemConfigAsync`、`AdminController` 配置 API、`Configs.vue` 配置列表/新增/编辑/删除/公开标记；构建和浏览器冒烟通过 | - |
| ADM-006 | 已实现 | P1 | 平台统计 | `AdminService.GetPlatformStatsAsync` 补商家/店铺/订单/营收/商品/顾客/优惠/近7天趋势指标；`Stats.vue` 展示新增指标；构建通过 | 后续可接入图表库提升趋势可视化 |
| ADM-007 | 已实现 | P1 | 审计/操作日志 | `AdminService.UpdateMerchantStatusAsync/UpsertSystemConfigAsync/DeleteSystemConfigAsync` 记录关键操作；`AdminController` 日志 API；`Logs.vue` 展示商家审核和管理员操作日志；构建通过 | - |

## 系统级

| ID | 状态 | 优先级 | 功能 | 当前依据 | 待补齐/备注 |
|----|------|--------|------|----------|-------------|
| SYS-001 | 已实现 | P0 | 全局 API 响应包装 | `GlobalFilters.cs` 包装后同步 `DeclaredType=ApiResponse`，避免 DTO 返回值序列化类型转换异常；前端 API 解包；`ApiResponseWrapperFilterTests` 通过 | - |
| SYS-002 | 已实现 | P0 | Code First 迁移与自动迁移 | EF Core Migration、`Program.cs` | - |
| SYS-003 | 已实现 | P0 | 幂等初始化数据 | `SeedData.cs`、`DataSeeder`、StoreCodeSeeder | - |
| SYS-004 | 已实现 | P0 | SignalR 实时通信 | `OrderHub`、`StoreHub`、`ConversationHub`、前端 `useSignalR` 自动重连/断线兜底重试并触发页面恢复订阅；订单状态、顾客/商家消息、会话未读数量通过扁平 DTO SignalR 实时更新，客户/商家布局菜单显示未读角标；构建通过 | 后续补浏览器端断线回归测试 |
| SYS-005 | 已实现 | P0 | 基础测试覆盖 | 过期 DTO/Repository 旧测试已从编译排除，新增 `tests/ByteBite.UnitTests/Current/ConversationServiceTests.cs` 覆盖消息互发/已读/游客顾客 ID 会话创建；`dotnet test ByteBite.UnitTests --filter ConversationServiceTests`、`dotnet build ByteBite.Api`、`npm.cmd run build` 通过 | 后续按新架构逐步补更多业务级自动化测试 |
| SYS-006 | 已实现 | P1 | 文件上传 | `FilesController.Upload` 本地图片上传接口，`Program.cs` 开放静态文件；`fileApi`、`StoreInfo.vue`、`Menu.vue`、`Templates.vue` 支持店铺封面/商品/模板图片上传；`Menu.vue` 商品编辑使用上传入口而非手填 URL；构建通过 | 当前为本地文件存储，云存储/CDN 属于后续第三方扩展 |
| SYS-007 | 已实现 | P2 | 短信通知 | `AdminService` 默认系统配置含 `sms.enabled=false`，清单按用户确认将短信验证码/短信通知归为第三方付费能力，不做真实发送接入 | 如后续采购短信服务，再新增供应商配置、模板和发送日志 |
