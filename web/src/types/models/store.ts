/** 店铺信息DTO */
export interface StoreDto {
  /** 店铺ID */
  id: string
  /** 商家ID */
  merchantId: string
  /** 店铺名称 */
  name: string
  /** 店铺描述 */
  description?: string
  /** 封面图URL */
  coverImageUrl?: string
  /** 二维码URL */
  qrCodeUrl?: string
  /** 营业状态：open-营业中, closed-休息中 */
  businessStatus: string
  /** 营业开始时间 */
  businessHoursStart?: string
  /** 营业结束时间 */
  businessHoursEnd?: string
  /** 行业分类ID */
  industryCategoryId?: string
  /** 就餐方式 */
  diningMode: string
  /** 外卖最低消费金额 */
  deliveryMinAmount: number
  /** 打包费 */
  packingFee: number
  /** 月销量 */
  monthlySales: number
}

/** 创建店铺请求 */
export interface CreateStoreRequest {
  /** 店铺名称 */
  name: string
  /** 店铺描述 */
  description?: string
  /** 行业分类ID */
  industryCategoryId?: string
}

/** 更新店铺请求 */
export interface UpdateStoreRequest {
  /** 店铺名称 */
  name?: string
  /** 店铺描述 */
  description?: string
  /** 封面图URL */
  coverImageUrl?: string
  /** 营业状态：open-营业中, closed-休息中 */
  businessStatus?: string
  /** 营业开始时间 */
  businessHoursStart?: string
  /** 营业结束时间 */
  businessHoursEnd?: string
  /** 就餐方式 */
  diningMode?: string
  /** 外卖最低消费金额 */
  deliveryMinAmount?: number
  /** 打包费 */
  packingFee?: number
}
