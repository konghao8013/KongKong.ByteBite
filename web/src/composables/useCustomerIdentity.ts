import { customerApi } from '@/api/modules/customer'
import { useDeviceId } from '@/composables/useDeviceId'

const CUSTOMER_ID_KEY = 'customer_id'
const CUSTOMER_INFO_KEY = 'customer_info'

export interface CustomerIdentity {
  customerId?: string
  deviceId: string
  isRegistered: boolean
}

let ensurePromise: Promise<CustomerIdentity> | null = null

export function useCustomerIdentity() {
  const { getDeviceId } = useDeviceId()

  const getCustomerIdentity = (): CustomerIdentity => ({
    customerId: localStorage.getItem(CUSTOMER_ID_KEY) || undefined,
    deviceId: getDeviceId(),
    isRegistered: Boolean(localStorage.getItem('customer_token')),
  })

  const ensureCustomerIdentity = async (): Promise<CustomerIdentity> => {
    const current = getCustomerIdentity()
    if (current.customerId) return current
    if (ensurePromise) return ensurePromise

    ensurePromise = customerApi.ensureAnonymous(current.deviceId)
      .then((customer) => {
        localStorage.setItem(CUSTOMER_ID_KEY, customer.id)
        localStorage.setItem(CUSTOMER_INFO_KEY, JSON.stringify(customer))
        return {
          customerId: customer.id,
          deviceId: current.deviceId,
          isRegistered: customer.isRegistered,
        }
      })
      .catch(() => current)
      .finally(() => {
        ensurePromise = null
      })

    return ensurePromise
  }

  return { getCustomerIdentity, ensureCustomerIdentity }
}
