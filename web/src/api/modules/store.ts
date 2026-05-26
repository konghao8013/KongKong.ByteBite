import request from '@/api'

export const storeApi = {
  getById: (id: string) =>
    request.get<any>(`/stores/${id}`),

  getByMerchantId: (merchantId: string) =>
    request.get<any[]>(`/stores/merchant/${merchantId}`),

  update: (id: string, data: {
    name?: string
    description?: string
    coverImageUrl?: string
    businessStatus?: string
    businessHoursStart?: string
    businessHoursEnd?: string
    industryCategoryId?: string
    diningMode?: string
    deliveryMinAmount?: number
    packingFee?: number
  }) =>
    request.put<any>(`/stores/${id}`, data),
}
