<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { conversationApi } from '@/api/modules/conversation'
import { useDeviceId } from '@/composables/useDeviceId'
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
const { getDeviceId } = useDeviceId()
const conversationStore = useConversationStore()
const { connection, connect, disconnect } = useSignalR('/hubs/conversation')

const conversations = ref<ConversationDto[]>([])
const selectedConversation = ref<ConversationDto | null>(null)
const messages = ref<ConversationMessageDto[]>([])
const replyText = ref('')
const loading = ref(false)
const messageLoading = ref(false)
const sending = ref(false)

const identity = computed(() => ({
  customerId: localStorage.getItem('customer_id') || undefined,
  deviceId: getDeviceId(),
}))

const formatTime = (value?: string) => {
  if (!value) return ''
  const date = new Date(value)
  return `${date.getMonth() + 1}/${date.getDate()} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`
}

const customerName = (conversation: ConversationDto) =>
  conversation.storeName || `店铺 ${conversation.storeId.slice(0, 6)}`

const orderTitle = (conversation: ConversationDto) =>
  conversation.order?.pickupCode ? `订单 #${conversation.order.pickupCode}` : `订单 ${conversation.orderId.slice(0, 6)}`

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
  if (!message || messages.value.some((item) => item.id === message.id)) return
  messages.value.push(message)
}

const loadConversations = async () => {
  loading.value = true
  try {
    conversations.value = await conversationApi.getByCustomer(identity.value)
    await conversationStore.loadCustomerUnreadCount(identity.value)
    if (!selectedConversation.value && conversations.value.length) {
      await openConversation(conversations.value.find((item) => item.customerUnreadCount > 0) || conversations.value[0])
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
  messageLoading.value = true
  try {
    messages.value = await conversationApi.getMessages(conversation.id)
    await connection.value?.invoke('SubscribeConversation', conversation.id)
    const result = await conversationApi.markRead(conversation.id, 'customer') as { unreadCount?: number }
    conversation.customerUnreadCount = 0
    if (typeof result?.unreadCount === 'number') conversationStore.setCustomerUnreadCount(result.unreadCount)
    else await conversationStore.loadCustomerUnreadCount(identity.value)
  } finally {
    messageLoading.value = false
  }
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

onMounted(async () => {
  await loadConversations()
  try {
    await connect()
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
    await connection.value?.invoke('SubscribeCustomer', identity.value.customerId || null, identity.value.deviceId || null)
    if (selectedConversation.value) await connection.value?.invoke('SubscribeConversation', selectedConversation.value.id)
  } catch {
  }
})

onUnmounted(async () => {
  try {
    if (selectedConversation.value) await connection.value?.invoke('UnsubscribeConversation', selectedConversation.value.id)
    await connection.value?.invoke('UnsubscribeCustomer', identity.value.customerId || null, identity.value.deviceId || null)
  } catch {
  }
  await disconnect()
})
</script>

<template>
  <div class="messages-page">
    <header class="page-header">
      <button class="back-btn" @click="router.back()">‹</button>
      <div>
        <h1>我的消息</h1>
        <p>查看商家的回复，继续沟通订单问题</p>
      </div>
      <button class="refresh-btn" @click="loadConversations">刷新</button>
    </header>

    <div v-if="loading" class="state-card">消息加载中...</div>
    <div v-else-if="!conversations.length" class="state-card">
      <strong>暂无消息</strong>
      <p>你可以从订单详情里联系商家，后续回复会出现在这里。</p>
      <button @click="router.push({ name: 'MyOrders', params: { code: route.params.code } })">去看订单</button>
    </div>

    <template v-else>
      <section class="conversation-list">
        <button
          v-for="conversation in conversations"
          :key="conversation.id"
          class="conversation-card"
          :class="{ active: selectedConversation?.id === conversation.id }"
          @click="openConversation(conversation)"
        >
          <div>
            <strong>{{ customerName(conversation) }}</strong>
            <span>{{ orderTitle(conversation) }}</span>
          </div>
          <small>{{ formatTime(conversation.lastMessageAt) }}</small>
          <em v-if="conversation.customerUnreadCount > 0">{{ conversation.customerUnreadCount }}</em>
        </button>
      </section>

      <section v-if="selectedConversation" class="chat-panel">
        <div class="chat-order">
          <div>
            <strong>{{ orderTitle(selectedConversation) }}</strong>
            <span v-if="selectedConversation.order">
              {{ selectedConversation.order.status }} · {{ formatPrice(selectedConversation.order.actualAmount) }}
            </span>
          </div>
          <button @click="openOrder(selectedConversation)">查看订单</button>
        </div>

        <div class="message-list">
          <div v-if="messageLoading" class="message-state">加载消息中...</div>
          <div
            v-for="message in messages"
            v-else
            :key="message.id"
            class="message-bubble"
            :class="{ mine: message.senderType === 'customer' }"
          >
            <span>{{ message.content }}</span>
            <small>{{ formatTime(message.createdAt) }}</small>
          </div>
        </div>

        <div class="reply-row">
          <input v-model="replyText" placeholder="输入要回复商家的内容" @keyup.enter="sendMessage" />
          <button :disabled="sending || !replyText.trim()" @click="sendMessage">
            {{ sending ? '发送中' : '发送' }}
          </button>
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped lang="scss">
.messages-page {
  min-height: 100%;
  padding-bottom: 18px;
  background: #F6F7F3;
  color: #1F2A26;
}

.page-header {
  position: sticky;
  top: 0;
  z-index: 10;
  display: grid;
  grid-template-columns: 42px minmax(0, 1fr) 58px;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
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
  color: #1F2A26;
  background: transparent;
  font-size: 26px;
}

.conversation-list {
  display: grid;
  gap: 10px;
  padding: 12px 16px;
}

.conversation-card,
.chat-panel,
.state-card {
  border: 1px solid #E2E8E3;
  border-radius: 10px;
  background: #fff;
  box-shadow: 0 6px 18px rgba(31, 42, 38, 0.05);
}

.conversation-card {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 13px 14px;
  text-align: left;

  &.active {
    border-color: #087E6B;
    background: #F4FBF8;
  }

  strong,
  span {
    display: block;
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
  margin: 0 16px;
  overflow: hidden;
}

.chat-order {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 12px;
  border-bottom: 1px solid #E2E8E3;

  span {
    display: block;
    margin-top: 3px;
    color: #687872;
    font-size: 12px;
  }
}

.message-list {
  min-height: 260px;
  max-height: 48vh;
  padding: 14px 12px;
  display: grid;
  align-content: start;
  gap: 8px;
  overflow-y: auto;
  background: #FAFCFA;
}

.message-bubble {
  max-width: 82%;
  justify-self: start;
  padding: 9px 11px;
  border-radius: 10px;
  background: #F1F4F2;

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
  display: grid;
  grid-template-columns: minmax(0, 1fr) 72px;
  gap: 8px;
  padding: 10px;
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
  margin: 16px;
  padding: 28px 16px;
  text-align: center;
  color: #687872;

  strong {
    display: block;
    color: #1F2A26;
    font-size: 16px;
  }
}
</style>
