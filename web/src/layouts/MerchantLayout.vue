<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { merchantApi } from '@/api/modules/merchant'

const route = useRoute()
const router = useRouter()

const activeTab = computed(() => {
  const path = route.path
  if (path.includes('/orders')) return 'orders'
  if (path.includes('/menu')) return 'menu'
  if (path.includes('/store')) return 'store'
  if (path.includes('/discounts')) return 'discounts'
  if (path.includes('/dashboard')) return 'dashboard'
  return 'orders'
})

const tabs = [
  { key: 'orders', label: '订单', icon: '📋', path: '/merchant/orders' },
  { key: 'menu', label: '菜单', icon: '🍽️', path: '/merchant/menu' },
  { key: 'store', label: '店铺', icon: '🏪', path: '/merchant/store' },
  { key: 'dashboard', label: '数据', icon: '📊', path: '/merchant/dashboard' },
]

const switchTab = (tab: typeof tabs[0]) => {
  router.push(tab.path)
}

const handleLogout = async () => {
  if (!confirm('确定退出登录？')) return
  const merchantId = localStorage.getItem('merchant_id')
  if (merchantId) {
    try { await merchantApi.logout(merchantId) } catch { /* ignore */ }
  }
  localStorage.removeItem('merchant_token')
  localStorage.removeItem('merchant_id')
  localStorage.removeItem('merchant_store_id')
  localStorage.removeItem('merchant_info')
  router.push('/login')
}
</script>

<template>
  <div class="merchant-layout">
    <div class="merchant-topbar">
      <span class="topbar-title">空空码上点单</span>
      <button class="topbar-logout" @click="handleLogout">退出</button>
    </div>
    <div class="merchant-content">
      <router-view />
    </div>
    <div class="merchant-tabbar">
      <div
        v-for="tab in tabs"
        :key="tab.key"
        class="tabbar-item"
        :class="{ active: activeTab === tab.key }"
        @click="switchTab(tab)"
      >
        <span class="tabbar-icon">{{ tab.icon }}</span>
        <span class="tabbar-label">{{ tab.label }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.merchant-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  min-height: 100dvh;
  background: #F7F7F7;
}

.merchant-topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: #FFFFFF;
  border-bottom: 1px solid #F0F0F0;

  .topbar-title { font-size: 16px; color: #FF6B6B; font-weight: 700; }

  .topbar-logout {
    background: none;
    border: 1px solid #E8E8E8;
    color: #8C8C8C;
    padding: 5px 14px;
    border-radius: 16px;
    font-size: 12px;
    cursor: pointer;
    transition: all 0.2s;

    &:active { color: #FF6B6B; border-color: #FF6B6B; }
  }
}

.merchant-content {
  flex: 1;
  padding-bottom: calc(56px + env(safe-area-inset-bottom));
}

.merchant-tabbar {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  display: flex;
  height: calc(56px + env(safe-area-inset-bottom));
  padding-bottom: env(safe-area-inset-bottom);
  background: #FFFFFF;
  border-top: 1px solid #F0F0F0;
  z-index: 100;
}

.tabbar-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 3px;
  color: #BFBFBF;
  transition: color 0.2s;

  &.active { color: #FF6B6B; }
}

.tabbar-icon { font-size: 22px; }
.tabbar-label { font-size: 11px; font-weight: 500; }
</style>