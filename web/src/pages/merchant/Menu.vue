<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { categoryApi } from '@/api/modules/category'
import { productApi } from '@/api/modules/product'

const storeId = localStorage.getItem('merchant_store_id') || ''
const loading = ref(false)
const categories = ref<any[]>([])
const products = ref<any[]>([])
const activeCategory = ref<string>('')
const showAddCategory = ref(false)
const newCategoryName = ref('')

const activeCategoryProducts = computed(() => {
  if (!activeCategory.value) return products.value
  return products.value.filter(p => p.categoryId === activeCategory.value)
})

const loadCategories = async () => {
  if (!storeId) return
  try {
    categories.value = await categoryApi.getByStoreId(storeId) || []
    if (categories.value.length > 0 && !activeCategory.value) {
      activeCategory.value = categories.value[0].id
    }
  } catch (e) { console.error('加载分类失败', e) }
}

const loadProducts = async () => {
  if (!storeId) return
  try {
    products.value = await productApi.getByStoreId(storeId) || []
  } catch (e) { console.error('加载商品失败', e) }
}

const loadData = async () => {
  loading.value = true
  await Promise.all([loadCategories(), loadProducts()])
  loading.value = false
}

const handleAddCategory = async () => {
  if (!newCategoryName.value.trim()) return
  try {
    await categoryApi.create(storeId, { name: newCategoryName.value, sortOrder: categories.value.length + 1 })
    newCategoryName.value = ''
    showAddCategory.value = false
    await loadCategories()
  } catch (e) { console.error('添加分类失败', e) }
}

const handleDeleteCategory = async (id: string) => {
  if (!confirm('确定删除该分类？')) return
  try {
    await categoryApi.delete(id)
    await loadCategories()
  } catch (e: any) {
    alert(e?.message || '删除失败，分类下可能还有商品')
  }
}

const toggleProductStatus = async (product: any) => {
  const newStatus = product.status === 'on' ? 'off' : 'on'
  try {
    await productApi.update(product.id, { status: newStatus })
    product.status = newStatus
  } catch (e) { console.error('操作失败', e) }
}

const statusLabel = (status: string) => {
  const map: Record<string, string> = { on: '上架', off: '下架', sold_out: '售罄' }
  return map[status] || status
}

const statusColor = (status: string) => {
  const map: Record<string, string> = { on: '#4CAF50', off: '#999', sold_out: '#FF9800' }
  return map[status] || '#999'
}

onMounted(loadData)
</script>

