<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter, type LocationQueryRaw } from 'vue-router'
import { conversationApi } from '@/api/modules/conversation'
import { useCustomerIdentity } from '@/composables/useCustomerIdentity'
import { useSignalR } from '@/composables/useSignalR'
import { useConversationStore } from '@/stores/modules/useConversationStore'
import { formatPrice } from '@/utils/format'
import type {
  ConversationDto,
  ConversationEventPayload,
  ConversationMessageDto,
  ConversationUnreadChangedPayload,
} from '@/types/models/conversation'

const route = useRoute()
const router = useRouter()
const { getCustomerIdentity, ensureCustomerIdentity } = useCustomerIdentity()
const conversationStore = useConversationStore()
const { connection, connect, disconnect, onReconnected } = useSignalR('/hubs/conversation')

const conversations = ref<ConversationDto[]>([])
const selectedConversation = ref<ConversationDto | null>(null)
const messages = ref<ConversationMessageDto[]>([])
const replyText = ref('')
const loading = ref(false)
const messageLoading = ref(false)
const sending = ref(false)
const identity = ref(getCustomerIdentity())
const messageListRef = ref<HTMLElement | null>(null)
const isDetailOpen = computed(() => Boolean(selectedConversation.value))
const queryOrderId = computed(() => (typeof route.query.orderId === 'string' ? route.query.orderId : ''))
const returnToPath = computed(() => {
  const value = route.query.returnTo
  return typeof value === 'string' && value.startsWith('/') && !value.startsWith('//') ? value : ''
})

const syncChatRouteState = (open: boolean) => {
  if ((route.query.chat === '1') === open) return
  const query: LocationQueryRaw = { ...route.query }
  if (open) query.chat = '1'
  else delete query.chat
  void router.replace({ query })
}

const formatTime = (value?: string) => {
  if (!value) return ''
  const date = new Date(value)
  return `${date.getMonth() + 1}/${date.getDate()} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`
}

const customerName = (conversation: ConversationDto) =>
  conversation.storeName || `店铺 ${conversation.storeId.slice(0, 6)}`

const orderTitle = (conversation: ConversationDto) =>
  conversation.order?.pickupCode ? `订单 #${conversation.order.pickupCode}` : `订单 ${conversation.orderId.slice(0, 6)}`

const scrollMessagesToBottom = async () => {
  await nextTick()
  const el = messageListRef.value
  if (el) el.scrollTop = el.scrollHeight
}

const upsertConversation = (conversation?: ConversationDto) => {
  if (!conversation) return
  const index = conversations.value.findIndex((item) => item.id === conversation.id)
  if (index >= 0) conversations.value[index] = { ...conversations.value[index], ...conversation }
  else conversations.value.unshift(conversation)
  conversations.value.sort((left, right) =>
    new Date(right.lastMessageAt).getTime() - new Date(left.lastMessageAt).getTime(),
  )
  if (selectedConversation.value?.id === conversation.id) {
    selectedConversation.value = { ...selectedConversation.value, ...conversation }
  }
}

const appendMessage = (message?: ConversationMessageDto) => {
  if (!message || messages.value.some((item) => item.id === message.id)) return false
  messages.value.push(message)
  void scrollMessagesToBottom()
  return true
}

const loadConversations = async () => {
  loading.value = true
  try {
    conversations.value = await conversationApi.getByCustomer(identity.value)
    await conversationStore.loadCustomerUnreadCount(identity.value)
    if (selectedConversation.value) {
      const freshConversation = conversations.value.find((item) => item.id === selectedConversation.value?.id)
      if (freshConversation) selectedConversation.value = { ...selectedConversation.value, ...freshConversation }
      else {
        selectedConversation.value = null
        messages.value = []
        syncChatRouteState(false)
      }
    }
  } finally {
    loading.value = false
  }
}

