<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { storeApi } from '@/api/modules/store'
import StoreShareDialog from '@/components/common/StoreShareDialog.vue'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const saving = ref(false)
const store = ref<any>(null)
const editMode = ref(false)
const shareVisible = ref(false)

const form = reactive({
  name: '',
  description: '',
  coverImageUrl: '',
  businessHoursStart: '09:00',
  businessHoursEnd: '23:00',
  dineIn: true,
  takeaway: true,
  delivery: false,
  deliveryMinAmount: 0,
  packingFee: 0,
})

const parseTime = (value?: string) => {
  if (!value) return ''
  return value.slice(0, 5)
}

const toApiTime = (value: string) => {
  if (!value) return undefined
  return value.length === 5 ? `${value}:00` : value
}

const loadStore = async () => {
  if (!storeId) return
  loading.value = true
  try {
    store.value = await storeApi.getById(storeId)
    const modes = String(store.value.diningMode || 'dine_in,takeaway').split(',')
    form.name = store.value.name || ''
    form.description = store.value.description || ''
    form.coverImageUrl = store.value.coverImageUrl || ''
    form.businessHoursStart = parseTime(store.value.businessHoursStart) || '09:00'
    form.businessHoursEnd = parseTime(store.value.businessHoursEnd) || '23:00'
    form.dineIn = modes.includes('dine_in')
    form.takeaway = modes.includes('takeaway')
    form.delivery = modes.includes('delivery')
    form.deliveryMinAmount = Number(store.value.deliveryMinAmount || 0)
    form.packingFee = Number(store.value.packingFee || 0)
  } finally {
    loading.value = false
  }
}

const buildDiningMode = () => {
  const modes: string[] = []
  if (form.dineIn) modes.push('dine_in')
  if (form.takeaway) modes.push('takeaway')
  if (form.delivery) modes.push('delivery')
  return modes.length ? modes.join(',') : 'dine_in'
}

const handleSave = async () => {
  if (!storeId) return
  saving.value = true
  try {
    await storeApi.update(storeId, {
      name: form.name,
      description: form.description,
      coverImageUrl: form.coverImageUrl,
      businessHoursStart: toApiTime(form.businessHoursStart),
      businessHoursEnd: toApiTime(form.businessHoursEnd),
      diningMode: buildDiningMode(),
      deliveryMinAmount: Number(form.deliveryMinAmount || 0),
      packingFee: Number(form.packingFee || 0),
    })
    editMode.value = false
    await loadStore()
  } finally {
    saving.value = false
  }
}

const toggleBusiness = async () => {
  if (!store.value) return
  const businessStatus = store.value.businessStatus === 'open' ? 'closed' : 'open'
  await storeApi.update(storeId, { businessStatus })
  store.value.businessStatus = businessStatus
}

const businessStatusLabel = (status: string) => status === 'open' ? '营业中' : '休息中'

const diningModeText = (value: string) => {
  const modes = String(value || '').split(',')
  const labels: string[] = []
  if (modes.includes('dine_in')) labels.push('堂食')
  if (modes.includes('takeaway')) labels.push('打包')
  if (modes.includes('delivery')) labels.push('外卖')
  return labels.join('、') || '堂食'
}

onMounted(loadStore)
</script>

