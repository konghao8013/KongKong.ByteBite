<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { categoryApi } from '@/api/modules/category'
import { productApi } from '@/api/modules/product'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const saving = ref(false)
const categories = ref<any[]>([])
const products = ref<any[]>([])
const activeCategory = ref<string>('')
const showCategoryEditor = ref(false)
const showProductEditor = ref(false)
const editingProductId = ref('')

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
  specGroups: [],
})

const activeCategoryProducts = computed(() => {
  if (!activeCategory.value) return products.value
  return products.value.filter(p => p.categoryId === activeCategory.value)
})

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
    options: [{ name: '', extraPrice: 0, sortOrder: 1, isDefault: true }],
  })
}

const removeSpecGroup = (index: number) => {
  productForm.specGroups.splice(index, 1)
}

const addSpecOption = (group: any) => {
  group.options.push({
    name: '',
    extraPrice: 0,
    sortOrder: group.options.length + 1,
    isDefault: group.options.length === 0,
  })
}

const removeSpecOption = (group: any, index: number) => {
  group.options.splice(index, 1)
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
          :class="{ active: activeCategory === cat.id }"
          @click="activeCategory = cat.id"
        >
          <span class="cat-icon">{{ cat.icon || '·' }}</span>
          <span class="cat-name">{{ cat.name }}</span>
          <span class="cat-type">{{ categoryLabel(cat.categoryType) }}</span>
          <span class="cat-count">{{ products.filter(p => p.categoryId === cat.id).length }}</span>
          <span class="cat-delete" @click.stop="deleteCategory(cat.id)">×</span>
        </button>
      </aside>

      <section class="product-list">
        <div v-if="categories.length === 0" class="empty-state">
          <p>先添加一个分类，再录入商品。</p>
        </div>
        <div v-else-if="activeCategoryProducts.length === 0" class="empty-state">
          <p>当前分类还没有商品。</p>
          <button class="primary-button compact" @click="openProductCreator">添加商品</button>
        </div>

        <article v-for="product in activeCategoryProducts" :key="product.id" class="product-card">
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
            </div>
            <div v-if="product.specGroups?.length" class="spec-summary">
              <span v-for="group in product.specGroups" :key="group.id">
                {{ group.name }}：{{ group.specOptions?.map((o: any) => `${o.name}${o.extraPrice ? `+￥${o.extraPrice}` : ''}`).join(' / ') }}
              </span>
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
          <label>
            图片 URL
            <input v-model="productForm.imageUrl" placeholder="可选" />
          </label>
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
              <label class="inline-check">
                <input v-model="option.isDefault" type="checkbox" />
                默认
              </label>
              <button class="text-button" @click="removeSpecOption(group, Number(optionIndex))">删除</button>
            </div>
            <button class="ghost-button compact" @click="addSpecOption(group)">添加选项</button>
          </div>
        </div>

        <div class="modal-actions">
          <button class="ghost-button" @click="showProductEditor = false">取消</button>
          <button class="primary-button" :disabled="saving" @click="saveProduct">
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
  gap: 12px;
  justify-content: space-between;
  padding: 14px;
  margin-bottom: 10px;
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;
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
  grid-template-columns: minmax(0, 1fr) 96px auto auto;
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

@media (max-width: 640px) {
  .menu-header,
  .product-card,
  .product-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .form-grid,
  .spec-group-head,
  .spec-option-row {
    grid-template-columns: 1fr;
  }
}
</style>
