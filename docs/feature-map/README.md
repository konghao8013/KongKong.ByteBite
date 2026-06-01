# 功能点定位索引

> 更新日期：2026-05-28
> 维护技能：`.codex/skills/bytebite-feature-locator/SKILL.md`、`docs/skills/bytebite-feature-locator/SKILL.md`

本索引用于把需求功能、后端接口/方法、前端接口/页面、关联文档和测试证据串起来。后续每次新增、修复或调整功能，都需要同步维护对应行。

## 维护规则

- 先从 `docs/requirements/feature-implementation-checklist.md` 找到功能 ID，再回到本文件定位代码。
- 后端变更要补控制器路由、Service 方法、实体/迁移/Hub/Seeder 和测试证据。
- 前端变更要补 API 模块方法、路由、页面、Store、Composable、类型和组件。
- API 契约变化同步 `docs/contracts/*.md`。
- 业务规则变化同步 `docs/specs/spec-*.md`。
- 数据结构或初始化变化同步 `docs/data/` 或 `docs/sql/`。
- 完成度状态变化同步 `docs/requirements/feature-implementation-checklist.md`。

## 文档关联

| 文档 | 用途 |
|------|------|
| `docs/requirements/requirements-overview.md` | 产品需求来源 |
| `docs/requirements/feature-implementation-checklist.md` | 功能 ID、状态和验收证据 |
| `docs/specs/README.md` | 领域规格索引 |
| `docs/contracts/*.md` | HTTP API 契约 |
| `docs/frontend/overview.md` | 前端架构、路由和 API 封装说明 |
| `docs/frontend/components.md` | 公共组件说明 |
| `docs/data/overview.md` | 数据实体关系 |
| `docs/architecture/overview.md` | 系统分层和跨端架构 |
| `.codex/skills/bytebite-feature-locator/SKILL.md` | 功能定位和文档同步流程 |
| `docs/skills/bytebite-feature-checklist/SKILL.md` | 功能完成度清单维护流程 |

## 后端功能点索引

