<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { categoryApi } from '@/api/modules/category'
import { productApi } from '@/api/modules/product'
import { fileApi } from '@/api/modules/file'
import { templateApi } from '@/api/modules/template'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const saving = ref(false)
const categories = ref<any[]>([])
const products = ref<any[]>([])
const activeCategory = ref<string>('')
const showCategoryEditor = ref(false)
const showProductEditor = ref(false)
const editingProductId = ref('')
const uploadingImage = ref(false)
const previewImageUrl = ref('')
const showTemplatePicker = ref(false)
const templateLoading = ref(false)
const templateDetailLoading = ref(false)
const templateImporting = ref(false)
const templates = ref<any[]>([])
const selectedTemplateId = ref('')
const draggingProductId = ref('')
const dragOverCategoryId = ref('')
const productOrderSaving = ref(false)

const categoryForm = reactive({
  name: '',
  icon: '',
  categoryType: 'normal',
})

const productForm = reactive<any>({
  categoryId: '',
  name: '',
  description: '',
  basePrice: 0,
  imageUrl: '',
  minOrderQty: 1,
  status: 'on',
  isCombo: false,
  comboItems: [],
  specGroups: [],
})

const sortProducts = (items: any[]) =>
  [...items].sort((left, right) =>
    Number(left.sortOrder || 0) - Number(right.sortOrder || 0)
    || String(left.name || '').localeCompare(String(right.name || ''))
  )

const getProductsByCategory = (categoryId: string) =>
  sortProducts(products.value.filter((product) => product.categoryId === categoryId))

const activeCategoryProducts = computed(() => {
  if (!activeCategory.value) return sortProducts(products.value)
  return getProductsByCategory(activeCategory.value)
})

const availableComboProducts = computed(() =>
  products.value.filter((product) => product.id !== editingProductId.value && !product.isCombo)
)

const activeTemplates = computed(() =>
  templates.value.filter((template) => (template.status || 'active') === 'active')
)

const selectedTemplate = computed(() =>
  activeTemplates.value.find((template) => template.id === selectedTemplateId.value) || null
)

const resetCategoryForm = () => {
  categoryForm.name = ''
  categoryForm.icon = ''
  categoryForm.categoryType = 'normal'
}

const resetProductForm = () => {
  editingProductId.value = ''
  productForm.categoryId = activeCategory.value || categories.value[0]?.id || ''
  productForm.name = ''
  productForm.description = ''
  productForm.basePrice = 0
  productForm.imageUrl = ''
  productForm.minOrderQty = 1
  productForm.status = 'on'
  productForm.isCombo = false
  productForm.comboItems = []
  productForm.specGroups = []
}

const loadCategories = async () => {
  if (!storeId) return
  categories.value = await categoryApi.getByStoreId(storeId) || []
  if (categories.value.length > 0 && !activeCategory.value) {
    activeCategory.value = categories.value[0].id
  }
}

const loadProducts = async () => {
  if (!storeId) return
  products.value = await productApi.getByStoreId(storeId) || []
}

const loadData = async () => {
  loading.value = true
  try {
    await Promise.all([loadCategories(), loadProducts()])
  } finally {
    loading.value = false
  }
}

const openCategoryEditor = () => {
  resetCategoryForm()
  showCategoryEditor.value = true
}

const saveCategory = async () => {
  if (!categoryForm.name.trim()) return
  await categoryApi.create(storeId, {
    name: categoryForm.name.trim(),
    sortOrder: categories.value.length + 1,
    categoryType: categoryForm.categoryType,
    icon: categoryForm.icon,
  })
  showCategoryEditor.value = false
  await loadCategories()
}

const deleteCategory = async (id: string) => {
  if (!confirm('确定删除这个分类？分类下仍有商品时不能删除。')) return
  await categoryApi.delete(id)
  if (activeCategory.value === id) activeCategory.value = ''
  await loadData()
}

const openProductCreator = () => {
  resetProductForm()
  showProductEditor.value = true
}

const openProductEditor = (product: any) => {
  editingProductId.value = product.id
  productForm.categoryId = product.categoryId
  productForm.name = product.name || ''
  productForm.description = product.description || ''
  productForm.basePrice = Number(product.basePrice || 0)
  productForm.imageUrl = product.imageUrl || ''
  productForm.minOrderQty = Number(product.minOrderQty || 1)
  productForm.status = product.status || 'on'
  productForm.isCombo = !!product.isCombo
  productForm.comboItems = (product.comboItemComboProducts || []).map((item: any) => ({
    productId: item.productId || item.product?.id || '',
    quantity: Number(item.quantity || 1),
    allowChangeSpec: item.allowChangeSpec ?? true,
    remark: item.remark || '',
  }))
  productForm.specGroups = (product.specGroups || []).map((group: any) => ({
    name: group.name || '',
    isRequired: group.isRequired ?? true,
    sortOrder: group.sortOrder || 0,
    options: (group.specOptions || group.options || []).map((option: any) => ({
      name: option.name || '',
      extraPrice: Number(option.extraPrice || 0),
      stock: option.stock ?? undefined,
      sortOrder: option.sortOrder || 0,
      isDefault: option.isDefault || false,
    })),
  }))
  showProductEditor.value = true
}

