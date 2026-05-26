<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { customerApi } from '@/api/modules/customer'
import { useCartStore } from '@/stores/modules/useCartStore'
import type { StoreMenuItemDto } from '@/types/models/customer'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const storeId = localStorage.getItem('current_store_id') || ''
const storeCode = route.params.code as string
const productId = route.params.productId as string

const product = ref<StoreMenuItemDto | null>(null)
const loading = ref(true)
const quantity = ref(1)
const selectedSpecs = ref<Record<string, string>>({})

onMounted(async () => {
  try {
    const menuData = storeId
      ? await customerApi.getStoreMenu(storeId)
      : await customerApi.getStoreMenuByCode(storeCode)
    if (menuData) {
      if (menuData.storeId) localStorage.setItem('current_store_id', menuData.storeId)
      for (const category of menuData.categories) {
        const found = category.items.find((item) => item.id === productId)
        if (found) {
          product.value = found
          for (const group of found.specs) {
            const defaultOption = group.options.find((opt) => opt.isDefault)
            if (defaultOption) {
              selectedSpecs.value[group.id] = defaultOption.id
            } else if (group.options.length > 0) {
              selectedSpecs.value[group.id] = group.options[0].id
            }
          }
          if (found.minOrderQty > 1) {
            quantity.value = found.minOrderQty
          }
          break
        }
      }
    }
  } catch {
    // 加载失败
  } finally {
    loading.value = false
  }
})

const currentPrice = computed(() => {
  if (!product.value) return 0
  let price = product.value.basePrice
  for (const group of product.value.specs) {
    const selectedId = selectedSpecs.value[group.id]
    if (selectedId) {
      const option = group.options.find((opt) => opt.id === selectedId)
      if (option) {
        price += option.extraPrice
      }
    }
  }
  return price
})

const totalPrice = computed(() => currentPrice.value * quantity.value)

const selectSpecOption = (groupId: string, optionId: string) => {
  selectedSpecs.value[groupId] = optionId
}

const decreaseQty = () => {
  const min = product.value?.minOrderQty || 1
  if (quantity.value > min) {
    quantity.value--
  }
}

const increaseQty = () => {
  quantity.value++
}

const addToCart = () => {
  if (!product.value) return

  for (const group of product.value.specs) {
    if (group.isRequired && !selectedSpecs.value[group.id]) {
      return
    }
  }

  const specs: { specGroupId: string; specGroupName: string; optionId: string; optionName: string; extraPrice: number }[] = []
  for (const group of product.value.specs) {
    const selectedId = selectedSpecs.value[group.id]
    if (selectedId) {
      const option = group.options.find((opt) => opt.id === selectedId)
      if (option) {
        specs.push({
          specGroupId: group.id,
          specGroupName: group.name,
          optionId: option.id,
          optionName: option.name,
          extraPrice: option.extraPrice,
        })
      }
    }
  }

  cartStore.addItem({
    productId: product.value.id,
    productName: product.value.name,
    price: currentPrice.value,
    quantity: quantity.value,
    specs,
    remark: '',
    imageUrl: product.value.imageUrl,
  })

  router.back()
}

const goBack = () => {
  router.back()
}

const formatPrice = (price: number) => price.toFixed(2)
</script>

<template>
  <div class="product-detail-page">
    <div v-if="loading" class="loading-wrapper">
      <div class="loading-spinner"></div>
    </div>

    <template v-else-if="product">
      <div class="product-image-section">
        <img
          v-if="product.imageUrl"
          :src="product.imageUrl"
          :alt="product.name"
          class="product-image"
        />
        <div v-else class="product-image-placeholder">
          <span>🍽️</span>
        </div>
        <span class="close-btn" @click="goBack">✕</span>
      </div>

      <div class="product-basic-info">
        <h1 class="product-name">{{ product.name }}</h1>
        <div class="product-meta">
          <span v-if="product.monthlySales > 0" class="monthly-sales">月售{{ product.monthlySales }}份</span>
          <el-tag v-if="product.isCombo" size="small" type="warning">套餐</el-tag>
        </div>
        <div class="product-price-row">
          <span class="current-price">
            <span class="price-symbol">¥</span>
            <span class="price-value">{{ formatPrice(currentPrice) }}</span>
          </span>
          <span v-if="product.specs.length > 0" class="price-from">/起</span>
        </div>
      </div>

      <div v-if="product.description" class="product-description">
        <p>{{ product.description }}</p>
      </div>

      <div v-if="product.specs.length > 0" class="spec-section">
        <div
          v-for="group in product.specs"
          :key="group.id"
          class="spec-group"
        >
          <div class="spec-group-header">
            <span class="spec-group-name">{{ group.name }}</span>
            <span v-if="group.isRequired" class="spec-required">必选</span>
          </div>
          <div class="spec-options">
            <div
              v-for="option in group.options"
              :key="option.id"
              class="spec-option"
              :class="{ active: selectedSpecs[group.id] === option.id }"
              @click="selectSpecOption(group.id, option.id)"
            >
              <span class="option-name">{{ option.name }}</span>
              <span v-if="option.extraPrice > 0" class="option-extra">
                +¥{{ formatPrice(option.extraPrice) }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <div class="quantity-section">
        <span class="quantity-label">数量</span>
        <div class="quantity-control">
          <button
            class="qty-btn minus"
            :class="{ disabled: quantity <= (product.minOrderQty || 1) }"
            @click="decreaseQty"
          >−</button>
          <span class="qty-num">{{ quantity }}</span>
          <button class="qty-btn plus" @click="increaseQty">+</button>
        </div>
      </div>

      <div class="bottom-bar">
        <div class="bar-price">
          <span class="total-label">小计</span>
          <span class="total-price">
            <span class="price-symbol">¥</span>
            <span class="price-value">{{ formatPrice(totalPrice) }}</span>
          </span>
        </div>
        <button
          class="add-cart-btn"
          :class="{ disabled: product.status === 'sold_out' }"
          @click="addToCart"
        >
          {{ product.status === 'sold_out' ? '已售罄' : '加入购物车' }}
        </button>
      </div>
    </template>

    <div v-else class="not-found">
      <p>商品不存在</p>
      <button class="go-back-btn" @click="goBack">返回</button>
    </div>
  </div>
</template>

<style scoped>
.product-detail-page {
  width: 100%;
  min-height: 100vh;
  background: #f6f7f3;
  padding-bottom: 70px;
}

.loading-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
}

