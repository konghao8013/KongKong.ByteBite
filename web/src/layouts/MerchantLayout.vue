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
  if (path.includes('/staff')) return 'staff'
  if (path.includes('/dashboard')) return 'dashboard'
  return 'orders'
})

const tabs = [
  { key: 'orders', label: '订单', icon: '订', path: '/merchant/orders' },
  { key: 'menu', label: '菜品', icon: '菜', path: '/merchant/menu' },
  { key: 'store', label: '门店', icon: '店', path: '/merchant/store' },
  { key: 'discounts', label: '优惠', icon: '惠', path: '/merchant/discounts' },
  { key: 'staff', label: '店员', icon: '员', path: '/merchant/staff' },
  { key: 'dashboard', label: '经营', icon: '数', path: '/merchant/dashboard' },
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
    <aside class="merchant-sidebar">
      <div class="brand">
        <span class="brand-logo">B</span>
        <div>
          <strong>ByteBite</strong>
          <small>商家工作台</small>
        </div>
      </div>
      <nav class="merchant-nav">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          class="nav-item"
          :class="{ active: activeTab === tab.key }"
          @click="switchTab(tab)"
        >
          <span class="nav-icon">{{ tab.icon }}</span>
          <span>{{ tab.label }}</span>
        </button>
      </nav>
      <button class="logout desktop-logout" @click="handleLogout">退出登录</button>
    </aside>

    <div class="merchant-main">
      <header class="merchant-topbar">
        <div>
          <span class="topbar-kicker">Merchant Console</span>
          <h1>商家工作台</h1>
        </div>
        <button class="logout" @click="handleLogout">退出登录</button>
      </header>
      <main class="merchant-content">
        <router-view />
      </main>
    </div>

    <nav class="merchant-tabbar">
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
.merchant-layout {
  display: grid;
  grid-template-columns: 196px minmax(0, 1fr);
  min-height: 100vh;
  min-height: 100dvh;
  background: #F6F7F3;
}

.merchant-sidebar {
  position: sticky;
  top: 0;
  height: 100vh;
  padding: 18px 12px;
  background: rgba(255, 255, 255, 0.92);
  border-right: 1px solid #E2E8E3;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 8px 14px;
  border-bottom: 1px solid #E2E8E3;

  strong {
    display: block;
    font-size: 15px;
    color: #1F2A26;
  }

  small {
    display: block;
    color: #687872;
    font-size: 11px;
  }
}

.brand-logo {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: grid;
  place-items: center;
  color: #fff;
  background: #087E6B;
  font-weight: 800;
}

.merchant-nav {
  display: grid;
  gap: 6px;
}

.nav-item {
  height: 38px;
  padding: 0 10px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  gap: 10px;
  color: #687872;
  text-align: left;
  font-size: 14px;
  font-weight: 700;
  transition: all 0.2s;
}

.nav-item:hover,
.nav-item.active {
  color: #087E6B;
  background: #E7F4EF;
}

.nav-icon {
  width: 22px;
  height: 22px;
  border-radius: 6px;
  display: grid;
  place-items: center;
  background: #FAFCFA;
  font-size: 12px;
}

.merchant-main {
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.merchant-topbar {
  height: 66px;
  padding: 0 24px;
  border-bottom: 1px solid #E2E8E3;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: rgba(255, 255, 255, 0.88);
  backdrop-filter: blur(12px);

  h1 {
    margin: 2px 0 0;
    font-size: 18px;
    color: #1F2A26;
  }
}

.topbar-kicker {
  color: #687872;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.logout {
  height: 34px;
  padding: 0 13px;
  border: 1px solid #DCE6E1;
  border-radius: 6px;
  color: #687872;
  background: #fff;
  font-size: 12px;
  font-weight: 700;
  transition: all 0.2s;
}

.logout:hover {
  color: #D94C4C;
  border-color: #F2CACA;
  background: #FFF7F7;
}

.desktop-logout {
  margin-top: auto;
}

.merchant-content {
  flex: 1;
  min-width: 0;
  padding: 18px;
}

.merchant-tabbar {
  display: none;
}

@media (max-width: 760px) {
  .merchant-layout {
    display: flex;
    flex-direction: column;
  }

  .merchant-sidebar {
    display: none;
  }

  .merchant-topbar {
    height: 58px;
    padding: 0 16px;
  }

  .merchant-content {
    padding: 0;
    padding-bottom: calc(62px + env(safe-area-inset-bottom));
  }

  .merchant-tabbar {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    display: flex;
    height: calc(62px + env(safe-area-inset-bottom));
    padding: 7px 8px env(safe-area-inset-bottom);
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
    font-size: 12px;
  }

  .tabbar-label {
    font-size: 11px;
  }
}
</style>
