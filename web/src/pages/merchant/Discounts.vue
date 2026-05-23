<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { discountApi } from '@/api/modules/discount'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const discounts = ref<any[]>([])

const fetchDiscounts = async () => {
  if (!storeId) return
  loading.value = true
  try {
    discounts.value = await discountApi.getByStoreId(storeId) || []
  } catch (e) { console.error('加载优惠活动失败', e) }
  finally { loading.value = false }
}

const toggleStatus = async (rule: any) => {
  const newStatus = rule.status === 'active' ? 'inactive' : 'active'
  try {
    await discountApi.update(rule.id, { status: newStatus })
    rule.status = newStatus
  } catch (e) { console.error('操作失败', e) }
}

const handleDelete = async (rule: any) => {
  if (!confirm(`确定删除优惠活动「${rule.name}」吗？`)) return
  try {
    await discountApi.delete(rule.id)
    await fetchDiscounts()
  } catch (e) { console.error('删除失败', e) }
}

const getDiscountDesc = (rule: any) => {
  if (rule.discountType === 'full_reduction') return `满${rule.thresholdAmount}减${rule.discountAmount}`
  if (rule.discountType === 'discount') return `${(rule.discountRate || 0) / 10}折`
  return ''
}

const isActive = (rule: any) => rule.status === 'active'
const isExpired = (rule: any) => rule.endTime && new Date(rule.endTime) < new Date()

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return `${d.getMonth() + 1}/${d.getDate()} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`
}

onMounted(fetchDiscounts)
</script>

<template>
  <div class="discounts-page">
    <div class="page-header">
      <h2>优惠活动</h2>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div v-else-if="discounts.length === 0" class="empty-state">
      <span class="empty-icon">🎁</span>
      <p>暂无优惠活动</p>
    </div>

    <div v-else class="discount-list">
      <div v-for="rule in discounts" :key="rule.id" class="discount-card" :class="{ inactive: !isActive(rule), expired: isExpired(rule) }">
        <div class="discount-header">
          <span class="discount-type" :class="rule.discountType">
            {{ rule.discountType === 'full_reduction' ? '满减' : '折扣' }}
          </span>
          <span class="discount-status" :class="{ active: isActive(rule), expired: isExpired(rule) }">
            {{ isActive(rule) ? '生效中' : isExpired(rule) ? '已过期' : '已停用' }}
          </span>
        </div>
        <div class="discount-body">
          <h4 class="discount-name">{{ rule.name }}</h4>
          <div class="discount-desc">{{ getDiscountDesc(rule) }}</div>
          <div class="discount-scope">适用: {{ rule.applyScope === 'all' ? '全店' : rule.applyScope === 'category' ? '指定分类' : '指定商品' }}</div>
          <div class="discount-time">{{ formatDate(rule.startTime) }} ~ {{ formatDate(rule.endTime) }}</div>
        </div>
        <div class="discount-footer">
          <span class="discount-usage">已使用 {{ rule.usedCount || 0 }} 次</span>
          <div class="discount-actions">
            <button class="btn-toggle" @click="toggleStatus(rule)">
              {{ isActive(rule) ? '停用' : '启用' }}
            </button>
            <button class="btn-delete" @click="handleDelete(rule)">删除</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.discounts-page {
  min-height: 100vh;
  background: #1a1a2e;
  color: #fff;
}

.page-header {
  padding: 16px;
  background: #FFD161;

  h2 { margin: 0; font-size: 18px; font-weight: 700; color: #333; }
}

.loading-state, .empty-state {
  text-align: center;
  padding: 80px 20px;
  color: #999;

  .empty-icon { font-size: 48px; display: block; margin-bottom: 12px; }
}

.discount-list { padding: 12px; }

.discount-card {
  background: #2A2A2A;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 12px;

  &.inactive { opacity: 0.6; }
  &.expired { opacity: 0.5; }
}

.discount-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.discount-type {
  font-size: 12px;
  font-weight: 600;
  padding: 2px 10px;
  border-radius: 20px;
  color: #fff;

  &.full_reduction { background: #FF6633; }
  &.discount { background: #E6A23C; }
}

.discount-status {
  font-size: 13px;
  font-weight: 600;

  &.active { color: #4CAF50; }
  &.expired { color: #F44336; }
}

.discount-body { margin-bottom: 12px; }

.discount-name { margin: 0 0 6px; font-size: 16px; font-weight: 600; }
.discount-desc { font-size: 18px; font-weight: 700; color: #FF6633; margin-bottom: 6px; }
.discount-scope { font-size: 13px; color: #999; margin-bottom: 4px; }
.discount-time { font-size: 12px; color: #666; }

.discount-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid #3A3A3A;
  padding-top: 12px;
}

.discount-usage { font-size: 13px; color: #999; }

.discount-actions { display: flex; gap: 8px; }

.btn-toggle {
  padding: 6px 14px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  border: none;
  cursor: pointer;
  background: #4CAF50;
  color: #fff;
}

.btn-delete {
  padding: 6px 14px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  border: none;
  cursor: pointer;
  background: #F44336;
  color: #fff;
}
</style>