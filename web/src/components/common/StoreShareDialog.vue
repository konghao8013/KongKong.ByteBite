<script setup lang="ts">
import { ref, computed } from 'vue'
import QrcodeVue from 'qrcode.vue'

const props = withDefaults(defineProps<{
  storeCode: string
  storeName: string
  visible: boolean
}>(), {
  storeCode: '',
  storeName: '',
  visible: false,
})

const emit = defineEmits(['close'])

const shareUrl = computed(() => {
  return `${window.location.origin}/A/${props.storeCode}`
})

const toastVisible = ref(false)
const toastMsg = ref('')

const showToast = (msg: string) => {
  toastMsg.value = msg
  toastVisible.value = true
  setTimeout(() => { toastVisible.value = false }, 2000)
}

const copyLink = async () => {
  try {
    await navigator.clipboard.writeText(shareUrl.value)
    showToast('链接已复制')
  } catch {
    const input = document.createElement('input')
    input.value = shareUrl.value
    document.body.appendChild(input)
    input.select()
    document.execCommand('copy')
    document.body.removeChild(input)
    showToast('链接已复制')
  }
}

const shareNative = async () => {
  if (navigator.share) {
    try {
      await navigator.share({
        title: `${props.storeName} - 空空码上点单`,
        text: `来${props.storeName}点单吧！`,
        url: shareUrl.value,
      })
    } catch {
      await copyLink()
    }
  } else {
    await copyLink()
  }
}

const closeDialog = () => {
  emit('close')
}
</script>

<template>
  <div v-if="visible" class="share-overlay" @click.self="closeDialog">
    <div class="share-panel">
      <div class="share-header">
        <h3>分享店铺</h3>
        <button class="close-btn" @click="closeDialog">✕</button>
      </div>

      <div class="qr-section">
        <div class="qr-wrapper">
          <QrcodeVue :value="shareUrl" :size="180" level="M" />
        </div>
        <p class="store-name">{{ storeName }}</p>
        <p class="share-code">店铺码：{{ storeCode }}</p>
        <p class="share-hint">扫一扫，进入店铺点单</p>
      </div>

      <div class="link-section">
        <div class="link-box">
          <span class="link-text">{{ shareUrl }}</span>
          <button class="btn-copy" @click="copyLink">复制</button>
        </div>
      </div>

      <div class="action-section">
        <button class="btn-share" @click="shareNative">
          <span class="share-icon">🔗</span>
          分享链接给好友
        </button>
      </div>
    </div>
  </div>

  <div v-if="toastVisible" class="toast">{{ toastMsg }}</div>
</template>

<style scoped lang="scss">
.share-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  z-index: 300;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.share-panel {
  background: #FFFFFF;
  border-radius: 20px;
  width: 100%;
  max-width: 340px;
  overflow: hidden;
}

.share-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 20px 10px;

  h3 { font-size: 17px; font-weight: 700; color: #1F2A26; margin: 0; }

  .close-btn {
    width: 30px;
    height: 30px;
    border-radius: 50%;
    background: #F6F7F3;
    border: none;
    font-size: 14px;
    color: #687872;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

.qr-section {
  text-align: center;
  padding: 8px 20px 16px;

  .qr-wrapper {
    display: inline-block;
    padding: 16px;
    background: #FFFFFF;
    border-radius: 16px;
    border: 2px solid #E2E8E3;
  }

  .store-name {
    font-size: 16px;
    font-weight: 700;
    color: #1F2A26;
    margin: 12px 0 4px;
  }

  .share-code {
    font-size: 15px;
    font-weight: 600;
    color: #087E6B;
    letter-spacing: 2px;
    margin: 4px 0;
  }

  .share-hint {
    font-size: 13px;
    color: #687872;
    margin: 0;
  }
}

.link-section {
  padding: 0 20px 14px;

  .link-box {
    display: flex;
    align-items: center;
    background: #F6F7F3;
    border-radius: 10px;
    padding: 10px 12px;
    gap: 8px;
  }

  .link-text {
    flex: 1;
    font-size: 12px;
    color: #595959;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .btn-copy {
    flex-shrink: 0;
    padding: 5px 14px;
    border-radius: 8px;
    background: linear-gradient(135deg, #087E6B, #0EA389);
    color: #FFFFFF;
    border: none;
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    white-space: nowrap;
  }
}

.action-section {
  padding: 0 20px 20px;

  .btn-share {
    width: 100%;
    padding: 13px;
    border-radius: 12px;
    background: linear-gradient(135deg, #087E6B, #0EA389);
    color: #FFFFFF;
    border: none;
    font-size: 16px;
    font-weight: 700;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;

    .share-icon { font-size: 18px; }
  }
}

.toast {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: rgba(0, 0, 0, 0.7);
  color: #FFFFFF;
  padding: 10px 24px;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 500;
  z-index: 400;
}
</style>