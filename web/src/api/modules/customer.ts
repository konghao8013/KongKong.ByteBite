import request from '@/api'
import type {
  CustomerDto,
  DataMergeResultDto,
  StoreMenuDto,
  CreateOrderData,
  RegisterCustomerRequest,
  LoginCustomerRequest,
  StoreSummaryDto,
} from '@/types/models/customer'
import type { OrderDto } from '@/types/models/order'

export const customerApi = {
  ensureAnonymous: (deviceId: string) =>
    request.post<CustomerDto>('/customers/anonymous', null, { params: { deviceId } }),

  register: (data: RegisterCustomerRequest) =>
    request.post<CustomerDto>('/customers/register', data),

  login: (data: LoginCustomerRequest) =>
    request.post<CustomerDto>('/customers/login', data),

  getById: (id: string) =>
    request.get<CustomerDto>(`/customers/${id}`),

  getStoreMenu: (storeId: string, params?: { customerId?: string; deviceId?: string }) =>
    request.get<StoreMenuDto>(`/CustomerStore/${storeId}/menu`, { params }),

  getStoreMenuByCode: (storeCode: string, params?: { customerId?: string; deviceId?: string }) =>
    request.get<StoreMenuDto>(`/CustomerStore/code/${storeCode}/menu`, { params }),

  searchStores: (keyword: string, pageSize = 20) =>
    request.get<StoreSummaryDto[]>('/CustomerStore/search', { params: { keyword, pageSize } }),

  getRecentStores: (params: { customerId?: string; deviceId?: string; pageSize?: number }) =>
    request.get<StoreSummaryDto[]>('/CustomerStore/recent', { params }),

  createOrder: (data: CreateOrderData) =>
    request.post<OrderDto>('/orders', data),

  getOrderByPickupCode: (pickupCode: string, storeId: string) =>
    request.get<OrderDto>(`/orders/pickup/${pickupCode}/store/${storeId}`),

  getMergePreview: (deviceId: string) =>
    request.get<DataMergeResultDto>('/customers/merge-preview', { params: { deviceId } }),

  mergeData: (customerId: string, deviceId: string) =>
    request.post<DataMergeResultDto>(`/customers/${customerId}/merge`, { sourceDeviceId: deviceId }),

  getCart: (params: { customerId?: string; deviceId?: string; storeId?: string }) =>
    request.get<any[]>('/customers/cart', { params }),

  saveCart: (data: { customerId?: string; deviceId?: string; storeId: string; items: any[] }) =>
    request.put<any>('/customers/cart', data),

  mergeCart: (data: { customerId?: string; deviceId?: string; stores: any[] }) =>
    request.post<any[]>('/customers/cart/merge', data),
}