const addSpecGroup = () => {
  productForm.specGroups.push({
    name: '',
    isRequired: true,
    sortOrder: productForm.specGroups.length + 1,
    options: [{ name: '', extraPrice: 0, stock: undefined, sortOrder: 1, isDefault: true }],
  })
}

const removeSpecGroup = (index: number) => {
  productForm.specGroups.splice(index, 1)
}

const addSpecOption = (group: any) => {
  group.options.push({
    name: '',
    extraPrice: 0,
    stock: undefined,
    sortOrder: group.options.length + 1,
    isDefault: group.options.length === 0,
  })
}

const removeSpecOption = (group: any, index: number) => {
  group.options.splice(index, 1)
}

const addComboItem = () => {
  productForm.comboItems.push({
    productId: availableComboProducts.value[0]?.id || '',
    quantity: 1,
    allowChangeSpec: true,
    remark: '',
  })
}

const removeComboItem = (index: number) => {
  productForm.comboItems.splice(index, 1)
}

const uploadProductImage = async (event: Event) => {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  uploadingImage.value = true
  try {
    const result = await fileApi.upload(file)
    productForm.imageUrl = result.url
  } finally {
    uploadingImage.value = false
    input.value = ''
  }
}

const clearProductImage = () => {
  productForm.imageUrl = ''
}

const openImagePreview = (imageUrl?: string) => {
  if (!imageUrl) return
  previewImageUrl.value = imageUrl
}

const closeImagePreview = () => {
  previewImageUrl.value = ''
}

const getTemplateCategories = (template: any) =>
  template?.templateCategories || template?.categories || []

const getTemplateProducts = (template: any) =>
  template?.templateProducts || template?.products || []

const getProductsInTemplateCategory = (template: any, category: any) => {
  const directProducts = category?.templateProducts || category?.products
  if (directProducts?.length) return directProducts

  return getTemplateProducts(template).filter((product: any) =>
    (product.templateCategoryId || product.categoryId) === category.id
  )
}

const countTemplateProducts = (template: any) =>
  getTemplateCategories(template).reduce(
    (total: number, category: any) => total + getProductsInTemplateCategory(template, category).length,
    0
  )

const templateIndustryName = (template: any) =>
  template?.industryCategory?.name || template?.industryCategoryName || '未分类'

const loadTemplates = async () => {
  templateLoading.value = true
  try {
    templates.value = await templateApi.getList() || []
    if (!activeTemplates.value.some((template) => template.id === selectedTemplateId.value)) {
      selectedTemplateId.value = ''
    }
  } finally {
    templateLoading.value = false
  }
}

const viewTemplate = async (template: any) => {
  selectedTemplateId.value = template.id
  templateDetailLoading.value = true
  try {
    const detail = await templateApi.getById(template.id)
    const index = templates.value.findIndex((item) => item.id === template.id)
    if (index >= 0) {
      templates.value.splice(index, 1, detail)
    }
  } finally {
    templateDetailLoading.value = false
  }
}

const openTemplatePicker = async () => {
  showTemplatePicker.value = true
  if (templates.value.length === 0) {
    await loadTemplates()
  }
  if (!selectedTemplate.value && activeTemplates.value.length > 0) {
    await viewTemplate(activeTemplates.value[0])
  }
}

const importSelectedTemplate = async () => {
  const template = selectedTemplate.value
  if (!template || !storeId) return

  const categoryCount = getTemplateCategories(template).length
  const productCount = countTemplateProducts(template)
  const confirmed = confirm(`确认引入「${template.name}」？将复制 ${categoryCount} 个分类、${productCount} 个商品到当前店铺，导入商品默认为下架。`)
  if (!confirmed) return

  templateImporting.value = true
  try {
    await templateApi.applyTemplate({
      templateId: template.id,
      storeId,
      applyAll: true,
    })
    showTemplatePicker.value = false
    await loadData()
    alert(`已引入「${template.name}」，请检查价格和上架状态后再发布。`)
  } finally {
    templateImporting.value = false
  }
}

const buildProductPayload = () => ({
  storeId,
  categoryId: productForm.categoryId,
  name: productForm.name.trim(),
  description: productForm.description,
  basePrice: Number(productForm.basePrice || 0),
  imageUrl: productForm.imageUrl,
  minOrderQty: Number(productForm.minOrderQty || 1),
  status: productForm.status,
  isCombo: !!productForm.isCombo,
  comboItems: productForm.isCombo
    ? productForm.comboItems
      .filter((item: any) => item.productId)
      .map((item: any, index: number) => ({
        productId: item.productId,
        quantity: Number(item.quantity || 1),
        allowChangeSpec: !!item.allowChangeSpec,
        remark: item.remark,
        sortOrder: index + 1,
      }))
    : [],
  specGroups: productForm.specGroups
    .filter((group: any) => group.name.trim())
    .map((group: any, groupIndex: number) => ({
      name: group.name.trim(),
      isRequired: !!group.isRequired,
      sortOrder: groupIndex + 1,
      options: group.options
        .filter((option: any) => option.name.trim())
        .map((option: any, optionIndex: number) => ({
          name: option.name.trim(),
          extraPrice: Number(option.extraPrice || 0),
          stock: option.stock === '' || option.stock == null ? undefined : Number(option.stock),
          sortOrder: optionIndex + 1,
          isDefault: !!option.isDefault,
        })),
    })),
})

