<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { adminApi } from '@/api/modules/admin'

const adminInfo = ref<any>(null)
const configs = ref<any[]>([])
const loading = ref(false)
const saving = ref(false)
const showEditor = ref(false)

const form = reactive({
  configKey: '',
  configValue: '',
  configType: 'string',
  description: '',
  isPublic: false,
})

const operatorId = () => localStorage.getItem('admin_id') || undefined

const loadAdminInfo = () => {
  const info = localStorage.getItem('admin_info')
  if (info) {
    try { adminInfo.value = JSON.parse(info) } catch { /* ignore */ }
  }
}

const loadConfigs = async () => {
  loading.value = true
  try {
    configs.value = await adminApi.getConfigs() || []
  } finally {
    loading.value = false
  }
}

const openCreator = () => {
  form.configKey = ''
  form.configValue = ''
  form.configType = 'string'
  form.description = ''
  form.isPublic = false
  showEditor.value = true
}

const openEditor = (config: any) => {
  form.configKey = config.configKey
  form.configValue = config.configValue
  form.configType = config.configType || 'string'
  form.description = config.description || ''
  form.isPublic = !!config.isPublic
  showEditor.value = true
}

const saveConfig = async () => {
  if (!form.configKey.trim()) return
  saving.value = true
  try {
    await adminApi.upsertConfig({
      ...form,
      configKey: form.configKey.trim(),
      operatorId: operatorId(),
    })
    showEditor.value = false
    await loadConfigs()
  } finally {
    saving.value = false
  }
}

const deleteConfig = async (config: any) => {
  if (!confirm(`确定删除配置「${config.configKey}」吗？`)) return
  await adminApi.deleteConfig(config.id, operatorId())
  await loadConfigs()
}

onMounted(() => {
  loadAdminInfo()
  loadConfigs()
})
</script>

<template>
  <div class="configs-page">
    <header class="page-header">
      <h2>系统配置</h2>
      <button class="primary-button" @click="openCreator">新增配置</button>
    </header>

    <section class="admin-card">
      <div>
        <span>当前管理员</span>
        <strong>{{ adminInfo?.displayName || adminInfo?.username || '-' }}</strong>
      </div>
      <div>
        <span>角色</span>
        <strong>{{ adminInfo?.role || '-' }}</strong>
      </div>
    </section>

    <div v-if="loading" class="loading-state">加载中...</div>
    <section v-else class="config-list">
      <article v-for="config in configs" :key="config.id" class="config-card">
        <div>
          <h3>{{ config.configKey }}</h3>
          <p>{{ config.description || '暂无说明' }}</p>
        </div>
        <div class="config-value">
          <span>{{ config.configType }}</span>
          <strong>{{ config.configValue }}</strong>
          <small>{{ config.isPublic ? '公开' : '内部' }}</small>
        </div>
        <div class="config-actions">
          <button class="ghost-button" @click="openEditor(config)">编辑</button>
          <button class="danger-button" @click="deleteConfig(config)">删除</button>
        </div>
      </article>
    </section>

    <div v-if="showEditor" class="modal-overlay" @click.self="showEditor = false">
      <section class="modal-content">
        <h3>配置项</h3>
        <label>
          配置键
          <input v-model="form.configKey" placeholder="例如 site.name" />
        </label>
        <label>
          配置值
          <textarea v-model="form.configValue" rows="3"></textarea>
        </label>
        <label>
          类型
          <select v-model="form.configType">
            <option value="string">字符串</option>
            <option value="number">数字</option>
            <option value="boolean">布尔</option>
            <option value="json">JSON</option>
          </select>
        </label>
        <label>
          说明
          <input v-model="form.description" />
        </label>
        <label class="inline-check">
          <input v-model="form.isPublic" type="checkbox" />
          允许前端公开读取
        </label>
        <div class="modal-actions">
          <button class="ghost-button" @click="showEditor = false">取消</button>
          <button class="primary-button" :disabled="saving" @click="saveConfig">{{ saving ? '保存中...' : '保存' }}</button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped lang="scss">
.configs-page { min-height: 100vh; color: #1F2A26; background: #F6F7F3; }
.page-header, .admin-card, .config-card {
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
}
.page-header { padding: 16px; display: flex; align-items: center; justify-content: space-between; h2 { margin: 0; font-size: 18px; } }
.primary-button, .ghost-button, .danger-button { min-height: 34px; padding: 0 12px; border-radius: 6px; font-weight: 800; }
.primary-button { color: #fff; background: #087E6B; }
.ghost-button { color: #087E6B; background: #E7F4EF; }
.danger-button { color: #D94C4C; background: #FFF0F0; }
.admin-card { margin: 12px 0; padding: 14px 16px; display: flex; gap: 28px; span { display: block; color: #687872; font-size: 12px; } strong { font-size: 15px; } }
.loading-state { padding: 60px; text-align: center; color: #687872; }
.config-list { display: grid; gap: 10px; }
.config-card { padding: 14px 16px; display: grid; grid-template-columns: minmax(0, 1fr) minmax(160px, .5fr) auto; gap: 12px; align-items: center; h3 { margin: 0 0 4px; font-size: 15px; } p { margin: 0; color: #687872; font-size: 12px; } }
.config-value { display: grid; gap: 2px; span, small { color: #687872; font-size: 12px; } strong { word-break: break-word; } }
.config-actions { display: flex; gap: 8px; }
.modal-overlay { position: fixed; inset: 0; z-index: 200; display: grid; place-items: center; padding: 16px; background: rgba(0,0,0,.45); }
.modal-content { width: min(520px, 100%); padding: 20px; border-radius: 8px; background: #fff; }
label { display: grid; gap: 6px; margin-bottom: 12px; color: #687872; font-size: 13px; }
input, select, textarea { min-height: 38px; padding: 8px 10px; border: 1px solid #DCE6E1; border-radius: 8px; color: #1F2A26; background: #fff; }
textarea { resize: vertical; }
.inline-check { display: flex; align-items: center; gap: 6px; } .inline-check input { width: auto; min-height: auto; }
.modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
@media (max-width: 720px) { .config-card { grid-template-columns: 1fr; } .config-actions { justify-content: flex-start; } }
</style>
