/** 规格项DTO */
export interface SpecOptionDto {
  /** 规格项ID */
  id: string
  /** 规格项名称 */
  name: string
  /** 加价 */
  extraPrice: number
  /** 库存（null表示不限库存） */
  stock?: number
  /** 是否为默认选中项 */
  isDefault: boolean
}

/** 规格组DTO */
export interface SpecGroupDto {
  /** 规格组ID */
  id: string
  /** 规格组名称 */
  name: string
  /** 是否必选 */
  isRequired: boolean
  /** 规格项列表 */
  options: SpecOptionDto[]
}

/** 商品信息DTO */
export interface ProductDto {
  /** 商品ID */
  id: string
  /** 所属店铺ID */
  storeId: string
  /** 所属分类ID */
  categoryId: string
  /** 分类名称 */
  categoryName: string
  /** 商品名称 */
  name: string
  /** 商品描述 */
  description?: string
  /** 基础价格 */
  basePrice: number
  /** 商品图片URL */
  imageUrl?: string
  /** 状态：on-上架, off-下架, sold_out-售罄 */
  status: string
  /** 排序序号 */
  sortOrder: number
  /** 最低起购数量 */
  minOrderQty: number
  /** 月销量 */
  monthlySales: number
  /** 总销量 */
  totalSales: number
  /** 是否为套餐商品 */
  isCombo: boolean
  /** 规格组列表 */
  specGroups: SpecGroupDto[]
}

/** 创建规格项请求 */
export interface CreateSpecOptionRequest {
  /** 规格项名称 */
  name: string
  /** 加价 */
  extraPrice: number
  /** 库存（null表示不限库存） */
  stock?: number
  /** 是否为默认选中项 */
  isDefault: boolean
}

/** 创建规格组请求 */
export interface CreateSpecGroupRequest {
  /** 规格组名称 */
  name: string
  /** 是否必选 */
  isRequired?: boolean
  /** 规格项列表 */
  options: CreateSpecOptionRequest[]
}

/** 创建商品请求 */
export interface CreateProductRequest {
  /** 所属分类ID */
  categoryId: string
  /** 商品名称 */
  name: string
  /** 商品描述 */
  description?: string
  /** 基础价格 */
  basePrice: number
  /** 商品图片URL */
  imageUrl?: string
  /** 最低起购数量 */
  minOrderQty?: number
  /** 是否为套餐商品 */
  isCombo?: boolean
  /** 规格组列表 */
  specGroups: CreateSpecGroupRequest[]
}

/** 更新商品请求 */
export interface UpdateProductRequest {
  /** 所属分类ID */
  categoryId?: string
  /** 商品名称 */
  name?: string
  /** 商品描述 */
  description?: string
  /** 基础价格 */
  basePrice?: number
  /** 商品图片URL */
  imageUrl?: string
  /** 状态：on-上架, off-下架, sold_out-售罄 */
  status?: string
  /** 排序序号 */
  sortOrder?: number
  /** 最低起购数量 */
  minOrderQty?: number
}
