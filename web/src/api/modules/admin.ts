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
}