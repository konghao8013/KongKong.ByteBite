<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { storeApi } from '@/api/modules/store'
import StoreShareDialog from '@/components/common/StoreShareDialog.vue'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const saving = ref(false)
const store = ref<any>(null)
const editMode = ref(false)
const shareVisible = ref(false)
const form = ref({ name: '', description: '', coverImageUrl: '' })

const loadStore = async () => {
  if (!storeId) return
  loading.value = true
  try {
    store.value = await storeApi.getById(storeId)
    form.value = {
      name: store.value.name || '',
      description: store.value.description || '',
      coverImageUrl: store.value.coverImageUrl || '',
    }
  } catch (e) { console.error('加载店铺信息失败', e) }
  finally { loading.value = false }
}

const handleSave = async () => {
  if (!storeId) return
  saving.value = true
  try {
    await storeApi.update(storeId, form.value)
    editMode.value = false
    await loadStore()
  } catch (e) { console.error('保存失败', e) }
  finally { saving.value = false }
}

const toggleBusiness = async () => {
  if (!store.value) return
  const newStatus = store.value.businessStatus === 'open' ? 'closed' : 'open'
  try {
    await storeApi.update(storeId, { businessStatus: newStatus })
    store.value.businessStatus = newStatus
  } catch (e) { console.error('操作失败', e) }
}

const businessStatusLabel = (status: string) => {
  const map: Record<string, string> = { open: '营业中', closed: '已打烊' }
  return map[status] || status
}

onMounted(loadStore)
</script>