| 领域 | 功能 ID | 控制器/接口 | Service 方法 | 数据/实时/测试 | 关联文档 |
|------|---------|-------------|---------------|----------------|----------|
| 统一认证 | MER-001, MER-002, ADM-001, CUST-016, CUST-017 | `AuthController.UnifiedLogin` `POST /api/auth/login`; `MerchantsController.Login/Register`; `CustomersController.Login/Register`; `AdminController.Login` | `MerchantService.LoginAsync/RegisterAsync`; `CustomerService.LoginAsync/RegisterAsync`; `AdminService.LoginAsync` | `Merchant`, `Customer`, `Admin`; `MerchantsIntegrationTests`, `CustomersIntegrationTests`, `AdminIntegrationTests` | `docs/contracts/auth.md`; `docs/specs/spec-auth.md` |
| 顾客入店和菜单 | CUST-001, CUST-002, CUST-003, CUST-004, CUST-008, CUST-019, CUST-020, CUST-022 | `CustomerStoreController.GetStoreMenu`; `GetStoreMenuByCode`; `Search`; `Recent` | `CustomerStoreService.GetStoreMenuAsync`; `GetStoreMenuByCodeAsync`; `SearchStoresAsync`; `GetRecentStoresAsync`; `BuildStoreMenuAsync`; `TouchVisitAsync` | `Store`, `Category`, `Product`, `DiscountRule`, `CustomerStoreVisit`; `StoresIntegrationTests`, `CategoriesAndProductsIntegrationTests` | `docs/contracts/customer.md`; `docs/specs/spec-merchant-store.md`; `docs/specs/spec-category-product.md`; `docs/specs/spec-customer.md` |
| 顾客账号、设备和购物车 | CUST-006, CUST-016, CUST-017, CUST-018 | `CustomersController.EnsureAnonymous`; `GetMergePreview`; `MergeData`; `GetCart`; `SaveCart`; `MergeCart` | `CustomerService.EnsureAnonymousAsync`; `GetMergePreviewAsync`; `MergeDataAsync`; `GetCartAsync`; `SaveCartAsync`; `MergeLocalCartAsync` | `CustomerSession.CartData`, `CustomerConsumptionStat`; `useCustomerIdentity` 缓存游客顾客 ID；`CustomerServiceTests`, `CustomersIntegrationTests` | `docs/contracts/customer.md`; `docs/specs/spec-customer.md` |
| 下单、取货码和订单状态 | CUST-007, CUST-009, CUST-010, CUST-011, CUST-012, CUST-013, CUST-014, CUST-015, CUST-023, MER-010, MER-011, MER-012 | `OrdersController.CreateOrder`; `GetById`; `GetByPickupCode`; `GetByStoreId`; `GetCustomerOrders`; `GetCustomerOrdersAcrossStores`; `AcceptOrder`; `RejectOrder`; `StartPreparing`; `MarkReady`; `CompleteOrder`; `CancelOrder` | `OrderService.CreateOrderAsync`; `GetByIdAsync`; `GetByPickupCodeAsync`; `GetByStoreIdAsync`; `GetCustomerOrdersAsync`; `GetCustomerOrdersAcrossStoresAsync`; status methods | `Order`, `OrderItem`, `PickupCodeGenerator`, `OrderHub`, `StoreHub`, `OrderNotificationService`; `OrderServiceTests`, `OrdersIntegrationTests` | `docs/contracts/order.md`; `docs/specs/spec-order-pickup.md` |
| 商家店铺 | MER-003, MER-004, MER-005 | `StoresController.GetById`; `GetByMerchantId`; `Update`; `MerchantsController.GetStores` | `StoreService.GetByIdAsync`; `GetByStoreCodeAsync`; `GetByMerchantIdAsync`; `UpdateAsync`; `GenerateStoreCodeAsync`; `MerchantService.GetStoresAsync` | `Store`, `StoreCodeSeeder`, `Base36Encoder`; `StoreServiceTests`, `StoresIntegrationTests` | `docs/contracts/merchant.md`; `docs/specs/spec-merchant-store.md` |
| 分类、商品、规格和套餐 | MER-006, MER-007, MER-008, MER-009 | `CategoriesController` CRUD; `ProductsController` CRUD/list/update | `CategoryService.CreateAsync/UpdateAsync/DeleteAsync/GetByStoreIdAsync`; `ProductService.CreateAsync/UpdateAsync/DeleteAsync/GetByIdAsync/GetByCategoryIdAsync/GetByStoreIdAsync`; `UpsertComboItemsAsync` | `Category`, `Product.SortOrder/CategoryId`, `SpecGroup`, `SpecOption`, `ComboItem`; `CategoryServiceTests`, `ProductServiceTests`, `CategoriesAndProductsIntegrationTests` | `docs/contracts/merchant.md`; `docs/specs/spec-category-product.md` |
| 优惠活动 | CUST-008, MER-013 | `DiscountRulesController` CRUD/list/active | `DiscountRuleService.CreateAsync`; `UpdateAsync`; `DeleteAsync`; `GetByStoreIdAsync`; `GetActiveByStoreIdAsync`; `ValidateAsync`; `OrderService.CalculateBestDiscountAsync` | `DiscountRule`, `DiscountEffectStat`; `DiscountRuleServiceTests`, `DiscountRulesIntegrationTests` | `docs/contracts/merchant.md`; `docs/specs/spec-discount.md` |
| 经营数据 | MER-014, ADM-006 | `DashboardController.GetOverview`; `GetCategorySales`; `GetHourlyDistribution`; `AdminController.GetPlatformStats/GetStats` | `DashboardService.GetOverviewAsync`; `GetCategorySalesAsync`; `GetHourlyDistributionAsync`; `AdminService.GetPlatformStatsAsync` | `DailyStoreStat`, `HourlyOrderStat`, `ProductSalesStat`, `PlatformDailyStat`; `DashboardServiceTests`, `DashboardIntegrationTests` | `docs/contracts/merchant.md`; `docs/specs/spec-dashboard.md` |
| 订单会话 | CUST-021, MER-015 | `ConversationsController.GetOrCreateByOrder`; `GetByStore`; `GetByCustomer`; `GetStoreUnreadCount`; `GetCustomerUnreadCount`; `GetMessages`; `SendMessage`; `MarkRead` | `ConversationService.GetOrCreateByOrderAsync`; `GetByStoreAsync`; `GetByCustomerAsync`; `GetUnreadCountForStoreAsync`; `GetUnreadCountForCustomerAsync`; `GetMessagesAsync`; `SendMessageAsync`; `MarkReadAsync` | `Conversation`, `ConversationMessage`, `ConversationHub` 支持 conversation/store/customer/device 订阅组且顾客 ID 优先；SignalR 推送使用扁平 DTO；`StoreHub` 兼容商家订单页消息提示；`ConversationServiceTests` | `docs/specs/spec-customer.md`; `docs/specs/spec-order-pickup.md`; `docs/contracts/order.md` |
| 店员管理 | MER-016 | `StaffController.GetByStoreId`; `Create`; `Update`; `ResetPassword`; `Delete` | `StaffService.GetByStoreIdAsync`; `CreateAsync`; `UpdateAsync`; `ResetPasswordAsync`; `DeleteAsync`; `ValidateAsync` | `Staff` | `docs/contracts/merchant.md`; `docs/specs/spec-merchant-store.md` |
| 管理后台 | ADM-002, ADM-003, ADM-005, ADM-007 | `AdminController.GetMerchants`; `UpdateMerchantStatus`; `GetMerchantAuditLogs`; `GetAdminOperationLogs`; `GetSystemConfigs`; `UpsertSystemConfig`; `DeleteSystemConfig`; `IndustryCategoriesController` CRUD/tree | `AdminService.GetMerchantsAsync`; `UpdateMerchantStatusAsync`; `GetMerchantAuditLogsAsync`; `GetAdminOperationLogsAsync`; `GetSystemConfigsAsync`; `UpsertSystemConfigAsync`; `DeleteSystemConfigAsync`; `IndustryCategoryService` methods | `MerchantAuditLog`, `AdminOperationLog`, `SystemConfig`, `IndustryCategory`; `AdminServiceTests`, `AdminIntegrationTests` | `docs/contracts/admin.md`; `docs/specs/spec-admin.md`; `docs/specs/spec-template.md` |
| 模板系统 | ADM-004, MER-007 | `TemplatesController.GetList/GetById`; `CreateFromScratch`; `CreateFromStore`; `CreateCombined`; `Update`; `AddCategory`; `AddProduct`; `RemoveCategory`; `RemoveProduct`; `ApplyToStore` | `TemplateService.GetListAsync`; `GetByIdAsync`; `CreateFromScratchAsync`; `CreateFromStoreAsync`; `CreateCombinedAsync`; `UpdateAsync`; `AddCategoryAsync`; `AddProductAsync`; `ApplyToStoreAsync` 同名分类复用并追加商品 | `StoreTemplate`, `TemplateCategory`, `TemplateProduct`, `TemplateSpecGroup`, `TemplateSpecOption`; `TemplateIntegrationTests` | `docs/specs/spec-template.md`; `docs/contracts/admin.md` |
| 文件上传 | SYS-006 | `FilesController.Upload` `POST /api/files/upload` | Local upload handling in controller | `wwwroot/uploads`, static files in `Program.cs` | `docs/specs/spec-frontend.md`; `docs/frontend/components.md` |
| 系统能力 | SYS-001, SYS-002, SYS-003, SYS-004, SYS-005, SYS-007 | `Program.cs`; `GlobalFilters.cs`; `SeedData`; `OrderHub`; `StoreHub`; `ConversationHub` | Seeders and notification services | `ApiResponse`, `ObjectResult.DeclaredType` wrapping fix, EF migrations, SignalR hubs, `useSignalR` 重连后恢复订阅, default configs; `ApiSurfaceBuildTests`, `ApiResponseTests`, `ApiResponseWrapperFilterTests`, current unit tests | `docs/architecture/overview.md`; `docs/specs/spec-testing.md`; `docs/data/seed-data.md` |

