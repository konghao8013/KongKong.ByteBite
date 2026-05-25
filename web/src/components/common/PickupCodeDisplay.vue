<script setup lang="ts">
withDefaults(defineProps<{
  code: string
  status?: string
}>(), {
  status: '',
})

const statusLabels: Record<string, string> = {
  pending: '待接单',
  accepted: '制作中',
  preparing: '制作中',
  ready: '备餐完毕',
  completed: '已完成',
  cancelled: '已取消',
  rejected: '已拒单',
}
</script>

<template>
  <div class="pickup-code-display">
    <div class="code-label">取货码</div>
    <div class="code-value">{{ code }}</div>
    <div v-if="status" class="code-status" :class="status">
      {{ statusLabels[status] || status }}
    </div>
  </div>
</template>

<style scoped lang="scss">
.pickup-code-display {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 32px 20px;
  background: linear-gradient(135deg, #FF6B6B 0%, #FF8E53 100%);
  border-radius: 16px;
  color: #fff;
}

.code-label {
  font-size: 14px;
  opacity: 0.9;
  margin-bottom: 8px;
}

.code-value {
  font-size: 56px;
  font-weight: 800;
  letter-spacing: 12px;
  line-height: 1.2;
  text-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.code-status {
  margin-top: 12px;
  padding: 4px 16px;
  background: rgba(255, 255, 255, 0.25);
  border-radius: 12px;
  font-size: 13px;

  &.completed {
    background: rgba(82, 196, 26, 0.5);
  }

  &.cancelled,
  &.rejected {
    background: rgba(255, 77, 79, 0.5);
  }
}
</style>
