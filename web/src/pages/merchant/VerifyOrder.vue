<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { orderApi } from '@/api/modules/order'
import { customerApi } from '@/api/modules/customer'
import { normalizeCustomerOrder } from '@/utils/order'
import { formatPrice, formatDate } from '@/utils/format'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { OrderDto } from '@/types/models/order'

const route = useRoute()
const router = useRouter()

const loading = ref(true)
const completing = ref(false)
const error = ref('')
const order = ref<OrderDto | null>(null)

const pickupCode = computed(() => String(route.query.pickupCode || '').trim())
const storeCode = computed(() => String(route.query.storeCode || '').trim())

const resolveStoreId = async () => {
  const localStoreId = localStorage.getItem('merchant_store_id')
  if (localStoreId) return localStoreId
  if (!storeCode.value) return ''
  const menu = await customerApi.getStoreMenuByCode(storeCode.value)
  return menu.storeId
}

const loadOrder = async () => {
  loading.value = true
  error.value = ''
  try {
    const storeId = await resolveStoreId()
    if (!storeId || !pickupCode.value) {
      error.value = '缺少取货码或店铺信息'
      return
    }
    order.value = normalizeCustomerOrder(await orderApi.getByPickupCode(pickupCode.value, storeId))
  } catch {
    error.value = '没有找到对应订单'
  } finally {
    loading.value = false
  }
}

const completeOrder = async () => {
  if (!order.value || order.value.status !== 'ready' || completing.value) return
  completing.value = true
  try {
    order.value = normalizeCustomerOrder(await orderApi.completeOrder(order.value.id), order.value.storeName)
    ElMessage.success('核销完成')
  } finally {
    completing.value = false
  }
}

onMounted(loadOrder)
</script>

<template>
  <div class="verify-page">
    <header class="page-header">
      <button @click="router.back()">‹</button>
      <h1>取货核销</h1>
      <span />
    </header>

    <LoadingSpinner v-if="loading" text="读取订单中..." />

    <div v-else-if="error" class="verify-card">
      <h2>{{ error }}</h2>
      <p>请确认二维码来自当前店铺，或回到订单页手动输入取货码。</p>
      <button @click="router.push('/merchant/orders')">返回订单</button>
    </div>

    <div v-else-if="order" class="verify-card">
      <div class="pickup-code">#{{ order.pickupCode }}</div>
      <h2>{{ order.storeName }}</h2>
      <p>{{ formatDate(order.createdAt) }} · {{ order.diningMode }}</p>

      <div class="info-row">
        <span>订单状态</span>
        <strong>{{ order.status }}</strong>
      </div>
      <div class="info-row">
        <span>实付金额</span>
        <strong>{{ formatPrice(order.actualAmount) }}</strong>
      </div>

      <button
        class="complete-btn"
        :disabled="order.status !== 'ready' || completing"
        @click="completeOrder"
      >
        {{ order.status === 'ready' ? (completing ? '核销中...' : '确认核销') : '仅待取餐订单可核销' }}
      </button>
    </div>
  </div>
</template>

<style scoped lang="scss">
.verify-page {
  min-height: 100vh;
  padding: 0 16px 32px;
  background: #F6F7F3;
}

.page-header {
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: space-between;

  button {
    width: 36px;
    height: 36px;
    border-radius: 6px;
    background: #E7F4EF;
    font-size: 24px;
  }

  h1 {
    margin: 0;
    font-size: 18px;
  }

  span {
    width: 36px;
  }
}

.verify-card {
  margin-top: 18px;
  padding: 22px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  text-align: center;
  box-shadow: 0 8px 24px rgba(31, 42, 38, 0.08);
}

.pickup-code {
  color: #087E6B;
  font-size: 34px;
  font-weight: 900;
  letter-spacing: 4px;
}

h2 {
  margin: 12px 0 6px;
  color: #1F2A26;
}

p {
  margin: 0 0 18px;
  color: #687872;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-top: 1px solid #E2E8E3;
}

.complete-btn,
.verify-card > button {
  width: 100%;
  height: 42px;
  margin-top: 18px;
  border-radius: 8px;
  color: #fff;
  background: #087E6B;
  font-weight: 900;

  &:disabled {
    background: #9AA9A3;
  }
}
</style>