<template>
  <div class="store-page">
    <header class="store-header">
      <h2>店铺设置</h2>
      <div class="header-actions">
        <button v-if="!editMode" class="ghost-button" @click="shareVisible = true">分享</button>
        <button v-if="!editMode" class="primary-button" @click="editMode = true">编辑</button>
      </div>
    </header>

    <div v-if="loading" class="loading-state">加载中...</div>

    <main v-else-if="store" class="store-content">
      <section class="store-banner">
        <div v-if="store.coverImageUrl" class="banner-img" :style="{ backgroundImage: `url(${store.coverImageUrl})` }"></div>
        <div v-else class="banner-placeholder">{{ store.name?.slice(0, 1) || '店' }}</div>
        <span class="status-badge" :class="store.businessStatus">{{ businessStatusLabel(store.businessStatus) }}</span>
      </section>

      <section class="info-panel">
        <template v-if="!editMode">
          <div class="info-row">
            <span>店铺名称</span>
            <strong>{{ store.name }}</strong>
          </div>
          <div class="info-row">
            <span>店铺码</span>
            <strong>{{ store.storeCode }}</strong>
          </div>
          <div class="info-row">
            <span>店铺描述</span>
            <strong>{{ store.description || '暂无描述' }}</strong>
          </div>
          <div class="info-row">
            <span>营业时间</span>
            <strong>{{ parseTime(store.businessHoursStart) || '00:00' }} - {{ parseTime(store.businessHoursEnd) || '23:59' }}</strong>
          </div>
          <div class="info-row">
            <span>就餐方式</span>
            <strong>{{ diningModeText(store.diningMode) }}</strong>
          </div>
          <div class="info-row">
            <span>外卖起送</span>
            <strong>￥{{ Number(store.deliveryMinAmount || 0).toFixed(2) }}</strong>
          </div>
          <div class="info-row">
            <span>打包费</span>
            <strong>￥{{ Number(store.packingFee || 0).toFixed(2) }}</strong>
          </div>
        </template>

        <template v-else>
          <label>
            店铺名称
            <input v-model="form.name" />
          </label>
          <label>
            店铺描述
            <textarea v-model="form.description" rows="3"></textarea>
          </label>
          <label>
            封面图 URL
            <input v-model="form.coverImageUrl" />
          </label>
          <div class="form-grid">
            <label>
              营业开始
              <input v-model="form.businessHoursStart" type="time" />
            </label>
            <label>
              营业结束
              <input v-model="form.businessHoursEnd" type="time" />
            </label>
            <label>
              外卖起送
              <input v-model.number="form.deliveryMinAmount" type="number" min="0" step="0.01" />
            </label>
            <label>
              打包费
              <input v-model.number="form.packingFee" type="number" min="0" step="0.01" />
            </label>
          </div>
          <div class="mode-group">
            <label class="check-card">
              <input v-model="form.dineIn" type="checkbox" />
              堂食
            </label>
            <label class="check-card">
              <input v-model="form.takeaway" type="checkbox" />
              打包
            </label>
            <label class="check-card">
              <input v-model="form.delivery" type="checkbox" />
              外卖
            </label>
          </div>
          <div class="form-actions">
            <button class="ghost-button" @click="editMode = false; loadStore()">取消</button>
            <button class="primary-button" :disabled="saving" @click="handleSave">
              {{ saving ? '保存中...' : '保存' }}
            </button>
          </div>
        </template>
      </section>

      <section class="business-panel">
        <div>
          <span>营业状态</span>
          <strong :class="store.businessStatus">{{ businessStatusLabel(store.businessStatus) }}</strong>
        </div>
        <button class="ghost-button" @click="toggleBusiness">
          {{ store.businessStatus === 'open' ? '打烊休息' : '开始营业' }}
        </button>
      </section>

      <button class="share-button" @click="shareVisible = true">分享店铺二维码和短链</button>
    </main>

    <StoreShareDialog
      v-if="shareVisible"
      :store-code="store?.storeCode || ''"
      :store-name="store?.name || ''"
      :visible="shareVisible"
      @close="shareVisible = false"
    />
  </div>
</template>

<style scoped lang="scss">
.store-page {
  min-height: 100vh;
  background: #f7f7f7;
  color: #1a1a2e;
}

.store-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;
  background: #fff;
  border-bottom: 1px solid #ececec;

  h2 {
    margin: 0;
    font-size: 20px;
  }
}

.header-actions,
.form-actions,
.mode-group {
  display: flex;
  gap: 8px;
}

.primary-button,
.ghost-button,
.share-button {
  min-height: 38px;
  padding: 0 16px;
  border: none;
  border-radius: 8px;
  font-weight: 800;
  cursor: pointer;
}

.primary-button,
.share-button {
  background: #ff6b6b;
  color: #fff;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}

.ghost-button {
  background: #fff1f0;
  color: #ff4d4f;
}

.loading-state {
  padding: 60px 0;
  text-align: center;
  color: #777;
}

.store-banner {
  position: relative;
  height: 168px;
  background: #303846;
  overflow: hidden;
}

.banner-img {
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
}

.banner-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  color: #fff;
  font-size: 56px;
  font-weight: 900;
}

.status-badge {
  position: absolute;
  right: 12px;
  top: 12px;
  padding: 5px 12px;
  border-radius: 999px;
  background: rgba(0, 0, 0, 0.52);
  color: #fff;
  font-size: 13px;

  &.open {
    background: #389e0d;
  }
}

.info-panel,
.business-panel {
  margin: 12px 16px;
  padding: 16px;
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;

  &:last-child {
    border-bottom: none;
  }

  span {
    color: #777;
  }

  strong {
    text-align: right;
  }
}

label {
  display: grid;
  gap: 6px;
  margin-bottom: 12px;
  color: #666;
  font-size: 13px;
}

input,
textarea {
  width: 100%;
  min-height: 38px;
  padding: 8px 10px;
  border: 1px solid #ddd;
  border-radius: 8px;
  box-sizing: border-box;
  color: #1a1a2e;
}

textarea {
  resize: vertical;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.mode-group {
  margin-bottom: 14px;
}

.check-card {
  display: flex;
  align-items: center;
  gap: 6px;
  margin: 0;
  padding: 10px 12px;
  border: 1px solid #eee;
  border-radius: 8px;
  background: #fafafa;

  input {
    width: auto;
    min-height: auto;
  }
}

.business-panel {
  display: flex;
  align-items: center;
  justify-content: space-between;

  div {
    display: grid;
    gap: 4px;
  }

  span {
    color: #777;
    font-size: 13px;
  }

  strong.open {
    color: #389e0d;
  }
}

.share-button {
  width: calc(100% - 32px);
  margin: 0 16px 20px;
}

@media (max-width: 560px) {
  .form-grid,
  .mode-group {
    grid-template-columns: 1fr;
    flex-direction: column;
  }
}
</style>