<template>
  <div class="menu-page">
    <div class="menu-header">
      <h2>菜单管理</h2>
      <button class="btn-add" @click="showAddCategory = true">+ 添加分类</button>
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div v-else class="menu-content">
      <div class="category-sidebar">
        <div
          v-for="cat in categories"
          :key="cat.id"
          class="category-item"
          :class="{ active: activeCategory === cat.id }"
          @click="activeCategory = cat.id"
        >
          <span class="cat-icon">{{ cat.icon || '📁' }}</span>
          <span class="cat-name">{{ cat.name }}</span>
          <span class="cat-count">{{ products.filter(p => p.categoryId === cat.id).length }}</span>
          <button class="cat-delete" @click.stop="handleDeleteCategory(cat.id)">×</button>
        </div>
      </div>

      <div class="product-list">
        <div v-if="activeCategoryProducts.length === 0" class="empty-state">
          <span>📭</span>
          <p>该分类下暂无商品</p>
        </div>

        <div v-for="product in activeCategoryProducts" :key="product.id" class="product-card">
          <div class="product-info">
            <div class="product-name">{{ product.name }}</div>
            <div class="product-desc">{{ product.description }}</div>
            <div class="product-meta">
              <span class="product-price">¥{{ product.basePrice }}</span>
              <span class="product-sales">月销 {{ product.monthlySales }}</span>
              <span class="product-status" :style="{ color: statusColor(product.status) }">{{ statusLabel(product.status) }}</span>
            </div>
            <div v-if="product.specGroups && product.specGroups.length > 0" class="product-specs">
              <span v-for="sg in product.specGroups" :key="sg.id" class="spec-group">
                {{ sg.name }}: {{ sg.specOptions?.map((o: any) => o.name + (o.extraPrice > 0 ? `+¥${o.extraPrice}` : '')).join('/') }}
              </span>
            </div>
          </div>
          <div class="product-actions">
            <button
              class="btn-toggle"
              :class="{ 'btn-on': product.status === 'on', 'btn-off': product.status !== 'on' }"
              @click="toggleProductStatus(product)"
            >
              {{ product.status === 'on' ? '下架' : '上架' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showAddCategory" class="modal-overlay" @click.self="showAddCategory = false">
      <div class="modal-content">
        <h3>添加分类</h3>
        <input v-model="newCategoryName" placeholder="输入分类名称" @keyup.enter="handleAddCategory" />
        <div class="modal-actions">
          <button class="btn-cancel" @click="showAddCategory = false">取消</button>
          <button class="btn-confirm" @click="handleAddCategory">确定</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.menu-page {
  background: #1a1a2e;
  min-height: 100vh;
  color: #fff;
}

.menu-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;

  h2 { font-size: 20px; font-weight: 700; margin: 0; }

  .btn-add {
    background: #FFD161;
    color: #1a1a2e;
    border: none;
    padding: 8px 16px;
    border-radius: 8px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
  }
}

.loading-state { text-align: center; padding: 60px; color: #999; }

.menu-content { display: flex; min-height: calc(100vh - 60px); }

.category-sidebar {
  width: 90px;
  background: #2A2A2A;
  padding: 8px 4px;
  overflow-y: auto;
}

.category-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 10px 4px;
  border-radius: 8px;
  margin-bottom: 4px;
  cursor: pointer;
  position: relative;
  color: #999;
  font-size: 12px;
  transition: all 0.2s;

  &.active { background: #3A3A3A; color: #FFD161; }

  .cat-icon { font-size: 20px; margin-bottom: 4px; }
  .cat-name { font-size: 11px; text-align: center; word-break: break-all; }
  .cat-count { font-size: 10px; color: #666; margin-top: 2px; }
  .cat-delete {
    position: absolute;
    top: 2px;
    right: 2px;
    background: none;
    border: none;
    color: #666;
    font-size: 14px;
    cursor: pointer;
    display: none;
  }
  &:hover .cat-delete { display: block; }
}

.product-list {
  flex: 1;
  padding: 12px;
  overflow-y: auto;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #999;
  span { font-size: 48px; display: block; margin-bottom: 12px; }
}

.product-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #2A2A2A;
  border-radius: 10px;
  padding: 12px;
  margin-bottom: 8px;
}

.product-info {
  flex: 1;

  .product-name { font-size: 15px; font-weight: 600; margin-bottom: 4px; }
  .product-desc { font-size: 12px; color: #999; margin-bottom: 6px; }
  .product-meta { display: flex; align-items: center; gap: 12px; font-size: 13px; }
  .product-price { color: #FFD161; font-weight: 600; }
  .product-sales { color: #999; }
  .product-status { font-weight: 600; }
  .product-specs { margin-top: 6px; }
  .spec-group { font-size: 11px; color: #999; background: #3A3A3A; padding: 2px 8px; border-radius: 4px; margin-right: 4px; }
}

.product-actions {
  .btn-toggle {
    padding: 6px 14px;
    border-radius: 6px;
    font-size: 13px;
    border: none;
    cursor: pointer;
    font-weight: 600;
  }
  .btn-on { background: #4CAF50; color: #fff; }
  .btn-off { background: #3A3A3A; color: #999; }
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
}

.modal-content {
  background: #2A2A2A;
  border-radius: 12px;
  padding: 24px;
  width: 90%;
  max-width: 360px;

  h3 { margin: 0 0 16px; font-size: 18px; }

  input {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid #3A3A3A;
    border-radius: 8px;
    font-size: 15px;
    background: #1a1a2e;
    color: #fff;
    outline: none;

    &:focus { border-color: #FFD161; }
  }
}

.modal-actions {
  display: flex;
  gap: 12px;
  margin-top: 16px;

  button {
    flex: 1;
    padding: 10px;
    border-radius: 8px;
    font-size: 14px;
    font-weight: 600;
    border: none;
    cursor: pointer;
  }
  .btn-cancel { background: #3A3A3A; color: #999; }
  .btn-confirm { background: #FFD161; color: #1a1a2e; }
}
</style>