const openConversation = async (conversation: ConversationDto) => {
  if (selectedConversation.value?.id && selectedConversation.value.id !== conversation.id) {
    try { await connection.value?.invoke('UnsubscribeConversation', selectedConversation.value.id) } catch {}
  }
  selectedConversation.value = conversation
  syncChatRouteState(true)
  messageLoading.value = true
  try {
    messages.value = await conversationApi.getMessages(conversation.id)
    await connection.value?.invoke('SubscribeConversation', conversation.id)
    const result = await conversationApi.markRead(conversation.id, 'customer') as { unreadCount?: number }
    const index = conversations.value.findIndex((item) => item.id === conversation.id)
    if (index >= 0) conversations.value[index] = { ...conversations.value[index], customerUnreadCount: 0 }
    selectedConversation.value = { ...conversation, customerUnreadCount: 0 }
    if (typeof result?.unreadCount === 'number') conversationStore.setCustomerUnreadCount(result.unreadCount)
    else await conversationStore.loadCustomerUnreadCount(identity.value)
    await scrollMessagesToBottom()
  } finally {
    messageLoading.value = false
  }
}

const openConversationFromOrder = async (orderId: string) => {
  if (!orderId) return
  const existing = conversations.value.find((conversation) => conversation.orderId === orderId)
  if (existing) {
    await openConversation(existing)
    return
  }

  identity.value = await ensureCustomerIdentity()
  const conversation = await conversationApi.startByOrder(orderId, {
    customerId: identity.value.customerId,
    deviceId: identity.value.deviceId,
  })
  upsertConversation(conversation)
  await openConversation(conversation)
}

const closeConversation = async () => {
  const conversationId = selectedConversation.value?.id
  if (conversationId) {
    try { await connection.value?.invoke('UnsubscribeConversation', conversationId) } catch {}
  }
  selectedConversation.value = null
  messages.value = []
  replyText.value = ''
  syncChatRouteState(false)
}

const handleBack = () => {
  if (returnToPath.value) {
    router.push(returnToPath.value)
    return
  }
  if (isDetailOpen.value) {
    void closeConversation()
    return
  }
  router.back()
}

const sendMessage = async () => {
  const content = replyText.value.trim()
  if (!selectedConversation.value || !content || sending.value) return
  sending.value = true
  try {
    const message = await conversationApi.sendMessage(selectedConversation.value.id, {
      senderType: 'customer',
      senderId: identity.value.customerId,
      content,
      storeId: selectedConversation.value.storeId,
      orderId: selectedConversation.value.orderId,
    })
    appendMessage(message)
    selectedConversation.value.lastMessageAt = message.createdAt
    replyText.value = ''
  } finally {
    sending.value = false
  }
}

const openOrder = (conversation: ConversationDto) => {
  const code = conversation.storeCode || String(route.params.code || '')
  router.push({ name: 'OrderDetail', params: { code, orderId: conversation.orderId } })
}

const subscribeCurrent = async () => {
  identity.value = getCustomerIdentity()
  await connection.value?.invoke('SubscribeCustomer', identity.value.customerId || null, identity.value.deviceId || null)
  if (selectedConversation.value) await connection.value?.invoke('SubscribeConversation', selectedConversation.value.id)
}

onMounted(async () => {
  identity.value = await ensureCustomerIdentity()
  await loadConversations()
  try {
    await openConversationFromOrder(queryOrderId.value)
  } catch {
  }
  if (!selectedConversation.value) syncChatRouteState(false)
  try {
    await connect()
    onReconnected(subscribeCurrent)
    connection.value?.on('ConversationMessageReceived', async (payload: ConversationEventPayload) => {
      if (payload.conversationId !== selectedConversation.value?.id) return
      appendMessage(payload.message)
      upsertConversation(payload.conversation)
      if (payload.message.senderType === 'merchant' && selectedConversation.value) {
        await conversationApi.markRead(selectedConversation.value.id, 'customer')
        await conversationStore.loadCustomerUnreadCount(identity.value)
      }
    })
    connection.value?.on('MerchantMessageReceived', async (payload: ConversationEventPayload) => {
      upsertConversation(payload.conversation)
      if (payload.conversationId === selectedConversation.value?.id) {
        appendMessage(payload.message)
        await conversationApi.markRead(payload.conversationId, 'customer')
        await conversationStore.loadCustomerUnreadCount(identity.value)
      } else if (typeof payload.unreadCount === 'number') {
        conversationStore.setCustomerUnreadCount(payload.unreadCount)
      }
    })
    connection.value?.on('ConversationUnreadChanged', (payload: ConversationUnreadChangedPayload) => {
      if (payload.scope === 'customer') conversationStore.setCustomerUnreadCount(payload.count)
    })
    await subscribeCurrent()
  } catch {
  }
})