const saveProduct = async () => {
  if (!productForm.categoryId || !productForm.name.trim()) return
  saving.value = true
  try {
    const payload = buildProductPayload()
    if (editingProductId.value) {
      await productApi.update(editingProductId.value, payload)
    } else {
      await productApi.create(payload)
    }
    showProductEditor.value = false
    await loadProducts()
  } finally {
    saving.value = false
  }
}

const deleteProduct = async (product: any) => {
  if (!confirm(`确定删除「${product.name}」？`)) return
  await productApi.delete(product.id)
  await loadProducts()
}

const toggleProductStatus = async (product: any) => {
  const status = product.status === 'on' ? 'off' : 'on'
  await productApi.update(product.id, { status })
  product.status = status
}

const startProductDrag = (product: any, event: DragEvent) => {
  if (productOrderSaving.value) return
  draggingProductId.value = product.id
  event.dataTransfer?.setData('text/plain', product.id)
  if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move'
}

const finishProductDrag = () => {
  draggingProductId.value = ''
  dragOverCategoryId.value = ''
}

const handleCategoryDragOver = (categoryId: string) => {
  if (!draggingProductId.value) return
  dragOverCategoryId.value = categoryId
}

const saveProductOrders = async (orders: { categoryId: string; items: any[] }[]) => {
  const orderByCategory = new Map<string, any[]>()
  for (const order of orders) {
    orderByCategory.set(order.categoryId, order.items)
  }

  const changes: { product: any; categoryId: string; sortOrder: number }[] = []
  for (const [categoryId, orderedProducts] of orderByCategory) {
    orderedProducts.forEach((product, index) => {
      const sortOrder = index + 1
      if (product.categoryId !== categoryId || Number(product.sortOrder || 0) !== sortOrder) {
        changes.push({ product, categoryId, sortOrder })
      }
    })
  }

  if (changes.length === 0) return

  productOrderSaving.value = true
  for (const change of changes) {
    change.product.categoryId = change.categoryId
    change.product.sortOrder = change.sortOrder
  }

  try {
    await Promise.all(changes.map((change) =>
      productApi.update(change.product.id, {
        categoryId: change.categoryId,
        sortOrder: change.sortOrder,
      })
    ))
    await loadProducts()
  } catch {
    await loadProducts()
  } finally {
    productOrderSaving.value = false
  }
}

const dropProduct = async (destinationCategoryId: string, targetProductId = '') => {
  const draggedProduct = products.value.find((product) => product.id === draggingProductId.value)
  if (!draggedProduct || !destinationCategoryId) {
    finishProductDrag()
    return
  }

  const sourceCategoryId = draggedProduct.categoryId
  const destinationProducts = getProductsByCategory(destinationCategoryId)
    .filter((product) => product.id !== draggedProduct.id)
  const targetIndex = targetProductId
    ? destinationProducts.findIndex((product) => product.id === targetProductId)
    : -1

  if (targetIndex >= 0) {
    destinationProducts.splice(targetIndex, 0, draggedProduct)
  } else {
    destinationProducts.push(draggedProduct)
  }

  const orders = [{ categoryId: destinationCategoryId, items: destinationProducts }]
  if (sourceCategoryId !== destinationCategoryId) {
    orders.push({
      categoryId: sourceCategoryId,
      items: getProductsByCategory(sourceCategoryId).filter((product) => product.id !== draggedProduct.id),
    })
    activeCategory.value = destinationCategoryId
  }

  await saveProductOrders(orders)
  finishProductDrag()
}

const dropProductOnCategory = async (categoryId: string, event: DragEvent) => {
  event.preventDefault()
  event.stopPropagation()
  await dropProduct(categoryId)
}

const dropProductOnProduct = async (product: any, event: DragEvent) => {
  event.preventDefault()
  event.stopPropagation()
  if (product.id === draggingProductId.value) {
    finishProductDrag()
    return
  }
  await dropProduct(product.categoryId, product.id)
}

const dropProductAtListEnd = async (event: DragEvent) => {
  event.preventDefault()
  if (!activeCategory.value) return
  await dropProduct(activeCategory.value)
}

const statusLabel = (status: string) => ({
  on: '上架',
  off: '下架',
  sold_out: '售罄',
}[status] || status)

const categoryLabel = (type: string) => ({
  normal: '普通',
  hot: '热销',
  welfare: '福利',
  combo: '套餐',
}[type] || type)

onMounted(loadData)
</script>

