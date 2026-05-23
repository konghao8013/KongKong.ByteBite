import request from '@/api'
import type { CustomerDto, DataMergeResultDto, StoreMenuDto, CreateOrderData } from '@/types/models/customer'
import type { OrderDto } from '@/types/models/order'

export const customerApi = {
  ensureAnonymous: (deviceId: string) =>
    request.post<CustomerDto>('/customers/anonymous', null, { params: { deviceId } }),

  register: (data: { phone: string; verifyCode: string; nickname?: string; deviceId?: string }) =>
    request.post<CustomerDto>('/customers/register', data),

  login: (data: { phone: string; verifyCode: string; deviceId?: string }) =>
    request.post<CustomerDto>('/customers/login', data),

  getById: (id: string) =>
    request.get<CustomerDto>(`/customers/${id}`),

  getStoreMenu: (storeId: string) =>
    request.get<StoreMenuDto>(`/customer/stores/${storeId}/menu`),

  createOrder: (data: CreateOrderData) =>
    request.post<OrderDto>('/orders', data),

  getOrderByPickupCode: (pickupCode: string, storeId: string) =>
    request.get<OrderDto>(`/orders/pickup/${pickupCode}/store/${storeId}`),

  getMergePreview: (deviceId: string) =>
    request.get<DataMergeResultDto>('/customers/merge-preview', { params: { deviceId } }),

  mergeData: (customerId: string, deviceId: string) =>
    request.post<DataMergeResultDto>(`/customers/${customerId}/merge`, null, { params: { deviceId } }),
}
