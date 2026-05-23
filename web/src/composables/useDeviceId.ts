import { ref } from 'vue'

const DEVICE_ID_KEY = 'kongkong_bytebite_device_id'

export function useDeviceId() {
  const deviceId = ref<string>('')

  const getDeviceId = (): string => {
    let id = localStorage.getItem(DEVICE_ID_KEY)
    if (!id) {
      id = generateDeviceId()
      localStorage.setItem(DEVICE_ID_KEY, id)
    }
    deviceId.value = id
    return id
  }

  const generateDeviceId = (): string => {
    const timestamp = Date.now().toString(36)
    const random = Math.random().toString(36).substring(2, 10)
    return `dev_${timestamp}_${random}`
  }

  const resetDeviceId = () => {
    const newId = generateDeviceId()
    localStorage.setItem(DEVICE_ID_KEY, newId)
    deviceId.value = newId
  }

  return { deviceId, getDeviceId, resetDeviceId }
}
