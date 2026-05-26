<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { orderApi } from '@/api/modules/order'
import { useOrderStore } from '@/stores/modules/useOrderStore'
import { formatPrice, formatDate } from '@/utils/format'
import { useSignalR } from '@/composables/useSignalR'
import PickupCodeDisplay from '@/components/common/PickupCodeDisplay.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { OrderDto } from '@/types/models/order'

const route = useRoute()
const router = useRouter()
const orderStore = useOrderStore()
const { connection, connect, disconnect } = useSignalR('/hubs/order')
const orderId = route.params.orderId as string

const order = ref<OrderDto | null>(null)
const loading = ref(true)
const cancelling = ref(false)

onMounted(async () => {
  try {
    const cached = orderStore.activeOrders.find((o) => o.id === orderId)
      || orderStore.orderHistory.find((o) => o.id === orderId)
    if (cached) {
      order.value = cached
    }
  } catch {
  }
  try {
    await connect()
    connection.value?.on('OrderStatusChanged', (payload: { orderId: string; status: string }) => {
      if (payload.orderId !== orderId || !order.value) return
      order.value.status = payload.status
      orderStore.updateOrderStatus(orderId, payload.status)
    })
    connection.value?.on('OrderCancelled', (payload: { orderId: string }) => {
      if (payload.orderId !== orderId || !order.value) return
      order.value.status = 'cancelled'
      orderStore.updateOrderStatus(orderId, 'cancelled')
    })
    await connection.value?.invoke('SubscribeOrder', orderId)
  } catch {
  }
  loading.value = false
})

onUnmounted(async () => {
  try {
    await connection.value?.invoke('UnsubscribeOrder', orderId)
  } catch {
  }
  await disconnect()
})

const statusMap: Record<string, { label: string; color: string; icon: string }> = {
  pending: { label: '待接单', color: '#087E6B', icon: '⏳' },
  accepted: { label: '已接单', color: '#1890ff', icon: '✅' },
  preparing: { label: '制作中', color: '#F7B731', icon: '👨‍🍳' },
  ready: { label: '备餐完毕', color: '#52c41a', icon: '🔔' },
  completed: { label: '已完成', color: '#999', icon: '🎉' },
  cancelled: { label: '已取消', color: '#D94C4C', icon: '❌' },
  rejected: { label: '已拒单', color: '#D94C4C', icon: '❌' }
}

const currentStatus = computed(() => {
  if (!order.value) return null
  return statusMap[order.value.status] || { label: order.value.status, color: '#999', icon: '❓' }
})

const statusTimeline = computed(() => {
  if (!order.value) return []
  const flow = ['pending', 'accepted', 'preparing', 'ready', 'completed']
  const currentIdx = flow.indexOf(order.value.status)
  return flow.map((status, idx) => ({
    status,
    ...statusMap[status],
    isCurrent: status === order.value!.status,
    isPast: idx < currentIdx,
    isFuture: idx > currentIdx
  }))
})

const diningModeLabel = computed(() => {
  const map: Record<string, string> = { dine_in: '堂食', takeaway: '打包', delivery: '外卖' }
  return map[order.value?.diningMode || ''] || order.value?.diningMode
})

const canCancel = computed(() => {
  return order.value && (order.value.status === 'pending' || order.value.status === 'accepted')
})

const cancelOrder = async () => {
  if (!canCancel.value || cancelling.value) return
  cancelling.value = true
  try {
    const updated = await orderApi.cancelOrder(orderId)
    if (updated) {
      order.value = updated
      orderStore.updateOrderStatus(orderId, updated.status)
    }
  } catch {
  } finally {
    cancelling.value = false
  }
}

const goBack = () => {
  router.back()
}
</script>

