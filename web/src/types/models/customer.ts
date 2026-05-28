/** 顾客端规格项DTO */
export interface MenuItemSpecOptionDto {
  /** 规格项ID */
  id: string
  /** 规格项名称 */
  name: string
  /** 加价 */
  extraPrice: number
  /** 是否为默认选中项 */
  isDefault: boolean
}

/** 顾客端规格组DTO */
export interface MenuItemSpecGroupDto {
  /** 规格组ID */
  id: string
  /** 规格组名称 */
  name: string
  /** 是否必选 */
  isRequired: boolean
  /** 规格项列表 */
  options: MenuItemSpecOptionDto[]
}

/** 顾客端商品信息DTO */
export interface StoreMenuItemDto {
  /** 商品ID */
  id: string
  /** 商品名称 */
  name: string
  /** 基础价格 */
  basePrice: number
  /** 起步价 */
  fromPrice: number
  /** 商品图片URL */
  imageUrl?: string
  /** 商品描述 */
  description?: string
  /** 月销量 */
  monthlySales: number
  /** 状态：on-上架, off-下架, sold_out-售罄 */
  status: string
  /** 最低起购数量 */
  minOrderQty: number
  /** 是否为套餐商品 */
  isCombo: boolean
  /** 规格组列表 */
  specs: MenuItemSpecGroupDto[]
}

/** 顾客端菜单分类DTO */
export interface StoreMenuCategoryDto {
  /** 分类ID */
  id: string
  /** 分类名称 */
  name: string
  /** 图标 */
  icon?: string
  /** 分类类型 */
  categoryType: string
  /** 排序序号 */
  sortOrder: number
  /** 分类下的商品列表 */
  items: StoreMenuItemDto[]
}

/** 店铺菜单DTO */
export interface StoreMenuDto {
  /** 店铺ID */
  storeId: string
  /** 店铺码 */
  storeCode?: string
  /** 店铺名称 */
  storeName: string
  /** 店铺描述 */
  description?: string
  /** 封面图URL */
  coverImageUrl?: string
  /** 营业状态：open-营业中, closed-休息中 */
  businessStatus: string
  /** 是否可下单 */
  canOrder: boolean
  /** 就餐方式 */
  diningMode: string
  /** 打包费 */
  packingFee: number
  /** 当前生效优惠活动 */
  activeDiscounts?: {
    id: string
    name: string
    discountType: string
    thresholdAmount?: number
    discountAmount?: number
    discountRate?: number
    applyScope: string
  }[]
  /** 分类列表 */
  categories: StoreMenuCategoryDto[]
}

/** 顾客端店铺摘要 */
export interface StoreSummaryDto {
  id: string
  storeCode: string
  name: string
  description?: string
  coverImageUrl?: string
  businessStatus: string
  monthlySales?: number
  industryName?: string
  activeDiscounts?: {
    id: string
    name: string
    discountType: string
    thresholdAmount?: number
    discountAmount?: number
    discountRate?: number
  }[]
  lastVisitedAt?: string
  lastOrderedAt?: string
}

/** 顾客信息DTO */
export interface CustomerDto {
  /** 顾客ID */
  id: string
  /** 手机号 */
  phone?: string
  /** 账号名 */
  username?: string
  /** 昵称 */
  nickname?: string
  /** 头像URL */
  avatarUrl?: string
  /** 是否已注册 */
  isRegistered: boolean
  /** 认证Token */
  token?: string
}

/** 注册顾客请求 */
export interface RegisterCustomerRequest {
  /** 手机号 */
  phone?: string
  /** 账号名 */
  username?: string
  /** 密码 */
  password: string
  /** 昵称 */
  nickname?: string
  /** 设备标识 */
  deviceId?: string
}

/** 登录顾客请求 */
export interface LoginCustomerRequest {
  /** 手机号或账号名 */
  account: string
  /** 密码 */
  password: string
  /** 设备标识 */
  deviceId?: string
}

/** 店铺合并摘要DTO */
export interface StoreMergeSummaryDto {
  /** 店铺ID */
  storeId: string
  /** 店铺名称 */
  storeName: string
  /** 进行中订单数 */
  activeOrders: number
  /** 已完成订单数 */
  completedOrders: number
  /** 购物车商品数 */
  cartItems: number
}

/** 数据合并结果DTO */
export interface DataMergeResultDto {
  /** 是否需要合并 */
  needMerge: boolean
  /** 待合并数据摘要列表 */
  storeSummaries: StoreMergeSummaryDto[]
  /** 合并的订单总数 */
  ordersMerged: number
  /** 合并的购物车商品总数 */
  cartItemsMerged: number
  /** 合并的取货码总数 */
  pickupCodesMerged: number
}

/** 购物车项DTO */
export interface CartItemDto {
  /** 商品ID */
  productId: string
  /** 商品名称 */
  productName: string
  /** 数量 */
  quantity: number
  /** 单价 */
  unitPrice: number
  /** 小计金额 */
  totalPrice: number
  /** 选中的规格项ID列表 */
  selectedSpecOptionIds: string[]
  /** 备注 */
  remark?: string
}

/** 前端购物车项（含规格展示信息） */
export interface CartItem {
  /** 商品ID */
  productId: string
  /** 商品名称 */
  productName: string
  /** 商品图片 */
  imageUrl?: string
  /** 单价（含规格加价） */
  price: number
  /** 数量 */
  quantity: number
  /** 选中的规格项ID列表 */
  specs: { specGroupId: string; specGroupName: string; optionId: string; optionName: string; extraPrice: number }[]
  /** 备注 */
  remark?: string
}

/** 创建订单请求数据 */
export interface CreateOrderData {
  /** 店铺ID */
  storeId: string
  /** 顾客ID */
  customerId?: string
  /** 匿名设备标识 */
  deviceId?: string
  /** 订单项列表 */
  items: {
    /** 商品ID */
    productId: string
    /** 数量 */
    quantity: number
    /** 选中的规格项ID列表 */
    selectedSpecOptionIds: string[]
    /** 备注 */
    remark?: string
  }[]
  /** 就餐方式：dine_in-堂食, takeaway-打包, delivery-外卖 */
  diningMode: string
  /** 桌号（堂食时使用） */
  tableNo?: string
  /** 外卖配送地址 */
  deliveryAddress?: string
  /** 外卖联系电话 */
  deliveryPhone?: string
  /** 订单备注 */
  remark?: string
  /** 使用的优惠活动ID */
  discountRuleId?: string
}
