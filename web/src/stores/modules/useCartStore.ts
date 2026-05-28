import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { CartItem } from '@/types/models/customer'
import { customerApi } from '@/api/modules/customer'

const DEVICE_ID_KEY = 'kongkong_bytebite_device_id'

const getDeviceId = () => {
  let id = localStorage.getItem(DEVICE_ID_KEY)
  if (!id) {
    id = `dev_${Date.now().toString(36)}_${Math.random().toString(36).substring(2, 10)}`
    localStorage.setItem(DEVICE_ID_KEY, id)
  }
  return id
}

const itemKey = (item: CartItem) =>
  `${item.productId}:${item.specs.map((spec) => spec.optionId).sort().join('|')}:${item.remark || ''}`

export const useCartStore = defineStore('cart', () => {
  const storeId = ref<string | null>(null)
  const items = ref<CartItem[]>([])

  const totalPrice = computed(() =>
    items.value.reduce((sum, item) => sum + item.price * item.quantity, 0)
  )

  const totalCount = computed(() =>
    items.value.reduce((sum, item) => sum + item.quantity, 0)
  )

  const addItem = (item: CartItem) => {
    const existing = items.value.find(
      i => i.productId === item.productId && JSON.stringify(i.specs) === JSON.stringify(item.specs)
    )
    if (existing) {
      existing.quantity += item.quantity
    } else {
      items.value.push({ ...item })
    }
    saveToLocalStorage()
  }

  const removeItem = (index: number) => {
    items.value.splice(index, 1)
    saveToLocalStorage()
  }

  const updateQuantity = (index: number, quantity: number) => {
    if (quantity <= 0) {
      items.value.splice(index, 1)
    } else {
      items.value[index].quantity = quantity
    }
    saveToLocalStorage()
  }

  const clearCart = () => {
    items.value = []
    saveToLocalStorage()
  }

  const saveToLocalStorage = () => {
    if (!storeId.value) return
    const cacheKey = `kongkong_bytebite_cart_${storeId.value}`
    localStorage.setItem(cacheKey, JSON.stringify({
      storeId: storeId.value,
      items: items.value,
      updatedAt: new Date().toISOString(),
    }))
    void syncToServer()
  }

  const loadFromLocalStorage = (currentStoreId: string) => {
    storeId.value = currentStoreId
    const cacheKey = `kongkong_bytebite_cart_${currentStoreId}`
    const cached = localStorage.getItem(cacheKey)
    if (cached) {
      try {
        const data = JSON.parse(cached)
        const updatedAt = new Date(data.updatedAt)
        if (Date.now() - updatedAt.getTime() < 24 * 60 * 60 * 1000) {
          items.value = data.items || []
          return
        }
      } catch {
        // 缓存数据异常，清空购物车
      }
    }
    items.value = []
  }

  const mergeItems = (incoming: CartItem[], addQuantity = true) => {
    for (const item of incoming) {
      const existing = items.value.find(i => itemKey(i) === itemKey(item))
      if (existing) existing.quantity = addQuantity ? existing.quantity + item.quantity : Math.max(existing.quantity, item.quantity)
      else items.value.push({ ...item })
    }
  }

  const loadFromServer = async (currentStoreId: string) => {
    storeId.value = currentStoreId
    try {
      const remote = await customerApi.getCart({
        storeId: currentStoreId,
        customerId: localStorage.getItem('customer_id') || undefined,
        deviceId: getDeviceId(),
      })
      const remoteItems = (remote?.[0]?.items || []) as CartItem[]
      if (remoteItems.length) {
        mergeItems(remoteItems, false)
        saveToLocalStorage()
      }
    } catch {
      // 本地购物车可继续使用，服务端同步失败不阻塞点单。
    }
  }

  const syncToServer = async () => {
    if (!storeId.value) return
    try {
      await customerApi.saveCart({
        storeId: storeId.value,
        customerId: localStorage.getItem('customer_id') || undefined,
        deviceId: getDeviceId(),
        items: items.value,
      })
    } catch {
      // 静默失败，下一次打开店铺时会继续尝试同步。
    }
  }

  const getItemQuantity = (productId: string) =>
    items.value
      .filter((i) => i.productId === productId)
      .reduce((sum, i) => sum + i.quantity, 0)

  return {
    storeId, items, totalPrice, totalCount,
    addItem, removeItem, updateQuantity, clearCart, loadFromLocalStorage, loadFromServer, getItemQuantity,
  }
})
