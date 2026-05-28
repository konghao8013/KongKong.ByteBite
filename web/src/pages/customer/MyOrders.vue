<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { customerApi } from '@/api/modules/customer'
import { orderApi } from '@/api/modules/order'
import { useDeviceId } from '@/composables/useDeviceId'
import { useCartStore } from '@/stores/modules/useCartStore'
import { formatPrice, formatDate } from '@/utils/format'
import { normalizeCustomerOrder } from '@/utils/order'
import EmptyState from '@/components/common/EmptyState.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { OrderDto } from '@/types/models/order'
import type { StoreSummaryDto } from '@/types/models/customer'

const router = useRouter()
const route = useRoute()
const { getDeviceId } = useDeviceId()
const cartStore = useCartStore()
const storeCode = computed(() => route.params.code as string)

const orders = ref<OrderDto[]>([])
const recentStores = ref<StoreSummaryDto[]>([])
const loading = ref(false)
const error = ref('')

const customerId = computed(() => localStorage.getItem('customer_id') || undefined)
const isLoggedIn = computed(() => Boolean(localStorage.getItem('customer_token')))

const statusMap: Record<string, { label: string; color: string; bgColor: string }> = {
  pending: { label: '待接单', color: '#087E6B', bgColor: '#E7F4EF' },
  accepted: { label: '已接单', color: '#346AC3', bgColor: '#EAF1FF' },
  preparing: { label: '制作中', color: '#9A6A00', bgColor: '#FFF6DB' },
  ready: { label: '待取餐', color: '#259D63', bgColor: '#E8F6EE' },
  completed: { label: '已完成', color: '#687872', bgColor: '#F1F4F2' },
  cancelled: { label: '已取消', color: '#D94C4C', bgColor: '#FFF0F0' },
  rejected: { label: '已拒单', color: '#D94C4C', bgColor: '#FFF0F0' },
}

const groupedOrders = computed(() => {
  const groups = new Map<string, { storeId: string; storeName: string; storeCode?: string; active: OrderDto[]; history: OrderDto[] }>()
  const historyStatuses = new Set(['completed', 'cancelled', 'rejected'])
  orders.value.forEach((order) => {
    const key = order.storeId || order.storeName || 'unknown'
    if (!groups.has(key)) {
      groups.set(key, {
        storeId: order.storeId,
        storeName: order.storeName || '店铺',
        storeCode: order.storeCode,
        active: [],
        history: [],
      })
    }
    const group = groups.get(key)!
    if (!group.storeCode && order.storeCode) group.storeCode = order.storeCode
    if (historyStatuses.has(order.status)) group.history.push(order)
    else group.active.push(order)
  })
  return Array.from(groups.values())
})

const hasOrders = computed(() => orders.value.length > 0)

const getStatusInfo = (status: string) =>
  statusMap[status] || { label: status, color: '#687872', bgColor: '#F1F4F2' }

const getItemsSummary = (order: OrderDto) => {
  const names = (order.items || []).map((item) => item.productName).filter(Boolean)
  return names.length ? names.join('、') : '订单商品'
}

