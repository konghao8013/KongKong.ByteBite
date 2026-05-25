<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { customerApi } from '@/api/modules/customer'
import { useCartStore } from '@/stores/modules/useCartStore'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import EmptyState from '@/components/common/EmptyState.vue'
import type { StoreMenuDto, StoreMenuItemDto, StoreMenuCategoryDto } from '@/types/models/customer'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const storeCode = route.params.code as string

const menuData = ref<StoreMenuDto | null>(null)
const activeCategory = ref<string>('')
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  try {
    const data = await customerApi.getStoreMenuByCode(storeCode)
    menuData.value = data
    if (data.storeId) localStorage.setItem('current_store_id', data.storeId)
    if (menuData.value?.categories?.length) {
      activeCategory.value = menuData.value.categories[0].id
    }
    const sid = menuData.value?.storeId || storeCode
    cartStore.loadFromLocalStorage(sid)
  } catch {
    error.value = '加载菜单失败，请稍后重试'
  } finally {
    loading.value = false
  }
})

const selectCategory = (category: StoreMenuCategoryDto) => {
  activeCategory.value = category.id
  const el = document.getElementById(`cat-${category.id}`)
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}

const addToCart = (item: StoreMenuItemDto) => {
  if (item.specs.length > 0) {
    router.push({ name: 'ProductDetail', params: { code: storeCode, productId: item.id } })
    return
  }
  cartStore.addItem({
    productId: item.id,
    productName: item.name,
    price: item.basePrice,
    quantity: 1,
    specs: [],
    remark: '',
    imageUrl: item.imageUrl
  })
}

const goToCart = () => {
  router.push({ name: 'Cart' })
}

const goToOrders = () => {
  router.push({ name: 'MyOrders' })
}

const getItemCartQuantity = (productId: string) => {
  const item = cartStore.items.find((i) => i.productId === productId)
  return item ? item.quantity : 0
}

const handleDecrease = (productId: string) => {
  const idx = cartStore.items.findIndex((i) => i.productId === productId)
  if (idx !== -1) {
    cartStore.updateQuantity(idx, cartStore.items[idx].quantity - 1)
  }
}
</script>

<template>
  <div class="store-menu">
    <!-- 顶部店铺信息 -->
    <div class="store-header">
      <div class="store-info">
        <h1 class="store-name">{{ menuData?.storeName || '加载中...' }}</h1>
        <p v-if="menuData?.description" class="store-desc">{{ menuData.description }}</p>
      </div>
      <span class="order-link" @click="goToOrders">我的订单</span>
    </div>

    <!-- 商家休息中提示 -->
    <div v-if="menuData && !menuData.canOrder" class="closed-banner">
      <span>💤</span>
      <span>商家休息中，暂不可下单</span>
    </div>

    <!-- 加载状态 -->
    <LoadingSpinner v-if="loading" text="加载菜单中..." />

    <!-- 错误状态 -->
    <EmptyState v-else-if="error" :description="error" icon="😔">
      <button @click="loading = true; error = ''">重新加载</button>
    </EmptyState>

    <!-- 菜单主体 -->
    <div v-else-if="menuData" class="menu-body">
      <!-- 左侧分类列表 -->
      <div class="category-sidebar">
        <div
          v-for="category in menuData.categories"
          :key="category.id"
          class="category-item"
          :class="{ active: activeCategory === category.id }"
          @click="selectCategory(category)"
        >
          <span class="category-name">{{ category.name }}</span>
          <span v-if="category.items.length" class="category-count">{{ category.items.length }}</span>
        </div>
      </div>

      <!-- 右侧商品列表 -->
      <div class="product-list">
        <div
          v-for="category in menuData.categories"
          :id="`cat-${category.id}`"
          :key="category.id"
          class="product-section"
        >
          <div class="section-title">{{ category.name }}</div>
          <div
            v-for="item in category.items"
            :key="item.id"
            class="product-card"
          >
            <div class="product-image-wrapper">
              <img v-if="item.imageUrl" :src="item.imageUrl" :alt="item.name" class="product-image" />
              <div v-else class="product-image-placeholder">🍽️</div>
              <span v-if="item.status === 'sold_out'" class="sold-out-badge">售罄</span>
            </div>
            <div class="product-info">
              <h3 class="product-name">{{ item.name }}</h3>
              <p v-if="item.description" class="product-desc">{{ item.description }}</p>
              <div class="product-meta">
                <span v-if="item.monthlySales > 0" class="monthly-sales">月售{{ item.monthlySales }}份</span>
                <el-tag v-if="item.isCombo" size="small" type="warning" class="combo-tag">套餐</el-tag>
              </div>
              <div class="product-bottom">
                <div class="product-price">
                  <span class="price-symbol">¥</span>
                  <span class="price-value">{{ item.fromPrice.toFixed(2) }}</span>
                  <span v-if="item.specs.length > 0 || item.fromPrice !== item.basePrice" class="price-from">起</span>
                </div>
                <div class="product-action">
                  <template v-if="item.status !== 'sold_out'">
                    <div v-if="getItemCartQuantity(item.id) > 0" class="quantity-control">
                      <button class="qty-btn minus" @click.stop="handleDecrease(item.id)">−</button>
                      <span class="qty-num">{{ getItemCartQuantity(item.id) }}</span>
                      <button class="qty-btn plus" @click.stop="addToCart(item)">+</button>
                    </div>
                    <button
                      v-else
                      class="add-btn"
                      :class="{ 'has-spec': item.specs.length > 0 }"
                      @click.stop="addToCart(item)"
                    >
                      {{ item.specs.length > 0 ? '选规格' : '+' }}
                    </button>
                  </template>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="list-bottom-spacer" />
      </div>
    </div>

    <!-- 底部购物车栏 -->
    <div v-if="cartStore.totalCount > 0" class="cart-bar" @click="goToCart">
      <div class="cart-bar-left">
        <div class="cart-icon-wrapper">
          <span class="cart-icon">🛒</span>
          <span class="cart-badge">{{ cartStore.totalCount }}</span>
        </div>
        <div class="cart-price">
          <span class="price-symbol">¥</span>
          <span class="price-value">{{ cartStore.totalPrice.toFixed(2) }}</span>
        </div>
      </div>
      <div class="cart-bar-right" :class="{ disabled: !menuData?.canOrder }">
        {{ menuData?.canOrder ? '去结算' : '暂不可下单' }}
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.store-menu {
  display: flex;
  flex-direction: column;
  min-height: 100%;
}

