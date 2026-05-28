<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { adminApi } from '@/api/modules/admin'

const loading = ref(false)
const stats = ref<any>(null)

const formatAmount = (amount: number) => {
  if (!amount) return '¥0.00'
  if (amount >= 10000) return `¥${(amount / 10000).toFixed(1)}万`
  return `¥${amount.toFixed(2)}`
}

const loadStats = async () => {
  loading.value = true
  try {
    stats.value = await adminApi.getPlatformStats()
  } catch (e) { console.error('加载平台统计失败', e) }
  finally { loading.value = false }
}

onMounted(loadStats)
</script>

<template>
  <div class="stats-page">
    <div class="page-header">
      <h2>平台统计</h2>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div v-else-if="stats" class="stats-content">
      <div class="stats-section">
        <h3 class="section-title">商家概览</h3>
        <div class="stat-cards">
          <div class="stat-card">
            <div class="stat-value">{{ stats.totalMerchants }}</div>
            <div class="stat-label">商家总数</div>
          </div>
          <div class="stat-card active">
            <div class="stat-value">{{ stats.activeMerchants }}</div>
            <div class="stat-label">正常运营</div>
          </div>
          <div class="stat-card warning">
            <div class="stat-value">{{ stats.pendingMerchants }}</div>
            <div class="stat-label">待审核</div>
          </div>
          <div class="stat-card danger">
            <div class="stat-value">{{ stats.suspendedMerchants }}</div>
            <div class="stat-label">已封禁</div>
          </div>
        </div>
      </div>

      <div class="stats-section">
        <h3 class="section-title">店铺概览</h3>
        <div class="stat-cards">
          <div class="stat-card">
            <div class="stat-value">{{ stats.totalStores }}</div>
            <div class="stat-label">店铺总数</div>
          </div>
          <div class="stat-card active">
            <div class="stat-value">{{ stats.openStores }}</div>
            <div class="stat-label">营业中</div>
          </div>
          <div class="stat-card warning">
            <div class="stat-value">{{ stats.closedStores }}</div>
            <div class="stat-label">休息中</div>
          </div>
        </div>
      </div>

      <div class="stats-section">
        <h3 class="section-title">订单数据</h3>
        <div class="stat-cards">
          <div class="stat-card">
            <div class="stat-value">{{ stats.totalOrders }}</div>
            <div class="stat-label">订单总数</div>
          </div>
          <div class="stat-card active">
            <div class="stat-value">{{ stats.completedOrders }}</div>
            <div class="stat-label">已完成</div>
          </div>
          <div class="stat-card warning">
            <div class="stat-value">{{ stats.pendingOrders }}</div>
            <div class="stat-label">待处理</div>
          </div>
          <div class="stat-card highlight">
            <div class="stat-value">{{ stats.todayOrders }}</div>
            <div class="stat-label">今日订单</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ stats.completionRate }}%</div>
            <div class="stat-label">完成率</div>
          </div>
        </div>
      </div>

      <div class="stats-section">
        <h3 class="section-title">营收数据</h3>
        <div class="stat-cards">
          <div class="stat-card">
            <div class="stat-value revenue">{{ formatAmount(stats.totalRevenue) }}</div>
            <div class="stat-label">累计营收</div>
          </div>
          <div class="stat-card highlight">
            <div class="stat-value revenue">{{ formatAmount(stats.todayRevenue) }}</div>
            <div class="stat-label">今日营收</div>
          </div>
          <div class="stat-card">
            <div class="stat-value revenue">{{ formatAmount(stats.averageOrderAmount) }}</div>
            <div class="stat-label">平均客单价</div>
          </div>
        </div>
      </div>

      <div class="stats-section">
        <h3 class="section-title">商品数据</h3>
        <div class="stat-cards">
          <div class="stat-card">
            <div class="stat-value">{{ stats.totalProducts }}</div>
            <div class="stat-label">商品总数</div>
          </div>
          <div class="stat-card active">
            <div class="stat-value">{{ stats.onProducts }}</div>
            <div class="stat-label">上架商品</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ stats.totalCategories }}</div>
            <div class="stat-label">分类总数</div>
          </div>
          <div class="stat-card highlight">
            <div class="stat-value">{{ stats.activeDiscounts }}</div>
            <div class="stat-label">生效优惠</div>
          </div>
        </div>
      </div>

      <div class="stats-section">
        <h3 class="section-title">顾客数据</h3>
        <div class="stat-cards">
          <div class="stat-card active">
            <div class="stat-value">{{ stats.registeredCustomers }}</div>
            <div class="stat-label">注册顾客</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ stats.anonymousCustomers }}</div>
            <div class="stat-label">匿名设备</div>
          </div>
        </div>
      </div>

      <div class="stats-section" v-if="stats.last7DaysOrders?.length">
        <h3 class="section-title">近 7 天趋势</h3>
        <div class="trend-list">
          <div v-for="day in stats.last7DaysOrders" :key="day.date" class="trend-row">
            <span>{{ String(day.date).slice(0, 10) }}</span>
            <strong>{{ day.orders }} 单</strong>
            <small>{{ formatAmount(day.revenue) }}</small>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.stats-page {
  min-height: 100vh;
  background: #f6f7f3;
}

.page-header {
  background: #fff;
  padding: 16px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;

  h2 { margin: 0; font-size: 18px; font-weight: 800; color: #1F2A26; }
}

.loading-state { text-align: center; padding: 80px 0; color: #999; }

.stats-content { padding: 16px; }

.stats-section { margin-bottom: 20px; }

.section-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 12px;
}

.stat-cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}

.stat-card {
  background: #fff;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  padding: 16px 12px;
  text-align: center;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);

  &.active { border-top: 3px solid #4CAF50; }
  &.warning { border-top: 3px solid #F7B731; }
  &.highlight { border-top: 3px solid #087E6B; }
  &.danger { border-top: 3px solid #D94C4C; }

  .stat-value {
    font-size: 24px;
    font-weight: 800;
    color: #333;
    margin-bottom: 4px;

    &.revenue { color: #087E6B; }
  }

  .stat-label {
    font-size: 12px;
    color: #999;
  }
}

.trend-list {
  display: grid;
  gap: 8px;
}

.trend-row {
  min-height: 42px;
  padding: 8px 12px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto auto;
  gap: 12px;
  align-items: center;
  background: #fff;
  color: #687872;

  strong { color: #1F2A26; }
  small { color: #087E6B; font-weight: 800; }
}

@media (max-width: 760px) {
  .stat-cards {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .trend-row {
    grid-template-columns: 1fr;
    gap: 4px;
  }
}
</style>