<template>
  <div class="menu-page">
    <header class="menu-header">
      <h2>菜单管理</h2>
      <div class="header-actions">
        <button class="ghost-button" :disabled="!storeId" @click="openTemplatePicker">引用系统模板</button>
        <button class="ghost-button" @click="openCategoryEditor">分类</button>
        <button class="primary-button" :disabled="categories.length === 0" @click="openProductCreator">添加商品</button>
      </div>
    </header>

    <div v-if="loading" class="loading-state">加载中...</div>

    <main v-else class="menu-content">
      <aside class="category-sidebar">
        <button
          v-for="cat in categories"
          :key="cat.id"
          class="category-item"
          :class="{ active: activeCategory === cat.id, 'drag-over': dragOverCategoryId === cat.id }"
          @click="activeCategory = cat.id"
          @dragenter.prevent="handleCategoryDragOver(cat.id)"
          @dragover.prevent="handleCategoryDragOver(cat.id)"
          @drop="dropProductOnCategory(cat.id, $event)"
        >
          <span class="cat-icon">{{ cat.icon || '·' }}</span>
          <span class="cat-name">{{ cat.name }}</span>
          <span class="cat-type">{{ categoryLabel(cat.categoryType) }}</span>
          <span class="cat-count">{{ products.filter(p => p.categoryId === cat.id).length }}</span>
          <span class="cat-delete" @click.stop="deleteCategory(cat.id)">×</span>
        </button>
      </aside>

      <section class="product-list" @dragover.prevent @drop="dropProductAtListEnd">
        <div v-if="categories.length === 0" class="empty-state">
          <p>先添加一个分类，再录入商品。</p>
        </div>
        <div v-else-if="activeCategoryProducts.length === 0" class="empty-state">
          <p>当前分类还没有商品。</p>
          <button class="primary-button compact" @click="openProductCreator">添加商品</button>
        </div>

        <article
          v-for="product in activeCategoryProducts"
          :key="product.id"
          class="product-card"
          :class="{ dragging: draggingProductId === product.id }"
          :draggable="!productOrderSaving"
          @dragstart="startProductDrag(product, $event)"
          @dragend="finishProductDrag"
          @dragover.prevent
          @drop="dropProductOnProduct(product, $event)"
        >
          <span class="drag-handle" title="拖动排序或移动分类">⋮⋮</span>
          <button
            v-if="product.imageUrl"
            type="button"
            class="product-image-button"
            @click="openImagePreview(product.imageUrl)"
          >
            <img :src="product.imageUrl" :alt="`${product.name}图片`" />
            <span>点击放大</span>
          </button>
          <div v-else class="product-image-placeholder">暂无图片</div>
          <div class="product-main">
            <div class="product-title-row">
              <strong>{{ product.name }}</strong>
              <span class="status-pill" :class="product.status">{{ statusLabel(product.status) }}</span>
            </div>
            <p class="product-desc">{{ product.description || '暂无描述' }}</p>
            <div class="product-meta">
              <span>￥{{ Number(product.basePrice || 0).toFixed(2) }}</span>
              <span>起购 {{ product.minOrderQty || 1 }} 份</span>
              <span>月销 {{ product.monthlySales || 0 }}</span>
              <span v-if="product.isCombo" class="combo-pill">套餐</span>
            </div>
            <div v-if="product.specGroups?.length" class="spec-summary">
              <span v-for="group in product.specGroups" :key="group.id">
                {{ group.name }}：{{ group.specOptions?.map((o: any) => `${o.name}${o.extraPrice ? `+￥${o.extraPrice}` : ''}`).join(' / ') }}
              </span>
            </div>
            <div v-if="product.comboItemComboProducts?.length" class="spec-summary">
              <span>包含：{{ product.comboItemComboProducts.map((i: any) => `${i.product?.name || '商品'}×${i.quantity}`).join(' / ') }}</span>
            </div>
          </div>
          <div class="product-actions">
            <button class="text-button" @click="openProductEditor(product)">编辑</button>
            <button class="text-button" @click="toggleProductStatus(product)">{{ product.status === 'on' ? '下架' : '上架' }}</button>
            <button class="danger-button" @click="deleteProduct(product)">删除</button>
          </div>
        </article>
      </section>
    </main>

    <div v-if="previewImageUrl" class="image-preview-overlay" @click.self="closeImagePreview">
      <section class="image-preview-dialog">
        <button type="button" class="image-preview-close" @click="closeImagePreview">×</button>
        <img :src="previewImageUrl" alt="菜品大图" />
      </section>
    </div>

    <div v-if="showTemplatePicker" class="modal-overlay" @click.self="showTemplatePicker = false">
      <section class="modal-content template-picker-modal">
        <div class="template-picker-head">
          <div>
            <h3>引用系统模板</h3>
            <p>选择一个平台模板，查看内容后复制到当前店铺菜单。</p>
          </div>
          <button class="text-button" @click="loadTemplates">刷新</button>
        </div>

        <div v-if="templateLoading" class="loading-state compact-state">模板加载中...</div>
        <div v-else class="template-picker-body">
          <aside class="system-template-list">
            <div v-if="activeTemplates.length === 0" class="template-list-empty">暂无可引用模板</div>
            <button
              v-for="template in activeTemplates"
              :key="template.id"
              type="button"
              class="system-template-item"
              :class="{ active: selectedTemplateId === template.id }"
              @click="viewTemplate(template)"
            >
              <img v-if="template.coverImageUrl" :src="template.coverImageUrl" :alt="template.name" />
              <div v-else class="template-cover-placeholder">模板</div>
              <span class="template-item-main">
                <strong>{{ template.name }}</strong>
                <small>{{ templateIndustryName(template) }} · 使用 {{ template.useCount || 0 }} 次</small>
                <small>{{ getTemplateCategories(template).length }} 个分类 · {{ countTemplateProducts(template) }} 个商品</small>
              </span>
            </button>
          </aside>

          <section class="template-preview-panel">
            <div v-if="!selectedTemplate" class="template-list-empty">请选择左侧模板查看内容</div>
            <template v-else>
              <div class="template-preview-head">
                <div>
                  <h4>{{ selectedTemplate.name }}</h4>
                  <p>{{ selectedTemplate.description || '暂无模板说明' }}</p>
                </div>
                <span>{{ getTemplateCategories(selectedTemplate).length }} 类 / {{ countTemplateProducts(selectedTemplate) }} 品</span>
              </div>

              <div v-if="templateDetailLoading" class="loading-state compact-state">模板内容加载中...</div>
              <div v-else class="template-category-preview-list">
                <article
                  v-for="category in getTemplateCategories(selectedTemplate)"
                  :key="category.id"
                  class="template-category-preview"
                >
                  <div class="preview-category-head">
                    <strong>{{ category.icon || '·' }} {{ category.name }}</strong>
                    <span>{{ getProductsInTemplateCategory(selectedTemplate, category).length }} 个商品</span>
                  </div>
                  <div class="preview-product-list">
                    <div
                      v-for="product in getProductsInTemplateCategory(selectedTemplate, category)"
                      :key="product.id"
                      class="preview-product"
                    >
                      <img v-if="product.imageUrl" :src="product.imageUrl" :alt="product.name" />
                      <div v-else class="preview-product-placeholder">图</div>
                      <div class="preview-product-info">
                        <strong>{{ product.name }}</strong>
                        <span>{{ product.description || '暂无描述' }}</span>
                        <small v-if="(product.templateSpecGroups || product.specGroups)?.length">
                          {{ (product.templateSpecGroups || product.specGroups).length }} 组规格
                        </small>
                      </div>
                      <strong class="preview-product-price">￥{{ Number(product.referencePrice || product.basePrice || 0).toFixed(2) }}</strong>
                    </div>
                  </div>
                </article>
              </div>
            </template>
          </section>
        </div>

        <div class="modal-actions">
          <button class="ghost-button" @click="showTemplatePicker = false">取消</button>
          <button
            class="primary-button"
            :disabled="!selectedTemplate || templateImporting"
            @click="importSelectedTemplate"
          >
            {{ templateImporting ? '引入中...' : '引入当前模板' }}
          </button>
        </div>
      </section>
    </div>

    <div v-if="showCategoryEditor" class="modal-overlay" @click.self="showCategoryEditor = false">
      <section class="modal-content small">
        <h3>添加分类</h3>
        <label>
          分类名称
          <input v-model="categoryForm.name" placeholder="例如 烧烤、小吃、饮品" @keyup.enter="saveCategory" />
        </label>
        <label>
          分类类型
          <select v-model="categoryForm.categoryType">
            <option value="normal">普通</option>
            <option value="hot">热销</option>
            <option value="welfare">进店福利</option>
            <option value="combo">套餐</option>
          </select>
        </label>
        <label>
          图标
          <input v-model="categoryForm.icon" placeholder="可选，填一个短文字或符号" />
        </label>
        <div class="modal-actions">
          <button class="ghost-button" @click="showCategoryEditor = false">取消</button>
          <button class="primary-button" @click="saveCategory">保存</button>
        </div>
      </section>
    </div>

    <div v-if="showProductEditor" class="modal-overlay" @click.self="showProductEditor = false">
      <section class="modal-content">
        <h3>{{ editingProductId ? '编辑商品' : '添加商品' }}</h3>
        <div class="form-grid">
          <label>
            所属分类
            <select v-model="productForm.categoryId">
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
          </label>
          <label>
            商品名称
            <input v-model="productForm.name" placeholder="例如 烤羊肉串" />
          </label>
          <label>
            基础价格
            <input v-model.number="productForm.basePrice" type="number" min="0" step="0.01" />
          </label>
          <label>
            起购数量
            <input v-model.number="productForm.minOrderQty" type="number" min="1" step="1" />
          </label>
          <label>
            状态
            <select v-model="productForm.status">
              <option value="on">上架</option>
              <option value="off">下架</option>
              <option value="sold_out">售罄</option>
            </select>
          </label>
          <label class="inline-check form-check">
            <input v-model="productForm.isCombo" type="checkbox" />
            套餐商品
          </label>
        </div>
        <div class="image-upload-section">
          <span class="field-label">菜品图片</span>
          <div class="image-upload-row">
            <button
              v-if="productForm.imageUrl"
              type="button"
              class="editor-image-preview"
              @click="openImagePreview(productForm.imageUrl)"
            >
              <img :src="productForm.imageUrl" alt="当前菜品图片" />
              <span>点击放大</span>
            </button>
            <div v-else class="editor-image-placeholder">还没有图片</div>
            <label class="upload-button" :class="{ disabled: uploadingImage }">
              {{ uploadingImage ? '上传中...' : productForm.imageUrl ? '更换图片' : '上传图片' }}
              <input type="file" accept="image/*" :disabled="uploadingImage" @change="uploadProductImage" />
            </label>
            <button
              v-if="productForm.imageUrl"
              type="button"
              class="text-button"
              @click="clearProductImage"
            >
              移除图片
            </button>
          </div>
          <p class="upload-tip">选择本地图片上传即可，无需填写图片 URL；支持 JPG、PNG、WebP，保存商品后生效。</p>
        </div>
        <label>
          商品描述
          <textarea v-model="productForm.description" rows="3" placeholder="口味、份量或制作说明"></textarea>
        </label>

        <div class="spec-editor">
          <div class="section-title">
            <span>规格</span>
            <button class="ghost-button compact" @click="addSpecGroup">添加规格组</button>
          </div>
          <div v-for="(group, groupIndex) in productForm.specGroups" :key="groupIndex" class="spec-group-editor">
            <div class="spec-group-head">
              <input v-model="group.name" placeholder="规格组，如 份量、辣度" />
              <label class="inline-check">
                <input v-model="group.isRequired" type="checkbox" />
                必选
              </label>
              <button class="danger-button compact" @click="removeSpecGroup(Number(groupIndex))">删除组</button>
            </div>
            <div v-for="(option, optionIndex) in group.options" :key="optionIndex" class="spec-option-row">
              <input v-model="option.name" placeholder="选项，如 大份" />
              <input v-model.number="option.extraPrice" type="number" step="0.01" placeholder="加价" />
              <input v-model.number="option.stock" type="number" min="0" step="1" placeholder="库存" />
              <label class="inline-check">
                <input v-model="option.isDefault" type="checkbox" />
                默认
              </label>
              <button class="text-button" @click="removeSpecOption(group, Number(optionIndex))">删除</button>
            </div>
            <button class="ghost-button compact" @click="addSpecOption(group)">添加选项</button>
          </div>
        </div>

        <div v-if="productForm.isCombo" class="combo-editor">
          <div class="section-title">
            <span>套餐明细</span>
            <button class="ghost-button compact" :disabled="availableComboProducts.length === 0" @click="addComboItem">添加子商品</button>
          </div>
          <div v-if="availableComboProducts.length === 0" class="combo-empty">请先录入普通商品，再创建套餐。</div>
          <div v-for="(item, index) in productForm.comboItems" :key="index" class="combo-row">
            <select v-model="item.productId">
              <option value="">选择商品</option>
              <option v-for="product in availableComboProducts" :key="product.id" :value="product.id">
                {{ product.name }}
              </option>
            </select>
            <input v-model.number="item.quantity" type="number" min="1" step="1" placeholder="数量" />
            <label class="inline-check">
              <input v-model="item.allowChangeSpec" type="checkbox" />
              可改规格
            </label>
            <input v-model="item.remark" placeholder="备注" />
            <button class="danger-button compact" @click="removeComboItem(Number(index))">删除</button>
          </div>
        </div>

        <div class="modal-actions">
          <button class="ghost-button" @click="showProductEditor = false">取消</button>
          <button class="primary-button" :disabled="saving || uploadingImage" @click="saveProduct">
            {{ saving ? '保存中...' : '保存商品' }}
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped lang="scss">
.menu-page {
  min-height: 100vh;
  background: #f6f7f3;
  color: #1a1a2e;
}