<template>
  <div class="order-detail-page">
    <!-- 顶部导航 -->
    <div class="page-header">
      <span class="back-btn" @click="goBack">←</span>
      <h1 class="page-title">订单详情</h1>
      <span />
    </div>

    <LoadingSpinner v-if="loading" text="加载订单中..." />

    <template v-else-if="order">
      <!-- 取货码区域 -->
      <div class="pickup-section">
        <PickupCodeDisplay :code="order.pickupCode" :status="order.status" />
      </div>

      <!-- 订单状态 -->
      <div class="status-section">
        <div class="current-status" :style="{ color: currentStatus?.color }">
          <span class="status-icon">{{ currentStatus?.icon }}</span>
          <span class="status-label">{{ currentStatus?.label }}</span>
        </div>
        <div class="status-timeline">
          <div
            v-for="(step, idx) in statusTimeline"
            :key="step.status"
            class="timeline-step"
            :class="{ past: step.isPast, current: step.isCurrent, future: step.isFuture }"
          >
            <div class="timeline-dot" />
            <div v-if="idx < statusTimeline.length - 1" class="timeline-line" />
            <span class="timeline-label">{{ step.label }}</span>
          </div>
        </div>
      </div>

      <!-- 订单信息 -->
      <div class="info-section">
        <div class="info-row">
          <span class="info-label">店铺</span>
          <span class="info-value">{{ order.storeName }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">就餐方式</span>
          <span class="info-value">{{ diningModeLabel }}</span>
        </div>
        <div v-if="order.tableNo" class="info-row">
          <span class="info-label">桌号</span>
          <span class="info-value">{{ order.tableNo }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">下单时间</span>
          <span class="info-value">{{ formatDate(order.createdAt) }}</span>
        </div>
        <div v-if="order.remark" class="info-row">
          <span class="info-label">备注</span>
          <span class="info-value remark">{{ order.remark }}</span>
        </div>
      </div>

      <!-- 商品列表 -->
      <div class="items-section">
        <div class="section-title">订单商品</div>
        <div v-for="item in order.items" :key="item.id" class="order-item">
          <div class="item-left">
            <div v-if="item.productImage" class="item-image-wrapper">
              <img :src="item.productImage" :alt="item.productName" class="item-image" />
            </div>
            <div class="item-info">
              <span class="item-name">{{ item.productName }}</span>
              <span v-if="item.specSnapshot" class="item-spec">{{ item.specSnapshot }}</span>
              <span v-if="item.remark" class="item-remark">备注: {{ item.remark }}</span>
            </div>
          </div>
          <div class="item-right">
            <span class="item-qty">×{{ item.quantity }}</span>
            <span class="item-price">{{ formatPrice(item.totalPrice) }}</span>
          </div>
        </div>
      </div>

      <!-- 金额明细 -->
      <div class="amount-section">
        <div class="amount-row">
          <span>商品合计</span>
          <span>{{ formatPrice(order.totalAmount) }}</span>
        </div>
        <div v-if="order.packingFee > 0" class="amount-row">
          <span>打包费</span>
          <span>{{ formatPrice(order.packingFee) }}</span>
        </div>
        <div v-if="order.discountAmount > 0" class="amount-row discount">
          <span>优惠减免</span>
          <span>-{{ formatPrice(order.discountAmount) }}</span>
        </div>
        <div class="amount-row total">
          <span>实付金额</span>
          <span class="total-amount">{{ formatPrice(order.actualAmount) }}</span>
        </div>
      </div>

      <!-- 取消订单 -->
      <div v-if="canCancel" class="cancel-section">
        <button class="cancel-btn" :class="{ disabled: cancelling }" @click="cancelOrder">
          {{ cancelling ? '取消中...' : '取消订单' }}
        </button>
      </div>
    </template>
  </div>
</template>

<style scoped lang="scss">
.order-detail-page {
  min-height: 100%;
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

.back-btn {
  font-size: 22px;
  cursor: pointer;
  width: 32px;
}

.page-title {
  font-size: 17px;
  font-weight: 800;
  color: $text-color;
}

.pickup-section {
  padding: 16px;
}

.status-section {
  background: #fff;
  padding: 16px;
  margin-bottom: 10px;
  border-top: 1px solid $border-color;
  border-bottom: 1px solid $border-color;
}

.current-status {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 18px;
  font-weight: 700;
  margin-bottom: 16px;
}

.status-icon {
  font-size: 22px;
}

.status-timeline {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 0 8px;
}

.timeline-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  flex: 1;
}

.timeline-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #ddd;
  z-index: 1;
  flex-shrink: 0;

  .timeline-step.past &,
  .timeline-step.current & {
    background: $brand-color;
  }

  .timeline-step.current & {
    box-shadow: 0 0 0 4px rgba(8, 126, 107, 0.14);
  }
}

.timeline-line {
  position: absolute;
  top: 5px;
  left: 50%;
  width: 100%;
  height: 2px;
  background: #ddd;
  z-index: 0;

  .timeline-step.past & {
    background: $brand-color;
  }
}

.timeline-label {
  margin-top: 8px;
  text-align: center;
  font-size: 11px;
  color: $text-secondary;

  .timeline-step.past &,
  .timeline-step.current & {
    color: $brand-color;
    font-weight: 600;
  }
}

.info-section {
  background: #fff;
  padding: 14px 16px;
  margin-bottom: 10px;
  border-top: 1px solid $border-color;
  border-bottom: 1px solid $border-color;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 6px 0;
}

.info-label {
  font-size: 14px;
  color: $text-secondary;
  flex-shrink: 0;
}

.info-value {
  font-size: 14px;
  text-align: right;
  max-width: 200px;

  &.remark {
    color: $brand-color;
  }
}

.items-section {
  background: #fff;
  padding: 14px 16px;
  margin-bottom: 10px;
  border-top: 1px solid $border-color;
  border-bottom: 1px solid $border-color;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 12px;
}

.order-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid $border-color;

  &:last-child {
    border-bottom: none;
  }
}

.item-left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  min-width: 0;
}

.item-image-wrapper {
  width: 44px;
  height: 44px;
  border-radius: 6px;
  overflow: hidden;
  flex-shrink: 0;
}

.item-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.item-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.item-name {
  font-size: 14px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.item-spec {
  font-size: 12px;
  color: $text-secondary;
}

.item-remark {
  font-size: 12px;
  color: $brand-color;
}

.item-right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  flex-shrink: 0;
  margin-left: 12px;
}

.item-qty {
  font-size: 13px;
  color: $text-secondary;
}

.item-price {
  font-size: 14px;
  font-weight: 600;
}

.amount-section {
  background: #fff;
  padding: 14px 16px;
  margin-bottom: 10px;
}

.amount-row {
  display: flex;
  justify-content: space-between;
  padding: 4px 0;
  font-size: 14px;
  color: $text-secondary;

  &.discount {
    color: $brand-color;
  }

  &.total {
    padding-top: 10px;
    margin-top: 6px;
    border-top: 1px solid $border-color;
    font-size: 15px;
    color: $text-color;
    font-weight: 600;
  }
}

.total-amount {
  color: $brand-color;
  font-size: 18px;
  font-weight: 700;
}

.cancel-section {
  padding: 20px 16px 40px;
  text-align: center;
}

.cancel-btn {
  background: #fff;
  color: $error-color;
  border: 1px solid $error-color;
  padding: 10px 40px;
  border-radius: 22px;
  font-size: 15px;

  &.disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}
</style>
