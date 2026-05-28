<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { adminApi } from '@/api/modules/admin'

const loading = ref(false)
const auditLogs = ref<any[]>([])
const operationLogs = ref<any[]>([])
const activeTab = ref<'audit' | 'operation'>('audit')

const loadLogs = async () => {
  loading.value = true
  try {
    const [auditRows, operationRows] = await Promise.all([
      adminApi.getAuditLogs(),
      adminApi.getOperationLogs(),
    ])
    auditLogs.value = auditRows || []
    operationLogs.value = operationRows || []
  } finally {
    loading.value = false
  }
}

const formatDate = (dateStr: string) => dateStr ? new Date(dateStr).toLocaleString() : '-'

const actionLabel = (action: string) => {
  if (action?.startsWith('status_change:')) {
    const [from, to] = action.replace('status_change:', '').split('->')
    return `商家状态 ${from} -> ${to}`
  }
  return action || '-'
}

onMounted(loadLogs)
</script>

<template>
  <div class="logs-page">
    <header class="page-header">
      <h2>审计日志</h2>
      <button class="ghost-button" @click="loadLogs">刷新</button>
    </header>

    <div class="tabs">
      <button :class="{ active: activeTab === 'audit' }" @click="activeTab = 'audit'">商家审核</button>
      <button :class="{ active: activeTab === 'operation' }" @click="activeTab = 'operation'">管理员操作</button>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <section v-else-if="activeTab === 'audit'" class="log-list">
      <article v-for="log in auditLogs" :key="log.id" class="log-card">
        <strong>{{ actionLabel(log.action) }}</strong>
        <span>{{ formatDate(log.createdAt) }}</span>
        <p v-if="log.reason">原因：{{ log.reason }}</p>
        <small>商家 {{ log.merchantId }} · 管理员 {{ log.adminId }}</small>
      </article>
      <div v-if="auditLogs.length === 0" class="empty-state">暂无审核日志</div>
    </section>

    <section v-else class="log-list">
      <article v-for="log in operationLogs" :key="log.id" class="log-card">
        <strong>{{ log.operation }}</strong>
        <span>{{ formatDate(log.createdAt) }}</span>
        <p>{{ log.detail || '无详情' }}</p>
        <small>{{ log.targetType || '-' }} {{ log.targetId || '' }}</small>
      </article>
      <div v-if="operationLogs.length === 0" class="empty-state">暂无操作日志</div>
    </section>
  </div>
</template>

<style scoped lang="scss">
.logs-page { min-height: 100vh; color: #1F2A26; background: #F6F7F3; }
.page-header, .tabs, .log-card { border: 1px solid #E2E8E3; border-radius: 8px; background: #fff; }
.page-header { padding: 16px; display: flex; align-items: center; justify-content: space-between; h2 { margin: 0; font-size: 18px; } }
.ghost-button { min-height: 34px; padding: 0 12px; border-radius: 6px; color: #087E6B; background: #E7F4EF; font-weight: 800; }
.tabs { margin: 12px 0; padding: 5px; display: inline-flex; gap: 6px; button { min-height: 32px; padding: 0 12px; border-radius: 6px; color: #687872; font-weight: 800; } .active { color: #fff; background: #087E6B; } }
.loading-state, .empty-state { padding: 60px; text-align: center; color: #687872; }
.log-list { display: grid; gap: 10px; }
.log-card { padding: 14px 16px; display: grid; gap: 5px; span, small { color: #687872; font-size: 12px; } p { margin: 0; color: #1F2A26; word-break: break-word; } }
</style>