.menu-header {
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
.modal-actions,
.product-actions,
.section-title {
  display: flex;
  align-items: center;
  gap: 8px;
}

.primary-button,
.ghost-button,
.danger-button,
.text-button {
  border: none;
  border-radius: 8px;
  min-height: 36px;
  padding: 0 14px;
  font-weight: 700;
  cursor: pointer;
}

.primary-button {
  background: #087e6b;
  color: #fff;

  &:disabled {
    opacity: 0.45;
    cursor: not-allowed;
  }
}

.ghost-button:disabled,
.danger-button:disabled,
.text-button:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.ghost-button {
  background: #e7f4ef;
  color: #D94C4C;
}

.danger-button {
  background: #e7f4ef;
  color: #d9363e;
}

.text-button {
  background: transparent;
  color: #1677ff;
  padding: 0 4px;
}

.compact {
  min-height: 30px;
  padding: 0 10px;
}

.loading-state,
.empty-state {
  padding: 48px 16px;
  text-align: center;
  color: #777;
}

.menu-content {
  display: flex;
  min-height: calc(100vh - 69px);
}

.category-sidebar {
  width: 108px;
  padding: 8px;
  background: #fff;
  border-right: 1px solid #ececec;
  overflow-y: auto;
}

.category-item {
  position: relative;
  display: grid;
  gap: 2px;
  width: 100%;
  min-height: 76px;
  margin-bottom: 8px;
  padding: 8px;
  border: 1px solid transparent;
  border-radius: 8px;
  background: #f6f7f3;
  color: #666;
  text-align: center;
  cursor: pointer;

  &.active {
    border-color: #087e6b;
    background: #e7f4ef;
    color: #D94C4C;
  }

  &.drag-over {
    border-color: #087e6b;
    box-shadow: inset 0 0 0 1px #087e6b;
  }
}

.cat-icon {
  font-size: 18px;
}

.cat-name {
  font-size: 13px;
  font-weight: 700;
  word-break: break-word;
}

.cat-type,
.cat-count {
  font-size: 11px;
  color: #999;
}

.cat-delete {
  position: absolute;
  top: 2px;
  right: 6px;
  color: #d9363e;
}

.product-list {
  flex: 1;
  padding: 12px;
  overflow-y: auto;
}

.product-card {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  justify-content: space-between;
  padding: 14px;
  margin-bottom: 10px;
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;

  &.dragging {
    opacity: 0.56;
  }
}

.drag-handle {
  flex: 0 0 20px;
  min-height: 86px;
  display: grid;
  place-items: center;
  color: #9aa9a3;
  cursor: grab;
  font-size: 18px;
  line-height: 1;
  user-select: none;
}

.product-image-button,
.product-image-placeholder {
  flex: 0 0 86px;
  width: 86px;
  height: 86px;
  border-radius: 10px;
}

.product-image-button {
  position: relative;
  padding: 0;
  border: none;
  overflow: hidden;
  background: #eef3ef;
  cursor: zoom-in;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
  }

  span {
    position: absolute;
    inset-inline: 0;
    bottom: 0;
    padding: 4px 0;
    background: rgba(0, 0, 0, 0.55);
    color: #fff;
    font-size: 11px;
  }
}

