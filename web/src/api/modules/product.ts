import request from '@/api'

export const productApi = {
  getByCategoryId: (categoryId: string) =>
    request.get<any[]>(`/products/category/${categoryId}`),

  getByStoreId: (storeId: string) =>
    request.get<any[]>(`/products/store/${storeId}`),

  getById: (id: string) =>
    request.get<any>(`/products/${id}`),

  create: (data: any) =>
    request.post<any>('/products', data),

  update: (id: string, data: any) =>
    request.put<any>(`/products/${id}`, data),

  delete: (id: string) =>
    request.delete<void>(`/products/${id}`),
}