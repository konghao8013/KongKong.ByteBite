<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { customerApi } from '@/api/modules/customer'
import { orderApi } from '@/api/modules/order'
import { useOrderStore } from '@/stores/modules/useOrderStore'
import { useDeviceId } from '@/composables/useDeviceId'
import { formatPrice, formatDate } from '@/utils/format'
import { normalizeCustomerOrder } from '@/utils/order'
import EmptyState from '@/components/common/EmptyState.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'

const router = useRouter()
const route = useRoute()
const orderStore = useOrderStore()
const { getDeviceId } = useDeviceId()
const storeCode = route.params.code as string

const storeId = ref(localStorage.getItem('current_store_id') || '')
const storeName = ref(localStorage.getItem('current_store_name') || '')
const loading = ref(false)
const error = ref('')

const statusMap: Record<string, { label: string; color: string; bgColor: string }> = {
  pending: { label: '待接单', color: '#087E6B', bgColor: '#E7F4EF' },
  accepted: { label: '已接单', color: '#346AC3', bgColor: '#EAF1FF' },
  preparing: { label: '制作中', color: '#9A6A00', bgColor: '#FFF6DB' },
  ready: { label: '待取餐', color: '#259D63', bgColor: '#E8F6EE' },
  completed: { label: '已完成', color: '#687872', bgColor: '#F1F4F2' },
  cancelled: { label: '已取消', color: '#D94C4C', bgColor: '#FFF0F0' },
  rejected: { label: '已拒单', color: '#D94C4C', bgColor: '#FFF0F0' },
}

const getStatusInfo = (status: string) =>
  statusMap[status] || { label: status, color: '#687872', bgColor: '#F1F4F2' }

const goBack = () => {
  router.back()
}

const goToDetail = (orderId: string) => {
  router.push({ name: 'OrderDetail', params: { code: storeCode, orderId } })
}

const goToMenu = () => {
  router.push({ name: 'StoreMenu', params: { code: storeCode } })
}

const hasOrders = computed(() =>
  orderStore.activeOrders.length > 0 || orderStore.orderHistory.length > 0
)

const ensureStoreInfo = async () => {
  if (storeId.value) return

  const menu = await customerApi.getStoreMenuByCode(storeCode)
  storeId.value = menu.storeId
  storeName.value = menu.storeName
  localStorage.setItem('current_store_id', menu.storeId)
  localStorage.setItem('current_store_name', menu.storeName)
}