const loadOrders = async () => {
  loading.value = true
  error.value = ''
  try {
    const params = {
      deviceId: getDeviceId(),
      customerId: customerId.value,
      pageSize: 100,
    }
    const [orderRows, stores] = await Promise.all([
      orderApi.getCustomerOrdersAcrossStores(params),
      customerApi.getRecentStores({ ...params, pageSize: 20 }),
    ])
    orders.value = orderRows.map((order) => normalizeCustomerOrder(order))
    recentStores.value = stores || []
  } catch {
    error.value = '订单加载失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

const goBack = () => {
  router.back()
}

const goToDetail = (order: OrderDto) => {
  const code = order.storeCode || storeCode.value || localStorage.getItem('current_store_code') || ''
  router.push({ name: 'OrderDetail', params: { code, orderId: order.id } })
}

const goToMenu = (code?: string) => {
  const targetCode = code || storeCode.value || localStorage.getItem('current_store_code') || ''
  if (targetCode) router.push({ name: 'StoreMenu', params: { code: targetCode } })
  else router.push({ name: 'StoreSearch' })
}

const goToSearch = () => {
  router.push({ name: 'StoreSearch' })
}

const goToLogin = () => {
  router.push({ name: 'CustomerLogin', query: { redirect: route.fullPath } })
}

const parseSpecs = (snapshot?: string) => {
  if (!snapshot) return []
  try {
    const rows = JSON.parse(snapshot)
    if (!Array.isArray(rows)) return []
    return rows.map((row: any) => ({
      specGroupId: row.SpecGroupId || row.specGroupId,
      specGroupName: row.SpecGroupName || row.specGroupName,
      optionId: row.OptionId || row.optionId,
      optionName: row.OptionName || row.optionName,
      extraPrice: Number(row.ExtraPrice ?? row.extraPrice ?? 0),
    })).filter((row: any) => row.optionId)
  } catch {
    return []
  }
}

const reorder = (order: OrderDto) => {
  if (!order.storeId || !order.items?.length) return
  cartStore.loadFromLocalStorage(order.storeId)
  order.items.forEach((item) => {
    cartStore.addItem({
      productId: item.productId,
      productName: item.productName,
      imageUrl: item.productImage,
      price: Number(item.unitPrice || 0),
      quantity: item.quantity,
      specs: parseSpecs(item.specSnapshot),
      remark: item.remark || '',
    })
  })
  const code = order.storeCode || storeCode.value || localStorage.getItem('current_store_code') || ''
  if (code) router.push({ name: 'Cart', params: { code } })
}

onMounted(loadOrders)
</script>

<template>
  <div class="my-orders-page">
    <div class="page-header">
      <button class="back-btn" @click="goBack">‹</button>
      <h1 class="page-title">我的订单</h1>
      <button class="search-btn" @click="goToSearch">找店</button>
    </div>

    <div v-if="!isLoggedIn" class="account-tip">
      <div>
        <strong>注册后可跨设备查看订单</strong>
        <p>当前先展示本机匿名订单，登录后会自动合并。</p>
      </div>
      <button @click="goToLogin">登录/注册</button>
    </div>

    <LoadingSpinner v-if="loading" text="加载订单中..." />

    <EmptyState v-else-if="error" :description="error" icon="!">
      <button class="empty-action" @click="loadOrders">重新加载</button>
    </EmptyState>

    <template v-else-if="hasOrders">
      <section v-for="group in groupedOrders" :key="group.storeId" class="store-group">
        <div class="group-header">
          <div>
            <h2>{{ group.storeName }}</h2>
            <p v-if="group.storeCode">店铺码 {{ group.storeCode }}</p>
          </div>
          <button @click="goToMenu(group.storeCode)">进入店铺</button>
        </div>

        <div v-if="group.active.length" class="order-section">
          <div class="section-header">进行中</div>
          <article v-for="order in group.active" :key="order.id" class="order-card" @click="goToDetail(order)">
            <div class="card-header">
              <span class="pickup-code">#{{ order.pickupCode }}</span>
              <span
                class="order-status"
                :style="{ color: getStatusInfo(order.status).color, background: getStatusInfo(order.status).bgColor }"
              >
                {{ getStatusInfo(order.status).label }}
              </span>
            </div>
            <div class="card-items">{{ getItemsSummary(order) }}</div>
            <div class="card-footer">
              <span>{{ formatDate(order.createdAt) }}</span>
              <strong>{{ formatPrice(order.actualAmount) }}</strong>
            </div>
            <button class="reorder-btn" @click.stop="reorder(order)">再来一单</button>
          </article>
        </div>

        <div v-if="group.history.length" class="order-section">
          <div class="section-header">历史订单</div>
          <article v-for="order in group.history" :key="order.id" class="order-card completed" @click="goToDetail(order)">
            <div class="card-header">
              <span class="items-summary">{{ getItemsSummary(order) }}</span>
              <span
                class="order-status"
                :style="{ color: getStatusInfo(order.status).color, background: getStatusInfo(order.status).bgColor }"
              >
                {{ getStatusInfo(order.status).label }}
              </span>
            </div>
            <div class="card-footer">
              <span>{{ formatDate(order.createdAt) }}</span>
              <strong>{{ formatPrice(order.actualAmount) }}</strong>
            </div>
            <button class="reorder-btn" @click.stop="reorder(order)">再来一单</button>
          </article>
        </div>
      </section>
    </template>

    <EmptyState v-else description="暂无订单记录" icon="□">
      <button class="empty-action" @click="goToMenu()">去点单</button>
      <button class="empty-secondary" @click="goToSearch">搜索店铺</button>
    </EmptyState>

    <section v-if="recentStores.length" class="recent-section">
      <div class="section-header">最近访问的店铺</div>
      <button
        v-for="store in recentStores"
        :key="store.id"
        class="recent-store"
        @click="goToMenu(store.storeCode)"
      >
        <span>{{ store.name }}</span>
        <small>{{ store.industryName || store.storeCode }}</small>
      </button>
    </section>
  </div>
</template>

<style scoped lang="scss">
.my-orders-page {
  min-height: 100%;
  padding-bottom: 24px;
  background: #F6F7F3;
}

.page-header {
  position: sticky;
  top: 0;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #E2E8E3;
  background: rgba(255, 255, 255, 0.96);
  backdrop-filter: blur(12px);
}

.back-btn,
.search-btn {
  width: 44px;
  height: 32px;
  border-radius: 6px;
  color: #087E6B;
  background: #E7F4EF;
  font-weight: 800;
}

.back-btn {
  color: #1F2A26;
  background: transparent;
  font-size: 24px;
}

.page-title {
  margin: 0;
  color: #1F2A26;
  font-size: 17px;
  font-weight: 800;
}

.account-tip {
  display: flex;
  gap: 12px;
  align-items: center;
  justify-content: space-between;
  margin: 12px 16px;
  padding: 12px;
  border: 1px solid #F5E2A8;
  border-radius: 8px;
  background: #FFF8E6;

  strong {
    color: #1F2A26;
    font-size: 14px;
  }

  p {
    margin: 4px 0 0;
    color: #9A6A00;
    font-size: 12px;
  }

  button {
    flex-shrink: 0;
    height: 32px;
    padding: 0 12px;
    border-radius: 6px;
    color: #fff;
    background: #087E6B;
    font-weight: 800;
  }
}

.store-group,
.recent-section {
  margin: 12px 16px 0;
}

.group-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 0 8px;

  h2 {
    margin: 0;
    color: #1F2A26;
    font-size: 16px;
    font-weight: 900;
  }

  p {
    margin: 3px 0 0;
    color: #687872;
    font-size: 12px;
  }

  button {
    height: 32px;
    padding: 0 12px;
    border: 1px solid #DCE6E1;
    border-radius: 6px;
    color: #087E6B;
    background: #fff;
    font-weight: 800;
  }
}

.section-header {
  padding: 8px 0;
  color: #687872;
  font-size: 13px;
  font-weight: 800;
}

.order-card {
  margin-bottom: 10px;
  padding: 14px 16px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);

  &.completed {
    opacity: 0.82;
  }
}

.card-header,
.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.reorder-btn {
  width: 100%;
  height: 32px;
  margin-top: 10px;
  border-radius: 6px;
  color: #087E6B;
  background: #E7F4EF;
  font-weight: 800;
}

.pickup-code {
  color: #087E6B;
  font-size: 20px;
  font-weight: 900;
  letter-spacing: 2px;
}

.order-status {
  flex-shrink: 0;
  padding: 3px 8px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 800;
}

.card-items,
.items-summary {
  display: block;
  margin: 8px 0;
  color: #687872;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.card-footer {
  color: #9AA9A3;
  font-size: 12px;

  strong {
    color: #FF6B4A;
    font-size: 16px;
  }
}

.empty-action,
.empty-secondary {
  height: 34px;
  padding: 0 14px;
  border-radius: 6px;
  color: #fff;
  background: #087E6B;
  font-weight: 800;
}

.empty-secondary {
  margin-left: 8px;
  color: #087E6B;
  background: #E7F4EF;
}

.recent-store {
  width: 100%;
  margin-bottom: 8px;
  padding: 12px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #fff;
  color: #1F2A26;
  font-weight: 800;

  small {
    color: #687872;
    font-weight: 600;
  }
}
</style>
