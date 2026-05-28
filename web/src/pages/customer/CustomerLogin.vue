<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { customerApi } from '@/api/modules/customer'
import { useDeviceId } from '@/composables/useDeviceId'

const route = useRoute()
const router = useRouter()
const { getDeviceId } = useDeviceId()

const mode = ref<'login' | 'register'>('login')
const registerType = ref<'phone' | 'username'>('phone')
const account = ref('')
const phone = ref('')
const username = ref('')
const password = ref('')
const nickname = ref('')
const submitting = ref(false)

const title = computed(() => mode.value === 'login' ? '顾客登录' : '顾客注册')

const saveCustomer = (customer: { id: string; token?: string }) => {
  if (customer.token) localStorage.setItem('customer_token', customer.token)
  localStorage.setItem('customer_id', customer.id)
  localStorage.setItem('customer_info', JSON.stringify(customer))
}

const finish = async (customer: { id: string; token?: string }) => {
  saveCustomer(customer)
  try {
    await customerApi.mergeData(customer.id, getDeviceId())
  } catch {
  }
  ElMessage.success('已登录')
  const redirect = String(route.query.redirect || '')
  router.replace(redirect || { name: 'StoreSearch' })
}

const submit = async () => {
  if (submitting.value) return
  submitting.value = true
  try {
    if (mode.value === 'login') {
      await finish(await customerApi.login({
        account: account.value.trim(),
        password: password.value,
        deviceId: getDeviceId(),
      }))
      return
    }

    await finish(await customerApi.register({
      phone: registerType.value === 'phone' ? phone.value.trim() : undefined,
      username: registerType.value === 'username' ? username.value.trim() : undefined,
      password: password.value,
      nickname: nickname.value.trim() || undefined,
      deviceId: getDeviceId(),
    }))
  } finally {
    submitting.value = false
  }
}

const switchMode = (next: 'login' | 'register') => {
  mode.value = next
  password.value = ''
}
</script>

<template>
  <div class="customer-login-page">
    <div class="login-panel">
      <button class="back-btn" @click="router.back()">‹</button>
      <h1>{{ title }}</h1>
      <p>登录后可跨设备查看订单和最近访问的店铺。</p>

      <div class="mode-tabs">
        <button :class="{ active: mode === 'login' }" @click="switchMode('login')">登录</button>
        <button :class="{ active: mode === 'register' }" @click="switchMode('register')">注册</button>
      </div>

      <div v-if="mode === 'register'" class="register-type">
        <button :class="{ active: registerType === 'phone' }" @click="registerType = 'phone'">手机号</button>
        <button :class="{ active: registerType === 'username' }" @click="registerType = 'username'">账号名</button>
      </div>

      <div class="form-fields">
        <label v-if="mode === 'login'">
          <span>手机号或账号名</span>
          <input v-model="account" autocomplete="username" placeholder="输入手机号或账号名" />
        </label>

        <label v-else-if="registerType === 'phone'">
          <span>手机号</span>
          <input v-model="phone" autocomplete="tel" placeholder="输入手机号" />
        </label>

        <label v-else>
          <span>账号名</span>
          <input v-model="username" autocomplete="username" placeholder="输入账号名" />
        </label>

        <label>
          <span>密码</span>
          <input v-model="password" autocomplete="current-password" type="password" placeholder="至少 6 位" />
        </label>

        <label v-if="mode === 'register'">
          <span>昵称</span>
          <input v-model="nickname" placeholder="可选" />
        </label>
      </div>

      <button class="submit-btn" :disabled="submitting" @click="submit">
        {{ submitting ? '处理中...' : title }}
      </button>
    </div>
  </div>
</template>

<style scoped lang="scss">
.customer-login-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px 16px;
  background: #F6F7F3;
}

.login-panel {
  width: 100%;
  max-width: 420px;
  padding: 22px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 12px 30px rgba(31, 42, 38, 0.08);
}

.back-btn {
  width: 34px;
  height: 34px;
  border-radius: 6px;
  color: #1F2A26;
  background: #F1F4F2;
  font-size: 24px;
}

h1 {
  margin: 18px 0 6px;
  color: #1F2A26;
  font-size: 24px;
}

p {
  margin: 0 0 18px;
  color: #687872;
  font-size: 13px;
}

.mode-tabs,
.register-type {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 6px;
  padding: 5px;
  border: 1px solid #E2E8E3;
  border-radius: 8px;
  background: #F6F7F3;
}

.register-type {
  margin-top: 10px;
}

.mode-tabs button,
.register-type button {
  height: 34px;
  border-radius: 6px;
  color: #687872;
  font-weight: 800;

  &.active {
    color: #fff;
    background: #087E6B;
  }
}

.form-fields {
  display: grid;
  gap: 12px;
  margin-top: 16px;
}

label {
  display: grid;
  gap: 6px;
  color: #1F2A26;
  font-size: 13px;
  font-weight: 800;
}

input {
  height: 42px;
  padding: 0 12px;
  border: 1px solid #DCE6E1;
  border-radius: 6px;
  outline: 0;
  color: #1F2A26;
  background: #FAFCFA;
  font-size: 14px;
}

.submit-btn {
  width: 100%;
  height: 44px;
  margin-top: 18px;
  border-radius: 6px;
  color: #fff;
  background: #087E6B;
  font-size: 15px;
  font-weight: 900;

  &:disabled {
    opacity: 0.65;
  }
}
</style>
