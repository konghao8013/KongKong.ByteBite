<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { adminApi } from '@/api/modules/admin'

const adminInfo = ref<any>(null)
const auditLogs = ref<any[]>([])
const loading = ref(false)

const loadAdminInfo = () => {
  const info = localStorage.getItem('admin_info')
  if (info) {
    try { adminInfo.value = JSON.parse(info) } catch { /* ignore */ }
  }
}

const loadAuditLogs = async () => {
  loading.value = true
  try {
    auditLogs.value = await adminApi.getAuditLogs() || []
  } catch (e) { console.error('加载审计日志失败', e) }
  finally { loading.value = false }
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`
}

const actionLabel = (action: string) => {
  if (action.startsWith('status_change:')) {
    const parts = action.replace('status_change:', '').split('->')
    const statusMap: Record<string, string> = { active: '正常', pending: '待审核', suspended: '已封禁', rejected: '已拒绝' }
    return `状态变更: ${statusMap[parts[0]] || parts[0]} → ${statusMap[parts[1]] || parts[1]}`
  }
  return action
}

onMounted(() => {
  loadAdminInfo()
  loadAuditLogs()
})
</script>

<template>
  <div class="configs-page">
    <div class="page-header">
      <h2>系统配置</h2>
    </div>

    <div class="config-content">
      <div class="config-section">
        <h3 class="section-title">管理员信息</h3>
        <div class="info-card">
          <div class="info-row">
            <span class="info-label">用户名</span>
            <span class="info-value">{{ adminInfo?.username || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">显示名称</span>
            <span class="info-value">{{ adminInfo?.displayName || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">角色</span>
            <span class="info-value">{{ adminInfo?.role === 'super_admin' ? '超级管理员' : adminInfo?.role === 'admin' ? '管理员' : adminInfo?.role || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">最近登录</span>
            <span class="info-value">{{ formatDate(adminInfo?.lastLoginAt) }}</span>
          </div>
        </div>
      </div>

      <div class="config-section">
        <h3 class="section-title">系统参数</h3>
        <div class="info-card">
          <div class="info-row">
            <span class="info-label">系统版本</span>
            <span class="info-value">v1.0.0</span>
          </div>
          <div class="info-row">
            <span class="info-label">API地址</span>
            <span class="info-value">/api</span>
          </div>
          <div class="info-row">
            <span class="info-label">数据存储</span>
            <span class="info-value">PostgreSQL 17</span>
          </div>
          <div class="info-row">
            <span class="info-label">运行环境</span>
            <span class="info-value">.NET 10 + Vue 3</span>
          </div>
        </div>
      </div>

      <div class="config-section">
        <h3 class="section-title">操作审计日志</h3>
        <div v-if="loading" class="loading-state">加载中...</div>
        <div v-else-if="auditLogs.length === 0" class="empty-state">暂无审计日志</div>
        <div v-else class="log-list">
          <div v-for="log in auditLogs" :key="log.id" class="log-item">
            <div class="log-header">
              <span class="log-action">{{ actionLabel(log.action) }}</span>
              <span class="log-time">{{ formatDate(log.createdAt) }}</span>
            </div>
            <div v-if="log.reason" class="log-reason">原因: {{ log.reason }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.configs-page {
  min-height: 100vh;
  background: #f5f5f5;
}

.page-header {
  background: linear-gradient(135deg, #FF6B6B, #FF8E53);
  padding: 16px;

  h2 { margin: 0; font-size: 18px; font-weight: 700; color: #333; }
}

.config-content {
  padding: 16px;
}

.config-section {
  margin-bottom: 20px;
}

.section-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 12px;
}

.info-card {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f5f5f5;

  &:last-child { border-bottom: none; }

  .info-label { font-size: 14px; color: #999; }
  .info-value { font-size: 14px; font-weight: 500; color: #333; }
}

.loading-state, .empty-state {
  text-align: center;
  padding: 40px;
  color: #999;
  background: #fff;
  border-radius: 12px;
}

.log-list {
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.log-item {
  padding: 12px 16px;
  border-bottom: 1px solid #f5f5f5;

  &:last-child { border-bottom: none; }
}

.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.log-action {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.log-time {
  font-size: 12px;
  color: #999;
}

.log-reason {
  font-size: 13px;
  color: #FF9800;
  margin-top: 4px;
}
</style>