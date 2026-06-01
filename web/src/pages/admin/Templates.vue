<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { templateApi, industryCategoryApi } from '@/api/modules/template'
import { fileApi } from '@/api/modules/file'

const loading = ref(false)
const saving = ref(false)
const templates = ref<any[]>([])
const industries = ref<any[]>([])
const selectedId = ref('')
const editingTemplateId = ref('')
const showTemplateEditor = ref(false)
const showCategoryEditor = ref(false)
const showProductEditor = ref(false)

const selectedTemplate = computed(() => templates.value.find(t => t.id === selectedId.value))
const templateCategories = computed(() => selectedTemplate.value?.templateCategories || [])
const templateProducts = computed(() => selectedTemplate.value?.templateProducts || [])

const templateForm = reactive({
  name: '',
  industryCategoryId: '',
  coverImageUrl: '',
  description: '',
  status: 'active',
})

const categoryForm = reactive({
  name: '',
  categoryType: 'normal',
  icon: '',
})

const productForm = reactive<any>({
  categoryId: '',
  name: '',
  referencePrice: 0,
  description: '',
  imageUrl: '',
  minOrderQty: 1,
})

const flatIndustries = computed(() => {
  const rows: any[] = []
  const walk = (items: any[], depth = 0) => {
    items.forEach((item) => {
      rows.push({ ...item, depth })
      if (item.children?.length) walk(item.children, depth + 1)
    })
  }
  walk(industries.value)
  return rows
})

const loadData = async () => {
  loading.value = true
  try {
    const [templateRows, industryRows] = await Promise.all([
      templateApi.getList(),
      industryCategoryApi.getTree(),
    ])
    templates.value = templateRows || []
    industries.value = industryRows || []
    if (!selectedId.value && templates.value.length) selectedId.value = templates.value[0].id
  } finally {
    loading.value = false
  }
}

const openTemplateCreator = () => {
  editingTemplateId.value = ''
  templateForm.name = ''
  templateForm.industryCategoryId = flatIndustries.value[0]?.id || ''
  templateForm.coverImageUrl = ''
  templateForm.description = ''
  templateForm.status = 'active'
  showTemplateEditor.value = true
}

const openTemplateEditor = () => {
  const template = selectedTemplate.value
  if (!template) return
  editingTemplateId.value = template.id
  templateForm.name = template.name || ''
  templateForm.industryCategoryId = template.industryCategoryId || ''
  templateForm.coverImageUrl = template.coverImageUrl || ''
  templateForm.description = template.description || ''
  templateForm.status = template.status || 'active'
  showTemplateEditor.value = true
}

const saveTemplate = async () => {
  if (!templateForm.name.trim()) return
  saving.value = true
  try {
    const payload = {
      name: templateForm.name.trim(),
      industryCategoryId: templateForm.industryCategoryId || undefined,
      coverImageUrl: templateForm.coverImageUrl,
      description: templateForm.description,
      status: templateForm.status,
    }
    if (editingTemplateId.value) {
      await templateApi.update(editingTemplateId.value, payload)
    } else {
      const template = await templateApi.createFromScratch(payload as any)
      selectedId.value = template.id
    }
    showTemplateEditor.value = false
    await loadData()
  } finally {
    saving.value = false
  }
}

const toggleTemplateStatus = async () => {
  if (!selectedTemplate.value) return
  await templateApi.update(selectedTemplate.value.id, { status: selectedTemplate.value.status === 'active' ? 'inactive' : 'active' })
  await loadData()
}

const openCategoryCreator = () => {
  categoryForm.name = ''
  categoryForm.categoryType = 'normal'
  categoryForm.icon = ''
  showCategoryEditor.value = true
}

const saveCategory = async () => {
  if (!selectedTemplate.value || !categoryForm.name.trim()) return
  await templateApi.addCategory(selectedTemplate.value.id, {
    name: categoryForm.name.trim(),
    categoryType: categoryForm.categoryType,
    icon: categoryForm.icon,
    sortOrder: templateCategories.value.length + 1,
  })
  showCategoryEditor.value = false
  await loadData()
}

const removeCategory = async (category: any) => {
  if (!selectedTemplate.value || !confirm(`确定删除分类「${category.name}」吗？`)) return
  await templateApi.removeCategory(selectedTemplate.value.id, category.id)
  await loadData()
}

const openProductCreator = (categoryId = '') => {
  productForm.categoryId = categoryId || templateCategories.value[0]?.id || ''
  productForm.name = ''
  productForm.referencePrice = 0
  productForm.description = ''
  productForm.imageUrl = ''
  productForm.minOrderQty = 1
  showProductEditor.value = true
}

