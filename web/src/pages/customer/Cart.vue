<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useCartStore } from '@/stores/modules/useCartStore'
import { useOrderStore } from '@/stores/modules/useOrderStore'
import { customerApi } from '@/api/modules/customer'
import { formatPrice } from '@/utils/format'
import { useDeviceId } from '@/composables/useDeviceId'
import EmptyState from '@/components/common/EmptyState.vue'
import type { CreateOrderData } from '@/types/models/customer'
import { normalizeCustomerOrder } from '@/utils/order'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const orderStore = useOrderStore()
const { getDeviceId } = useDeviceId()
const storeCode = route.params.code as string
const storeId = ref(localStorage.getItem('current_store_id') || '')

const diningMode = ref<string>('dine_in')
const tableNo = ref('')
const deliveryAddress = ref('')
const deliveryPhone = ref('')
const orderRemark = ref('')
const submitting = ref(false)

const diningModeOptions = [
  { label: '堂食', value: 'dine_in', icon: '🪑' },
  { label: '打包', value: 'takeaway', icon: '🥡' },
  { label: '外卖', value: 'delivery', icon: '🛵' }
]

const isEmpty = computed(() => cartStore.items.length === 0)

onMounted(async () => {
  if (storeId.value) return
  try {
    const menu = await customerApi.getStoreMenuByCode(storeCode)
    if (menu.storeId) {
      storeId.value = menu.storeId
      localStorage.setItem('current_store_id', menu.storeId)
    }
  } catch {
  }
})

const goBack = () => {
  router.back()
}

const goToMenu = () => {
  router.push({ name: 'StoreMenu', params: { code: storeCode } })
}

const decreaseQuantity = (index: number) => {
  cartStore.updateQuantity(index, cartStore.items[index].quantity - 1)
}

const increaseQuantity = (index: number) => {
  cartStore.updateQuantity(index, cartStore.items[index].quantity + 1)
}

const removeItem = (index: number) => {
  cartStore.removeItem(index)
}

