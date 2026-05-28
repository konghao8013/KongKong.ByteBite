import request from '@/api'

export const staffApi = {
  getByStoreId: (storeId: string) =>
    request.get<any[]>(`/staff/store/${storeId}`),

  create: (data: any) =>
    request.post<any>('/staff', data),

  update: (id: string, data: any) =>
    request.put<any>(`/staff/${id}`, data),

  resetPassword: (id: string, password: string) =>
    request.patch<any>(`/staff/${id}/password`, { password }),

  delete: (id: string) =>
    request.delete<any>(`/staff/${id}`),
}
