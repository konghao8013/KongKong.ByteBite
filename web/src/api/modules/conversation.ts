import request from '@/api'
import type {
  ConversationDto,
  ConversationMessageDto,
  SendConversationMessageRequest,
  StartConversationRequest,
} from '@/types/models/conversation'

export const conversationApi = {
  startByOrder: (orderId: string, data: StartConversationRequest) =>
    request.post<ConversationDto>(`/orders/${orderId}/conversation`, data),

  getByStore: (storeId: string) =>
    request.get<ConversationDto[]>(`/stores/${storeId}/conversations`),

  getStoreUnreadCount: (storeId: string) =>
    request.get<{ count: number }>(`/stores/${storeId}/conversations/unread-count`),

  getByCustomer: (params: { customerId?: string; deviceId?: string }) =>
    request.get<ConversationDto[]>('/customer-conversations', { params }),

  getCustomerUnreadCount: (params: { customerId?: string; deviceId?: string }) =>
    request.get<{ count: number }>('/customer-conversations/unread-count', { params }),

  getMessages: (conversationId: string) =>
    request.get<ConversationMessageDto[]>(`/conversations/${conversationId}/messages`),

  sendMessage: (conversationId: string, data: SendConversationMessageRequest) =>
    request.post<ConversationMessageDto>(`/conversations/${conversationId}/messages`, data),

  markRead: (conversationId: string, readerType: 'customer' | 'merchant') =>
    request.post(`/conversations/${conversationId}/read`, { readerType }),
}
