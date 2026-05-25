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
          <div class="stat-card highlight">
            <div class="stat-value">{{ stats.todayOrders }}</div>
            <div class="stat-label">今日订单</div>
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
        </div>
      </div>

      <div class="stats-section">
        <h3 class="section-title">商品数据</h3>
        <div class="stat-cards">
          <div class="stat-card">
            <div class="stat-value">{{ stats.totalProducts }}</div>
            <div class="stat-label">商品总数</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ stats.totalCategories }}</div>
            <div class="stat-label">分类总数</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.stats-page {
  min-height: 100vh;
  background: #f5f5f5;
}

.page-header {
  background: linear-gradient(135deg, #FF6B6B, #FF8E53);
  padding: 16px;

  h2 { margin: 0; font-size: 18px; font-weight: 700; color: #333; }
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
  border-radius: 12px;
  padding: 16px 12px;
  text-align: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

  &.active { border-top: 3px solid #4CAF50; }
  &.warning { border-top: 3px solid #FF9800; }
  &.highlight { border-top: 3px solid #FF6B6B; }

  .stat-value {
    font-size: 24px;
    font-weight: 800;
    color: #333;
    margin-bottom: 4px;

    &.revenue { color: #FF6B6B; }
  }

  .stat-label {
    font-size: 12px;
    color: #999;
  }
}
</style>