const loadOrders = async () => {
  loading.value = true
  error.value = ''
  try {
    await ensureStoreInfo()
    orderStore.loadFromLocalStorage(storeId.value)

    const orders = await orderApi.getCustomerOrders(storeId.value, {
      deviceId: getDeviceId(),
      customerId: localStorage.getItem('customer_id') || undefined,
      pageSize: 50,
    })

    orderStore.setOrders(orders.map((order) => normalizeCustomerOrder(order, storeName.value)))
  } catch {
    error.value = '订单加载失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

const getItemsSummary = (order: { items?: { productName: string }[] }) => {
  const names = (order.items || []).map((item) => item.productName).filter(Boolean)
  return names.length ? names.join('、') : '订单商品'
}

onMounted(loadOrders)
</script>

<template>
  <div class="my-orders-page">
    <div class="page-header">
      <button class="back-btn" @click="goBack">‹</button>
      <h1 class="page-title">我的订单</h1>
      <span class="header-spacer" />
    </div>

    <LoadingSpinner v-if="loading" text="加载订单中..." />

    <EmptyState v-else-if="error" :description="error" icon="!">
      <button class="empty-action" @click="loadOrders">重新加载</button>
    </EmptyState>

    <EmptyState v-else-if="!hasOrders" description="暂无订单记录" icon="▤">
      <button class="empty-action" @click="goToMenu">去点单</button>
    </EmptyState>

    <div v-if="orderStore.activeOrders.length > 0" class="order-section">
      <div class="section-header">进行中</div>
      <article
        v-for="order in orderStore.activeOrders"
        :key="order.id"
        class="order-card"
        @click="goToDetail(order.id)"
      >
        <div class="card-header">
          <span class="store-name">{{ order.storeName }}</span>
          <span
            class="order-status"
            :style="{ color: getStatusInfo(order.status).color, background: getStatusInfo(order.status).bgColor }"
          >
            {{ getStatusInfo(order.status).label }}
          </span>
        </div>
        <div class="card-pickup">
          <span class="pickup-label">取餐码</span>
          <span class="pickup-code">{{ order.pickupCode }}</span>
        </div>
        <div class="card-items">
          <span class="items-summary">{{ getItemsSummary(order) }}</span>
        </div>
        <div class="card-footer">
          <span class="order-time">{{ formatDate(order.createdAt) }}</span>
          <span class="order-amount">{{ formatPrice(order.actualAmount) }}</span>
        </div>
      </article>
    </div>

    <div v-if="orderStore.orderHistory.length > 0" class="order-section">
      <div class="section-header">已完成</div>
      <article
        v-for="order in orderStore.orderHistory"
        :key="order.id"
        class="order-card completed"
        @click="goToDetail(order.id)"
      >
        <div class="card-header">
          <span class="store-name">{{ order.storeName }}</span>
          <span
            class="order-status"
            :style="{ color: getStatusInfo(order.status).color, background: getStatusInfo(order.status).bgColor }"
          >
            {{ getStatusInfo(order.status).label }}
          </span>
        </div>
        <div class="card-items">
          <span class="items-summary">{{ getItemsSummary(order) }}</span>
        </div>
        <div class="card-footer">
          <span class="order-time">{{ formatDate(order.createdAt) }}</span>
          <span class="order-amount">{{ formatPrice(order.actualAmount) }}</span>
        </div>
      </article>
    </div>
  </div>
</template>

<style scoped lang="scss">
.my-orders-page {
  min-height: 100%;
  background: $bg-color;
}

.page-header {
  background: rgba(255, 255, 255, 0.96);
  padding: 12px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 10;
  border-bottom: 1px solid $border-color;
  backdrop-filter: blur(12px);
}

.back-btn,
.header-spacer {
  width: 32px;
}

.back-btn {
  color: $text-color;
  font-size: 24px;
  cursor: pointer;
}

.page-title {
  margin: 0;
  color: $text-color;
  font-size: 17px;
  font-weight: 800;
}

.empty-action {
  height: 34px;
  padding: 0 14px;
  border-radius: 6px;
  color: #fff;
  background: $primary-color;
  font-weight: 700;
}

.order-section {
  padding: 0 16px;
  margin-top: 12px;
}

.section-header {
  padding: 8px 0;
  color: $text-secondary;
  font-size: 14px;
  font-weight: 700;
}

.order-card {
  margin-bottom: 10px;
  padding: 14px 16px;
  border: 1px solid $border-color;
  border-radius: 8px;
  background: #fff;
  box-shadow: $card-shadow;
  cursor: pointer;
  transition: transform 0.15s;

  &:active {
    transform: scale(0.98);
  }

  &.completed {
    opacity: 0.78;
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.store-name {
  min-width: 0;
  color: $text-color;
  font-size: 16px;
  font-weight: 800;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.order-status {
  flex-shrink: 0;
  padding: 3px 8px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.card-pickup {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  margin-bottom: 8px;
  border-radius: 8px;
  background: #FAFCFA;
}

.pickup-label {
  color: $text-secondary;
  font-size: 12px;
}

.pickup-code {
  color: $primary-color;
  font-size: 20px;
  font-weight: 800;
  letter-spacing: 4px;
}

.card-items {
  margin-bottom: 8px;
}

.items-summary {
  display: block;
  color: $text-secondary;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.order-time {
  color: #9AA9A3;
  font-size: 12px;
}

.order-amount {
  color: $accent-color;
  font-size: 16px;
  font-weight: 800;
}
</style>