## 前端功能点索引

| 领域 | 功能 ID | API 模块/方法 | 路由和页面 | Store/Composable/类型 | 关联文档 |
|------|---------|---------------|------------|-----------------------|----------|
| 统一登录 | MER-001, MER-002, ADM-001, CUST-016, CUST-017 | `merchantApi.login/register`; `customerApi.login/register`; `adminApi.login`; unified login in page flow | `/login` `Login.vue`; `/customer/login` `CustomerLogin.vue`; admin/merchant login redirects | `useMerchantStore.login`; localStorage token keys; `models/customer`, `models/merchant`, `models/admin` | `docs/frontend/overview.md`; `docs/contracts/auth.md` |
| 顾客入店和菜单 | CUST-001, CUST-002, CUST-003, CUST-004, CUST-008, CUST-019, CUST-020, CUST-022 | `customerApi.getStoreMenu`; `getStoreMenuByCode`; `searchStores`; `getRecentStores`; `storeApi.getById` | `/A/:code` `StoreMenu.vue`; `/store/:storeId` legacy redirect; `/stores/search` `StoreSearch.vue` | `useDeviceId`; `StoreMenuDto`, `StoreSummaryDto` | `docs/frontend/overview.md`; `docs/contracts/customer.md` |
| 商品详情和购物车 | CUST-005, CUST-006, CUST-018, CUST-023 | `customerApi.getCart`; `saveCart`; `mergeCart`; `productApi.getById` | `/A/:code/product/:productId` `ProductDetail.vue`; `/A/:code/cart` `Cart.vue`; `/A/:code/orders` `MyOrders.vue` | `useCartStore`; `models/product`, `models/customer`, `utils/order.ts` | `docs/specs/spec-category-product.md`; `docs/specs/spec-customer.md` |
| 顾客下单和订单详情 | CUST-007, CUST-009, CUST-010, CUST-011, CUST-012, CUST-013, CUST-014, CUST-015 | `customerApi.createOrder`; `orderApi.getById`; `orderApi.getCustomerOrders`; `orderApi.getCustomerOrdersAcrossStores`; `orderApi.cancelOrder`; `orderApi.getByPickupCode` | `/A/:code/order/:orderId` `OrderDetail.vue`; `/A/:code/orders` `MyOrders.vue` | `useSignalR`, `useDeviceId`, `PickupCodeDisplay.vue`, `models/order` | `docs/contracts/order.md`; `docs/specs/spec-order-pickup.md` |
| 商家订单和核销 | MER-010, MER-011, MER-012 | `orderApi.getByStoreId`; `acceptOrder`; `rejectOrder`; `startPreparing`; `markReady`; `completeOrder`; `getByPickupCode` | `/merchant/orders` `Orders.vue`; `/merchant/verify` `VerifyOrder.vue` | `useSignalR`; `useOrderStore`; `models/order` | `docs/contracts/order.md`; `docs/specs/spec-order-pickup.md` |
| 商家店铺 | MER-003, MER-004, MER-005, SYS-006 | `storeApi.getByMerchantId`; `storeApi.update`; `fileApi.upload` | `/merchant/store` `StoreInfo.vue`; `StoreShareDialog.vue` | `models/store`; upload URL usage | `docs/contracts/merchant.md`; `docs/specs/spec-merchant-store.md` |
| 菜单管理 | MER-006, MER-007, MER-008, MER-009, SYS-006 | `categoryApi`; `productApi.update` 保存 `categoryId/sortOrder`; `templateApi.getList/getById/applyTemplate`; `fileApi.upload` | `/merchant/menu` `Menu.vue` 商品缩略图/放大预览/上传式图片编辑、系统模板列表/内容预览/引入、拖拽排序和跨分类移动 | `models/category`, `models/product`, `models/template` | `docs/contracts/merchant.md`; `docs/specs/spec-category-product.md`; `docs/specs/spec-template.md` |
| 优惠活动 | CUST-008, MER-013 | `discountApi.getByStoreId`; `getActiveByStoreId`; `create`; `update`; `delete` | `/merchant/discounts` `Discounts.vue`; customer promotion display in `StoreMenu.vue` and `Cart.vue` | `models/discount` | `docs/specs/spec-discount.md` |
| 经营数据 | MER-014, ADM-006 | `dashboardApi.getOverview`; `adminApi.getPlatformStats` | `/merchant/dashboard` `Dashboard.vue`; `/admin/stats` `Stats.vue` | `models/dashboard` | `docs/specs/spec-dashboard.md`; `docs/contracts/admin.md` |
| 订单会话 | CUST-021, MER-015 | `conversationApi.startByOrder`; `getByStore`; `getByCustomer`; `getStoreUnreadCount`; `getCustomerUnreadCount`; `getMessages`; `sendMessage`; `markRead` | `OrderDetail.vue`; `Orders.vue`; `/A/:code/messages` `customer/Messages.vue` 先列表后会话详情且固定顶部信息/底部输入；`/merchant/messages` `merchant/Messages.vue` 先列表后会话详情且固定顶部信息/底部回复；`CustomerLayout.vue`; `MerchantLayout.vue` | `useSignalR` 重连回调；`useCustomerIdentity` 游客顾客 ID；`useConversationStore`; `models/conversation`; 仅中间消息区滚动，消息加载/发送/接收后滚动到底 | `docs/specs/spec-customer.md`; `docs/specs/spec-order-pickup.md`; `docs/frontend/overview.md` |
| 店员管理 | MER-016 | `staffApi.getByStoreId`; `create`; `update`; `resetPassword`; `delete` | `/merchant/staff` `Staff.vue` | staff model shape inline/API any | `docs/contracts/merchant.md`; `docs/specs/spec-merchant-store.md` |
| 管理后台 | ADM-002, ADM-003, ADM-005, ADM-007 | `adminApi.getMerchants`; `updateMerchantStatus`; `getAuditLogs`; `getOperationLogs`; `getConfigs`; `upsertConfig`; `deleteConfig`; `industryCategoryApi` | `/admin/merchants`; `/admin/industries`; `/admin/configs`; `/admin/logs`; `AdminLayout.vue` | `models/admin`, `models/template` | `docs/contracts/admin.md`; `docs/specs/spec-admin.md` |
| 模板系统 | ADM-004, MER-007, SYS-006 | `templateApi.getList/getById/createFromScratch/createFromStore/createCombined/update/addCategory/addProduct/removeCategory/removeProduct/applyTemplate`; `fileApi.upload` | `/admin/templates` `Templates.vue`; `/merchant/menu` 模板引用入口 | `models/template` | `docs/specs/spec-template.md`; `docs/contracts/admin.md` |
| 全局前端基础 | SYS-001, SYS-004, SYS-005 | `web/src/api/index.ts` response unwrap; API modules under `web/src/api/modules` | `web/src/router/index.ts`; shared layouts | `useSignalR` 幂等连接/自动重连/重连回调；`useDeviceId`; `useCustomerIdentity`; common components | `docs/frontend/overview.md`; `docs/frontend/components.md`; `docs/specs/spec-testing.md` |

## 新功能落地时的更新清单

1. 在 `docs/requirements/feature-implementation-checklist.md` 添加或更新功能 ID 和状态。
2. 在本文件添加或更新后端、前端和测试定位信息。
3. 如果新增或改变接口，更新 `docs/contracts/` 对应文件。
4. 如果新增业务规则，更新 `docs/specs/` 对应文件。
5. 如果新增实体、迁移、种子数据或 SQL，更新 `docs/data/` 或 `docs/sql/`。
6. 如果新增页面、路由、Store、Composable 或公共组件，更新 `docs/frontend/`。
7. 在最终回复中说明更新过哪些功能点文档和验证结果。
