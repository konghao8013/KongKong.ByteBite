<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { industryCategoryApi } from '@/api/modules/template'

const loading = ref(false)
const categories = ref<any[]>([])
const showEditor = ref(false)
const editingId = ref('')

const form = reactive({
  parentId: '',
  name: '',
  icon: '',
  sortOrder: 1,
})

const flattenCategories = computed(() => {
  const rows: any[] = []
  const walk = (items: any[], depth = 0) => {
    items.forEach((item) => {
      rows.push({ ...item, depth })
      if (item.children?.length) walk(item.children, depth + 1)
    })
  }
  walk(categories.value)
  return rows
})

const loadCategories = async () => {
  loading.value = true
  try {
    categories.value = await industryCategoryApi.getTree() || []
  } finally {
    loading.value = false
  }
}

const openCreator = (parentId = '') => {
  editingId.value = ''
  form.parentId = parentId
  form.name = ''
  form.icon = ''
  form.sortOrder = flattenCategories.value.length + 1
  showEditor.value = true
}

const openEditor = (item: any) => {
  editingId.value = item.id
  form.parentId = item.parentId || ''
  form.name = item.name || ''
  form.icon = item.icon || ''
  form.sortOrder = item.sortOrder || 1
  showEditor.value = true
}

const saveCategory = async () => {
  if (!form.name.trim()) return
  if (editingId.value) {
    await industryCategoryApi.update(editingId.value, { name: form.name.trim(), icon: form.icon, sortOrder: Number(form.sortOrder || 1) })
  } else {
    await industryCategoryApi.create({ parentId: form.parentId || undefined, name: form.name.trim(), icon: form.icon, sortOrder: Number(form.sortOrder || 1) })
  }
  showEditor.value = false
  await loadCategories()
}

const deleteCategory = async (item: any) => {
  if (!confirm(`确定删除「${item.name}」吗？`)) return
  await industryCategoryApi.delete(item.id)
  await loadCategories()
}

onMounted(loadCategories)
</script>

<template>
  <div class="industry-page">
    <header class="page-header">
      <h2>行业分类</h2>
      <button class="primary-button" @click="openCreator()">新增一级分类</button>
    </header>

    <div v-if="loading" class="loading-state">加载中...</div>
    <section v-else class="category-list">
      <article v-for="item in flattenCategories" :key="item.id" class="category-row" :style="{ paddingLeft: `${16 + item.depth * 22}px` }">
        <span class="category-icon">{{ item.icon || '类' }}</span>
        <div>
          <strong>{{ item.name }}</strong>
          <small>第 {{ item.level }} 级，排序 {{ item.sortOrder }}</small>
        </div>
        <div class="row-actions">
          <button v-if="item.level < 3" class="ghost-button" @click="openCreator(item.id)">加子类</button>
          <button class="ghost-button" @click="openEditor(item)">编辑</button>
          <button class="danger-button" @click="deleteCategory(item)">删除</button>
        </div>
      </article>
    </section>

    <div v-if="showEditor" class="modal-overlay" @click.self="showEditor = false">
      <section class="modal-content">
        <h3>{{ editingId ? '编辑分类' : '新增分类' }}</h3>
        <label v-if="!editingId">
          父级分类
          <select v-model="form.parentId">
            <option value="">一级分类</option>
            <option v-for="item in flattenCategories.filter(i => i.level < 3)" :key="item.id" :value="item.id">
              {{ '　'.repeat(item.depth) }}{{ item.name }}
            </option>
          </select>
        </label>
        <label>名称<input v-model="form.name" /></label>
        <label>图标<input v-model="form.icon" /></label>
        <label>排序<input v-model.number="form.sortOrder" type="number" min="1" /></label>
        <div class="modal-actions">
          <button class="ghost-button" @click="showEditor = false">取消</button>
          <button class="primary-button" @click="saveCategory">保存</button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped lang="scss">
.industry-page { min-height: 100vh; background: #F6F7F3; color: #1F2A26; }
.page-header, .category-row { border: 1px solid #E2E8E3; border-radius: 8px; background: #fff; }
.page-header { padding: 16px; display: flex; align-items: center; justify-content: space-between; h2 { margin: 0; font-size: 18px; } }
.primary-button, .ghost-button, .danger-button { min-height: 34px; padding: 0 12px; border-radius: 6px; font-weight: 800; }
.primary-button { color: #fff; background: #087E6B; } .ghost-button { color: #087E6B; background: #E7F4EF; } .danger-button { color: #D94C4C; background: #FFF0F0; }
.loading-state { padding: 60px; text-align: center; color: #687872; }
.category-list { display: grid; gap: 8px; margin-top: 12px; }
.category-row { min-height: 58px; display: grid; grid-template-columns: auto minmax(0, 1fr) auto; align-items: center; gap: 10px; padding: 10px 16px; }
.category-icon { width: 30px; height: 30px; border-radius: 6px; display: grid; place-items: center; color: #087E6B; background: #E7F4EF; font-weight: 800; }
small { display: block; color: #687872; margin-top: 2px; } .row-actions { display: flex; flex-wrap: wrap; gap: 8px; }
.modal-overlay { position: fixed; inset: 0; z-index: 200; display: grid; place-items: center; padding: 16px; background: rgba(0,0,0,.45); }
.modal-content { width: min(420px, 100%); padding: 20px; border-radius: 8px; background: #fff; }
label { display: grid; gap: 6px; margin-bottom: 12px; color: #687872; font-size: 13px; } input, select { min-height: 38px; padding: 8px 10px; border: 1px solid #DCE6E1; border-radius: 8px; }
.modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
@media (max-width: 720px) { .category-row { grid-template-columns: auto 1fr; } .row-actions { grid-column: 1 / -1; } }
</style>
