<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { dashboardApi } from '@/api/modules/dashboard'
import { orderApi } from '@/api/modules/order'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)

const overview = ref<any>({
  pendingOrderCount: 0,
  yesterdayOrderCount: 0,
  yesterdayRevenue: 0,
  todayOrderCount: 0,
  todayRevenue: 0,
  totalOrderCount: 0,
  totalRevenue: 0,
})

const recentOrders = ref<any[]>([])

const formatAmount = (amount: number) => {
  if (!amount) return '¥0.00'
  if (amount >= 10000) return `¥${(amount / 10000).toFixed(1)}万`
  return `¥${amount.toFixed(2)}`
}

const statusLabel = (status: string) => {
  const map: Record<string, string> = {
    pending: '待接单', accepted: '已接单', preparing: '制作中',
    ready: '待取餐', completed: '已完成', cancelled: '已取消'
  }
  return map[status] || status
}

const statusColor = (status: string) => {
  const map: Record<string, string> = {
    pending: '#087E6B', accepted: '#259D63', preparing: '#F7B731',
    ready: '#346AC3', completed: '#687872', cancelled: '#687872'
  }
  return map[status] || '#687872'
}

const formatTime = (dateStr: string) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`
}

const fetchOverview = async () => {
  if (!storeId) return
  try {
    overview.value = await dashboardApi.getOverview(storeId)
  } catch (e) { console.error('获取概览数据失败', e) }
}

const fetchRecentOrders = async () => {
  if (!storeId) return
  try {
    const orders = await orderApi.getByStoreId(storeId, { pageSize: 5 })
    recentOrders.value = orders || []
  } catch (e) { console.error('获取近期订单失败', e) }
}

const fetchAllData = async () => {
  loading.value = true
  await Promise.allSettled([fetchOverview(), fetchRecentOrders()])
  loading.value = false
}

onMounted(fetchAllData)
</script>

<template>
  <div class="dashboard-page">
    <div class="page-header">
      <h2 class="page-title">经营数据</h2>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div v-else class="dashboard-content">
      <div class="overview-cards">
        <div class="overview-card">
          <div class="overview-label">今日订单</div>
          <div class="overview-value">{{ overview.todayOrderCount || 0 }}</div>
        </div>
        <div class="overview-card">
          <div class="overview-label">今日营收</div>
          <div class="overview-value revenue">{{ formatAmount(overview.todayRevenue) }}</div>
        </div>
        <div class="overview-card">
          <div class="overview-label">昨日订单</div>
          <div class="overview-value">{{ overview.yesterdayOrderCount || 0 }}</div>
        </div>
        <div class="overview-card">
          <div class="overview-label">昨日营收</div>
          <div class="overview-value revenue">{{ formatAmount(overview.yesterdayRevenue) }}</div>
        </div>
        <div class="overview-card highlight">
          <div class="overview-label">待处理订单</div>
          <div class="overview-value">{{ overview.pendingOrderCount || 0 }}</div>
          <div class="overview-hint">需及时处理</div>
        </div>
        <div class="overview-card">
          <div class="overview-label">累计营收</div>
          <div class="overview-value revenue">{{ formatAmount(overview.totalRevenue) }}</div>
        </div>
      </div>

      <div class="chart-section">
        <h3 class="section-title">营收趋势</h3>
        <div class="chart-placeholder">
          <div class="chart-icon">📊</div>
          <p>ECharts 图表集成区域</p>
          <p class="chart-hint">将在此处渲染营收趋势折线图</p>
        </div>
      </div>

      <div class="chart-section">
        <h3 class="section-title">订单趋势</h3>
        <div class="chart-placeholder">
          <div class="chart-icon">📈</div>
          <p>ECharts 图表集成区域</p>
          <p class="chart-hint">将在此处渲染订单趋势柱状图</p>
        </div>
      </div>

      <div class="recent-section">
        <h3 class="section-title">近期订单</h3>
        <div v-if="recentOrders.length === 0" class="empty-state">暂无订单</div>
        <div v-else class="recent-list">
          <div v-for="order in recentOrders" :key="order.id" class="recent-item">
            <div class="recent-left">
              <span class="recent-code">#{{ order.pickupCode }}</span>
              <span class="recent-time">{{ formatTime(order.createdAt) }}</span>
            </div>
            <div class="recent-right">
              <span class="recent-amount">{{ formatAmount(order.actualAmount) }}</span>
              <span class="recent-status" :style="{ color: statusColor(order.status) }">
                {{ statusLabel(order.status) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.dashboard-page {
  min-height: 100vh;
  background: #F6F7F3;
}

.page-header {
  background: #fff;
  padding: 16px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;

  .page-title { margin: 0; font-size: 18px; font-weight: 800; color: #1F2A26; }
}

.loading-state { text-align: center; padding: 80px 0; color: #687872; }

.dashboard-content { padding: 12px; }

.overview-cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-bottom: 16px;
}

.overview-card {
  background: #FFFFFF;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);

  &.highlight {
    background: linear-gradient(135deg, #E7F4EF, #FFE4E1);
    border: 1px solid #BFE5DA;
  }

  .overview-label { font-size: 13px; color: #687872; margin-bottom: 8px; }
  .overview-value { font-size: 28px; font-weight: 800; color: #1F2A26; margin-bottom: 4px; }
  .overview-value.revenue { color: #087E6B; }
  .overview-hint { font-size: 12px; color: #087E6B; }
}

.chart-section {
  background: #FFFFFF;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 12px;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);

  .section-title { margin: 0 0 12px; font-size: 16px; font-weight: 600; color: #1F2A26; }
}

.chart-placeholder {
  text-align: center;
  padding: 40px 0;
  background: #FAFCFA;
  border-radius: 10px;
  border: 2px dashed #DCE6E1;

  .chart-icon { font-size: 36px; margin-bottom: 8px; }
  p { margin: 4px 0; color: #687872; font-size: 14px; }
  .chart-hint { font-size: 12px; color: #D9D9D9; }
}

.recent-section {
  background: #FFFFFF;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 12px;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);

  .section-title { margin: 0 0 12px; font-size: 16px; font-weight: 600; color: #1F2A26; }
}

.empty-state { text-align: center; padding: 40px 0; color: #687872; font-size: 14px; }

.recent-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #E2E8E3;

  &:last-child { border-bottom: none; }
}

.recent-left { display: flex; align-items: center; gap: 8px; }
.recent-code { font-size: 16px; font-weight: 700; color: #087E6B; }
.recent-time { font-size: 12px; color: #687872; }
.recent-right { display: flex; align-items: center; gap: 8px; }
.recent-amount { font-size: 14px; font-weight: 600; color: #1F2A26; }
.recent-status { font-size: 13px; font-weight: 600; }
</style>
