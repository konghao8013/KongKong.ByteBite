<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const storeCode = computed(() => route.params.code as string)

const activeTab = computed(() => {
  const path = route.path
  if (path.includes('/cart')) return 'cart'
  if (path.includes('/orders')) return 'orders'
  return 'menu'
})

const tabs = [
  { key: 'menu', label: '菜单', icon: '🍽️', routeName: 'StoreMenu' },
  { key: 'cart', label: '购物车', icon: '🛒', routeName: 'Cart' },
  { key: 'orders', label: '我的订单', icon: '📋', routeName: 'MyOrders' },
]

const switchTab = (tab: typeof tabs[0]) => {
  router.push({ name: tab.routeName, params: { code: storeCode.value } })
}
</script>

<template>
  <div class="customer-layout">
    <div class="customer-content">
      <router-view />
    </div>
    <div class="customer-tabbar">
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
.customer-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  min-height: 100dvh;
}

.customer-content {
  flex: 1;
  padding-bottom: calc(50px + env(safe-area-inset-bottom));
}

.customer-tabbar {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  display: flex;
  height: calc(50px + env(safe-area-inset-bottom));
  padding-bottom: env(safe-area-inset-bottom);
  background: #fff;
  border-top: 1px solid #eee;
  z-index: 100;
}

.tabbar-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  color: #999;
  transition: color 0.2s;

  &.active {
    color: #FF6B6B;
  }
}

.tabbar-icon {
  font-size: 20px;
}

.tabbar-label {
  font-size: 11px;
}
</style>
