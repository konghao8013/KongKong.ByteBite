import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'

type ReconnectedHandler = (connectionId?: string) => void | Promise<void>

export function useSignalR(hubUrl: string) {
  const connection = ref<signalR.HubConnection | null>(null)
  const connected = ref(false)
  let connectingPromise: Promise<void> | null = null
  let reconnectTimer: ReturnType<typeof setTimeout> | null = null
  let stoppedByUser = false
  const reconnectedHandlers = new Set<ReconnectedHandler>()

  const getAccessToken = () =>
    localStorage.getItem('customer_token') || localStorage.getItem('merchant_token') || localStorage.getItem('admin_token') || ''

  const notifyReconnected = async (connectionId?: string) => {
    for (const handler of reconnectedHandlers) {
      try {
        await handler(connectionId)
      } catch {
      }
    }
  }

  const scheduleReconnect = () => {
    if (stoppedByUser || reconnectTimer) return
    reconnectTimer = setTimeout(() => {
      reconnectTimer = null
      connect().catch(scheduleReconnect)
    }, 3000)
  }

  const connect = async () => {
    stoppedByUser = false
    if (reconnectTimer) {
      clearTimeout(reconnectTimer)
      reconnectTimer = null
    }
    if (connection.value && connection.value.state !== signalR.HubConnectionState.Disconnected) {
      connected.value = connection.value.state === signalR.HubConnectionState.Connected
      return
    }
    if (connectingPromise) return connectingPromise

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: getAccessToken,
      })
      .withAutomaticReconnect([0, 1000, 3000, 5000, 10000])
      .build()

    connection.value.onreconnected(async (connectionId) => {
      connected.value = true
      await notifyReconnected(connectionId)
    })
    connection.value.onreconnecting(() => { connected.value = false })
    connection.value.onclose(() => {
      connected.value = false
      scheduleReconnect()
    })

    connectingPromise = connection.value.start()
      .then(async () => {
        connected.value = true
        await notifyReconnected(connection.value?.connectionId || undefined)
      })
      .catch((error) => {
        scheduleReconnect()
        throw error
      })
      .finally(() => { connectingPromise = null })
    await connectingPromise
  }

  const disconnect = async () => {
    stoppedByUser = true
    if (reconnectTimer) {
      clearTimeout(reconnectTimer)
      reconnectTimer = null
    }
    const current = connection.value
    if (!current) return
    if (current.state !== signalR.HubConnectionState.Disconnected) {
      await current.stop()
    }
    if (connection.value === current) connection.value = null
    connected.value = false
  }

  const onReconnected = (handler: ReconnectedHandler) => {
    reconnectedHandlers.add(handler)
    return () => reconnectedHandlers.delete(handler)
  }

  return { connection, connected, connect, disconnect, onReconnected }
}