<template>
  <div class="store-page">
    <div class="store-header">
      <h2>店铺信息</h2>
      <div class="header-actions">
        <button v-if="!editMode" class="btn-share" @click="shareVisible = true">分享</button>
        <button v-if="!editMode" class="btn-edit" @click="editMode = true">编辑</button>
      </div>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div v-else-if="store" class="store-content">
      <div class="store-banner">
        <div v-if="store.coverImageUrl" class="banner-img" :style="{ backgroundImage: `url(${store.coverImageUrl})` }"></div>
        <div v-else class="banner-placeholder">🏪</div>
        <div class="store-status-badge" :class="store.businessStatus">
          {{ businessStatusLabel(store.businessStatus) }}
        </div>
      </div>

      <div class="store-info-card">
        <div v-if="!editMode">
          <div class="info-row">
            <span class="info-label">店铺名称</span>
            <span class="info-value">{{ store.name }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">店铺描述</span>
            <span class="info-value">{{ store.description || '暂无描述' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">营业时间</span>
            <span class="info-value">{{ store.businessHoursStart || '00:00' }} - {{ store.businessHoursEnd || '23:59' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">用餐模式</span>
            <span class="info-value">{{ store.diningMode === 'dine_in' ? '堂食' : store.diningMode === 'takeaway' ? '打包' : '堂食+打包' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">月销量</span>
            <span class="info-value">{{ store.monthlySales || 0 }} 单</span>
          </div>
        </div>

        <div v-else class="edit-form">
          <div class="form-group">
            <label>店铺名称</label>
            <input v-model="form.name" placeholder="输入店铺名称" />
          </div>
          <div class="form-group">
            <label>店铺描述</label>
            <textarea v-model="form.description" placeholder="输入店铺描述" rows="3"></textarea>
          </div>
          <div class="form-group">
            <label>封面图片URL</label>
            <input v-model="form.coverImageUrl" placeholder="输入图片URL" />
          </div>
          <div class="form-actions">
            <button class="btn-cancel" @click="editMode = false; loadStore()">取消</button>
            <button class="btn-save" :disabled="saving" @click="handleSave">
              {{ saving ? '保存中...' : '保存' }}
            </button>
          </div>
        </div>
      </div>

      <div class="business-toggle">
        <div class="toggle-info">
          <span class="toggle-label">营业状态</span>
          <span class="toggle-status" :class="store.businessStatus">
            {{ businessStatusLabel(store.businessStatus) }}
          </span>
        </div>
        <button
          class="btn-toggle"
          :class="store.businessStatus"
          @click="toggleBusiness"
        >
          {{ store.businessStatus === 'open' ? '打烊休息' : '开始营业' }}
        </button>
      </div>

      <div class="share-section">
        <button class="btn-share-store" @click="shareVisible = true">🔗 分享店铺</button>
      </div>
    </div>

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
  background: #F7F7F7;
  min-height: 100vh;
  color: #1A1A2E;
}

.store-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;

  h2 { font-size: 20px; font-weight: 700; margin: 0; }

  .header-actions { display: flex; gap: 8px; }

  .btn-edit {
    background: linear-gradient(135deg, #FF6B6B, #FF8E53);
    color: #fff;
    border: none;
    padding: 8px 18px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
  }

  .btn-share {
    background: #FFF1F0;
    color: #FF6B6B;
    border: none;
    padding: 8px 18px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
  }
}

.loading-state { text-align: center; padding: 60px; color: #8C8C8C; }

.store-banner {
  position: relative;
  height: 160px;
  background: linear-gradient(135deg, #FF6B6B, #FF8E53);
  overflow: hidden;

  .banner-img {
    width: 100%;
    height: 100%;
    background-size: cover;
    background-position: center;
  }

  .banner-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 64px;
  }

  .store-status-badge {
    position: absolute;
    top: 12px;
    right: 12px;
    padding: 4px 14px;
    border-radius: 16px;
    font-size: 13px;
    font-weight: 600;

    &.open { background: #52C41A; color: #fff; }
    &.closed { background: rgba(0, 0, 0, 0.4); color: #fff; }
  }
}

.store-info-card {
  margin: -20px 16px 16px;
  background: #FFFFFF;
  border-radius: 16px;
  padding: 20px;
  position: relative;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #F0F0F0;

  &:last-child { border-bottom: none; }

  .info-label { font-size: 14px; color: #8C8C8C; }
  .info-value { font-size: 14px; font-weight: 500; color: #1A1A2E; }
}

.edit-form {
  .form-group {
    margin-bottom: 16px;

    label { display: block; font-size: 14px; color: #8C8C8C; margin-bottom: 6px; }

    input, textarea {
      width: 100%;
      padding: 10px 14px;
      border: 1px solid #E8E8E8;
      border-radius: 10px;
      font-size: 15px;
      background: #F7F7F7;
      color: #1A1A2E;
      outline: none;

      &:focus { border-color: #FF6B6B; box-shadow: 0 0 0 3px rgba(255, 107, 107, 0.1); }
    }

    textarea { resize: vertical; }
  }
}

.form-actions {
  display: flex;
  gap: 12px;

  button {
    flex: 1;
    padding: 10px;
    border-radius: 10px;
    font-size: 14px;
    font-weight: 600;
    border: none;
    cursor: pointer;
  }
  .btn-cancel { background: #F7F7F7; color: #8C8C8C; }
  .btn-save { background: linear-gradient(135deg, #FF6B6B, #FF8E53); color: #fff; }
}

.business-toggle {
  margin: 0 16px 16px;
  background: #FFFFFF;
  border-radius: 16px;
  padding: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

  .toggle-info { display: flex; align-items: center; gap: 8px; }
  .toggle-label { font-size: 14px; color: #8C8C8C; }
  .toggle-status {
    font-size: 14px;
    font-weight: 600;
    &.open { color: #52C41A; }
    &.closed { color: #8C8C8C; }
  }

  .btn-toggle {
    padding: 8px 18px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 600;
    border: none;
    cursor: pointer;

    &.open { background: #FFF1F0; color: #FF6B6B; }
    &.closed { background: #F6FFED; color: #52C41A; }
  }
}

.share-section {
  margin: 0 16px 16px;

  .btn-share-store {
    width: 100%;
    padding: 14px;
    border-radius: 12px;
    background: linear-gradient(135deg, #FF6B6B, #FF8E53);
    color: #FFFFFF;
    border: none;
    font-size: 16px;
    font-weight: 700;
    cursor: pointer;
    box-shadow: 0 4px 12px rgba(255, 107, 107, 0.3);
  }
}
</style>