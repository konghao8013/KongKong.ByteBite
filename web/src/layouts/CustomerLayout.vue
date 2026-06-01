<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useCustomerIdentity } from '@/composables/useCustomerIdentity'
import { useSignalR } from '@/composables/useSignalR'
import { useConversationStore } from '@/stores/modules/useConversationStore'
import type { ConversationEventPayload, ConversationUnreadChangedPayload } from '@/types/models/conversation'

const route = useRoute()
const router = useRouter()
const { getCustomerIdentity, ensureCustomerIdentity } = useCustomerIdentity()
const conversationStore = useConversationStore()
const { connection, connect, disconnect, onReconnected } = useSignalR('/hubs/conversation')

const storeCode = computed(() => route.params.code as string)
const messageBadge = computed(() => conversationStore.customerUnreadCount)
const isMessageRoute = computed(() => route.name === 'CustomerMessages')

const activeTab = computed(() => {
  const path = route.path
  if (path.includes('/cart')) return 'cart'
  if (path.includes('/messages')) return 'messages'
  if (path.includes('/orders') || path.includes('/order/')) return 'orders'
  return 'menu'
})

const tabs = [
  { key: 'menu', label: '菜单', icon: '单', routeName: 'StoreMenu' },
  { key: 'cart', label: '购物车', icon: '车', routeName: 'Cart' },
  { key: 'messages', label: '消息', icon: '信', routeName: 'CustomerMessages' },
  { key: 'orders', label: '订单', icon: '订', routeName: 'MyOrders' },
]

const switchTab = (tab: typeof tabs[0]) => {
  router.push({ name: tab.routeName, params: { code: storeCode.value } })
}

const subscribeCustomer = async () => {
  const identity = getCustomerIdentity()
  await connection.value?.invoke('SubscribeCustomer', identity.customerId || null, identity.deviceId || null)
}

onMounted(async () => {
  const identity = await ensureCustomerIdentity()
  try {
    await conversationStore.loadCustomerUnreadCount(identity)
  } catch {
  }

  try {
    await connect()
    onReconnected(subscribeCustomer)
    connection.value?.on('MerchantMessageReceived', (payload: ConversationEventPayload) => {
      if (typeof payload.unreadCount === 'number') conversationStore.setCustomerUnreadCount(payload.unreadCount)
      else conversationStore.loadCustomerUnreadCount(getCustomerIdentity()).catch(() => {})
      if (route.name !== 'CustomerMessages') ElMessage.info('收到商家新回复')
    })
    connection.value?.on('ConversationUnreadChanged', (payload: ConversationUnreadChangedPayload) => {
      if (payload.scope === 'customer') conversationStore.setCustomerUnreadCount(payload.count)
    })
    await subscribeCustomer()
  } catch {
  }
})

onUnmounted(async () => {
  try {
    const identity = getCustomerIdentity()
    await connection.value?.invoke('UnsubscribeCustomer', identity.customerId || null, identity.deviceId || null)
  } catch {
  }
  await disconnect()
})
</script>

<template>
  <div class="customer-layout" :class="{ 'with-tabbar': !isMessageRoute, 'message-route': isMessageRoute }">
    <main class="customer-content">
      <router-view />
    </main>
    <nav v-if="!isMessageRoute" class="customer-tabbar">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        class="tabbar-item"
        :class="{ active: activeTab === tab.key }"
        @click="switchTab(tab)"
      >
        <span class="tabbar-icon">{{ tab.icon }}</span>
        <span v-if="tab.key === 'messages' && messageBadge > 0" class="tabbar-badge">{{ messageBadge }}</span>
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
}

.customer-layout.message-route {
  height: 100vh;
  height: 100dvh;
  overflow: hidden;
}

.customer-layout.message-route .customer-content {
  display: flex;
  overflow: hidden;
}

.customer-layout.with-tabbar .customer-content {
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
  position: relative;
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

.tabbar-badge {
  position: absolute;
  top: 3px;
  left: 50%;
  min-width: 16px;
  height: 16px;
  padding: 0 5px;
  border-radius: 999px;
  display: grid;
  place-items: center;
  color: #fff;
  background: #D94C4C;
  font-size: 10px;
  font-weight: 800;
}
</style>
