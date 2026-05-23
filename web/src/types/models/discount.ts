/** 优惠活动信息DTO */
export interface DiscountRuleDto {
  /** 优惠活动ID */
  id: string
  /** 所属店铺ID */
  storeId: string
  /** 优惠活动名称 */
  name: string
  /** 优惠类型：full_reduction-满减, discount-折扣 */
  discountType: string
  /** 满减门槛金额 */
  thresholdAmount?: number
  /** 满减减免金额 */
  discountAmount?: number
  /** 折扣率（如80=8折） */
  discountRate?: number
  /** 适用范围：all-全店, category-指定分类, product-指定商品 */
  applyScope: string
  /** 适用范围的分类ID或商品ID */
  applyScopeId?: string
  /** 是否允许与其他优惠叠加 */
  allowStack: boolean
  /** 活动开始时间 */
  startTime: string
  /** 活动结束时间 */
  endTime: string
  /** 状态：active-生效中, inactive-已停用 */
  status: string
  /** 已使用次数 */
  usedCount: number
}

/** 创建优惠活动请求 */
export interface CreateDiscountRuleRequest {
  /** 优惠活动名称 */
  name: string
  /** 优惠类型：full_reduction-满减, discount-折扣 */
  discountType: string
  /** 满减门槛金额（仅满减类型） */
  thresholdAmount?: number
  /** 满减减免金额（仅满减类型） */
  discountAmount?: number
  /** 折扣率（如80=8折，仅折扣类型） */
  discountRate?: number
  /** 适用范围：all-全店, category-指定分类, product-指定商品 */
  applyScope: string
  /** 适用范围的分类ID或商品ID */
  applyScopeId?: string
  /** 是否允许与其他优惠叠加 */
  allowStack: boolean
  /** 活动开始时间 */
  startTime: string
  /** 活动结束时间 */
  endTime: string
}

/** 更新优惠活动请求 */
export interface UpdateDiscountRuleRequest {
  /** 优惠活动名称 */
  name?: string
  /** 满减门槛金额 */
  thresholdAmount?: number
  /** 满减减免金额 */
  discountAmount?: number
  /** 折扣率 */
  discountRate?: number
  /** 适用范围 */
  applyScope?: string
  /** 适用范围的分类ID或商品ID */
  applyScopeId?: string
  /** 是否允许与其他优惠叠加 */
  allowStack?: boolean
  /** 活动开始时间 */
  startTime?: string
  /** 活动结束时间 */
  endTime?: string
  /** 状态：active-生效中, inactive-已停用 */
  status?: string
}