watch(
  () => route.query.orderId,
  async () => {
    await loadConversations()
    try {
      await openConversationFromOrder(queryOrderId.value)
    } catch {
    }
    if (!selectedConversation.value) syncChatRouteState(false)
  },
)

onUnmounted(async () => {
  try {
    if (selectedConversation.value) await connection.value?.invoke('UnsubscribeConversation', selectedConversation.value.id)
    const currentIdentity = getCustomerIdentity()
    await connection.value?.invoke('UnsubscribeCustomer', currentIdentity.customerId || null, currentIdentity.deviceId || null)
  } catch {
  }
  await disconnect()
})
</script>

<template>
  <div class="messages-page" :class="{ 'is-chat': isDetailOpen }">
    <header class="page-header">
      <button class="back-btn" @click="handleBack">←</button>
      <div>
        <h1>{{ selectedConversation ? customerName(selectedConversation) : '我的消息' }}</h1>
        <p>{{ selectedConversation ? orderTitle(selectedConversation) : '查看商家的回复，继续沟通订单问题' }}</p>
      </div>
      <button v-if="!selectedConversation" class="refresh-btn" @click="loadConversations">刷新</button>
      <button v-else class="refresh-btn" @click="openOrder(selectedConversation)">订单</button>
    </header>

    <div v-if="loading && !selectedConversation" class="state-card">消息加载中...</div>
    <div v-else-if="!selectedConversation && !conversations.length" class="state-card">
      <strong>暂无消息</strong>
      <p>你可以从订单详情里联系商家，后续回复会出现在这里。</p>
      <button @click="router.push({ name: 'MyOrders', params: { code: route.params.code } })">去看订单</button>
    </div>

    <template v-else-if="!selectedConversation">
      <section class="conversation-list">
        <button
          v-for="conversation in conversations"
          :key="conversation.id"
          class="conversation-card"
          @click="openConversation(conversation)"
        >
          <div>
            <strong>{{ customerName(conversation) }}</strong>
            <span>
              {{ orderTitle(conversation) }}
              <template v-if="conversation.order"> · {{ conversation.order.status }}</template>
            </span>
          </div>
          <small>{{ formatTime(conversation.lastMessageAt) }}</small>
          <em v-if="conversation.customerUnreadCount > 0">{{ conversation.customerUnreadCount }}</em>
        </button>
      </section>
    </template>

    <section v-else class="chat-panel">
      <div class="chat-order">
        <div>
          <strong>{{ orderTitle(selectedConversation) }}</strong>
          <span v-if="selectedConversation.order">
            {{ selectedConversation.order.status }} · {{ formatPrice(selectedConversation.order.actualAmount) }}
          </span>
        </div>
        <button @click="openOrder(selectedConversation)">查看订单</button>
      </div>

      <div ref="messageListRef" class="message-list">
        <div v-if="messageLoading" class="message-state">加载消息中...</div>
        <template v-else>
          <div
            v-for="message in messages"
            :key="message.id"
            class="message-bubble"
            :class="{ mine: message.senderType === 'customer' }"
          >
            <span>{{ message.content }}</span>
            <small>{{ formatTime(message.createdAt) }}</small>
          </div>
        </template>
      </div>

      <div class="reply-row">
        <input v-model="replyText" placeholder="输入要回复商家的内容" @keyup.enter="sendMessage" />
        <button :disabled="sending || !replyText.trim()" @click="sendMessage">
          {{ sending ? '发送中' : '发送' }}
        </button>
      </div>
    </section>
  </div>
</template>

<style scoped lang="scss">
.messages-page {
  flex: 1;
  width: 100%;
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: #F6F7F3;
  color: #1F2A26;
}

