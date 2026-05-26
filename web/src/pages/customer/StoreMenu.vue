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
    if (data.storeName) localStorage.setItem('current_store_name', data.storeName)
    if (menuData.value?.categories?.length) {
      activeCategory.value = menuData.value.categories[0].id
    }
    const sid = menuData.value?.storeId || storeCode
    cartStore.loadFromLocalStorage(sid)
  } catch {
    error.value = '菜单加载失败，请稍后重试'
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
  router.push({ name: 'Cart', params: { code: storeCode } })
}

const goToOrders = () => {
  router.push({ name: 'MyOrders', params: { code: storeCode } })
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
    <header class="store-header">
      <div class="store-title-row">
        <div>
          <h1 class="store-name">{{ menuData?.storeName || '加载中...' }}</h1>
          <p v-if="menuData?.description" class="store-desc">{{ menuData.description }}</p>
        </div>
        <button class="order-link" @click="goToOrders">我的订单</button>
      </div>
      <div class="store-meta">
        <span :class="{ muted: !menuData?.canOrder }">{{ menuData?.canOrder ? '营业中' : '休息中' }}</span>
        <span>堂食</span>
        <span>外带</span>
        <span v-if="menuData?.diningMode?.includes('delivery')">配送</span>
      </div>
      <div class="menu-search">搜索菜品、套餐、饮品</div>
    </header>

    <div v-if="menuData && !menuData.canOrder" class="closed-banner">
      <span>商家休息中，暂不可下单</span>
    </div>

    <LoadingSpinner v-if="loading" text="加载菜单中..." />

    <EmptyState v-else-if="error" :description="error" icon="!">
      <button class="retry-btn" @click="loading = true; error = ''">重新加载</button>
    </EmptyState>

    <div v-else-if="menuData" class="menu-body">
      <aside class="category-sidebar">
        <button
          v-for="category in menuData.categories"
          :key="category.id"
          class="category-item"
          :class="{ active: activeCategory === category.id }"
          @click="selectCategory(category)"
        >
          <span class="category-name">{{ category.name }}</span>
          <span v-if="category.items.length" class="category-count">{{ category.items.length }} 件</span>
        </button>
      </aside>

      <main class="product-list">
        <section
          v-for="category in menuData.categories"
          :id="`cat-${category.id}`"
          :key="category.id"
          class="product-section"
        >
          <h2 class="section-title">{{ category.name }}</h2>
          <article
            v-for="item in category.items"
            :key="item.id"
            class="product-card"
          >
            <div class="product-image-wrapper">
              <img v-if="item.imageUrl" :src="item.imageUrl" :alt="item.name" class="product-image" />
              <div v-else class="product-image-placeholder">餐</div>
              <span v-if="item.status === 'sold_out'" class="sold-out-badge">售罄</span>
            </div>
            <div class="product-info">
              <div>
                <h3 class="product-name">{{ item.name }}</h3>
                <p v-if="item.description" class="product-desc">{{ item.description }}</p>
              </div>
              <div class="product-meta">
                <span v-if="item.monthlySales > 0" class="monthly-sales">月售 {{ item.monthlySales }} 份</span>
                <span v-if="item.isCombo" class="combo-tag">套餐</span>
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
                      <button class="qty-btn minus" @click.stop="handleDecrease(item.id)">-</button>
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
          </article>
        </section>
        <div class="list-bottom-spacer" />
      </main>
    </div>

    <div v-if="cartStore.totalCount > 0" class="cart-bar" @click="goToCart">
      <div class="cart-bar-left">
        <div class="cart-icon-wrapper">
          <span class="cart-icon">购</span>
          <span class="cart-badge">{{ cartStore.totalCount }}</span>
        </div>
        <div>
          <div class="cart-price">
            <span class="price-symbol">¥</span>
            <span class="price-value">{{ cartStore.totalPrice.toFixed(2) }}</span>
          </div>
          <span class="cart-hint">已选 {{ cartStore.totalCount }} 件</span>
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
  height: calc(100dvh - 58px - env(safe-area-inset-bottom));
  min-height: 0;
  overflow: hidden;
  background: #F6F7F3;
}

