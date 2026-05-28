import request from '@/api'

export const adminApi = {
  login: (data: { username: string; password: string }) =>
    request.post<any>('/admin/login', data),

  logout: (id: string) =>
    request.post<any>(`/admin/${id}/logout`),

  getMerchants: (params?: { status?: string; keyword?: string }) =>
    request.get<any[]>('/admin/merchants', { params }),

  updateMerchantStatus: (merchantId: string, data: { status: string; operatorId: string; reason: string }) =>
    request.patch<any>(`/admin/merchants/${merchantId}/status`, data),

  getAuditLogs: (merchantId?: string) =>
    request.get<any[]>('/admin/audit-logs', { params: merchantId ? { merchantId } : {} }),

  getOperationLogs: (adminId?: string) =>
    request.get<any[]>('/admin/operation-logs', { params: adminId ? { adminId } : {} }),

  getPlatformStats: () =>
    request.get<any>('/admin/platform-stats'),

  getConfigs: (publicOnly = false) =>
    request.get<any[]>('/admin/configs', { params: { publicOnly } }),

  upsertConfig: (data: {
    configKey: string
    configValue: string
    configType: string
    description?: string
    isPublic: boolean
    operatorId?: string
  }) =>
    request.put<any>('/admin/configs', data),

  deleteConfig: (id: string, operatorId?: string) =>
    request.delete<any>(`/admin/configs/${id}`, { params: operatorId ? { operatorId } : {} }),
}
