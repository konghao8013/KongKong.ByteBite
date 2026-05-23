# Vue 3 组合式 API 常用模式

本文件按需加载，不是常驻协议。

## Axios 实例与拦截器

```typescript
import axios from 'axios'
import type { ApiResponse } from '@/types/api'
import { useAuthStore } from '@/stores/modules/useAuthStore'
import router from '@/router'

const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
})

request.interceptors.request.use((config) => {
  const authStore = useAuthStore()
  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`
  }
  return config
})

request.interceptors.response.use(
  (response) => {
    const data = response.data as ApiResponse<unknown>
    if (data.code !== 200) {
      return Promise.reject(new Error(data.message))
    }
    return data
  },
  async (error) => {
    const originalRequest = error.config
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true
      const authStore = useAuthStore()
      try {
        await authStore.refreshToken()
        return request(originalRequest)
      } catch {
        authStore.logout()
        router.push('/login')
      }
    }
    return Promise.reject(error)
  },
)

export default request
```

## 组合式函数模式

```typescript
import { ref, shallowRef } from 'vue'
import type { PagedResult, PageParams } from '@/types/api'

export function usePagedList<T>(fetchFn: (params: PageParams) => Promise<PagedResult<T>>) {
  const items = shallowRef<T[]>([]) as Ref<T[]>
  const loading = ref(false)
  const total = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(20)

  const fetch = async () => {
    loading.value = true
    try {
      const result = await fetchFn({ page: currentPage.value, pageSize: pageSize.value })
      items.value = result.items
      total.value = result.totalCount
    } finally {
      loading.value = false
    }
  }

  const changePage = (page: number) => {
    currentPage.value = page
    fetch()
  }

  return { items, loading, total, currentPage, pageSize, fetch, changePage }
}
```

## 表单组合式函数

```typescript
import { reactive, ref } from 'vue'
import type { FormInstance } from 'element-plus'

export function useForm<T extends Record<string, unknown>>(initialValues: T) {
  const formRef = ref<FormInstance>()
  const form = reactive({ ...initialValues }) as T
  const submitting = ref(false)

  const resetForm = () => {
    formRef.value?.resetFields()
    Object.assign(form, initialValues)
  }

  const validate = async () => {
    if (!formRef.value) return true
    return formRef.value.validate()
  }

  return { formRef, form, submitting, resetForm, validate }
}
```

## 权限指令

```typescript
import type { Directive } from 'vue'
import { useAuthStore } from '@/stores/modules/useAuthStore'

export const vPermission: Directive<HTMLElement, string[]> = {
  mounted(el, binding) {
    const authStore = useAuthStore()
    const requiredPermissions = binding.value
    if (!requiredPermissions.some((p) => authStore.permissions.includes(p))) {
      el.parentNode?.removeChild(el)
    }
  },
}
```

## 常见踩坑

| 坑 | 说明 | 解法 |
|---|---|---|
| reactive 丢失响应性 | 解构 reactive 对象 | 使用 `toRefs` 或直接用 `ref` |
| shallowRef 不触发更新 | 嵌套对象属性变更不触发 | 替换整个引用或使用 `triggerRef` |
| watch 深层监听性能 | `deep: true` 监听大对象 | 用 `watchEffect` 或精确指定监听路径 |
| onMounted 内异步 | 组件卸载后仍执行 | 使用 `onUnmounted` 取消或检查组件状态 |
| Pinia store 循环依赖 | Store 之间互相引用 | 提取共享逻辑到 composable |
| v-for 没有 key | 列表渲染性能差 | 始终绑定唯一 `:key` |
| API 类型不同步 | 后端 DTO 变更前端未更新 | 建立 API 类型生成流程或手动同步检查 |