.store-header {
  flex-shrink: 0;
  padding: 16px 16px 12px;
  background: rgba(255, 255, 255, 0.96);
  border-bottom: 1px solid #E2E8E3;
  z-index: 5;
}

.store-title-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.store-name {
  margin: 0;
  color: #1F2A26;
  font-size: 19px;
  font-weight: 800;
  line-height: 1.25;
}

.store-desc {
  max-width: 520px;
  margin: 5px 0 0;
  color: #687872;
  font-size: 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.order-link {
  flex-shrink: 0;
  height: 32px;
  padding: 0 11px;
  border: 1px solid #E2E8E3;
  border-radius: 6px;
  color: #087E6B;
  background: #fff;
  font-size: 12px;
  font-weight: 700;
}

.store-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 10px;

  span {
    padding: 4px 8px;
    border-radius: 999px;
    color: #087E6B;
    background: #E7F4EF;
    font-size: 11px;
    font-weight: 700;
  }

  .muted {
    color: #687872;
    background: #F1F4F2;
  }
}

.menu-search {
  height: 34px;
  margin-top: 10px;
  padding: 0 12px;
  border: 1px solid #E2E8E3;
  border-radius: 6px;
  display: flex;
  align-items: center;
  color: #9AA9A3;
  background: #FAFCFA;
  font-size: 12px;
}

.closed-banner {
  flex-shrink: 0;
  padding: 10px 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #9A6A00;
  background: #FFF6DB;
  border-bottom: 1px solid #F5E2A8;
  font-size: 13px;
}

.retry-btn {
  height: 34px;
  padding: 0 14px;
  border-radius: 6px;
  color: #fff;
  background: #087E6B;
  font-weight: 700;
}

.menu-body {
  flex: 1;
  min-height: 0;
  display: grid;
  grid-template-columns: 88px minmax(0, 1fr);
  overflow: hidden;
}

.category-sidebar {
  min-height: 0;
  height: 100%;
  background: #EFF4F1;
  border-right: 1px solid #E2E8E3;
  overscroll-behavior: contain;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}

.category-item {
  width: 100%;
  min-height: 54px;
  padding: 9px 8px 8px 12px;
  display: grid;
  align-content: center;
  gap: 2px;
  color: #687872;
  text-align: left;
  position: relative;
  cursor: pointer;
}

.category-item.active {
  background: #fff;
  color: #087E6B;
  font-weight: 800;
}

.category-item.active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 13px;
  bottom: 13px;
  width: 3px;
  border-radius: 0 99px 99px 0;
  background: #087E6B;
}

.category-name {
  font-size: 12px;
  line-height: 1.25;
}

.category-count {
  color: #9AA9A3;
  font-size: 10px;
}

.product-list {
  min-width: 0;
  min-height: 0;
  height: 100%;
  overflow-y: auto;
  overscroll-behavior: contain;
  -webkit-overflow-scrolling: touch;
  padding: 0 10px;
}

.product-section {
  margin-bottom: 10px;
}

.section-title {
  margin: 0;
  padding: 12px 0 8px;
  position: sticky;
  top: 0;
  color: #1F2A26;
  background: #F6F7F3;
  z-index: 1;
  font-size: 14px;
  font-weight: 800;
}

.product-card {
  display: grid;
  grid-template-columns: 78px minmax(0, 1fr);
  gap: 10px;
  padding: 10px;
  margin-bottom: 8px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);
}

.product-image-wrapper {
  width: 78px;
  height: 78px;
  border-radius: 6px;
  overflow: hidden;
  position: relative;
  background: #F1F4F2;
}

