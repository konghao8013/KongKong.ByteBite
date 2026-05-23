import request from '@/api'

export const storeApi = {
  getById: (id: string) =>
    request.get<any>(`/stores/${id}`),

  getByMerchantId: (merchantId: string) =>
    request.get<any[]>(`/stores/merchant/${merchantId}`),

  update: (id: string, data: { name?: string; description?: string; coverImageUrl?: string; businessStatus?: string }) =>
    request.put<any>(`/stores/${id}`, data),
}