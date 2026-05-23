<script setup lang="ts">
withDefaults(defineProps<{
  description?: string
  icon?: string
}>(), {
  description: '暂无数据',
  icon: '📭',
})
</script>

<template>
  <div class="empty-state">
    <div class="empty-icon">{{ icon }}</div>
    <div class="empty-description">{{ description }}</div>
    <div class="empty-action">
      <slot />
    </div>
  </div>
</template>

<style scoped lang="scss">
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.empty-description {
  font-size: 14px;
  color: #999;
  margin-bottom: 20px;
}

.empty-action {
  :deep(button) {
    padding: 8px 24px;
    background: #FF6633;
    color: #fff;
    border-radius: 20px;
    font-size: 14px;
  }
}
</style>