const submitOrder = async () => {
  if (isEmpty.value || submitting.value) return
  if (!storeId.value) return

  submitting.value = true
  try {
    const data: CreateOrderData = {
      storeId: storeId.value,
      customerId: localStorage.getItem('customer_id') || undefined,
      deviceId: getDeviceId(),
      items: cartStore.items.map((item) => ({
        productId: item.productId,
        quantity: item.quantity,
        selectedSpecOptionIds: item.specs.map((s) => s.optionId),
        remark: item.remark || undefined
      })),
      diningMode: diningMode.value,
      tableNo: diningMode.value === 'dine_in' ? tableNo.value || undefined : undefined,
      deliveryAddress: diningMode.value === 'delivery' ? deliveryAddress.value || undefined : undefined,
      deliveryPhone: diningMode.value === 'delivery' ? deliveryPhone.value || undefined : undefined,
      remark: orderRemark.value || undefined
    }

    const orderData = normalizeCustomerOrder(
      await customerApi.createOrder(data),
      localStorage.getItem('current_store_name') || ''
    )
    orderStore.addActiveOrder(orderData)
    cartStore.clearCart()
    router.push({ name: 'OrderDetail', params: { code: storeCode, orderId: orderData.id } })
  } catch {
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="cart-page">
    <!-- 顶部导航 -->
    <div class="page-header">
      <span class="back-btn" @click="goBack">←</span>
      <h1 class="page-title">购物车</h1>
      <span v-if="!isEmpty" class="clear-btn" @click="cartStore.clearCart()">清空</span>
    </div>

    <!-- 空状态 -->
    <EmptyState v-if="isEmpty" description="购物车空空如也" icon="🛒">
      <button @click="goToMenu">去点单</button>
    </EmptyState>

    <!-- 购物车内容 -->
    <template v-else>
      <!-- 就餐方式选择 -->
      <div class="dining-mode-section">
        <div class="section-label">就餐方式</div>
        <div class="dining-mode-options">
          <div
            v-for="option in diningModeOptions"
            :key="option.value"
            class="mode-option"
            :class="{ active: diningMode === option.value }"
            @click="diningMode = option.value"
          >
            <span class="mode-icon">{{ option.icon }}</span>
            <span class="mode-label">{{ option.label }}</span>
          </div>
        </div>
      </div>

      <!-- 堂食：桌号输入 -->
      <div v-if="diningMode === 'dine_in'" class="extra-info-section">
        <div class="section-label">桌号（选填）</div>
        <el-input v-model="tableNo" placeholder="输入桌号，如：A3" clearable />
      </div>

      <!-- 外卖：配送信息 -->
      <div v-if="diningMode === 'delivery'" class="extra-info-section">
        <div class="section-label">配送信息</div>
        <el-input v-model="deliveryAddress" placeholder="配送地址" clearable class="delivery-input" />
        <el-input v-model="deliveryPhone" placeholder="联系电话" clearable class="delivery-input" />
      </div>

      <!-- 商品列表 -->
      <div class="cart-items">
        <div
          v-for="(item, index) in cartStore.items"
          :key="`${item.productId}-${index}`"
          class="cart-item"
        >
          <div class="item-main">
            <div class="item-info">
              <h3 class="item-name">{{ item.productName }}</h3>
              <div v-if="item.specs.length > 0" class="item-specs">
                <el-tag
                  v-for="spec in item.specs"
                  :key="spec.optionId"
                  size="small"
                  type="info"
                  class="spec-tag"
                >
                  {{ spec.optionName }}
                </el-tag>
              </div>
              <div class="item-price-row">
                <span class="item-price">{{ formatPrice(item.price * item.quantity) }}</span>
                <span v-if="item.quantity > 1" class="item-unit-price">
                  {{ formatPrice(item.price) }}/份
                </span>
              </div>
            </div>
            <div class="item-actions">
              <div class="quantity-control">
                <button class="qty-btn minus" @click="decreaseQuantity(index)">−</button>
                <span class="qty-num">{{ item.quantity }}</span>
                <button class="qty-btn plus" @click="increaseQuantity(index)">+</button>
              </div>
              <span class="delete-btn" @click="removeItem(index)">删除</span>
            </div>
          </div>
          <div class="item-remark">
            <el-input
              v-model="item.remark"
              placeholder="备注（如：少冰、不加葱）"
              size="small"
              clearable
            />
          </div>
        </div>
      </div>

      <!-- 订单备注 -->
      <div class="order-remark-section">
        <div class="section-label">订单备注</div>
        <el-input
          v-model="orderRemark"
          type="textarea"
          placeholder="对订单有什么想说的..."
          :rows="2"
          resize="none"
        />
      </div>

      <!-- 底部结算栏 -->
      <div class="cart-footer">
        <div class="footer-left">
          <div class="footer-total">
            <span class="total-label">合计</span>
            <span class="total-price">
              <span class="price-symbol">¥</span>
              <span class="price-value">{{ cartStore.totalPrice.toFixed(2) }}</span>
            </span>
          </div>
          <span class="footer-count">共{{ cartStore.totalCount }}件</span>
        </div>
        <button
          class="submit-btn"
          :class="{ disabled: submitting }"
          @click="submitOrder"
        >
          {{ submitting ? '提交中...' : '提交订单' }}
        </button>
      </div>
    </template>
  </div>
</template>

<style scoped lang="scss">
.cart-page {
  display: flex;
  flex-direction: column;
  min-height: 100%;
}

/* 顶部导航 */
.page-header {
  background: rgba(255, 255, 255, 0.96);
  padding: 12px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 10;
  border-bottom: 1px solid $border-color;
  backdrop-filter: blur(12px);
}

.back-btn {
  font-size: 22px;
  cursor: pointer;
  width: 32px;
}

.page-title {
  font-size: 17px;
  font-weight: 800;
  color: $text-color;
}

.clear-btn {
  font-size: 14px;
  color: $primary-color;
  cursor: pointer;
  font-weight: 700;
}

/* 就餐方式 */
.dining-mode-section {
  background: #fff;
  padding: 14px 16px;
  margin-bottom: 10px;
  border: 1px solid $border-color;
  border-left: 0;
  border-right: 0;
}

.section-label {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 10px;
}

.dining-mode-options {
  display: flex;
  gap: 10px;
}

.mode-option {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 10px 0;
  border: 2px solid $border-color;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s;

  &.active {
    border-color: $primary-color;
    background: $primary-light;
  }
}

.mode-icon {
  font-size: 22px;
}

.mode-label {
  font-size: 13px;

  .mode-option.active & {
    color: $brand-color;
    font-weight: 600;
  }
}

.extra-info-section {
  background: #fff;
  padding: 14px 16px;
  margin-bottom: 10px;

  .delivery-input {
    margin-bottom: 8px;

    &:last-child { margin-bottom: 0; }
  }
}

/* 商品列表 */
.cart-items {
  background: #fff;
  padding: 0 16px;
  margin-bottom: 10px;
  border-top: 1px solid $border-color;
  border-bottom: 1px solid $border-color;
}

.cart-item {
  padding: 14px 0;
  border-bottom: 1px solid $border-color;

  &:last-child {
    border-bottom: none;
  }
}

.item-main {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.item-info {
  flex: 1;
  min-width: 0;
}

.item-name {
  font-size: 15px;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.item-specs {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-top: 6px;
}

.spec-tag {
  transform: scale(0.85);
}

.item-price-row {
  margin-top: 6px;
  display: flex;
  align-items: baseline;
  gap: 6px;
}

.item-price {
  font-size: 16px;
  font-weight: 700;
  color: $accent-color;
}

.item-unit-price {
  font-size: 12px;
  color: $text-secondary;
}

.item-actions {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 8px;
  flex-shrink: 0;
  margin-left: 12px;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 8px;
}

.qty-btn {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  font-size: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;

  &.minus {
    background: #fff;
    border: 1px solid #ddd;
    color: #999;
  }

  &.plus {
    background: $primary-color;
    color: #fff;
  }
}

.qty-num {
  font-size: 15px;
  font-weight: 600;
  min-width: 20px;
  text-align: center;
}

.delete-btn {
  font-size: 12px;
  color: #ccc;
  cursor: pointer;
}

.item-remark {
  margin-top: 8px;
}

/* 订单备注 */
.order-remark-section {
  background: #fff;
  padding: 14px 16px;
  margin-bottom: 80px;
}

/* 底部结算栏 */
.cart-footer {
  position: fixed;
  bottom: calc(50px + env(safe-area-inset-bottom));
  left: 0;
  right: 0;
  height: 60px;
  background: $dark-bar;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  z-index: 100;
}

.footer-left {
  display: flex;
  flex-direction: column;
}

.footer-total {
  display: flex;
  align-items: baseline;
  gap: 6px;
}

.total-label {
  font-size: 13px;
  color: #999;
}

.total-price {
  color: #fff;
  font-weight: 700;

  .price-symbol {
    font-size: 12px;
  }

  .price-value {
    font-size: 22px;
  }
}

.footer-count {
  font-size: 11px;
  color: #999;
  margin-top: 2px;
}

.submit-btn {
  background: $primary-color;
  color: #fff;
  padding: 12px 28px;
  border-radius: 22px;
  font-size: 16px;
  font-weight: 600;

  &.disabled {
    opacity: 0.7;
    cursor: not-allowed;
  }
}
</style>
