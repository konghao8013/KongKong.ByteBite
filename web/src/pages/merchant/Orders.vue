<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { orderApi } from '@/api/modules/order'
import { useSignalR } from '@/composables/useSignalR'

const storeId = localStorage.getItem('merchant_store_id') || ''
const { connection, connect, disconnect } = useSignalR('/hubs/store')
const loading = ref(false)
const orders = ref<any[]>([])
const activeTab = ref('pending')
const selectedOrder = ref<any>(null)
const showDetail = ref(false)
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
    pending: '#087E6B', accepted: '#259D63', preparing: '#F7B731',
    ready: '#346AC3', completed: '#687872', cancelled: '#687872', rejected: '#D94C4C'
  }
  return map[status] || '#687872'
}

const diningModeLabel = (mode: string) => {
  const map: Record<string, string> = { dine_in: '堂食', takeaway: '打包', delivery: '外卖' }
  return map[mode] || mode
}

const diningModeIcon = (mode: string) => {
  const map: Record<string, string> = { dine_in: '🪑', takeaway: '🥡', delivery: '🛵' }
  return map[mode] || '🍽️'
}

const formatTime = (dateStr: string) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getMonth() + 1}/${d.getDate()} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`
}

const formatFullTime = (dateStr: string) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}:${d.getSeconds().toString().padStart(2, '0')}`
}

const openDetail = (order: any) => {
  selectedOrder.value = order
  showDetail.value = true
}

