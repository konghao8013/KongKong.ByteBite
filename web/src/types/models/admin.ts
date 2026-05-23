/** 管理员信息DTO */
export interface AdminDto {
  /** 管理员ID */
  id: string
  /** 用户名 */
  username: string
  /** 显示名称 */
  displayName?: string
  /** 角色 */
  role: string
  /** 认证Token */
  token?: string
}

/** 商家管理DTO（管理员视角） */
export interface MerchantManageDto {
  /** 商家ID */
  id: string
  /** 手机号 */
  phone: string
  /** 昵称 */
  nickname?: string
  /** 状态 */
  status: string
  /** 店铺数量 */
  storeCount: number
  /** 创建时间 */
  createdAt: string
  /** 最后登录时间 */
  lastLoginAt?: string
}

/** 系统配置DTO */
export interface SystemConfigDto {
  /** 配置ID */
  id: string
  /** 配置键名 */
  configKey: string
  /** 配置值 */
  configValue: string
  /** 值类型 */
  configType: string
  /** 配置说明 */
  description?: string
  /** 是否公开 */
  isPublic: boolean
}

/** 平台统计数据DTO */
export interface PlatformStatsDto {
  /** 商家总数 */
  totalMerchants: number
  /** 活跃商家数 */
  activeMerchants: number
  /** 待审核商家数 */
  pendingMerchants: number
  /** 店铺总数 */
  totalStores: number
  /** 今日订单总数 */
  todayOrders: number
  /** 今日平台营收 */
  todayRevenue: number
  /** 顾客总数 */
  totalCustomers: number
  /** 模板使用总次数 */
  templateUsageCount: number
}

/** 管理员登录请求 */
export interface LoginAdminRequest {
  /** 用户名 */
  username: string
  /** 密码 */
  password: string
}

/** 商家审核请求 */
export interface MerchantAuditRequest {
  /** 操作：approve-通过, reject-拒绝, disable-禁用, enable-解禁 */
  action: string
  /** 原因 */
  reason?: string
}

/** 更新系统配置请求 */
export interface UpdateConfigRequest {
  /** 配置值 */
  configValue: string
}
