<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const storeCode = computed(() => route.params.code as string)

const activeTab = computed(() => {
  const path = route.path
  if (path.includes('/cart')) return 'cart'
  if (path.includes('/orders') || path.includes('/order/')) return 'orders'
  return 'menu'
})

const tabs = [
  { key: 'menu', label: '菜单', icon: '□', routeName: 'StoreMenu' },
  { key: 'cart', label: '购物车', icon: '▣', routeName: 'Cart' },
  { key: 'orders', label: '我的订单', icon: '▤', routeName: 'MyOrders' },
]

const switchTab = (tab: typeof tabs[0]) => {
  router.push({ name: tab.routeName, params: { code: storeCode.value } })
}
</script>

<template>
  <div class="customer-layout">
    <main class="customer-content">
      <router-view />
    </main>
    <nav class="customer-tabbar">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        class="tabbar-item"
        :class="{ active: activeTab === tab.key }"
        @click="switchTab(tab)"
      >
        <span class="tabbar-icon">{{ tab.icon }}</span>
        <span class="tabbar-label">{{ tab.label }}</span>
      </button>
    </nav>
  </div>
</template>

<style scoped lang="scss">
.customer-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  min-height: 100dvh;
  max-width: 750px;
  margin: 0 auto;
  background: #F6F7F3;
}

.customer-content {
  flex: 1;
  min-height: 0;
  padding-bottom: calc(58px + env(safe-area-inset-bottom));
}

.customer-tabbar {
  position: fixed;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  max-width: 750px;
  display: flex;
  height: calc(58px + env(safe-area-inset-bottom));
  padding: 7px 18px env(safe-area-inset-bottom);
  background: rgba(255, 255, 255, 0.96);
  border-top: 1px solid #E2E8E3;
  box-shadow: 0 -8px 24px rgba(31, 42, 38, 0.06);
  z-index: 100;
  backdrop-filter: blur(12px);
}

.tabbar-item {
  flex: 1;
  display: grid;
  justify-items: center;
  align-content: center;
  gap: 2px;
  color: #9AA9A3;
  transition: color 0.2s;
}

.tabbar-item.active {
  color: #087E6B;
  font-weight: 700;
}

.tabbar-icon {
  width: 22px;
  height: 22px;
  border-radius: 6px;
  display: grid;
  place-items: center;
  font-size: 13px;
}

.tabbar-label {
  font-size: 11px;
}
</style>
