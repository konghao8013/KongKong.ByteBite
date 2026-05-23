import request from '@/api'

export const categoryApi = {
  getByStoreId: (storeId: string) =>
    request.get<any[]>(`/categories/store/${storeId}`),

  create: (storeId: string, data: { name: string; sortOrder: number }) =>
    request.post<any>('/categories', { storeId, name: data.name, sortOrder: data.sortOrder }),

  update: (id: string, data: { name: string; sortOrder: number }) =>
    request.put<any>(`/categories/${id}`, data),

  delete: (id: string) =>
    request.delete<void>(`/categories/${id}`),
}