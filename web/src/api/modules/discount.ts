import request from '@/api'

export const discountApi = {
  getByStoreId: (storeId: string) =>
    request.get<any[]>(`/discount-rules/store/${storeId}`),

  getActiveByStoreId: (storeId: string) =>
    request.get<any[]>(`/discount-rules/store/${storeId}/active`),

  create: (data: any) =>
    request.post<any>('/discount-rules', data),

  update: (id: string, data: { name?: string; status?: string }) =>
    request.put<any>(`/discount-rules/${id}`, data),

  delete: (id: string) =>
    request.delete(`/discount-rules/${id}`),
}