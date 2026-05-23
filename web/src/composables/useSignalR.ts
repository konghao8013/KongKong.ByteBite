import { ref, onUnmounted } from 'vue'
import * as signalR from '@microsoft/signalr'

export function useSignalR(hubUrl: string) {
  const connection = ref<signalR.HubConnection | null>(null)
  const connected = ref(false)

  const connect = async () => {
    const token = localStorage.getItem('customer_token') || localStorage.getItem('merchant_token') || localStorage.getItem('admin_token')
    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build()

    connection.value.onreconnected(() => { connected.value = true })
    connection.value.onreconnecting(() => { connected.value = false })
    connection.value.onclose(() => { connected.value = false })

    await connection.value.start()
    connected.value = true
  }

  const disconnect = async () => {
    if (connection.value) {
      await connection.value.stop()
      connection.value = null
      connected.value = false
    }
  }

  onUnmounted(() => { disconnect() })

  return { connection, connected, connect, disconnect }
}
