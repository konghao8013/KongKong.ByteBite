<script setup lang="ts">
withDefaults(defineProps<{
  size?: 'small' | 'default' | 'large'
  text?: string
}>(), {
  size: 'default',
  text: '加载中...',
})
</script>

<template>
  <div class="loading-spinner" :class="size">
    <div class="spinner" />
    <span v-if="text" class="loading-text">{{ text }}</span>
  </div>
</template>

<style scoped lang="scss">
.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px;
  gap: 12px;
}

.spinner {
  border: 3px solid #eee;
  border-top-color: #087E6B;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.loading-spinner.small .spinner {
  width: 20px;
  height: 20px;
  border-width: 2px;
}

.loading-spinner.default .spinner {
  width: 32px;
  height: 32px;
}

.loading-spinner.large .spinner {
  width: 48px;
  height: 48px;
  border-width: 4px;
}

.loading-text {
  font-size: 14px;
  color: #999;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
