<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { conversationApi } from '@/api/modules/conversation'
import { useSignalR } from '@/composables/useSignalR'
import { useConversationStore } from '@/stores/modules/useConversationStore'
import { formatPrice } from '@/utils/format'
import type {
  ConversationDto,
  ConversationEventPayload,
  ConversationMessageDto,
  ConversationUnreadChangedPayload,
} from '@/types/models/conversation'

const router = useRouter()
const storeId = localStorage.getItem('merchant_store_id') || ''
const merchantId = localStorage.getItem('merchant_id') || undefined
const conversationStore = useConversationStore()
const { connection, connect, disconnect, onReconnected } = useSignalR('/hubs/conversation')

const conversations = ref<ConversationDto[]>([])
const selectedConversation = ref<ConversationDto | null>(null)
const messages = ref<ConversationMessageDto[]>([])
const replyText = ref('')
const loading = ref(false)
const messageLoading = ref(false)
const sending = ref(false)
const messageListRef = ref<HTMLElement | null>(null)

const unreadCount = computed(() => conversationStore.merchantUnreadCount)
const isDetailOpen = computed(() => Boolean(selectedConversation.value))

const formatTime = (value?: string) => {
  if (!value) return ''
  const date = new Date(value)
  return `${date.getMonth() + 1}/${date.getDate()} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`
}