.product-image-placeholder {
  display: grid;
  place-items: center;
  border: 1px dashed #cfdad5;
  color: #8a9892;
  background: #f6f7f3;
  font-size: 12px;
}

.product-main {
  flex: 1;
  min-width: 0;
}

.product-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 4px;
}

.status-pill {
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 12px;
  background: #f1f1f1;
  color: #666;

  &.on {
    background: #f6ffed;
    color: #389e0d;
  }

  &.sold_out {
    background: #FFF6DB;
    color: #d46b08;
  }
}

.product-desc {
  margin: 0 0 8px;
  color: #777;
  font-size: 13px;
}

.product-meta,
.spec-summary {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  font-size: 12px;
  color: #666;
}

.combo-pill {
  padding: 2px 7px;
  border-radius: 999px;
  color: #9A6A00;
  background: #FFF6DB;
  font-weight: 800;
}

.spec-summary {
  margin-top: 8px;

  span {
    padding: 3px 8px;
    border-radius: 6px;
    background: #f6f7f3;
  }
}

.product-actions {
  flex-shrink: 0;
}

.image-preview-overlay {
  position: fixed;
  inset: 0;
  z-index: 260;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background: rgba(0, 0, 0, 0.74);
}

.image-preview-dialog {
  position: relative;
  max-width: min(92vw, 820px);
  max-height: 88vh;

  img {
    display: block;
    max-width: 100%;
    max-height: 88vh;
    border-radius: 12px;
    box-shadow: 0 18px 48px rgba(0, 0, 0, 0.35);
    object-fit: contain;
  }
}

