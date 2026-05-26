import request from '@/api'

export const categoryApi = {
  getByStoreId: (storeId: string) =>
    request.get<any[]>(`/categories/store/${storeId}`),

  create: (storeId: string, data: { name: string; sortOrder: number; categoryType?: string; icon?: string }) =>
    request.post<any>('/categories', { storeId, name: data.name, sortOrder: data.sortOrder, categoryType: data.categoryType, icon: data.icon }),

  update: (id: string, data: { name: string; sortOrder: number; categoryType?: string; icon?: string; isVisible?: boolean }) =>
    request.put<any>(`/categories/${id}`, data),

  delete: (id: string) =>
    request.delete<void>(`/categories/${id}`),
}
