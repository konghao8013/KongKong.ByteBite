<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { orderApi } from '@/api/modules/order'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const orders = ref<any[]>([])
const activeTab = ref('pending')
const tabs = [
  { key: 'pending', label: '待接单', icon: '🔔' },
  { key: 'accepted', label: '已接单', icon: '✅' },
  { key: 'preparing', label: '制作中', icon: '🔥' },
  { key: 'ready', label: '待取餐', icon: '📦' },
  { key: 'completed', label: '已完成', icon: '✨' },
]

const filteredOrders = computed(() => {
  if (activeTab.value === 'all') return orders.value
  return orders.value.filter(o => o.status === activeTab.value)
})

const pendingCount = computed(() => orders.value.filter(o => o.status === 'pending').length)

const statusLabel = (status: string) => {
  const map: Record<string, string> = {
    pending: '待接单', accepted: '已接单', preparing: '制作中',
    ready: '待取餐', completed: '已完成', cancelled: '已取消', rejected: '已拒单'
  }
  return map[status] || status
}

const statusColor = (status: string) => {
  const map: Record<string, string> = {
    pending: '#FF6B6B', accepted: '#52C41A', preparing: '#FF9800',
    ready: '#1890FF', completed: '#8C8C8C', cancelled: '#8C8C8C', rejected: '#FF4D4F'
  }
  return map[status] || '#8C8C8C'
}

const diningModeLabel = (mode: string) => {
  const map: Record<string, string> = { dine_in: '堂食', takeaway: '打包', delivery: '外卖' }
  return map[mode] || mode
}

