import request from '@/api'

export const orderApi = {
  getByStoreId: (storeId: string, params?: { status?: string; pageIndex?: number; pageSize?: number }) =>
    request.get<any[]>(`/stores/${storeId}/orders`, { params }),

  getCustomerOrders: (storeId: string, params: { deviceId?: string; customerId?: string; pageSize?: number }) =>
    request.get<any[]>(`/stores/${storeId}/customer-orders`, { params }),

  getCustomerOrdersAcrossStores: (params: { deviceId?: string; customerId?: string; pageSize?: number }) =>
    request.get<any[]>('/customer-orders', { params }),

  getById: (orderId: string, params?: { deviceId?: string; customerId?: string }) =>
    request.get<any>(`/orders/${orderId}`, { params }),

  getByPickupCode: (pickupCode: string, storeId: string) =>
    request.get<any>(`/orders/pickup/${pickupCode}/store/${storeId}`),

  acceptOrder: (orderId: string) =>
    request.patch<any>(`/orders/${orderId}/accept`),

  rejectOrder: (orderId: string, reason: string) =>
    request.patch<any>(`/orders/${orderId}/reject`, { reason }),

  startPreparing: (orderId: string) =>
    request.patch<any>(`/orders/${orderId}/prepare`),

  markReady: (orderId: string) =>
    request.patch<any>(`/orders/${orderId}/ready`),

  completeOrder: (orderId: string) =>
    request.patch<any>(`/orders/${orderId}/complete`),

  cancelOrder: (orderId: string) =>
    request.patch<any>(`/orders/${orderId}/cancel`),
}