.image-preview-close {
  position: absolute;
  top: -14px;
  right: -14px;
  width: 34px;
  height: 34px;
  border: none;
  border-radius: 50%;
  background: #fff;
  color: #1a1a2e;
  font-size: 22px;
  line-height: 1;
  cursor: pointer;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.24);
}

.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 200;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  background: rgba(0, 0, 0, 0.45);
}

.modal-content {
  width: min(720px, 100%);
  max-height: 90vh;
  overflow-y: auto;
  padding: 20px;
  border-radius: 8px;
  background: #fff;

  &.small {
    width: min(380px, 100%);
  }

  h3 {
    margin: 0 0 16px;
  }
}

.template-picker-modal {
  width: min(960px, 100%);
}

.template-picker-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;

  h3 {
    margin: 0 0 4px;
  }

  p {
    margin: 0;
    color: #687872;
    font-size: 13px;
  }
}

.compact-state {
  padding: 28px 16px;
}

.template-picker-body {
  display: grid;
  grid-template-columns: 288px minmax(0, 1fr);
  gap: 12px;
  min-height: 430px;
}

.system-template-list,
.template-preview-panel {
  min-height: 0;
  border: 1px solid #e6eee9;
  border-radius: 10px;
  background: #fafcfa;
}

.system-template-list {
  display: grid;
  align-content: start;
  gap: 8px;
  padding: 8px;
  overflow-y: auto;
}

.system-template-item {
  display: grid;
  grid-template-columns: 62px minmax(0, 1fr);
  gap: 10px;
  width: 100%;
  padding: 8px;
  border: 1px solid transparent;
  border-radius: 8px;
  background: #fff;
  color: #1a1a2e;
  text-align: left;
  cursor: pointer;

  &.active {
    border-color: #087e6b;
    background: #e7f4ef;
  }

  img,
  .template-cover-placeholder {
    width: 62px;
    height: 62px;
    border-radius: 8px;
  }

  img {
    object-fit: cover;
  }
}

.template-cover-placeholder {
  display: grid;
  place-items: center;
  color: #087e6b;
  background: #e7f4ef;
  font-size: 13px;
  font-weight: 800;
}