/* 顶部店铺信息 */
.store-header {
  background: $primary-color;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.store-name {
  font-size: 18px;
  font-weight: 700;
}

.store-desc {
  font-size: 12px;
  color: #666;
  margin-top: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 240px;
}

.order-link {
  font-size: 14px;
  color: $brand-color;
  cursor: pointer;
  font-weight: 500;
  flex-shrink: 0;
}

/* 商家休息中提示 */
.closed-banner {
  background: #fff3e0;
  padding: 10px 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #e65100;
}

/* 菜单主体 */
.menu-body {
  flex: 1;
  display: flex;
  overflow: hidden;
}

/* 左侧分类 */
.category-sidebar {
  width: 88px;
  flex-shrink: 0;
  background: #f7f7f7;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}

.category-item {
  padding: 14px 8px;
  font-size: 13px;
  color: #666;
  text-align: center;
  position: relative;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;

  &.active {
    background: #fff;
    color: $text-color;
    font-weight: 600;

    &::before {
      content: '';
      position: absolute;
      left: 0;
      top: 50%;
      transform: translateY(-50%);
      width: 3px;
      height: 20px;
      background: $brand-color;
      border-radius: 0 2px 2px 0;
    }
  }
}

.category-name {
  line-height: 1.3;
}

.category-count {
  font-size: 10px;
  color: #bbb;
}

/* 右侧商品列表 */
.product-list {
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
  padding: 0 12px;
}

.product-section {
  margin-bottom: 8px;
}

.section-title {
  font-size: 14px;
  font-weight: 600;
  padding: 12px 0 8px;
  position: sticky;
  top: 0;
  background: $bg-color;
  z-index: 1;
}

.product-card {
  display: flex;
  gap: 10px;
  background: #fff;
  border-radius: 8px;
  padding: 10px;
  margin-bottom: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.product-image-wrapper {
  width: 90px;
  height: 90px;
  flex-shrink: 0;
  border-radius: 6px;
  overflow: hidden;
  position: relative;
}

.product-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.product-image-placeholder {
  width: 100%;
  height: 100%;
  background: #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
}

.sold-out-badge {
  position: absolute;
  top: 0;
  left: 0;
  background: rgba(0, 0, 0, 0.5);
  color: #fff;
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 0 0 6px 0;
}

.product-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.product-name {
  font-size: 15px;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-desc {
  font-size: 12px;
  color: $text-secondary;
  margin-top: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 4px;
}

.monthly-sales {
  font-size: 11px;
  color: #bbb;
}

.combo-tag {
  transform: scale(0.85);
}

.product-bottom {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: auto;
  padding-top: 6px;
}

.product-price {
  color: $brand-color;
  font-weight: 700;
}

.price-symbol {
  font-size: 12px;
}

.price-value {
  font-size: 18px;
}

.price-from {
  font-size: 11px;
  color: $text-secondary;
  margin-left: 2px;
}

.product-action {
  flex-shrink: 0;
}

.add-btn {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: $brand-color;
  color: #fff;
  font-size: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;

  &.has-spec {
    width: auto;
    border-radius: 14px;
    padding: 0 12px;
    font-size: 12px;
  }
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 6px;
}

.qty-btn {
  width: 24px;
  height: 24px;
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
    background: $brand-color;
    color: #fff;
  }
}

.qty-num {
  font-size: 14px;
  font-weight: 600;
  min-width: 16px;
  text-align: center;
}

.list-bottom-spacer {
  height: 70px;
}

/* 底部购物车栏 */
.cart-bar {
  position: fixed;
  bottom: calc(50px + env(safe-area-inset-bottom));
  left: 0;
  right: 0;
  height: 56px;
  background: $dark-bar;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  z-index: 100;
  cursor: pointer;
}

.cart-bar-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.cart-icon-wrapper {
  position: relative;
  width: 44px;
  height: 44px;
  background: #3a3a3a;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: -14px;
}

.cart-icon {
  font-size: 22px;
}

.cart-badge {
  position: absolute;
  top: -4px;
  right: -4px;
  background: $brand-color;
  color: #fff;
  font-size: 11px;
  min-width: 18px;
  height: 18px;
  border-radius: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
}

.cart-price {
  color: #fff;
  font-weight: 700;

  .price-symbol {
    font-size: 12px;
  }

  .price-value {
    font-size: 20px;
  }
}

.cart-bar-right {
  background: $brand-color;
  color: #fff;
  padding: 10px 24px;
  border-radius: 20px;
  font-size: 15px;
  font-weight: 600;

  &.disabled {
    background: #666;
    cursor: not-allowed;
  }
}
</style>
