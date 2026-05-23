/** 订单项DTO */
export interface OrderItemDto {
  /** 订单项ID */
  id: string
  /** 商品ID */
  productId: string
  /** 商品名称 */
  productName: string
  /** 商品图片 */
  productImage?: string
  /** 数量 */
  quantity: number
  /** 单价 */
  unitPrice: number
  /** 小计金额 */
  totalPrice: number
  /** 规格快照 */
  specSnapshot?: string
  /** 备注 */
  remark?: string
  /** 是否为套餐 */
  isCombo: boolean
}

/** 订单状态变更历史DTO */
export interface OrderStatusHistoryDto {
  /** 状态 */
  status: string
  /** 变更时间 */
  changedAt: string
}

/** 订单状态DTO */
export interface OrderStatusDto {
  /** 订单ID */
  orderId: string
  /** 取货码 */
  pickupCode: string
  /** 当前状态 */
  status: string
  /** 状态变更历史 */
  statusHistory: OrderStatusHistoryDto[]
}

/** 订单DTO */
export interface OrderDto {
  /** 订单ID */
  id: string
  /** 订单编号 */
  orderNo: string
  /** 店铺ID */
  storeId: string
  /** 店铺名称 */
  storeName: string
  /** 取货码 */
  pickupCode: string
  /** 就餐方式 */
  diningMode: string
  /** 桌号 */
  tableNo?: string
  /** 商品合计金额 */
  totalAmount: number
  /** 优惠减免金额 */
  discountAmount: number
  /** 应付金额 */
  actualAmount: number
  /** 打包费 */
  packingFee: number
  /** 订单备注 */
  remark?: string
  /** 状态 */
  status: string
  /** 订单项列表 */
  items: OrderItemDto[]
  /** 创建时间 */
  createdAt: string
}
