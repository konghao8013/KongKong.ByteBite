<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { staffApi } from '@/api/modules/staff'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const saving = ref(false)
const showEditor = ref(false)
const editingId = ref('')
const staffRows = ref<any[]>([])

const form = reactive({
  name: '',
  phone: '',
  password: '',
  permission: 'order_only',
  status: 'active',
})

const resetForm = () => {
  editingId.value = ''
  form.name = ''
  form.phone = ''
  form.password = ''
  form.permission = 'order_only'
  form.status = 'active'
}

const loadStaff = async () => {
  if (!storeId) return
  loading.value = true
  try {
    staffRows.value = await staffApi.getByStoreId(storeId) || []
  } finally {
    loading.value = false
  }
}

const openCreator = () => {
  resetForm()
  showEditor.value = true
}

const openEditor = (staff: any) => {
  editingId.value = staff.id
  form.name = staff.name || ''
  form.phone = staff.phone || ''
  form.password = ''
  form.permission = staff.permission || 'order_only'
  form.status = staff.status || 'active'
  showEditor.value = true
}

const saveStaff = async () => {
  if (!form.name.trim() || !form.phone.trim()) return
  saving.value = true
  try {
    const payload = {
      storeId,
      name: form.name.trim(),
      phone: form.phone.trim(),
      password: form.password || undefined,
      permission: form.permission,
      status: form.status,
    }
    if (editingId.value) await staffApi.update(editingId.value, payload)
    else await staffApi.create(payload)
    showEditor.value = false
    await loadStaff()
  } finally {
    saving.value = false
  }
}

const toggleStatus = async (staff: any) => {
  await staffApi.update(staff.id, { storeId, status: staff.status === 'active' ? 'inactive' : 'active' })
  await loadStaff()
}

const resetPassword = async (staff: any) => {
  const password = prompt('请输入新密码，至少 6 位')
  if (!password) return
  await staffApi.resetPassword(staff.id, password)
}

const deleteStaff = async (staff: any) => {
  if (!confirm(`确定删除店员「${staff.name}」吗？`)) return
  await staffApi.delete(staff.id)
  await loadStaff()
}

const permissionLabel = (value: string) => value === 'full' ? '全部权限' : '仅接单'
const statusLabel = (value: string) => value === 'active' ? '启用' : '停用'

onMounted(loadStaff)
</script>

<template>
  <div class="staff-page">
    <header class="page-header">
      <h2>店员管理</h2>
      <button class="primary-button" @click="openCreator">新增店员</button>
    </header>

    <div v-if="loading" class="loading-state">加载中...</div>
    <div v-else-if="staffRows.length === 0" class="empty-state">
      <p>暂无店员</p>
      <button class="primary-button" @click="openCreator">新增店员</button>
    </div>

    <section v-else class="staff-list">
      <article v-for="staff in staffRows" :key="staff.id" class="staff-card">
        <div>
          <h3>{{ staff.name }}</h3>
          <p>{{ staff.phone }}</p>
        </div>
        <div class="staff-meta">
          <span>{{ permissionLabel(staff.permission) }}</span>
          <span :class="{ active: staff.status === 'active' }">{{ statusLabel(staff.status) }}</span>
        </div>
        <div class="staff-actions">
          <button class="ghost-button" @click="openEditor(staff)">编辑</button>
          <button class="ghost-button" @click="toggleStatus(staff)">{{ staff.status === 'active' ? '停用' : '启用' }}</button>
          <button class="ghost-button" @click="resetPassword(staff)">重置密码</button>
          <button class="danger-button" @click="deleteStaff(staff)">删除</button>
        </div>
      </article>
    </section>

    <div v-if="showEditor" class="modal-overlay" @click.self="showEditor = false">
      <section class="modal-content">
        <h3>{{ editingId ? '编辑店员' : '新增店员' }}</h3>
        <label>
          姓名
          <input v-model="form.name" placeholder="店员姓名" />
        </label>
        <label>
          手机号
          <input v-model="form.phone" placeholder="用于店员登录或联系" />
        </label>
        <label>
          密码
          <input v-model="form.password" type="password" :placeholder="editingId ? '留空不修改' : '默认 123456'" />
        </label>
        <label>
          权限
          <select v-model="form.permission">
            <option value="order_only">仅接单</option>
            <option value="full">全部权限</option>
          </select>
        </label>
        <label>
          状态
          <select v-model="form.status">
            <option value="active">启用</option>
            <option value="inactive">停用</option>
          </select>
        </label>
        <div class="modal-actions">
          <button class="ghost-button" @click="showEditor = false">取消</button>
          <button class="primary-button" :disabled="saving" @click="saveStaff">{{ saving ? '保存中...' : '保存' }}</button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped lang="scss">
.staff-page { min-height: 100vh; color: #1F2A26; background: #F6F7F3; }
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
  min-height: 34px;
  padding: 0 12px;
  border-radius: 6px;
  font-weight: 800;
}
.primary-button { color: #fff; background: #087E6B; }
.ghost-button { color: #087E6B; background: #E7F4EF; }
.danger-button { color: #D94C4C; background: #FFF0F0; }
.loading-state, .empty-state { padding: 70px 20px; text-align: center; color: #687872; }
.staff-list { display: grid; gap: 10px; padding: 12px; }
.staff-card {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 10px;
  padding: 16px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;

  h3 { margin: 0 0 4px; font-size: 16px; }
  p { margin: 0; color: #687872; font-size: 13px; }
}
.staff-meta { display: flex; gap: 8px; align-items: center; color: #687872; font-size: 13px; }
.staff-meta .active { color: #087E6B; font-weight: 800; }
.staff-actions { grid-column: 1 / -1; display: flex; flex-wrap: wrap; gap: 8px; justify-content: flex-end; border-top: 1px solid #E2E8E3; padding-top: 12px; }
.modal-overlay { position: fixed; inset: 0; z-index: 200; display: grid; place-items: center; padding: 16px; background: rgba(0,0,0,.45); }
.modal-content { width: min(420px, 100%); padding: 20px; border-radius: 8px; background: #fff; }
label { display: grid; gap: 6px; margin-bottom: 12px; color: #687872; font-size: 13px; }
input, select { min-height: 38px; padding: 8px 10px; border: 1px solid #DCE6E1; border-radius: 8px; color: #1F2A26; background: #fff; }
.modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
@media (max-width: 620px) { .staff-card { grid-template-columns: 1fr; } .staff-actions { justify-content: flex-start; } }
</style>
