import axios from 'axios'
import { ElMessage } from 'element-plus'
import type { ApiResponse } from '@/types/api'

const instance = axios.create({
  baseURL: '/api',
  timeout: 15000,
})

instance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('merchant_token') || localStorage.getItem('customer_token') || localStorage.getItem('admin_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

instance.interceptors.response.use(
  (response) => {
    const res = response.data as ApiResponse
    if (res.code !== 200) {
      ElMessage.error(res.message || '请求失败')
      return Promise.reject(new Error(res.message || '请求失败'))
    }
    return res.data
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('merchant_token')
      localStorage.removeItem('customer_token')
      localStorage.removeItem('admin_token')
      const currentPath = window.location.pathname
      if (currentPath.startsWith('/merchant')) {
        window.location.href = '/merchant/login'
      } else if (currentPath.startsWith('/admin')) {
        window.location.href = '/admin/login'
      }
      ElMessage.error('登录已过期，请重新登录')
    } else {
      const message = error.response?.data?.message || error.message || '网络错误'
      ElMessage.error(message)
    }
    return Promise.reject(error)
  }
)

const request = {
  get<T = any>(url: string, config?: object): Promise<T> {
    return instance.get(url, config) as Promise<T>
  },
  post<T = any>(url: string, data?: any, config?: object): Promise<T> {
    return instance.post(url, data, config) as Promise<T>
  },
  put<T = any>(url: string, data?: any, config?: object): Promise<T> {
    return instance.put(url, data, config) as Promise<T>
  },
  patch<T = any>(url: string, data?: any, config?: object): Promise<T> {
    return instance.patch(url, data, config) as Promise<T>
  },
  delete<T = any>(url: string, config?: object): Promise<T> {
    return instance.delete(url, config) as Promise<T>
  },
}

export default request
