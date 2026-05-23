<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const goToMerchant = () => {
  router.push('/login')
}

const goToAdmin = () => {
  router.push('/login')
}
</script>

<template>
  <div class="home-page">
    <div class="home-hero">
      <div class="hero-icon">🍔</div>
      <h1 class="hero-title">空空码上点单</h1>
      <p class="hero-subtitle">KongKong.ByteBite</p>
      <p class="hero-desc">扫码即点，码上就好</p>
    </div>

    <div class="home-actions">
      <div class="action-card" @click="goToMerchant">
        <span class="action-icon">🏪</span>
        <div class="action-info">
          <h3>商家入口</h3>
          <p>登录管理店铺、菜单和订单</p>
        </div>
        <span class="action-arrow">›</span>
      </div>

      <div class="action-card" @click="goToAdmin">
        <span class="action-icon">⚙️</span>
        <div class="action-info">
          <h3>管理后台</h3>
          <p>系统配置与商家审核</p>
        </div>
        <span class="action-arrow">›</span>
      </div>
    </div>

    <div class="home-tip">
      <p>顾客请扫描商家二维码直接进入店铺</p>
    </div>
  </div>
</template>

<style scoped lang="scss">
.home-page {
  min-height: 100vh;
  min-height: 100dvh;
  display: flex;
  flex-direction: column;
  align-items: center;
  background: linear-gradient(135deg, #FFD161 0%, #FF6633 100%);
  padding: 60px 24px 40px;
}

.home-hero {
  text-align: center;
  margin-bottom: 48px;
}

.hero-icon {
  font-size: 72px;
  margin-bottom: 16px;
}

.hero-title {
  font-size: 32px;
  font-weight: 700;
  color: #fff;
  margin: 0 0 4px;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.hero-subtitle {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
  margin: 0 0 12px;
  letter-spacing: 2px;
}

.hero-desc {
  font-size: 18px;
  color: rgba(255, 255, 255, 0.9);
  margin: 0;
}

.home-actions {
  width: 100%;
  max-width: 400px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.action-card {
  display: flex;
  align-items: center;
  gap: 16px;
  background: #fff;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;

  &:active {
    transform: scale(0.98);
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
  }
}

.action-icon {
  font-size: 36px;
  flex-shrink: 0;
}

.action-info {
  flex: 1;

  h3 {
    font-size: 17px;
    font-weight: 600;
    color: #333;
    margin: 0 0 4px;
  }

  p {
    font-size: 13px;
    color: #999;
    margin: 0;
  }
}

.action-arrow {
  font-size: 24px;
  color: #ccc;
  flex-shrink: 0;
}

.home-tip {
  margin-top: 40px;
  text-align: center;

  p {
    font-size: 13px;
    color: rgba(255, 255, 255, 0.7);
    margin: 0;
  }
}
</style>
