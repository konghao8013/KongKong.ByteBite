/** 经营数据概览DTO */
export interface DashboardOverviewDto {
  /** 今日订单数 */
  todayOrders: number
  /** 今日营收 */
  todayRevenue: number
  /** 较昨日订单变化百分比 */
  orderChangePercent: number
  /** 较昨日营收变化百分比 */
  revenueChangePercent: number
  /** 待处理订单数 */
  pendingOrders: number
}

/** 商品销售统计DTO */
export interface ProductSalesDto {
  /** 商品ID */
  productId: string
  /** 商品名称 */
  productName: string
  /** 分类名称 */
  categoryName?: string
  /** 销量 */
  salesQuantity: number
  /** 销售额 */
  salesAmount: number
  /** 平均单价 */
  avgUnitPrice: number
  /** 取消率 */
  cancelRate: number
  /** 规格分布（JSON） */
  specDistribution?: string
}

/** 分类销售占比DTO */
export interface CategorySalesDto {
  /** 分类ID */
  categoryId: string
  /** 分类名称 */
  categoryName: string
  /** 订单数 */
  orderCount: number
  /** 销售额 */
  salesAmount: number
  /** 占比百分比 */
  percentage: number
}

/** 趋势数据DTO */
export interface TrendDataDto {
  /** 日期 */
  date: string
  /** 数值 */
  value: number
  /** 对比数值（周对比时使用） */
  compareValue?: number
}

/** 时段分布DTO */
export interface HourlyDistributionDto {
  /** 时段（0-23） */
  hour: number
  /** 订单数 */
  orderCount: number
  /** 营收 */
  revenue: number
}

/** 顾客消费记录DTO */
export interface CustomerConsumptionDto {
  /** 顾客标识 */
  customerLabel: string
  /** 顾客ID */
  customerId?: string
  /** 设备标识 */
  deviceId?: string
  /** 订单数 */
  totalOrders: number
  /** 总消费 */
  totalAmount: number
  /** 最近下单时间 */
  lastOrderAt?: string
  /** 常购商品Top3（JSON） */
  topProducts?: string
  /** 是否为回头客 */
  isReturning: boolean
}

/** 优惠活动效果DTO */
export interface DiscountEffectDto {
  /** 优惠活动ID */
  discountRuleId: string
  /** 活动名称 */
  discountName: string
  /** 使用次数 */
  usedCount: number
  /** 优惠总额 */
  totalDiscountAmount: number
  /** 拉动营收 */
  totalDrivenRevenue: number
  /** ROI */
  roi: number
}

/** 经营数据查询参数 */
export interface DashboardQueryDto {
  /** 店铺ID */
  storeId: string
  /** 开始日期 */
  startDate?: string
  /** 结束日期 */
  endDate?: string
  /** 快捷时间范围：today, yesterday, last7days, last30days, custom */
  timeRange?: string
}
