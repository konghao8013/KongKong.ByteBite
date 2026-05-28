import { defineStore } from 'pinia'
import { ref } from 'vue'
import { conversationApi } from '@/api/modules/conversation'

export const useConversationStore = defineStore('conversation', () => {
  const customerUnreadCount = ref(0)
  const merchantUnreadCount = ref(0)

  const loadCustomerUnreadCount = async (params: { customerId?: string; deviceId?: string }) => {
    const result = await conversationApi.getCustomerUnreadCount(params)
    customerUnreadCount.value = result?.count || 0
    return customerUnreadCount.value
  }

  const loadMerchantUnreadCount = async (storeId: string) => {
    if (!storeId) {
      merchantUnreadCount.value = 0
      return 0
    }
    const result = await conversationApi.getStoreUnreadCount(storeId)
    merchantUnreadCount.value = result?.count || 0
    return merchantUnreadCount.value
  }

  const setCustomerUnreadCount = (count: number) => {
    customerUnreadCount.value = Math.max(0, count || 0)
  }

  const setMerchantUnreadCount = (count: number) => {
    merchantUnreadCount.value = Math.max(0, count || 0)
  }

  return {
    customerUnreadCount,
    merchantUnreadCount,
    loadCustomerUnreadCount,
    loadMerchantUnreadCount,
    setCustomerUnreadCount,
    setMerchantUnreadCount,
  }
})
