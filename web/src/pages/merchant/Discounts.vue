<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { discountApi } from '@/api/modules/discount'
import { categoryApi } from '@/api/modules/category'
import { productApi } from '@/api/modules/product'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const saving = ref(false)
const showEditor = ref(false)
const editingId = ref('')
const discounts = ref<any[]>([])
const categories = ref<any[]>([])
const products = ref<any[]>([])

const form = reactive<any>({
  name: '',
  discountType: 'full_reduction',
  thresholdAmount: 30,
  discountAmount: 5,
  discountRate: 90,
  applyScope: 'all',
  applyScopeId: '',
  allowStack: false,
  startTime: '',
  endTime: '',
  status: 'active',
})

const scopeOptions = computed(() => form.applyScope === 'category' ? categories.value : products.value)

const toLocalInput = (value?: string) => {
  const d = value ? new Date(value) : new Date()
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset())
  return d.toISOString().slice(0, 16)
}

const resetForm = () => {
  editingId.value = ''
  form.name = ''
  form.discountType = 'full_reduction'
  form.thresholdAmount = 30
  form.discountAmount = 5
  form.discountRate = 90
  form.applyScope = 'all'
  form.applyScopeId = ''
  form.allowStack = false
  form.startTime = toLocalInput()
  form.endTime = toLocalInput(new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString())
  form.status = 'active'
}

const fetchData = async () => {
  if (!storeId) return
  loading.value = true
  try {
    const [rules, categoryRows, productRows] = await Promise.all([
      discountApi.getByStoreId(storeId),
      categoryApi.getByStoreId(storeId),
      productApi.getByStoreId(storeId),
    ])
    discounts.value = rules || []
    categories.value = categoryRows || []
    products.value = productRows || []
  } finally {
    loading.value = false
  }
}

const openCreator = () => {
  resetForm()
  showEditor.value = true
}

const openEditor = (rule: any) => {
  editingId.value = rule.id
  form.name = rule.name || ''
  form.discountType = rule.discountType || 'full_reduction'
  form.thresholdAmount = Number(rule.thresholdAmount || 30)
  form.discountAmount = Number(rule.discountAmount || 5)
  form.discountRate = Number(rule.discountRate || 90)
  form.applyScope = rule.applyScope || 'all'
  form.applyScopeId = rule.applyScopeId || ''
  form.allowStack = !!rule.allowStack
  form.startTime = toLocalInput(rule.startTime)
  form.endTime = toLocalInput(rule.endTime)
  form.status = rule.status || 'active'
  showEditor.value = true
}

const buildPayload = () => ({
  storeId,
  name: form.name.trim(),
  discountType: form.discountType,
  thresholdAmount: form.discountType === 'full_reduction' ? Number(form.thresholdAmount || 0) : undefined,
  discountAmount: form.discountType === 'full_reduction' ? Number(form.discountAmount || 0) : undefined,
  discountRate: form.discountType === 'discount' ? Number(form.discountRate || 0) : undefined,
  applyScope: form.applyScope,
  applyScopeId: form.applyScope === 'all' ? undefined : form.applyScopeId,
  allowStack: !!form.allowStack,
  startTime: new Date(form.startTime).toISOString(),
  endTime: new Date(form.endTime).toISOString(),
  status: form.status,
})

const saveRule = async () => {
  if (!form.name.trim()) return
  saving.value = true
  try {
    const payload = buildPayload()
    if (editingId.value) await discountApi.update(editingId.value, payload)
    else await discountApi.create(payload)
    showEditor.value = false
    await fetchData()
  } finally {
    saving.value = false
  }
}

const toggleStatus = async (rule: any) => {
  await discountApi.update(rule.id, { ...rule, status: rule.status === 'active' ? 'inactive' : 'active' })
  await fetchData()
}

const handleDelete = async (rule: any) => {
  if (!confirm(`确定删除优惠活动「${rule.name}」吗？`)) return
  await discountApi.delete(rule.id)
  await fetchData()
}

const getDiscountDesc = (rule: any) => {
  if (rule.discountType === 'full_reduction') return `满${rule.thresholdAmount}减${rule.discountAmount}`
  if (rule.discountType === 'discount') return `${(Number(rule.discountRate || 0) / 10).toFixed(1)}折`
  return ''
}

const scopeLabel = (rule: any) => {
  if (rule.applyScope === 'all') return '全店'
  const rows = rule.applyScope === 'category' ? categories.value : products.value
  return rows.find((item) => item.id === rule.applyScopeId)?.name || '指定范围'
}

const isExpired = (rule: any) => rule.endTime && new Date(rule.endTime) < new Date()
const formatDate = (dateStr: string) => dateStr ? new Date(dateStr).toLocaleString() : '-'

onMounted(() => {
  resetForm()
  fetchData()
})
</script>

