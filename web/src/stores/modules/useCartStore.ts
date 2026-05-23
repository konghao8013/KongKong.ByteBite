import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { CartItem } from '@/types/models/customer'

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

  const getItemQuantity = (productId: string) =>
    items.value
      .filter((i) => i.productId === productId)
      .reduce((sum, i) => sum + i.quantity, 0)

  return {
    storeId, items, totalPrice, totalCount,
    addItem, removeItem, updateQuantity, clearCart, loadFromLocalStorage, getItemQuantity,
  }
})
