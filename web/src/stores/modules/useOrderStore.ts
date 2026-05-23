import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { OrderDto } from '@/types/models/order'

interface CachedOrderData {
  storeId: string
  activeOrders: string[]
  orderHistory: string[]
  updatedAt: string
}

export const useOrderStore = defineStore('order', () => {
  const activeOrders = ref<OrderDto[]>([])
  const orderHistory = ref<OrderDto[]>([])
  const currentStoreId = ref<string | null>(null)

  const hasActiveOrders = computed(() => activeOrders.value.length > 0)

  const addActiveOrder = (order: OrderDto) => {
    const exists = activeOrders.value.find(o => o.id === order.id)
    if (!exists) {
      activeOrders.value.unshift(order)
    }
    saveToLocalStorage()
  }

  const moveToHistory = (orderId: string) => {
    const index = activeOrders.value.findIndex(o => o.id === orderId)
    if (index !== -1) {
      const order = activeOrders.value.splice(index, 1)[0]
      orderHistory.value.unshift(order)
      saveToLocalStorage()
    }
  }

  const updateOrderStatus = (orderId: string, status: string) => {
    const order = activeOrders.value.find(o => o.id === orderId)
    if (order) {
      order.status = status
      saveToLocalStorage()
    }
  }

  const getPickupCodes = computed(() =>
    activeOrders.value
      .filter(o => o.pickupCode)
      .map(o => ({ orderId: o.id, pickupCode: o.pickupCode, storeName: o.storeName }))
  )

  const saveToLocalStorage = () => {
    if (!currentStoreId.value) return
    const cacheKey = `kongkong_bytebite_orders_${currentStoreId.value}`
    const data: CachedOrderData = {
      storeId: currentStoreId.value,
      activeOrders: activeOrders.value.map(o => o.id),
      orderHistory: orderHistory.value.map(o => o.id),
      updatedAt: new Date().toISOString(),
    }
    localStorage.setItem(cacheKey, JSON.stringify(data))
  }

  const loadFromLocalStorage = (storeId: string) => {
    currentStoreId.value = storeId
    const cacheKey = `kongkong_bytebite_orders_${storeId}`
    const cached = localStorage.getItem(cacheKey)
    if (cached) {
      try {
        const data: CachedOrderData = JSON.parse(cached)
        if (Date.now() - new Date(data.updatedAt).getTime() < 7 * 24 * 60 * 60 * 1000) {
          return data
        }
      } catch {
        // 缓存数据异常
      }
    }
    activeOrders.value = []
    orderHistory.value = []
    return null
  }

  const clearOrders = () => {
    activeOrders.value = []
    orderHistory.value = []
    if (currentStoreId.value) {
      const cacheKey = `kongkong_bytebite_orders_${currentStoreId.value}`
      localStorage.removeItem(cacheKey)
    }
  }

  return {
    activeOrders, orderHistory, currentStoreId,
    hasActiveOrders, getPickupCodes,
    addActiveOrder, moveToHistory, updateOrderStatus,
    loadFromLocalStorage, clearOrders,
  }
})
