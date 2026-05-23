/** 商家信息DTO */
export interface MerchantDto {
  /** 商家ID */
  id: string
  /** 手机号 */
  phone: string
  /** 昵称 */
  nickname?: string
  /** 头像URL */
  avatarUrl?: string
  /** 状态：pending-待审核, active-已激活, disabled-已禁用 */
  status: string
  /** 认证Token */
  token?: string
}

/** 商家注册请求 */
export interface RegisterMerchantRequest {
  /** 手机号 */
  phone: string
  /** 密码（最少6位） */
  password: string
  /** 店铺名称 */
  storeName: string
  /** 行业分类ID（可选） */
  industryCategoryId?: string
}

/** 商家登录请求 */
export interface LoginMerchantRequest {
  /** 手机号 */
  phone: string
  /** 密码 */
  password: string
}