.product-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.product-image-placeholder {
  width: 100%;
  height: 100%;
  display: grid;
  place-items: center;
  color: #087E6B;
  background:
    radial-gradient(circle at 34% 38%, rgba(255, 255, 255, 0.8) 0 12%, transparent 13%),
    radial-gradient(circle at 64% 60%, rgba(31, 42, 38, 0.12) 0 12%, transparent 13%),
    linear-gradient(135deg, #E7F4EF, #BFE5DA);
  font-size: 20px;
  font-weight: 800;
}

.sold-out-badge {
  position: absolute;
  top: 0;
  left: 0;
  padding: 2px 8px;
  border-radius: 0 0 6px 0;
  color: #fff;
  background: rgba(31, 42, 38, 0.64);
  font-size: 11px;
}

.product-info {
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.product-name {
  margin: 0;
  color: #1F2A26;
  font-size: 15px;
  font-weight: 800;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-desc {
  margin: 4px 0 0;
  color: #687872;
  font-size: 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 6px;
}

.monthly-sales {
  color: #9AA9A3;
  font-size: 11px;
}

.combo-tag {
  padding: 2px 6px;
  border-radius: 999px;
  color: #9A6A00;
  background: #FFF6DB;
  font-size: 10px;
  font-weight: 700;
}

.product-bottom {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-top: auto;
  padding-top: 6px;
}

.product-price {
  color: #FF6B4A;
  font-weight: 900;
}

.price-symbol {
  font-size: 12px;
}

.price-value {
  font-size: 18px;
}

.price-from {
  margin-left: 2px;
  color: #687872;
  font-size: 11px;
}

.product-action {
  flex-shrink: 0;
}

.add-btn {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  color: #fff;
  background: #087E6B;
  font-size: 18px;
  font-weight: 800;
  line-height: 1;
}

.add-btn.has-spec {
  width: auto;
  padding: 0 11px;
  border-radius: 999px;
  font-size: 12px;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 6px;
}

.qty-btn {
  width: 25px;
  height: 25px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  font-size: 16px;
  line-height: 1;
}

.qty-btn.minus {
  border: 1px solid #DCE6E1;
  color: #687872;
  background: #fff;
}

.qty-btn.plus {
  color: #fff;
  background: #087E6B;
}

.qty-num {
  min-width: 16px;
  text-align: center;
  font-size: 14px;
  font-weight: 800;
}

.list-bottom-spacer {
  height: 94px;
}

.cart-bar {
  position: fixed;
  left: 50%;
  right: auto;
  bottom: calc(66px + env(safe-area-inset-bottom));
  transform: translateX(-50%);
  width: calc(100% - 28px);
  max-width: 722px;
  height: 48px;
  padding: 0 12px 0 10px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #fff;
  background: #202522;
  box-shadow: 0 10px 24px rgba(31, 42, 38, 0.2);
  z-index: 101;
  cursor: pointer;
}

.cart-bar-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.cart-icon-wrapper {
  position: relative;
  width: 34px;
  height: 34px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  color: #087E6B;
  background: #fff;
  font-size: 13px;
  font-weight: 900;
}

.cart-badge {
  position: absolute;
  top: -5px;
  right: -5px;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  border-radius: 999px;
  display: grid;
  place-items: center;
  color: #fff;
  background: #FF6B4A;
  font-size: 11px;
  font-weight: 800;
}

.cart-price {
  color: #fff;
  font-weight: 900;

  .price-value {
    font-size: 20px;
  }
}

.cart-hint {
  display: block;
  margin-top: -2px;
  color: rgba(255, 255, 255, 0.68);
  font-size: 10px;
}

.cart-bar-right {
  height: 34px;
  padding: 0 16px;
  border-radius: 999px;
  display: grid;
  place-items: center;
  color: #fff;
  background: #087E6B;
  font-size: 14px;
  font-weight: 800;
}

.cart-bar-right.disabled {
  background: #687872;
  cursor: not-allowed;
}
</style>
