<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { adminApi } from '@/api/modules/admin'

const loading = ref(false)
const merchants = ref<any[]>([])
const statusFilter = ref('')
const keyword = ref('')

const filteredMerchants = computed(() => {
  let list = merchants.value
  if (statusFilter.value) {
    list = list.filter(m => m.status === statusFilter.value)
  }
  if (keyword.value) {
    const kw = keyword.value.toLowerCase()
    list = list.filter(m =>
      m.phone?.toLowerCase().includes(kw) ||
      m.nickname?.toLowerCase().includes(kw) ||
      m.storeName?.toLowerCase().includes(kw)
    )
  }
  return list
})

const statusLabel = (status: string) => {
  const map: Record<string, string> = {
    active: '正常', pending: '待审核', suspended: '已封禁', rejected: '已拒绝'
  }
  return map[status] || status
}

const statusColor = (status: string) => {
  const map: Record<string, string> = {
    active: '#4CAF50', pending: '#FF9800', suspended: '#F44336', rejected: '#999'
  }
  return map[status] || '#999'
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`
}

const loadMerchants = async () => {
  loading.value = true
  try {
    merchants.value = await adminApi.getMerchants() || []
  } catch (e) { console.error('加载商家列表失败', e) }
  finally { loading.value = false }
}

const handleApprove = async (merchant: any) => {
  if (!confirm('确定通过该商家审核？')) return
  try {
    const adminId = localStorage.getItem('admin_id') || ''
    await adminApi.updateMerchantStatus(merchant.id, { status: 'active', operatorId: adminId, reason: '审核通过' })
    merchant.status = 'active'
  } catch (e) { console.error('操作失败', e) }
}

const handleSuspend = async (merchant: any) => {
  const reason = prompt('请输入封禁原因')
  if (!reason) return
  try {
    const adminId = localStorage.getItem('admin_id') || ''
    await adminApi.updateMerchantStatus(merchant.id, { status: 'suspended', operatorId: adminId, reason })
    merchant.status = 'suspended'
  } catch (e) { console.error('操作失败', e) }
}

const handleActivate = async (merchant: any) => {
  if (!confirm('确定解封该商家？')) return
  try {
    const adminId = localStorage.getItem('admin_id') || ''
    await adminApi.updateMerchantStatus(merchant.id, { status: 'active', operatorId: adminId, reason: '解封' })
    merchant.status = 'active'
  } catch (e) { console.error('操作失败', e) }
}

onMounted(loadMerchants)
</script>

<template>
  <div class="merchants-page">
    <div class="page-header">
      <h2>商家管理</h2>
    </div>

    <div class="filter-bar">
      <input v-model="keyword" placeholder="搜索手机号/昵称" class="search-input" />
      <div class="status-filters">
        <span
          class="filter-tag"
          :class="{ active: statusFilter === '' }"
          @click="statusFilter = ''"
        >全部</span>
        <span
          class="filter-tag"
          :class="{ active: statusFilter === 'active' }"
          @click="statusFilter = 'active'"
        >正常</span>
        <span
          class="filter-tag"
          :class="{ active: statusFilter === 'pending' }"
          @click="statusFilter = 'pending'"
        >待审核</span>
        <span
          class="filter-tag"
          :class="{ active: statusFilter === 'suspended' }"
          @click="statusFilter = 'suspended'"
        >已封禁</span>
      </div>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div v-else-if="filteredMerchants.length === 0" class="empty-state">
      <span>📭</span>
      <p>暂无商家数据</p>
    </div>

    <div v-else class="merchants-list">
      <div v-for="merchant in filteredMerchants" :key="merchant.id" class="merchant-card">
        <div class="merchant-header">
          <div class="merchant-avatar">{{ (merchant.nickname || merchant.phone || '?')[0] }}</div>
          <div class="merchant-basic">
            <div class="merchant-name">{{ merchant.nickname || merchant.phone }}</div>
            <div class="merchant-phone">{{ merchant.phone }}</div>
          </div>
          <span class="merchant-status" :style="{ color: statusColor(merchant.status) }">
            {{ statusLabel(merchant.status) }}
          </span>
        </div>

        <div class="merchant-detail">
          <div class="detail-row">
            <span class="detail-label">注册时间</span>
            <span class="detail-value">{{ formatDate(merchant.createdAt) }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">最近登录</span>
            <span class="detail-value">{{ formatDate(merchant.lastLoginAt) }}</span>
          </div>
        </div>

        <div class="merchant-actions">
          <button v-if="merchant.status === 'pending'" class="btn-approve" @click="handleApprove(merchant)">通过审核</button>
          <button v-if="merchant.status === 'active'" class="btn-suspend" @click="handleSuspend(merchant)">封禁</button>
          <button v-if="merchant.status === 'suspended'" class="btn-activate" @click="handleActivate(merchant)">解封</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.merchants-page {
  background: #f5f5f5;
  min-height: 100vh;
}

.page-header {
  background: #FFD161;
  padding: 16px;

  h2 { margin: 0; font-size: 18px; font-weight: 700; color: #333; }
}

.filter-bar {
  padding: 12px 16px;
  background: #fff;
  border-bottom: 1px solid #eee;
}

.search-input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
  margin-bottom: 8px;
  box-sizing: border-box;

  &:focus { border-color: #FFD161; }
}

.status-filters {
  display: flex;
  gap: 8px;
}

.filter-tag {
  padding: 4px 12px;
  border-radius: 16px;
  font-size: 13px;
  color: #666;
  background: #f0f0f0;
  cursor: pointer;
  transition: all 0.2s;

  &.active { background: #FF6633; color: #fff; }
}

.loading-state, .empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #999;

  span { font-size: 48px; display: block; margin-bottom: 12px; }
}

.merchants-list {
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.merchant-card {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.merchant-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.merchant-avatar {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: #FFD161;
  color: #333;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  font-weight: 700;
  flex-shrink: 0;
}

.merchant-basic {
  flex: 1;
  min-width: 0;

  .merchant-name { font-size: 16px; font-weight: 600; color: #333; }
  .merchant-phone { font-size: 13px; color: #999; }
}

.merchant-status { font-size: 14px; font-weight: 600; }

.merchant-detail {
  margin-bottom: 12px;

  .detail-row {
    display: flex;
    justify-content: space-between;
    padding: 6px 0;
    font-size: 13px;

    .detail-label { color: #999; }
    .detail-value { color: #333; }
  }
}

.merchant-actions {
  display: flex;
  gap: 8px;
  padding-top: 12px;
  border-top: 1px solid #f0f0f0;

  button {
    flex: 1;
    padding: 8px;
    border-radius: 8px;
    font-size: 14px;
    font-weight: 600;
    border: none;
    cursor: pointer;
  }

  .btn-approve { background: #4CAF50; color: #fff; }
  .btn-suspend { background: #F44336; color: #fff; }
  .btn-activate { background: #2196F3; color: #fff; }
}
</style>