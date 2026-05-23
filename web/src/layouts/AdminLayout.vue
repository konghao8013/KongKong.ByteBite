<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { adminApi } from '@/api/modules/admin'

const route = useRoute()
const router = useRouter()
const collapsed = ref(false)

const menuItems = [
  { key: 'merchants', label: '商家管理', icon: '👥', path: '/admin/merchants' },
  { key: 'configs', label: '系统配置', icon: '⚙️', path: '/admin/configs' },
  { key: 'stats', label: '平台统计', icon: '📈', path: '/admin/stats' },
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
        <h2 v-if="!collapsed">管理后台</h2>
        <span v-else>管</span>
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
        <div class="menu-item logout" @click="handleLogout">
          <span class="menu-icon">🚪</span>
          <span v-if="!collapsed" class="menu-label">退出登录</span>
        </div>
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
}

.admin-sidebar {
  width: 220px;
  background: #2A2A2A;
  color: #fff;
  transition: width 0.3s;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;

  &.collapsed {
    width: 64px;
  }
}

.sidebar-header {
  padding: 20px 16px;
  border-bottom: 1px solid #444;
  text-align: center;

  h2 {
    margin: 0;
    font-size: 18px;
    color: #FFD161;
  }
}

.sidebar-menu {
  padding: 12px 0;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 20px;
  color: #ccc;
  text-decoration: none;
  transition: all 0.2s;

  &:hover {
    background: #3A3A3A;
    color: #FFD161;
  }

  &.active {
    background: #3A3A3A;
    color: #FFD161;
    border-right: 3px solid #FFD161;
  }
}

.menu-icon {
  font-size: 18px;
  flex-shrink: 0;
}

.menu-label {
  font-size: 14px;
  white-space: nowrap;
}

.sidebar-footer {
  margin-top: auto;
  padding: 12px 0;
  border-top: 1px solid #444;

  .logout {
    color: #F44336;
    cursor: pointer;

    &:hover { background: #3A3A3A; }
  }
}

.admin-main {
  flex: 1;
  padding: 24px;
  background: #f5f5f5;
  overflow-y: auto;
}
</style>