const saveProduct = async () => {
  if (!selectedTemplate.value || !productForm.categoryId || !productForm.name.trim()) return
  await templateApi.addProduct(selectedTemplate.value.id, {
    categoryId: productForm.categoryId,
    name: productForm.name.trim(),
    referencePrice: Number(productForm.referencePrice || 0),
    description: productForm.description,
    imageUrl: productForm.imageUrl,
    minOrderQty: Number(productForm.minOrderQty || 1),
    specGroups: [],
  })
  showProductEditor.value = false
  await loadData()
}

const removeProduct = async (product: any) => {
  if (!selectedTemplate.value || !confirm(`确定删除商品「${product.name}」吗？`)) return
  await templateApi.removeProduct(selectedTemplate.value.id, product.id)
  await loadData()
}

const uploadTemplateCover = async (event: Event) => {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  try {
    const result = await fileApi.upload(file)
    templateForm.coverImageUrl = result.url
  } catch {
    // The upload API already shows the concrete error message.
  } finally {
    input.value = ''
  }
}

const uploadProductImage = async (event: Event) => {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  try {
    const result = await fileApi.upload(file)
    productForm.imageUrl = result.url
  } catch {
    // The upload API already shows the concrete error message.
  } finally {
    input.value = ''
  }
}

const productsInCategory = (categoryId: string) => templateProducts.value.filter((p: any) => p.templateCategoryId === categoryId)

onMounted(loadData)
</script>

<template>
  <div class="templates-page">
    <header class="page-header">
      <h2>模板管理</h2>
      <button class="primary-button" @click="openTemplateCreator">新建模板</button>
    </header>

    <div v-if="loading" class="loading-state">加载中...</div>
    <main v-else class="template-shell">
      <aside class="template-list">
        <button
          v-for="template in templates"
          :key="template.id"
          class="template-item"
          :class="{ active: selectedId === template.id }"
          @click="selectedId = template.id"
        >
          <strong>{{ template.name }}</strong>
          <span>{{ template.industryCategory?.name || '未分类' }} · 使用 {{ template.useCount || 0 }} 次</span>
        </button>
      </aside>

      <section v-if="selectedTemplate" class="template-detail">
        <div class="detail-head">
          <div>
            <h3>{{ selectedTemplate.name }}</h3>
            <p>{{ selectedTemplate.description || '暂无描述' }}</p>
          </div>
          <div class="detail-actions">
            <button class="ghost-button" @click="openTemplateEditor">编辑模板</button>
            <button class="ghost-button" @click="toggleTemplateStatus">{{ selectedTemplate.status === 'active' ? '停用' : '启用' }}</button>
            <button class="primary-button" @click="openCategoryCreator">添加分类</button>
          </div>
        </div>

        <article v-for="category in templateCategories" :key="category.id" class="template-category">
          <div class="category-head">
            <strong>{{ category.name }}</strong>
            <div>
              <button class="ghost-button compact" @click="openProductCreator(category.id)">加商品</button>
              <button class="danger-button compact" @click="removeCategory(category)">删分类</button>
            </div>
          </div>
          <div class="product-grid">
            <div v-for="product in productsInCategory(category.id)" :key="product.id" class="template-product">
              <span>{{ product.name }}</span>
              <strong>￥{{ Number(product.referencePrice || 0).toFixed(2) }}</strong>
              <button class="danger-button compact" @click="removeProduct(product)">删除</button>
            </div>
          </div>
        </article>
      </section>
      <section v-else class="empty-state">暂无模板</section>
    </main>

    <div v-if="showTemplateEditor" class="modal-overlay" @click.self="showTemplateEditor = false">
      <section class="modal-content">
        <h3>模板信息</h3>
        <label>模板名称<input v-model="templateForm.name" /></label>
        <label>
          行业分类
          <select v-model="templateForm.industryCategoryId">
            <option value="">未分类</option>
            <option v-for="item in flatIndustries" :key="item.id" :value="item.id">{{ '　'.repeat(item.depth) }}{{ item.name }}</option>
          </select>
        </label>
        <label>封面 URL<input v-model="templateForm.coverImageUrl" /></label>
        <label>上传封面<input type="file" accept="image/*" @change="uploadTemplateCover" /></label>
        <label>说明<textarea v-model="templateForm.description" rows="3"></textarea></label>
        <div class="modal-actions">
          <button class="ghost-button" @click="showTemplateEditor = false">取消</button>
          <button class="primary-button" :disabled="saving" @click="saveTemplate">{{ saving ? '保存中...' : '保存' }}</button>
        </div>
      </section>
    </div>

    <div v-if="showCategoryEditor" class="modal-overlay" @click.self="showCategoryEditor = false">
      <section class="modal-content">
        <h3>模板分类</h3>
        <label>分类名称<input v-model="categoryForm.name" /></label>
        <label>
          分类类型
          <select v-model="categoryForm.categoryType">
            <option value="normal">普通</option>
            <option value="hot">热销</option>
            <option value="welfare">进店福利</option>
            <option value="combo">套餐</option>
          </select>
        </label>
        <label>图标<input v-model="categoryForm.icon" /></label>
        <div class="modal-actions">
          <button class="ghost-button" @click="showCategoryEditor = false">取消</button>
          <button class="primary-button" @click="saveCategory">保存</button>
        </div>
      </section>
    </div>

    <div v-if="showProductEditor" class="modal-overlay" @click.self="showProductEditor = false">
      <section class="modal-content">
        <h3>模板商品</h3>
        <label>
          所属分类
          <select v-model="productForm.categoryId">
            <option v-for="category in templateCategories" :key="category.id" :value="category.id">{{ category.name }}</option>
          </select>
        </label>
        <label>商品名称<input v-model="productForm.name" /></label>
        <label>参考价格<input v-model.number="productForm.referencePrice" type="number" min="0" step="0.01" /></label>
        <label>起购数量<input v-model.number="productForm.minOrderQty" type="number" min="1" /></label>
        <label>图片 URL<input v-model="productForm.imageUrl" /></label>
        <label>上传图片<input type="file" accept="image/*" @change="uploadProductImage" /></label>
        <label>描述<textarea v-model="productForm.description" rows="3"></textarea></label>
        <div class="modal-actions">
          <button class="ghost-button" @click="showProductEditor = false">取消</button>
          <button class="primary-button" @click="saveProduct">保存</button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped lang="scss">