.template-item-main {
  min-width: 0;
  display: grid;
  gap: 3px;

  strong {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  small {
    color: #687872;
    font-size: 12px;
  }
}

.template-list-empty {
  display: grid;
  place-items: center;
  min-height: 160px;
  padding: 18px;
  color: #687872;
  text-align: center;
}

.template-preview-panel {
  padding: 12px;
  overflow-y: auto;
}

.template-preview-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 10px;
  border-bottom: 1px solid #e6eee9;

  h4 {
    margin: 0;
    font-size: 17px;
  }

  p {
    margin: 4px 0 0;
    color: #687872;
    font-size: 13px;
  }

  span {
    flex-shrink: 0;
    padding: 4px 8px;
    border-radius: 999px;
    color: #087e6b;
    background: #e7f4ef;
    font-size: 12px;
    font-weight: 800;
  }
}

.template-category-preview-list {
  display: grid;
  gap: 10px;
  margin-top: 12px;
}

.template-category-preview {
  padding: 10px;
  border: 1px solid #e6eee9;
  border-radius: 8px;
  background: #fff;
}

.preview-category-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 8px;

  span {
    color: #687872;
    font-size: 12px;
  }
}

.preview-product-list {
  display: grid;
  gap: 8px;
}

.preview-product {
  display: grid;
  grid-template-columns: 48px minmax(0, 1fr) auto;
  gap: 8px;
  align-items: center;
  padding: 8px;
  border-radius: 8px;
  background: #f6f7f3;

  img,
  .preview-product-placeholder {
    width: 48px;
    height: 48px;
    border-radius: 6px;
  }

  img {
    object-fit: cover;
  }
}

.preview-product-placeholder {
  display: grid;
  place-items: center;
  color: #8a9892;
  background: #fff;
  font-size: 12px;
}

.preview-product-info {
  min-width: 0;
  display: grid;
  gap: 2px;

  strong,
  span {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  span,
  small {
    color: #687872;
    font-size: 12px;
  }
}

.preview-product-price {
  color: #087e6b;
  font-size: 13px;
}

label {
  display: grid;
  gap: 6px;
  margin-bottom: 12px;
  font-size: 13px;
  color: #666;
}

input,
select,
textarea {
  width: 100%;
  min-height: 38px;
  padding: 8px 10px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #fff;
  color: #1a1a2e;
  font-size: 14px;
  box-sizing: border-box;
}

textarea {
  resize: vertical;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.image-upload-section {
  margin: 4px 0 12px;
  padding: 12px;
  border: 1px solid #e6eee9;
  border-radius: 10px;
  background: #fafcfa;
}

.field-label {
  display: block;
  margin-bottom: 8px;
  color: #666;
  font-size: 13px;
}

.image-upload-row {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.editor-image-preview,
.editor-image-placeholder {
  width: 104px;
  height: 104px;
  border-radius: 10px;
}

.editor-image-preview {
  position: relative;
  padding: 0;
  border: none;
  overflow: hidden;
  background: #eef3ef;
  cursor: zoom-in;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
  }

  span {
    position: absolute;
    inset-inline: 0;
    bottom: 0;
    padding: 5px 0;
    background: rgba(0, 0, 0, 0.55);
    color: #fff;
    font-size: 12px;
  }
}

.editor-image-placeholder {
  display: grid;
  place-items: center;
  border: 1px dashed #cfdad5;
  color: #8a9892;
  background: #fff;
  font-size: 13px;
}

.upload-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 36px;
  padding: 0 14px;
  margin: 0;
  border-radius: 8px;
  background: #087e6b;
  color: #fff;
  font-weight: 800;
  cursor: pointer;

  &.disabled {
    opacity: 0.55;
    cursor: not-allowed;
  }

  input {
    display: none;
  }
}

.upload-tip {
  margin: 8px 0 0;
  color: #8a9892;
  font-size: 12px;
}

.section-title {
  justify-content: space-between;
  margin: 14px 0 10px;
  font-weight: 800;
}

.spec-group-editor {
  padding: 12px;
  margin-bottom: 10px;
  border: 1px solid #eee;
  border-radius: 8px;
  background: #fafcfa;
}

.spec-group-head,
.spec-option-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto auto;
  gap: 8px;
  align-items: center;
  margin-bottom: 8px;
}

.spec-option-row {
  grid-template-columns: minmax(0, 1fr) 86px 86px auto auto;
}

.inline-check {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  margin: 0;
  white-space: nowrap;

  input {
    width: auto;
    min-height: auto;
  }
}

.form-check {
  align-self: end;
  min-height: 38px;
  padding: 0 10px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #fafcfa;
}

.combo-editor {
  margin-top: 10px;
}

.combo-empty {
  padding: 10px 12px;
  border: 1px dashed #DCE6E1;
  border-radius: 8px;
  color: #687872;
  background: #FAFCFA;
  font-size: 13px;
}

.combo-row {
  display: grid;
  grid-template-columns: minmax(0, 1.2fr) 80px auto minmax(0, 1fr) auto;
  gap: 8px;
  align-items: center;
  margin-bottom: 8px;
}

@media (max-width: 640px) {
  .menu-header,
  .product-card,
  .product-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .product-image-button,
  .product-image-placeholder {
    width: 100%;
    height: 150px;
    flex-basis: auto;
  }

  .drag-handle {
    min-height: 24px;
    place-items: start;
  }

  .form-grid,
  .template-picker-body,
  .image-upload-row,
  .spec-group-head,
  .spec-option-row,
  .combo-row {
    grid-template-columns: 1fr;
  }
}
</style>
