/** 分类信息DTO */
export interface CategoryDto {
  /** 分类ID */
  id: string
  /** 所属店铺ID */
  storeId: string
  /** 分类名称 */
  name: string
  /** 图标 */
  icon?: string
  /** 分类类型：normal-普通, hot-热销, welfare-进店福利, combo-套餐 */
  categoryType: string
  /** 排序序号 */
  sortOrder: number
  /** 是否可见 */
  isVisible: boolean
  /** 热销分类自动聚合的Top数量（仅hot类型有效） */
  hotTopCount?: number
  /** 分类下的商品数量 */
  productCount: number
}

/** 创建分类请求 */
export interface CreateCategoryRequest {
  /** 分类名称 */
  name: string
  /** 图标 */
  icon?: string
  /** 分类类型：normal-普通, hot-热销, welfare-进店福利, combo-套餐 */
  categoryType?: string
  /** 排序序号 */
  sortOrder?: number
  /** 热销分类自动聚合的Top数量 */
  hotTopCount?: number
}

/** 更新分类请求 */
export interface UpdateCategoryRequest {
  /** 分类名称 */
  name?: string
  /** 图标 */
  icon?: string
  /** 分类类型 */
  categoryType?: string
  /** 排序序号 */
  sortOrder?: number
  /** 是否可见 */
  isVisible?: boolean
  /** 热销分类自动聚合的Top数量 */
  hotTopCount?: number
}
