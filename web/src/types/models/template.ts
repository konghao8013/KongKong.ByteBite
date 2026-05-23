/** 模板规格项DTO */
export interface TemplateSpecOptionDto {
  /** 规格项ID */
  id: string
  /** 规格项名称 */
  name: string
  /** 加价 */
  extraPrice: number
  /** 是否为默认选中项 */
  isDefault: boolean
}

/** 模板规格组DTO */
export interface TemplateSpecGroupDto {
  /** 规格组ID */
  id: string
  /** 规格组名称 */
  name: string
  /** 是否必选 */
  isRequired: boolean
  /** 规格项列表 */
  options: TemplateSpecOptionDto[]
}

/** 模板商品DTO */
export interface TemplateProductDto {
  /** 商品ID */
  id: string
  /** 商品名称 */
  name: string
  /** 商品描述 */
  description?: string
  /** 参考价格 */
  referencePrice: number
  /** 商品图片URL */
  imageUrl?: string
  /** 排序序号 */
  sortOrder: number
  /** 最低起购数量 */
  minOrderQty: number
  /** 规格组列表 */
  specGroups: TemplateSpecGroupDto[]
}

/** 模板分类DTO */
export interface TemplateCategoryDto {
  /** 分类ID */
  id: string
  /** 分类名称 */
  name: string
  /** 分类类型：normal-普通, hot-热销, welfare-进店福利, combo-套餐 */
  categoryType: string
  /** 图标 */
  icon?: string
  /** 排序序号 */
  sortOrder: number
  /** 热销分类自动聚合的Top数量 */
  hotTopCount?: number
  /** 分类下的模板商品列表 */
  products: TemplateProductDto[]
}

/** 商家模板DTO */
export interface TemplateDto {
  /** 模板ID */
  id: string
  /** 模板名称 */
  name: string
  /** 所属行业分类ID */
  industryCategoryId?: string
  /** 所属行业分类名称 */
  industryCategoryName?: string
  /** 封面图URL */
  coverImageUrl?: string
  /** 模板描述 */
  description?: string
  /** 创建方式：manual-从零创建, from_store-从商家引入, combined-多商家组合 */
  sourceType: string
  /** 来源商家ID列表 */
  sourceStoreIds?: string
  /** 状态：active-生效中, inactive-已停用 */
  status: string
  /** 被应用次数 */
  useCount: number
  /** 模板分类列表 */
  categories: TemplateCategoryDto[]
}

/** 行业分类DTO */
export interface IndustryCategoryDto {
  /** 分类ID */
  id: string
  /** 父级分类ID */
  parentId?: string
  /** 分类名称 */
  name: string
  /** 层级：1-一级行业, 2-二级行业, 3-三级行业 */
  level: number
  /** 排序序号 */
  sortOrder: number
  /** 图标 */
  icon?: string
  /** 子级分类列表 */
  children: IndustryCategoryDto[]
}

/** 应用模板到店铺请求 */
export interface ApplyTemplateRequest {
  /** 模板ID */
  templateId: string
  /** 目标店铺ID */
  storeId: string
  /** 是否全部应用 */
  applyAll?: boolean
  /** 选中的分类ID列表（部分应用时使用） */
  selectedCategoryIds?: string[]
  /** 选中的商品ID列表（部分应用时使用） */
  selectedProductIds?: string[]
}