const customerLabel = (conversation: ConversationDto) => {
  if (conversation.customer?.nickname) return conversation.customer.nickname
  if (conversation.customer?.phone) return conversation.customer.phone
  if (conversation.customer?.username) return conversation.customer.username
  return conversation.deviceId ? `匿名顾客 ${conversation.deviceId.slice(-4)}` : '顾客'
}

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
  if (!storeId) return
  loading.value = true
  try {
    conversations.value = await conversationApi.getByStore(storeId)
    await conversationStore.loadMerchantUnreadCount(storeId)
    if (selectedConversation.value) {
      const freshConversation = conversations.value.find((item) => item.id === selectedConversation.value?.id)
      if (freshConversation) selectedConversation.value = { ...selectedConversation.value, ...freshConversation }
      else {
        selectedConversation.value = null
        messages.value = []
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
  messageLoading.value = true
  try {
    messages.value = await conversationApi.getMessages(conversation.id)
    await connection.value?.invoke('SubscribeConversation', conversation.id)
    const result = await conversationApi.markRead(conversation.id, 'merchant') as { unreadCount?: number }
    const index = conversations.value.findIndex((item) => item.id === conversation.id)
    if (index >= 0) conversations.value[index] = { ...conversations.value[index], merchantUnreadCount: 0 }
    selectedConversation.value = { ...conversation, merchantUnreadCount: 0 }
    if (typeof result?.unreadCount === 'number') conversationStore.setMerchantUnreadCount(result.unreadCount)
    else await conversationStore.loadMerchantUnreadCount(storeId)
    await scrollMessagesToBottom()
  } finally {
    messageLoading.value = false
  }
}

const closeConversation = async () => {
  const conversationId = selectedConversation.value?.id
  if (conversationId) {
    try { await connection.value?.invoke('UnsubscribeConversation', conversationId) } catch {}
  }
  selectedConversation.value = null
  messages.value = []
  replyText.value = ''
}

const handleBack = () => {
  if (isDetailOpen.value) {
    void closeConversation()
    return
  }
  router.back()
}

const sendReply = async () => {
  const content = replyText.value.trim()
  if (!selectedConversation.value || !content || sending.value) return
  sending.value = true
  try {
    const message = await conversationApi.sendMessage(selectedConversation.value.id, {
      senderType: 'merchant',
      senderId: merchantId,
      content,
      storeId,
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
  router.push({ name: 'MerchantOrders', query: { orderId: conversation.orderId } })
}

const subscribeCurrent = async () => {
  if (storeId) await connection.value?.invoke('SubscribeStore', storeId)
  if (selectedConversation.value) await connection.value?.invoke('SubscribeConversation', selectedConversation.value.id)
}

onMounted(async () => {
  await loadConversations()
  try {
    await connect()
    onReconnected(subscribeCurrent)
    connection.value?.on('ConversationMessageReceived', async (payload: ConversationEventPayload) => {
      if (payload.conversationId !== selectedConversation.value?.id) return
      appendMessage(payload.message)
      upsertConversation(payload.conversation)
      if (payload.message.senderType === 'customer') {
        await conversationApi.markRead(payload.conversationId, 'merchant')
        await conversationStore.loadMerchantUnreadCount(storeId)
      }
    })
    connection.value?.on('CustomerMessageReceived', async (payload: ConversationEventPayload) => {
      upsertConversation(payload.conversation)
      if (payload.conversationId === selectedConversation.value?.id) {
        appendMessage(payload.message)
        await conversationApi.markRead(payload.conversationId, 'merchant')
        await conversationStore.loadMerchantUnreadCount(storeId)
      } else if (typeof payload.unreadCount === 'number') {
        conversationStore.setMerchantUnreadCount(payload.unreadCount)
      }
    })
    connection.value?.on('ConversationUnreadChanged', (payload: ConversationUnreadChangedPayload) => {
      if (payload.scope === 'merchant') conversationStore.setMerchantUnreadCount(payload.count)
    })
    await subscribeCurrent()
  } catch {
  }
})

onUnmounted(async () => {
  try {
    if (selectedConversation.value) await connection.value?.invoke('UnsubscribeConversation', selectedConversation.value.id)
    if (storeId) await connection.value?.invoke('UnsubscribeStore', storeId)
  } catch {
  }
  await disconnect()
})
</script>

<template>
  <div class="merchant-messages-page" :class="{ 'is-chat': isDetailOpen }">
    <header class="page-header">
      <button class="back-btn" @click="handleBack">←</button>
      <div>
        <span v-if="!selectedConversation" class="kicker">Messages</span>
        <h2>{{ selectedConversation ? customerLabel(selectedConversation) : '顾客消息' }}</h2>
        <p>{{ selectedConversation ? orderTitle(selectedConversation) : `集中处理订单沟通，未读 ${unreadCount} 条` }}</p>
      </div>
      <button v-if="!selectedConversation" class="refresh-btn" @click="loadConversations">刷新</button>
      <button v-else class="refresh-btn" @click="openOrder(selectedConversation)">订单</button>
    </header>

    <div v-if="!storeId" class="state-card">当前账号未绑定门店，暂无法查看消息。</div>
    <div v-else-if="loading && !selectedConversation" class="state-card">消息加载中...</div>
    <div v-else-if="!selectedConversation && !conversations.length" class="state-card">
      <strong>暂无顾客消息</strong>
      <p>顾客从订单详情发起联系后，会在这里出现。</p>
    </div>

    <div v-else-if="!selectedConversation" class="messages-shell">
      <aside class="conversation-list">
        <button
          v-for="conversation in conversations"
          :key="conversation.id"
          class="conversation-card"
          @click="openConversation(conversation)"
        >
          <div>
            <strong>{{ customerLabel(conversation) }}</strong>
            <span>
              {{ orderTitle(conversation) }}
              <template v-if="conversation.order"> · {{ conversation.order.status }}</template>
            </span>
          </div>
          <small>{{ formatTime(conversation.lastMessageAt) }}</small>
          <em v-if="conversation.merchantUnreadCount > 0">{{ conversation.merchantUnreadCount }}</em>
        </button>
      </aside>
    </div>

    <section v-else class="chat-panel">
      <div class="chat-order">
        <div>
          <strong>{{ orderTitle(selectedConversation) }}</strong>
          <span v-if="selectedConversation.order">
            {{ selectedConversation.order.status }} · {{ formatPrice(selectedConversation.order.actualAmount) }}
          </span>
        </div>
        <button @click="openOrder(selectedConversation)">打开订单</button>
      </div>

      <div ref="messageListRef" class="message-list">
        <div v-if="messageLoading" class="message-state">加载消息中...</div>
        <template v-else>
          <div
            v-for="message in messages"
            :key="message.id"
            class="message-bubble"
            :class="{ mine: message.senderType === 'merchant' }"
          >
            <span>{{ message.content }}</span>
            <small>{{ formatTime(message.createdAt) }}</small>
          </div>
        </template>
      </div>

      <div class="reply-row">
        <input v-model="replyText" placeholder="回复顾客" @keyup.enter="sendReply" />
        <button :disabled="sending || !replyText.trim()" @click="sendReply">
          {{ sending ? '发送中' : '回复' }}
        </button>
      </div>
    </section>
  </div>
</template>

<style scoped lang="scss">
.merchant-messages-page {
  flex: 1;
  width: 100%;
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
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

  h2 {
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
.chat-order button {
  height: 32px;
  border-radius: 6px;
  color: #087E6B;
  background: #E7F4EF;
  font-weight: 900;
}

.back-btn {
  width: 34px;
  color: #1F2A26;
  background: transparent;
  font-size: 22px;
}

.refresh-btn {
  width: 58px;
}

.kicker {
  color: #087E6B;
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.messages-shell {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding: 12px 16px 22px;
}

.conversation-list,
.state-card {
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 6px 18px rgba(31, 42, 38, 0.05);
}

.conversation-list {
  padding: 10px;
  display: grid;
  align-content: start;
  gap: 8px;
}

.conversation-card {
  position: relative;
  min-height: 74px;
  padding: 13px 40px 13px 14px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  display: grid;
  gap: 6px;
  text-align: left;
  background: #FAFCFA;

  &.active {
    border-color: #087E6B;
    background: #F4FBF8;
  }

  strong,
  span,
  small {
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
  overflow: hidden;
  display: grid;
  grid-template-rows: auto minmax(0, 1fr) auto;
  background: #fff;
}

.chat-order {
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

  button {
    height: 32px;
    padding: 0 12px;
    border-radius: 6px;
    color: #087E6B;
    background: #E7F4EF;
    font-weight: 900;
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
  grid-template-columns: minmax(0, 1fr) 82px;
  gap: 8px;
  padding: 10px;
  padding-bottom: calc(10px + env(safe-area-inset-bottom));
  border-top: 1px solid #E2E8E3;
  background: #fff;

  input {
    height: 40px;
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

@media (max-width: 760px) {
  .merchant-messages-page {
    background: #F6F7F3;
  }
}
</style>