.page-header {
  position: sticky;
  top: 0;
  z-index: 20;
  flex-shrink: 0;
  display: grid;
  grid-template-columns: 38px minmax(0, 1fr) 58px;
  align-items: center;
  gap: 8px;
  min-height: 58px;
  padding: 8px 14px;
  border-bottom: 1px solid #E2E8E3;
  background: rgba(255, 255, 255, 0.96);
  backdrop-filter: blur(12px);

  h1 {
    margin: 0;
    font-size: 18px;
    font-weight: 900;
  }

  p {
    margin: 2px 0 0;
    color: #687872;
    font-size: 12px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
}

.back-btn,
.refresh-btn,
.chat-order button,
.state-card button {
  height: 32px;
  border-radius: 6px;
  color: #087E6B;
  background: #E7F4EF;
  font-weight: 800;
}

.back-btn {
  width: 34px;
  color: #1F2A26;
  background: transparent;
  font-size: 22px;
}

.conversation-list {
  flex: 1;
  min-height: 0;
  display: grid;
  align-content: start;
  gap: 10px;
  overflow-y: auto;
  padding: 12px 16px 22px;
}

.conversation-card,
.state-card {
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 6px 18px rgba(31, 42, 38, 0.05);
}

.conversation-card {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  min-height: 74px;
  padding: 13px 40px 13px 14px;
  text-align: left;

  &.active {
    border-color: #087E6B;
    background: #F4FBF8;
  }

  strong,
  span {
    display: block;
  }

  strong {
    margin-bottom: 4px;
    font-size: 15px;
  }

  span,
  small {
    color: #687872;
    font-size: 12px;
  }

  em {
    position: absolute;
    top: 8px;
    right: 8px;
    min-width: 18px;
    height: 18px;
    padding: 0 5px;
    border-radius: 999px;
    display: grid;
    place-items: center;
    color: #fff;
    background: #D94C4C;
    font-size: 11px;
    font-style: normal;
    font-weight: 900;
  }
}

.chat-panel {
  flex: 1;
  min-height: 0;
  display: grid;
  grid-template-rows: auto minmax(0, 1fr) auto;
  overflow: hidden;
  background: #fff;
}

.chat-order {
  min-height: 0;
  display: flex;
  flex-shrink: 0;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 12px;
  border-bottom: 1px solid #E2E8E3;
  background: #fff;

  span {
    display: block;
    margin-top: 3px;
    color: #687872;
    font-size: 12px;
  }
}

.message-list {
  min-height: 0;
  padding: 16px 12px;
  display: grid;
  align-content: start;
  gap: 10px;
  overflow-y: auto;
  overscroll-behavior: contain;
  background: #FAFCFA;
}

.message-bubble {
  max-width: 82%;
  justify-self: start;
  padding: 10px 12px;
  border-radius: 8px;
  background: #F1F4F2;
  word-break: break-word;

  &.mine {
    justify-self: end;
    color: #fff;
    background: #087E6B;
  }

  span,
  small {
    display: block;
  }

  small {
    margin-top: 4px;
    opacity: 0.72;
    font-size: 10px;
  }
}

.reply-row {
  flex-shrink: 0;
  display: grid;
  grid-template-columns: minmax(0, 1fr) 72px;
  gap: 8px;
  padding: 10px;
  padding-bottom: calc(10px + env(safe-area-inset-bottom));
  border-top: 1px solid #E2E8E3;
  background: #fff;

  input {
    height: 38px;
    padding: 0 12px;
    border: 1px solid #DCE6E1;
    border-radius: 6px;
    outline: 0;
  }

  button {
    border-radius: 6px;
    color: #fff;
    background: #087E6B;
    font-weight: 900;

    &:disabled {
      background: #9AA9A3;
    }
  }
}

.state-card,
.message-state {
  padding: 28px 16px;
  text-align: center;
  color: #687872;

  strong {
    display: block;
    color: #1F2A26;
    font-size: 16px;
  }
}

.state-card {
  margin: 16px;
}

.message-state {
  margin: 0;
  align-self: center;
}
</style>