.loading-spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #eee;
  border-top-color: #087E6B;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.product-image-section {
  width: 100%;
  height: 280px;
  position: relative;
  overflow: hidden;
}

.product-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.product-image-placeholder {
  width: 100%;
  height: 100%;
  background: #e2e8e3;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 64px;
}

.close-btn {
  position: absolute;
  top: 12px;
  right: 12px;
  width: 32px;
  height: 32px;
  background: rgba(0, 0, 0, 0.4);
  color: #fff;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  cursor: pointer;
}

.product-basic-info {
  background: #fff;
  padding: 16px;
}

.product-name {
  font-size: 20px;
  font-weight: 700;
  color: #333;
  line-height: 1.4;
}

.product-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 6px;
}

.monthly-sales {
  font-size: 13px;
  color: #999;
}

.product-price-row {
  margin-top: 10px;
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.current-price {
  color: #087E6B;
  font-weight: 700;
}

.price-symbol {
  font-size: 14px;
}

.price-value {
  font-size: 24px;
}

.price-from {
  font-size: 13px;
  color: #999;
}

.product-description {
  background: #fff;
  padding: 0 16px 14px;
}

.product-description p {
  font-size: 14px;
  color: #999;
  line-height: 1.6;
}

.spec-section {
  background: #fff;
  padding: 16px;
  margin-top: 10px;
}

.spec-group {
  margin-bottom: 16px;
}

.spec-group:last-child {
  margin-bottom: 0;
}

.spec-group-header {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 10px;
}

.spec-group-name {
  font-size: 15px;
  font-weight: 600;
  color: #333;
}

.spec-required {
  font-size: 11px;
  color: #087E6B;
  background: #FFF0EA;
  padding: 1px 6px;
  border-radius: 4px;
}

.spec-options {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.spec-option {
  padding: 8px 16px;
  border: 1.5px solid #eee;
  border-radius: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s;
  background: #fff;
}

.spec-option.active {
  border-color: #087E6B;
  background: #FFF0EA;
}

.option-name {
  font-size: 14px;
  color: #333;
}

.spec-option.active .option-name {
  color: #087E6B;
  font-weight: 600;
}

.option-extra {
  font-size: 12px;
  color: #999;
}

.spec-option.active .option-extra {
  color: #087E6B;
}

.quantity-section {
  background: #fff;
  padding: 16px;
  margin-top: 10px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.quantity-label {
  font-size: 15px;
  font-weight: 600;
  color: #333;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 12px;
}

.qty-btn {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  border: none;
  font-size: 18px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}

.qty-btn.minus {
  background: #fff;
  border: 1.5px solid #ddd;
  color: #999;
}

.qty-btn.minus.disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.qty-btn.plus {
  background: #087E6B;
  color: #fff;
}

.qty-num {
  font-size: 18px;
  font-weight: 600;
  min-width: 24px;
  text-align: center;
}

.bottom-bar {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 60px;
  background: #FFFFFF; border-top: 1px solid #E2E8E3;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  z-index: 100;
}

.bar-price {
  display: flex;
  flex-direction: column;
}

.total-label {
  font-size: 12px;
  color: #999;
}

.total-price {
  color: #fff;
  font-weight: 700;
}

.total-price .price-symbol {
  font-size: 12px;
}

.total-price .price-value {
  font-size: 22px;
}

.add-cart-btn {
  background: #087E6B;
  color: #fff;
  border: none;
  padding: 12px 28px;
  border-radius: 22px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
}

.add-cart-btn.disabled {
  background: #999;
  cursor: not-allowed;
}

.not-found {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
  gap: 16px;
  color: #999;
}

.go-back-btn {
  background: #087E6B;
  color: #fff;
  border: none;
  padding: 10px 32px;
  border-radius: 20px;
  font-size: 15px;
  cursor: pointer;
}
</style>