.templates-page { min-height: 100vh; color: #1F2A26; background: #F6F7F3; }
.page-header, .template-list, .template-detail, .empty-state { border: 1px solid #E2E8E3; border-radius: 8px; background: #fff; }
.page-header { padding: 16px; display: flex; align-items: center; justify-content: space-between; h2 { margin: 0; font-size: 18px; } }
.primary-button, .ghost-button, .danger-button { min-height: 34px; padding: 0 12px; border-radius: 6px; font-weight: 800; }
.primary-button { color: #fff; background: #087E6B; } .ghost-button { color: #087E6B; background: #E7F4EF; } .danger-button { color: #D94C4C; background: #FFF0F0; }
.compact { min-height: 28px; padding: 0 9px; font-size: 12px; }
.loading-state, .empty-state { padding: 60px; text-align: center; color: #687872; }
.template-shell { display: grid; grid-template-columns: 260px minmax(0, 1fr); gap: 12px; margin-top: 12px; }
.template-list { padding: 8px; display: grid; align-content: start; gap: 6px; }
.template-item { padding: 10px; border-radius: 6px; display: grid; gap: 4px; text-align: left; color: #1F2A26; background: #F6F7F3; span { color: #687872; font-size: 12px; } &.active { color: #087E6B; background: #E7F4EF; } }
.template-detail { padding: 16px; }
.detail-head, .category-head { display: flex; align-items: center; justify-content: space-between; gap: 12px; h3 { margin: 0; } p { margin: 4px 0 0; color: #687872; font-size: 13px; } }
.detail-actions, .category-head div { display: flex; flex-wrap: wrap; gap: 8px; }
.template-category { margin-top: 14px; padding: 12px; border: 1px solid #E2E8E3; border-radius: 8px; background: #FAFCFA; }
.product-grid { display: grid; gap: 8px; margin-top: 10px; }
.template-product { min-height: 42px; padding: 8px 10px; border: 1px solid #E2E8E3; border-radius: 6px; display: grid; grid-template-columns: minmax(0, 1fr) auto auto; gap: 8px; align-items: center; background: #fff; }
.modal-overlay { position: fixed; inset: 0; z-index: 200; display: grid; place-items: center; padding: 16px; background: rgba(0,0,0,.45); }
.modal-content { width: min(520px, 100%); max-height: 90vh; overflow-y: auto; padding: 20px; border-radius: 8px; background: #fff; }
label { display: grid; gap: 6px; margin-bottom: 12px; color: #687872; font-size: 13px; }
input, select, textarea { min-height: 38px; padding: 8px 10px; border: 1px solid #DCE6E1; border-radius: 8px; color: #1F2A26; background: #fff; }
textarea { resize: vertical; }
.modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
@media (max-width: 820px) { .template-shell { grid-template-columns: 1fr; } .template-list { grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); } .detail-head { align-items: stretch; flex-direction: column; } }
</style>
