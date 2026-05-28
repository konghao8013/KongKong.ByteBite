<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { adminApi } from '@/api/modules/admin'

const route = useRoute()
const router = useRouter()
const collapsed = ref(false)

const menuItems = [
  { key: 'merchants', label: '商家管理', icon: '商', path: '/admin/merchants' },
  { key: 'industries', label: '行业分类', icon: '类', path: '/admin/industries' },
  { key: 'templates', label: '模板管理', icon: '模', path: '/admin/templates' },
  { key: 'configs', label: '系统配置', icon: '配', path: '/admin/configs' },
  { key: 'stats', label: '平台统计', icon: '数', path: '/admin/stats' },
  { key: 'logs', label: '审计日志', icon: '志', path: '/admin/logs' },
]

const handleLogout = async () => {
  if (!confirm('确定退出登录？')) return
  const adminId = localStorage.getItem('admin_id')
  if (adminId) {
    try { await adminApi.logout(adminId) } catch { /* ignore */ }
  }
  localStorage.removeItem('admin_token')
  localStorage.removeItem('admin_id')
  localStorage.removeItem('admin_info')
  router.push('/login')
}
</script>

<template>
  <div class="admin-layout">
    <aside class="admin-sidebar" :class="{ collapsed }">
      <div class="sidebar-header">
        <span class="admin-logo">B</span>
        <div v-if="!collapsed">
          <h2>ByteBite</h2>
          <small>平台管理端</small>
        </div>
      </div>
      <nav class="sidebar-menu">
        <router-link
          v-for="item in menuItems"
          :key="item.key"
          :to="item.path"
          class="menu-item"
          :class="{ active: route.path === item.path }"
        >
          <span class="menu-icon">{{ item.icon }}</span>
          <span v-if="!collapsed" class="menu-label">{{ item.label }}</span>
        </router-link>
      </nav>
      <div class="sidebar-footer">
        <button class="menu-item logout" @click="handleLogout">
          <span class="menu-icon">退</span>
          <span v-if="!collapsed" class="menu-label">退出登录</span>
        </button>
      </div>
    </aside>
    <main class="admin-main">
      <router-view />
    </main>
  </div>
</template>

<style scoped lang="scss">
.admin-layout {
  display: flex;
  min-height: 100vh;
  min-height: 100dvh;
  background: #F6F7F3;
}

.admin-sidebar {
  width: 220px;
  background: rgba(255, 255, 255, 0.94);
  color: #1F2A26;
  transition: width 0.3s;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  border-right: 1px solid #E2E8E3;

  &.collapsed {
    width: 68px;
  }
}

.sidebar-header {
  min-height: 72px;
  padding: 18px 16px;
  border-bottom: 1px solid #E2E8E3;
  display: flex;
  align-items: center;
  gap: 10px;

  h2 {
    margin: 0;
    font-size: 17px;
    color: #1F2A26;
    font-weight: 800;
  }

  small {
    color: #687872;
    font-size: 11px;
  }
}

.admin-logo {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: grid;
  place-items: center;
  color: #fff;
  background: #087E6B;
  font-weight: 800;
  flex-shrink: 0;
}

.sidebar-menu {
  padding: 12px;
  display: grid;
  gap: 6px;
}

.menu-item {
  min-height: 38px;
  padding: 0 10px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  gap: 10px;
  color: #687872;
  text-decoration: none;
  transition: all 0.2s;
  font-weight: 700;

  &:hover,
  &.active {
    background: #E7F4EF;
    color: #087E6B;
  }
}

.menu-icon {
  width: 22px;
  height: 22px;
  border-radius: 6px;
  display: grid;
  place-items: center;
  background: #FAFCFA;
  font-size: 12px;
  flex-shrink: 0;
}

.menu-label {
  font-size: 14px;
  white-space: nowrap;
}

.sidebar-footer {
  margin-top: auto;
  padding: 12px;
  border-top: 1px solid #E2E8E3;

  .logout {
    width: 100%;
    cursor: pointer;
    color: #D94C4C;

    &:hover {
      background: #FFF7F7;
    }
  }
}

.admin-main {
  flex: 1;
  min-width: 0;
  padding: 24px;
  background: #F6F7F3;
  overflow-y: auto;
}

@media (max-width: 760px) {
  .admin-sidebar {
    width: 68px;
  }

  .sidebar-header div,
  .menu-label {
    display: none;
  }

  .admin-main {
    padding: 14px;
  }
}
</style>
