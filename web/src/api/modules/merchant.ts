import request from '@/api'

export const merchantApi = {
  register: (data: { phone: string; password: string; nickname?: string; storeName?: string }) =>
    request.post<any>('/merchants/register', data),

  login: (data: { phone: string; password: string }) =>
    request.post<any>('/merchants/login', data),

  getById: (id: string) =>
    request.get<any>(`/merchants/${id}`),

  logout: (id: string) =>
    request.post<any>(`/merchants/${id}/logout`),
}