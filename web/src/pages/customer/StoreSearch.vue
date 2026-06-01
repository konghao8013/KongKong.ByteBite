<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { customerApi } from '@/api/modules/customer'
import { useCustomerIdentity } from '@/composables/useCustomerIdentity'
import EmptyState from '@/components/common/EmptyState.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { StoreSummaryDto } from '@/types/models/customer'

const router = useRouter()
const { ensureCustomerIdentity } = useCustomerIdentity()

const keyword = ref('')
const loading = ref(false)
const stores = ref<StoreSummaryDto[]>([])
const recentStores = ref<StoreSummaryDto[]>([])
const searched = ref(false)

const visibleStores = computed(() => searched.value ? stores.value : recentStores.value)

const searchStores = async () => {
  const text = keyword.value.trim()
  if (!text) {
    searched.value = false
    stores.value = []
    return
  }

  loading.value = true
  searched.value = true
  try {
    stores.value = await customerApi.searchStores(text, 30)
  } finally {
    loading.value = false
  }
}

const loadRecentStores = async () => {
  const identity = await ensureCustomerIdentity()
  recentStores.value = await customerApi.getRecentStores({
    customerId: identity.customerId,
    deviceId: identity.deviceId,
    pageSize: 20,
  })
}

const goToStore = (store: StoreSummaryDto) => {
  router.push({ name: 'StoreMenu', params: { code: store.storeCode } })
}

const goToLogin = () => {
  router.push({ name: 'CustomerLogin', query: { redirect: '/stores/search' } })
}

onMounted(loadRecentStores)
</script>

<template>
  <div class="store-search-page">
    <header class="page-header">
      <button class="back-btn" @click="router.back()">‹</button>
      <h1>搜索店铺</h1>
      <button class="login-btn" @click="goToLogin">账号</button>
    </header>

    <section class="search-box">
      <input
        v-model="keyword"
        type="search"
        placeholder="输入店铺名、店铺码或行业"
        @keyup.enter="searchStores"
      />
      <button @click="searchStores">搜索</button>
    </section>

    <LoadingSpinner v-if="loading" text="搜索中..." />

    <template v-else>
      <div class="section-title">{{ searched ? '搜索结果' : '最近店铺' }}</div>

      <EmptyState
        v-if="visibleStores.length === 0"
        :description="searched ? '没有找到匹配店铺' : '暂无最近访问店铺'"
        icon="□"
      />

      <div v-else class="store-list">
        <article v-for="store in visibleStores" :key="store.id" class="store-card" @click="goToStore(store)">
          <div class="cover">
            <img v-if="store.coverImageUrl" :src="store.coverImageUrl" :alt="store.name" />
            <span v-else>{{ store.name.slice(0, 1) }}</span>
          </div>
          <div class="store-info">
            <div class="store-title-row">
              <h2>{{ store.name }}</h2>
              <span :class="{ closed: store.businessStatus !== 'open' }">
                {{ store.businessStatus === 'open' ? '营业中' : '休息中' }}
              </span>
            </div>
            <p>{{ store.description || store.industryName || '店铺码 ' + store.storeCode }}</p>
            <small>{{ store.industryName || '店铺码 ' + store.storeCode }}</small>
            <div v-if="store.activeDiscounts?.length" class="discount-line">
              {{ store.activeDiscounts[0].name }}
            </div>
          </div>
        </article>
      </div>
    </template>
  </div>
</template>

<style scoped lang="scss">
.store-search-page {
  min-height: 100vh;
  background: #F6F7F3;
}

.page-header {
  position: sticky;
  top: 0;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #E2E8E3;
  background: rgba(255, 255, 255, 0.96);
  backdrop-filter: blur(12px);
}

.back-btn,
.login-btn {
  width: 44px;
  height: 32px;
  border-radius: 6px;
  color: #087E6B;
  background: #E7F4EF;
  font-weight: 800;
}

.back-btn {
  color: #1F2A26;
  background: transparent;
  font-size: 24px;
}

h1 {
  margin: 0;
  color: #1F2A26;
  font-size: 18px;
}

.search-box {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 70px;
  gap: 8px;
  padding: 14px 16px;
  background: #fff;
  border-bottom: 1px solid #E2E8E3;
}

input {
  height: 40px;
  padding: 0 12px;
  border: 1px solid #DCE6E1;
  border-radius: 6px;
  outline: 0;
  color: #1F2A26;
  background: #FAFCFA;
  font-size: 14px;
}

.search-box button {
  border-radius: 6px;
  color: #fff;
  background: #087E6B;
  font-weight: 900;
}

.section-title {
  padding: 14px 16px 8px;
  color: #687872;
  font-size: 13px;
  font-weight: 900;
}

.store-list {
  display: grid;
  gap: 10px;
  padding: 0 16px 24px;
}

.store-card {
  display: grid;
  grid-template-columns: 70px minmax(0, 1fr);
  gap: 12px;
  padding: 12px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);
}

.cover {
  width: 70px;
  height: 70px;
  border-radius: 6px;
  overflow: hidden;
  display: grid;
  place-items: center;
  color: #087E6B;
  background: #E7F4EF;
  font-size: 22px;
  font-weight: 900;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
}

.store-info {
  min-width: 0;
}

.store-title-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;

  h2 {
    margin: 0;
    color: #1F2A26;
    font-size: 16px;
    font-weight: 900;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  span {
    flex-shrink: 0;
    padding: 3px 7px;
    border-radius: 999px;
    color: #087E6B;
    background: #E7F4EF;
    font-size: 11px;
    font-weight: 800;

    &.closed {
      color: #687872;
      background: #F1F4F2;
    }
  }
}

p {
  margin: 8px 0 6px;
  color: #687872;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

small {
  color: #9AA9A3;
  font-size: 12px;
}

.discount-line {
  margin-top: 6px;
  color: #9A6A00;
  font-size: 12px;
  font-weight: 800;
}
</style>