<template>
  <div class="discounts-page">
    <header class="page-header">
      <h2>优惠活动</h2>
      <button class="primary-button" @click="openCreator">新建活动</button>
    </header>

    <div v-if="loading" class="loading-state">加载中...</div>
    <div v-else-if="discounts.length === 0" class="empty-state">
      <p>暂无优惠活动</p>
      <button class="primary-button" @click="openCreator">创建第一个活动</button>
    </div>

    <div v-else class="discount-list">
      <article v-for="rule in discounts" :key="rule.id" class="discount-card" :class="{ inactive: rule.status !== 'active', expired: isExpired(rule) }">
        <div class="discount-header">
          <span class="discount-type" :class="rule.discountType">{{ rule.discountType === 'full_reduction' ? '满减' : '折扣' }}</span>
          <span class="discount-status">{{ isExpired(rule) ? '已过期' : rule.status === 'active' ? '生效中' : '已停用' }}</span>
        </div>
        <h3>{{ rule.name }}</h3>
        <div class="discount-desc">{{ getDiscountDesc(rule) }}</div>
        <div class="discount-meta">
          <span>适用：{{ scopeLabel(rule) }}</span>
          <span>{{ formatDate(rule.startTime) }} 至 {{ formatDate(rule.endTime) }}</span>
          <span>已使用 {{ rule.usedCount || 0 }} 次</span>
        </div>
        <div class="discount-actions">
          <button class="ghost-button" @click="openEditor(rule)">编辑</button>
          <button class="ghost-button" @click="toggleStatus(rule)">{{ rule.status === 'active' ? '停用' : '启用' }}</button>
          <button class="danger-button" @click="handleDelete(rule)">删除</button>
        </div>
      </article>
    </div>

    <div v-if="showEditor" class="modal-overlay" @click.self="showEditor = false">
      <section class="modal-content">
        <h3>{{ editingId ? '编辑活动' : '新建活动' }}</h3>
        <div class="form-grid">
          <label>
            活动名称
            <input v-model="form.name" placeholder="例如 满30减5" />
          </label>
          <label>
            活动状态
            <select v-model="form.status">
              <option value="active">启用</option>
              <option value="inactive">停用</option>
            </select>
          </label>
          <label>
            优惠类型
            <select v-model="form.discountType">
              <option value="full_reduction">满减</option>
              <option value="discount">折扣</option>
            </select>
          </label>
          <template v-if="form.discountType === 'full_reduction'">
            <label>
              满减门槛
              <input v-model.number="form.thresholdAmount" type="number" min="0" step="0.01" />
            </label>
            <label>
              减免金额
              <input v-model.number="form.discountAmount" type="number" min="0" step="0.01" />
            </label>
          </template>
          <label v-else>
            折扣率
            <input v-model.number="form.discountRate" type="number" min="1" max="99" step="1" placeholder="80 表示 8 折" />
          </label>
          <label>
            适用范围
            <select v-model="form.applyScope" @change="form.applyScopeId = ''">
              <option value="all">全店</option>
              <option value="category">指定分类</option>
              <option value="product">指定商品</option>
            </select>
          </label>
          <label v-if="form.applyScope !== 'all'">
            选择范围
            <select v-model="form.applyScopeId">
              <option value="">请选择</option>
              <option v-for="item in scopeOptions" :key="item.id" :value="item.id">{{ item.name }}</option>
            </select>
          </label>
          <label>
            开始时间
            <input v-model="form.startTime" type="datetime-local" />
          </label>
          <label>
            结束时间
            <input v-model="form.endTime" type="datetime-local" />
          </label>
          <label class="inline-check">
            <input v-model="form.allowStack" type="checkbox" />
            允许叠加
          </label>
        </div>
        <div class="modal-actions">
          <button class="ghost-button" @click="showEditor = false">取消</button>
          <button class="primary-button" :disabled="saving" @click="saveRule">{{ saving ? '保存中...' : '保存活动' }}</button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped lang="scss">
.discounts-page { min-height: 100vh; color: #1F2A26; background: #F6F7F3; }
.page-header {
  padding: 16px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #fff;

  h2 { margin: 0; font-size: 18px; font-weight: 800; }
}
.primary-button, .ghost-button, .danger-button {
  min-height: 36px;
  padding: 0 14px;
  border-radius: 6px;
  font-weight: 800;
}
.primary-button { color: #fff; background: #087E6B; }
.ghost-button { color: #087E6B; background: #E7F4EF; }
.danger-button { color: #D94C4C; background: #FFF0F0; }
.loading-state, .empty-state { padding: 70px 20px; color: #687872; text-align: center; }
.discount-list { padding: 12px; display: grid; gap: 12px; }
.discount-card {
  padding: 16px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 5px 15px rgba(31, 42, 38, 0.05);

  &.inactive, &.expired { opacity: 0.72; }
  h3 { margin: 10px 0 4px; font-size: 16px; }
}
.discount-header, .discount-actions, .modal-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}
.discount-header { justify-content: space-between; }
.discount-type {
  padding: 3px 10px;
  border-radius: 999px;
  color: #fff;
  background: #087E6B;
  font-size: 12px;
  font-weight: 800;

  &.discount { color: #1F2A26; background: #FFE08A; }
}
.discount-status { color: #687872; font-size: 13px; font-weight: 800; }
.discount-desc { color: #FF6B4A; font-size: 22px; font-weight: 900; }
.discount-meta { display: grid; gap: 4px; margin: 8px 0 12px; color: #687872; font-size: 12px; }
.discount-actions { justify-content: flex-end; border-top: 1px solid #E2E8E3; padding-top: 12px; }
.modal-overlay { position: fixed; inset: 0; z-index: 200; display: grid; place-items: center; padding: 16px; background: rgba(0,0,0,.45); }
.modal-content { width: min(720px, 100%); max-height: 90vh; overflow-y: auto; padding: 20px; border-radius: 8px; background: #fff; }
.form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px; }
label { display: grid; gap: 6px; color: #687872; font-size: 13px; }
input, select { min-height: 38px; padding: 8px 10px; border: 1px solid #DCE6E1; border-radius: 8px; color: #1F2A26; background: #fff; }
.inline-check { display: flex; align-items: center; align-self: end; gap: 6px; min-height: 38px; }
.inline-check input { width: auto; min-height: auto; }
.modal-actions { justify-content: flex-end; margin-top: 16px; }
@media (max-width: 640px) { .form-grid { grid-template-columns: 1fr; } }
</style>
