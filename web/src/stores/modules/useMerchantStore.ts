import { defineStore } from 'pinia'
import { ref } from 'vue'
import { merchantApi } from '@/api/modules/merchant'
import type { MerchantDto } from '@/types/models/merchant'

export const useMerchantStore = defineStore('merchant', () => {
  const merchant = ref<MerchantDto | null>(null)
  const token = ref<string | null>(localStorage.getItem('merchant_token'))

  const login = async (phone: string, password: string) => {
    const data = await merchantApi.login({ phone, password })
    merchant.value = data
    token.value = data.token || null
    if (token.value) localStorage.setItem('merchant_token', token.value)
  }

  const logout = () => {
    merchant.value = null
    token.value = null
    localStorage.removeItem('merchant_token')
  }

  return { merchant, token, login, logout }
})