const formatTime = (dateStr: string) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`
}

const loadOrders = async () => {
  if (!storeId) return
  loading.value = true
  try {
    const res = await orderApi.getByStoreId(storeId, { pageSize: 50 })
    orders.value = res || []
  } catch (e) {
    console.error('加载订单失败', e)
  } finally {
    loading.value = false
  }
}

const handleAccept = async (order: any) => {
  try {
    await orderApi.acceptOrder(order.id)
    order.status = 'accepted'
    order.acceptedAt = new Date().toISOString()
  } catch (e) { console.error('接单失败', e) }
}

const handleReject = async (order: any) => {
  const reason = prompt('请输入拒单原因')
  if (!reason) return
  try {
    await orderApi.rejectOrder(order.id, reason)
    order.status = 'rejected'
    order.rejectReason = reason
  } catch (e) { console.error('拒单失败', e) }
}

const handlePrepare = async (order: any) => {
  try {
    await orderApi.startPreparing(order.id)
    order.status = 'preparing'
  } catch (e) { console.error('操作失败', e) }
}

const handleReady = async (order: any) => {
  try {
    await orderApi.markReady(order.id)
    order.status = 'ready'
  } catch (e) { console.error('操作失败', e) }
}

const handleComplete = async (order: any) => {
  try {
    await orderApi.completeOrder(order.id)
    order.status = 'completed'
  } catch (e) { console.error('操作失败', e) }
}

let refreshTimer: any = null
onMounted(() => {
  loadOrders()
  refreshTimer = setInterval(loadOrders, 15000)
})
onUnmounted(() => {
  if (refreshTimer) clearInterval(refreshTimer)
})
</script>

<template>
  <div class="orders-page">
    <div class="orders-header">
      <h2>订单管理</h2>
      <div v-if="pendingCount > 0" class="pending-badge">{{ pendingCount }} 待接单</div>
    </div>

    <div class="status-tabs">
      <div
        v-for="tab in tabs"
        :key="tab.key"
        class="status-tab"
        :class="{ active: activeTab === tab.key }"
        @click="activeTab = tab.key"
      >
        <span class="tab-icon">{{ tab.icon }}</span>
        <span class="tab-label">{{ tab.label }}</span>
        <span v-if="tab.key === 'pending' && pendingCount > 0" class="tab-count">{{ pendingCount }}</span>
      </div>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div v-else-if="filteredOrders.length === 0" class="empty-state">
      <span class="empty-icon">📋</span>
      <p>暂无订单</p>
    </div>

    <div v-else class="orders-list">
      <div v-for="order in filteredOrders" :key="order.id" class="order-card">
        <div class="order-header">
          <div class="order-info">
            <span class="order-no">#{{ order.pickupCode }}</span>
            <span class="order-mode">{{ diningModeLabel(order.diningMode) }}</span>
            <span v-if="order.tableNo" class="order-table">桌号: {{ order.tableNo }}</span>
          </div>
          <div class="order-status" :style="{ color: statusColor(order.status) }">
            {{ statusLabel(order.status) }}
          </div>
        </div>

        <div class="order-time">{{ formatTime(order.createdAt) }}</div>

        <div class="order-items">
          <div v-for="item in (order.orderItems || [])" :key="item.id" class="order-item">
            <span class="item-name">{{ item.productName }}</span>
            <span class="item-qty">×{{ item.quantity }}</span>
            <span class="item-price">¥{{ item.totalPrice }}</span>
          </div>
        </div>

        <div class="order-footer">
          <div class="order-amount">
            <span class="amount-label">合计</span>
            <span class="amount-value">¥{{ order.actualAmount }}</span>
            <span v-if="order.discountAmount > 0" class="discount-info">优惠 -¥{{ order.discountAmount }}</span>
          </div>

          <div class="order-actions">
            <button v-if="order.status === 'pending'" class="btn-accept" @click="handleAccept(order)">接单</button>
            <button v-if="order.status === 'pending'" class="btn-reject" @click="handleReject(order)">拒单</button>
            <button v-if="order.status === 'accepted'" class="btn-prepare" @click="handlePrepare(order)">开始制作</button>
            <button v-if="order.status === 'preparing'" class="btn-ready" @click="handleReady(order)">制作完成</button>
            <button v-if="order.status === 'ready'" class="btn-complete" @click="handleComplete(order)">核销完成</button>
          </div>
        </div>

        <div v-if="order.remark" class="order-remark">备注: {{ order.remark }}</div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.orders-page {
  padding: 16px;
  background: #F7F7F7;
  min-height: 100vh;
  color: #1A1A2E;
}

.orders-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;

  h2 { font-size: 20px; font-weight: 700; margin: 0; color: #1A1A2E; }

  .pending-badge {
    background: linear-gradient(135deg, #FF6B6B, #FF8E53);
    color: #fff;
    padding: 5px 14px;
    border-radius: 16px;
    font-size: 13px;
    font-weight: 600;
  }
}

.status-tabs {
  display: flex;
  gap: 6px;
  margin-bottom: 16px;
  background: #FFFFFF;
  padding: 6px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.status-tab {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 8px 4px;
  border-radius: 8px;
  font-size: 12px;
  color: #8C8C8C;
  transition: all 0.25s;
  position: relative;

  &.active {
    background: linear-gradient(135deg, #FF6B6B, #FF8E53);
    color: #fff;
    box-shadow: 0 2px 8px rgba(255, 107, 107, 0.3);
  }

  .tab-icon { font-size: 16px; }
  .tab-label { font-size: 11px; }
  .tab-count {
    position: absolute;
    top: 2px;
    right: 2px;
    background: #FF6B6B;
    color: #fff;
    font-size: 10px;
    padding: 1px 5px;
    border-radius: 8px;
    font-weight: 600;
  }
}

.loading-state, .empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #8C8C8C;

  .empty-icon { font-size: 48px; display: block; margin-bottom: 12px; }
}

.orders-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.order-card {
  background: #FFFFFF;
  border-radius: 12px;
  padding: 16px;
  border-left: 4px solid #FF6B6B;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;

  .order-info { display: flex; align-items: center; gap: 8px; }

  .order-no {
    font-size: 18px;
    font-weight: 700;
    color: #FF6B6B;
  }

  .order-mode {
    font-size: 12px;
    padding: 2px 8px;
    border-radius: 4px;
    background: #FFF1F0;
    color: #FF6B6B;
  }

  .order-table { font-size: 12px; color: #1890FF; font-weight: 500; }

  .order-status { font-size: 14px; font-weight: 600; }
}

.order-time {
  font-size: 12px;
  color: #8C8C8C;
  margin-bottom: 8px;
}

.order-items {
  margin-bottom: 12px;

  .order-item {
    display: flex;
    align-items: center;
    padding: 4px 0;
    font-size: 14px;
    color: #333;

    .item-name { flex: 1; }
    .item-qty { color: #8C8C8C; margin-right: 8px; }
    .item-price { color: #FF6B6B; font-weight: 500; }
  }
}

.order-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px solid #F0F0F0;

  .order-amount {
    .amount-label { font-size: 12px; color: #8C8C8C; }
    .amount-value { font-size: 18px; font-weight: 700; color: #FF6B6B; }
    .discount-info { font-size: 12px; color: #52C41A; margin-left: 8px; }
  }

  .order-actions { display: flex; gap: 8px; }
}

.btn-accept, .btn-prepare, .btn-ready, .btn-complete {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  border: none;
  cursor: pointer;
}

.btn-accept { background: linear-gradient(135deg, #FF6B6B, #FF8E53); color: #fff; }
.btn-reject { background: #F0F0F0; color: #FF4D4F; padding: 8px 16px; border-radius: 8px; font-size: 13px; font-weight: 600; border: none; cursor: pointer; }
.btn-prepare { background: #FFF7E6; color: #FF9800; }
.btn-ready { background: #E6F7FF; color: #1890FF; }
.btn-complete { background: linear-gradient(135deg, #FFBE0B, #FF6B6B); color: #fff; }

.order-remark {
  margin-top: 8px;
  padding: 8px 12px;
  background: #FFF7E6;
  border-radius: 8px;
  font-size: 13px;
  color: #FF9800;
}
</style>