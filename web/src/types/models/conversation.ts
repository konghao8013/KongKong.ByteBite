export interface ConversationOrderSummaryDto {
  id: string
  orderNo: string
  pickupCode: string
  status: string
  diningMode: string
  tableNo?: string
  totalAmount: number
  actualAmount: number
  createdAt: string
}

export interface ConversationCustomerSummaryDto {
  id: string
  phone?: string
  username?: string
  nickname?: string
}

export interface ConversationDto {
  id: string
  orderId: string
  storeId: string
  storeName?: string
  storeCode?: string
  customerId?: string
  deviceId?: string
  lastMessageAt: string
  customerUnreadCount: number
  merchantUnreadCount: number
  order?: ConversationOrderSummaryDto
  customer?: ConversationCustomerSummaryDto
}

export interface ConversationMessageDto {
  id: string
  conversationId: string
  senderType: 'customer' | 'merchant'
  senderId?: string
  content: string
  messageType: string
  readAt?: string
  createdAt: string
}

export interface StartConversationRequest {
  customerId?: string
  deviceId?: string
}

export interface SendConversationMessageRequest {
  senderType: 'customer' | 'merchant'
  senderId?: string
  content: string
  storeId?: string
  orderId?: string
}

export interface ConversationEventPayload {
  conversationId: string
  message: ConversationMessageDto
  conversation?: ConversationDto
  unreadCount?: number
}

export interface ConversationUnreadChangedPayload {
  scope: 'customer' | 'merchant'
  storeId?: string
  customerId?: string
  deviceId?: string
  count: number
}
