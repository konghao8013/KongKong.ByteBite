/** API统一响应格式 */
export interface ApiResponse<T = any> {
  /** 状态码 */
  code: number
  /** 响应消息 */
  message: string
  /** 响应数据 */
  data: T
}

/** 分页查询参数 */
export interface PageParams {
  /** 页码（从1开始） */
  pageIndex: number
  /** 每页条数 */
  pageSize: number
}

/** 分页结果 */
export interface PagedResult<T = any> {
  /** 数据列表 */
  items: T[]
  /** 总条数 */
  totalCount: number
  /** 当前页码 */
  pageIndex: number
  /** 每页条数 */
  pageSize: number
}
