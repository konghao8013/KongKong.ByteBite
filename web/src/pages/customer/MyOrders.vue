<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useOrderStore } from '@/stores/modules/useOrderStore'
import { formatPrice, formatDate } from '@/utils/format'
import EmptyState from '@/components/common/EmptyState.vue'

const router = useRouter()
const orderStore = useOrderStore()

const statusMap: Record<string, { label: string; color: string; bgColor: string }> = {
  pending: { label: '待接单', color: '#FF6633', bgColor: '#fff5f0' },
  accepted: { label: '已接单', color: '#1890ff', bgColor: '#e6f7ff' },
  preparing: { label: '制作中', color: '#fa8c16', bgColor: '#fff7e6' },
  ready: { label: '备餐完毕', color: '#52c41a', bgColor: '#f6ffed' },
  completed: { label: '已完成', color: '#999', bgColor: '#f5f5f5' },
  cancelled: { label: '已取消', color: '#ff4d4f', bgColor: '#fff1f0' },
  rejected: { label: '已拒单', color: '#ff4d4f', bgColor: '#fff1f0' }
}

const getStatusInfo = (status: string) =>
  statusMap[status] || { label: status, color: '#999', bgColor: '#f5f5f5' }

const goBack = () => {
  router.back()
}

const goToDetail = (orderId: string) => {
  router.push({ name: 'OrderDetail', params: { orderId } })
}

const goToMenu = () => {
  router.push({ name: 'StoreMenu' })
}

const hasOrders = computed(() =>
  orderStore.activeOrders.length > 0 || orderStore.orderHistory.length > 0
)
</script>

<template>
  <div class="my-orders-page">
    <!-- 顶部导航 -->
    <div class="page-header">
      <span class="back-btn" @click="goBack">←</span>
      <h1 class="page-title">我的订单</h1>
      <span />
    </div>

    <!-- 空状态 -->
    <EmptyState v-if="!hasOrders" description="暂无订单记录" icon="📋">
      <button @click="goToMenu">去点单</button>
    </EmptyState>

    <!-- 进行中的订单 -->
    <div v-if="orderStore.activeOrders.length > 0" class="order-section">
      <div class="section-header">进行中</div>
      <div
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
          <span class="pickup-label">取货码</span>
          <span class="pickup-code">{{ order.pickupCode }}</span>
        </div>
        <div class="card-items">
          <span class="items-summary">
            {{ order.items.map((i) => i.productName).join('、') }}
          </span>
        </div>
        <div class="card-footer">
          <span class="order-time">{{ formatDate(order.createdAt) }}</span>
          <span class="order-amount">{{ formatPrice(order.actualAmount) }}</span>
        </div>
      </div>
    </div>

    <!-- 已完成的订单 -->
    <div v-if="orderStore.orderHistory.length > 0" class="order-section">
      <div class="section-header">已完成</div>
      <div
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
          <span class="items-summary">
            {{ order.items.map((i) => i.productName).join('、') }}
          </span>
        </div>
        <div class="card-footer">
          <span class="order-time">{{ formatDate(order.createdAt) }}</span>
          <span class="order-amount">{{ formatPrice(order.actualAmount) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.my-orders-page {
  min-height: 100%;
}

.page-header {
  background: $primary-color;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 10;
}

.back-btn {
  font-size: 22px;
  cursor: pointer;
  width: 32px;
}

.page-title {
  font-size: 17px;
  font-weight: 700;
}

.order-section {
  padding: 0 16px;
  margin-top: 12px;
}

.section-header {
  font-size: 14px;
  font-weight: 600;
  color: $text-secondary;
  padding: 8px 0;
}

.order-card {
  background: #fff;
  border-radius: 10px;
  padding: 14px 16px;
  margin-bottom: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  cursor: pointer;
  transition: transform 0.15s;

  &:active {
    transform: scale(0.98);
  }

  &.completed {
    opacity: 0.75;
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.store-name {
  font-size: 16px;
  font-weight: 600;
}

.order-status {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 10px;
  font-weight: 500;
}

.card-pickup {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #f9f9f9;
  padding: 8px 12px;
  border-radius: 8px;
  margin-bottom: 8px;
}

.pickup-label {
  font-size: 12px;
  color: $text-secondary;
}

.pickup-code {
  font-size: 20px;
  font-weight: 800;
  color: $brand-color;
  letter-spacing: 4px;
}

.card-items {
  margin-bottom: 8px;
}

.items-summary {
  font-size: 13px;
  color: $text-secondary;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  display: block;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.order-time {
  font-size: 12px;
  color: #bbb;
}

.order-amount {
  font-size: 16px;
  font-weight: 700;
}
</style>