const closeDetail = () => {
  showDetail.value = false
  selectedOrder.value = null
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
onMounted(async () => {
  await loadOrders()
  try {
    await connect()
    connection.value?.on('NewOrder', (order: any) => {
      if (!orders.value.some(o => o.id === order.id)) orders.value.unshift(order)
    })
    connection.value?.on('OrderStatusUpdated', (payload: { orderId: string; status: string }) => {
      const target = orders.value.find(o => o.id === payload.orderId)
      if (target) target.status = payload.status
    })
    connection.value?.on('OrderCancelled', (payload: { orderId: string }) => {
      const target = orders.value.find(o => o.id === payload.orderId)
      if (target) target.status = 'cancelled'
    })
    if (storeId) await connection.value?.invoke('SubscribeStore', storeId)
  } catch {
  }
  refreshTimer = setInterval(loadOrders, 15000)
})
onUnmounted(async () => {
  if (refreshTimer) clearInterval(refreshTimer)
  try {
    if (storeId) await connection.value?.invoke('UnsubscribeStore', storeId)
  } catch {
  }
  await disconnect()
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
      <div v-for="order in filteredOrders" :key="order.id" class="order-card" @click="openDetail(order)">
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
          <div v-for="item in (order.orderItems || []).slice(0, 3)" :key="item.id" class="order-item">
            <span class="item-name">{{ item.productName }}</span>
            <span class="item-qty">×{{ item.quantity }}</span>
            <span class="item-price">¥{{ item.totalPrice }}</span>
          </div>
          <div v-if="(order.orderItems || []).length > 3" class="item-more">
            还有 {{ (order.orderItems || []).length - 3 }} 件商品...
          </div>
        </div>

        <div class="order-footer">
          <div class="order-amount">
            <span class="amount-label">合计</span>
            <span class="amount-value">¥{{ order.actualAmount }}</span>
            <span v-if="order.discountAmount > 0" class="discount-info">优惠 -¥{{ order.discountAmount }}</span>
          </div>

          <div class="order-actions" @click.stop>
            <button class="btn-detail" @click="openDetail(order)">详情</button>
            <button v-if="order.status === 'pending'" class="btn-accept" @click="handleAccept(order)">接单</button>
            <button v-if="order.status === 'pending'" class="btn-reject" @click="handleReject(order)">拒单</button>
            <button v-if="order.status === 'accepted'" class="btn-prepare" @click="handlePrepare(order)">开始制作</button>
            <button v-if="order.status === 'preparing'" class="btn-ready" @click="handleReady(order)">制作完成</button>
            <button v-if="order.status === 'ready'" class="btn-complete" @click="handleComplete(order)">核销</button>
          </div>
        </div>

        <div v-if="order.remark" class="order-remark">备注: {{ order.remark }}</div>
      </div>
    </div>

    <!-- 订单详情弹窗 -->
    <div v-if="showDetail && selectedOrder" class="detail-overlay" @click.self="closeDetail">
      <div class="detail-panel">
        <div class="detail-header">
          <div class="detail-title-row">
            <span class="detail-pickup-code">#{{ selectedOrder.pickupCode }}</span>
            <span class="detail-status" :style="{ color: statusColor(selectedOrder.status) }">
              {{ statusLabel(selectedOrder.status) }}
            </span>
          </div>
          <button class="detail-close" @click="closeDetail">✕</button>
        </div>

        <!-- 就餐方式 -->
        <div class="detail-section">
          <div class="section-title">{{ diningModeIcon(selectedOrder.diningMode) }} 就餐方式</div>
          <div class="dining-info">
            <span class="dining-mode-tag">{{ diningModeLabel(selectedOrder.diningMode) }}</span>
            <span v-if="selectedOrder.tableNo" class="dining-detail">桌号: {{ selectedOrder.tableNo }}</span>
            <span v-if="selectedOrder.diningMode === 'delivery' && selectedOrder.deliveryAddress" class="dining-detail">
              {{ selectedOrder.deliveryAddress }}
            </span>
            <span v-if="selectedOrder.diningMode === 'delivery' && selectedOrder.deliveryPhone" class="dining-detail">
              📞 {{ selectedOrder.deliveryPhone }}
            </span>
          </div>
        </div>

        <!-- 配送信息（外卖时突出显示） -->
        <div v-if="selectedOrder.diningMode === 'delivery' && selectedOrder.deliveryAddress" class="detail-section delivery-section">
          <div class="section-title">🛵 配送信息</div>
          <div class="delivery-card">
            <div class="delivery-row">
              <span class="delivery-label">配送地址</span>
              <span class="delivery-value">{{ selectedOrder.deliveryAddress }}</span>
            </div>
            <div v-if="selectedOrder.deliveryPhone" class="delivery-row">
              <span class="delivery-label">联系电话</span>
              <a :href="'tel:' + selectedOrder.deliveryPhone" class="delivery-phone">{{ selectedOrder.deliveryPhone }}</a>
            </div>
          </div>
        </div>

        <!-- 商品明细 -->
        <div class="detail-section">
          <div class="section-title">🍽️ 商品明细</div>
          <div class="detail-items">
            <div v-for="item in (selectedOrder.orderItems || [])" :key="item.id" class="detail-item">
              <div class="detail-item-main">
                <span class="detail-item-name">{{ item.productName }}</span>
                <span class="detail-item-spec" v-if="item.specOptions">（{{ item.specOptions }}）</span>
              </div>
              <div class="detail-item-right">
                <span class="detail-item-qty">×{{ item.quantity }}</span>
                <span class="detail-item-price">¥{{ item.totalPrice }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 金额明细 -->
        <div class="detail-section">
          <div class="section-title">💰 金额明细</div>
          <div class="amount-rows">
            <div class="amount-row">
              <span>商品合计</span>
              <span>¥{{ selectedOrder.totalAmount }}</span>
            </div>
            <div v-if="selectedOrder.discountAmount > 0" class="amount-row discount">
              <span>优惠减免</span>
              <span>-¥{{ selectedOrder.discountAmount }}</span>
            </div>
            <div v-if="selectedOrder.packingFee > 0" class="amount-row">
              <span>打包费</span>
              <span>¥{{ selectedOrder.packingFee }}</span>
            </div>
            <div class="amount-row total">
              <span>实付金额</span>
              <span>¥{{ selectedOrder.actualAmount }}</span>
            </div>
          </div>
        </div>

        <!-- 备注 -->
        <div v-if="selectedOrder.remark" class="detail-section">
          <div class="section-title">📝 备注</div>
          <div class="detail-remark">{{ selectedOrder.remark }}</div>
        </div>

        <!-- 拒单原因 -->
        <div v-if="selectedOrder.rejectReason" class="detail-section">
          <div class="section-title">❌ 拒单原因</div>
          <div class="detail-reject">{{ selectedOrder.rejectReason }}</div>
        </div>

        <!-- 订单时间线 -->
        <div class="detail-section">
          <div class="section-title">⏱️ 订单时间</div>
          <div class="timeline">
            <div class="timeline-item">
              <span class="timeline-label">下单时间</span>
              <span class="timeline-value">{{ formatFullTime(selectedOrder.createdAt) }}</span>
            </div>
            <div v-if="selectedOrder.acceptedAt" class="timeline-item">
              <span class="timeline-label">接单时间</span>
              <span class="timeline-value">{{ formatFullTime(selectedOrder.acceptedAt) }}</span>
            </div>
            <div v-if="selectedOrder.preparingAt" class="timeline-item">
              <span class="timeline-label">制作时间</span>
              <span class="timeline-value">{{ formatFullTime(selectedOrder.preparingAt) }}</span>
            </div>
            <div v-if="selectedOrder.readyAt" class="timeline-item">
              <span class="timeline-label">完成时间</span>
              <span class="timeline-value">{{ formatFullTime(selectedOrder.readyAt) }}</span>
            </div>
            <div v-if="selectedOrder.completedAt" class="timeline-item">
              <span class="timeline-label">核销时间</span>
              <span class="timeline-value">{{ formatFullTime(selectedOrder.completedAt) }}</span>
            </div>
            <div v-if="selectedOrder.cancelledAt" class="timeline-item">
              <span class="timeline-label">取消时间</span>
              <span class="timeline-value">{{ formatFullTime(selectedOrder.cancelledAt) }}</span>
            </div>
          </div>
        </div>

        <!-- 操作按钮 -->
        <div class="detail-actions">
          <button v-if="selectedOrder.status === 'pending'" class="btn-accept full" @click="handleAccept(selectedOrder); closeDetail()">接单</button>
          <button v-if="selectedOrder.status === 'pending'" class="btn-reject full" @click="handleReject(selectedOrder); closeDetail()">拒单</button>
          <button v-if="selectedOrder.status === 'accepted'" class="btn-prepare full" @click="handlePrepare(selectedOrder); closeDetail()">开始制作</button>
          <button v-if="selectedOrder.status === 'preparing'" class="btn-ready full" @click="handleReady(selectedOrder); closeDetail()">制作完成</button>
          <button v-if="selectedOrder.status === 'ready'" class="btn-complete full" @click="handleComplete(selectedOrder); closeDetail()">核销完成</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.orders-page {
  padding: 16px;
  background: #F6F7F3;
  min-height: 100vh;
  color: #1F2A26;
}

.orders-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;

  h2 { font-size: 20px; font-weight: 700; margin: 0; color: #1F2A26; }

  .pending-badge {
    background: linear-gradient(135deg, #087E6B, #0EA389);
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
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);
}

.status-tab {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 8px 4px;
  border-radius: 8px;
  font-size: 12px;
  color: #687872;
  transition: all 0.25s;
  position: relative;

  &.active {
    background: #087E6B;
    color: #fff;
    box-shadow: 0 2px 8px rgba(8, 126, 107, 0.22);
  }

  .tab-icon { font-size: 16px; }
  .tab-label { font-size: 11px; }
  .tab-count {
    position: absolute;
    top: 2px;
    right: 2px;
    background: #087E6B;
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
  color: #687872;

  .empty-icon { font-size: 48px; display: block; margin-bottom: 12px; }
}

.orders-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.order-card {
  background: #FFFFFF;
  border: 1px solid #E2E8E3;
  border-left: 4px solid #087E6B;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);
  cursor: pointer;
  transition: box-shadow 0.2s;

  &:active { box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08); }
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;

  .order-info { display: flex; align-items: center; gap: 8px; }

  .order-no { font-size: 18px; font-weight: 800; color: #1F2A26; }

  .order-mode {
    font-size: 12px;
    padding: 2px 8px;
    border-radius: 4px;
    background: #E7F4EF;
    color: #087E6B;
  }

  .order-table { font-size: 12px; color: #346AC3; font-weight: 500; }
  .order-status { font-size: 14px; font-weight: 600; }
}

.order-time { font-size: 12px; color: #687872; margin-bottom: 8px; }

.order-items {
  margin-bottom: 12px;

  .order-item {
    display: flex;
    align-items: center;
    padding: 3px 0;
    font-size: 14px;
    color: #333;

    .item-name { flex: 1; }
    .item-qty { color: #687872; margin-right: 8px; }
    .item-price { color: #FF6B4A; font-weight: 600; }
  }

  .item-more { font-size: 12px; color: #9AA9A3; padding-top: 2px; }
}

.order-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px solid #E2E8E3;

  .order-amount {
    .amount-label { font-size: 12px; color: #687872; }
    .amount-value { font-size: 18px; font-weight: 700; color: #087E6B; }
    .discount-info { font-size: 12px; color: #259D63; margin-left: 8px; }
  }

  .order-actions { display: flex; gap: 8px; }
}

.btn-accept, .btn-prepare, .btn-ready, .btn-complete, .btn-detail {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  border: none;
  cursor: pointer;
}

.btn-detail { background: #F6F7F3; color: #1F2A26; }
.btn-accept { background: #087E6B; color: #fff; }
.btn-reject { background: #E7F4EF; color: #D94C4C; padding: 8px 16px; border-radius: 8px; font-size: 13px; font-weight: 600; border: none; cursor: pointer; }
.btn-prepare { background: #FFF6DB; color: #F7B731; }
.btn-ready { background: #EAF1FF; color: #346AC3; }
.btn-complete { background: #087E6B; color: #fff; }

.order-remark {
  margin-top: 8px;
  padding: 8px 12px;
  background: #FFF6DB;
  border-radius: 8px;
  font-size: 13px;
  color: #F7B731;
}

/* 详情弹窗 */
.detail-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  z-index: 200;
  display: flex;
  align-items: flex-end;
  justify-content: center;
}

.detail-panel {
  background: #FFFFFF;
  border-radius: 20px 20px 0 0;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  padding: 0 0 24px;
}

.detail-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 20px 12px;
  border-bottom: 1px solid #E2E8E3;
  position: sticky;
  top: 0;
  background: #FFFFFF;
  border-radius: 20px 20px 0 0;
  z-index: 1;

  .detail-title-row { display: flex; align-items: center; gap: 10px; }
  .detail-pickup-code { font-size: 22px; font-weight: 800; color: #087E6B; }
  .detail-status { font-size: 15px; font-weight: 600; }

  .detail-close {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: #F6F7F3;
    border: none;
    font-size: 16px;
    color: #687872;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

.detail-section {
  padding: 14px 20px 0;

  .section-title {
    font-size: 14px;
    font-weight: 600;
    color: #1F2A26;
    margin-bottom: 10px;
  }
}

.dining-info {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;

  .dining-mode-tag {
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 13px;
    font-weight: 600;
    background: #E7F4EF;
    color: #087E6B;
  }

  .dining-detail {
    font-size: 13px;
    color: #1F2A26;
  }
}

.delivery-section {
  .delivery-card {
    background: #FFF6DB;
    border-radius: 12px;
    padding: 14px;
    border: 1px solid #FFE0B2;
  }

  .delivery-row {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    margin-bottom: 6px;

    &:last-child { margin-bottom: 0; }
  }

  .delivery-label {
    font-size: 13px;
    color: #687872;
    white-space: nowrap;
    min-width: 60px;
  }

  .delivery-value {
    font-size: 14px;
    font-weight: 600;
    color: #1F2A26;
  }

  .delivery-phone {
    font-size: 14px;
    font-weight: 600;
    color: #087E6B;
    text-decoration: none;
  }
}

.detail-items {
  .detail-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom: 1px solid #FAFCFA;

    &:last-child { border-bottom: none; }
  }

  .detail-item-main {
    flex: 1;
    display: flex;
    align-items: baseline;
    gap: 4px;
  }

  .detail-item-name { font-size: 14px; color: #1F2A26; font-weight: 500; }
  .detail-item-spec { font-size: 12px; color: #687872; }
  .detail-item-right { display: flex; align-items: center; gap: 12px; }
  .detail-item-qty { font-size: 13px; color: #687872; }
  .detail-item-price { font-size: 14px; color: #087E6B; font-weight: 600; min-width: 60px; text-align: right; }
}

.amount-rows {
  background: #FAFCFA;
  border-radius: 10px;
  padding: 12px;

  .amount-row {
    display: flex;
    justify-content: space-between;
    padding: 5px 0;
    font-size: 14px;
    color: #595959;

    &.discount { color: #259D63; }
    &.total { font-size: 16px; font-weight: 700; color: #087E6B; border-top: 1px solid #DCE6E1; padding-top: 10px; margin-top: 4px; }
  }
}

.detail-remark {
  background: #FFF6DB;
  border-radius: 10px;
  padding: 12px;
  font-size: 14px;
  color: #F7B731;
  line-height: 1.6;
}

.detail-reject {
  background: #E7F4EF;
  border-radius: 10px;
  padding: 12px;
  font-size: 14px;
  color: #D94C4C;
  line-height: 1.6;
}

.timeline {
  .timeline-item {
    display: flex;
    justify-content: space-between;
    padding: 5px 0;
    font-size: 13px;

    .timeline-label { color: #687872; }
    .timeline-value { color: #595959; }
  }
}

.detail-actions {
  padding: 20px;
  display: flex;
  gap: 10px;

  .full {
    flex: 1;
    padding: 14px;
    border-radius: 12px;
    font-size: 16px;
    font-weight: 700;
    border: none;
    cursor: pointer;
    text-align: center;
  }
}
</style>
